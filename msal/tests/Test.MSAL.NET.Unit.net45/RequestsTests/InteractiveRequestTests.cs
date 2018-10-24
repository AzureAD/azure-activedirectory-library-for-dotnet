﻿// ------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.Instance;
using Microsoft.Identity.Core.OAuth2;
using Microsoft.Identity.Core.Telemetry;
using Microsoft.Identity.Core.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.Core.Unit;
using Test.Microsoft.Identity.Core.Unit.Mocks;
using Test.MSAL.NET.Unit.Mocks;

namespace Test.MSAL.NET.Unit.RequestsTests
{
    [TestClass]
    public class InteractiveRequestTests
    {
        private readonly MyReceiver _myReceiver = new MyReceiver();
        private TokenCache _cache;

        [TestInitialize]
        public void TestInitialize()
        {
            RequestTestsCommon.InitializeRequestTests();
            Telemetry.GetInstance().RegisterReceiver(_myReceiver.OnEvents);

            _cache = new TokenCache();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _cache.tokenCacheAccessor.ClearAccessTokens();
            _cache.tokenCacheAccessor.ClearRefreshTokens();
        }

        [TestMethod]
        [TestCategory("InteractiveRequestTests")]
        public void SliceParametersTest()
        {
            var authority = Authority.CreateAuthority(MsalTestConstants.AuthorityHomeTenant, false);
            _cache = new TokenCache()
            {
                ClientId = MsalTestConstants.ClientId
            };

            var ui = new MockWebUI()
            {
                MockResult = new AuthorizationResult(
                    AuthorizationStatus.Success,
                    MsalTestConstants.AuthorityHomeTenant + "?code=some-code"),
                QueryParamsToValidate = new Dictionary<string, string>()
                {
                    {"key1", "value1%20with%20encoded%20space"},
                    {"key2", "value2"}
                }
            };

            using (var httpManager = new MockHttpManager())
            {
                RequestTestsCommon.MockInstanceDiscoveryAndOpenIdRequest(httpManager);

                httpManager.AddMockHandler(
                    new MockHttpMessageHandler
                    {
                        Method = HttpMethod.Post,
                        QueryParams = new Dictionary<string, string>()
                        {
                            {"key1", "value1%20with%20encoded%20space"},
                            {"key2", "value2"}
                        },
                        ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
                    });

                var parameters = new AuthenticationRequestParameters
                {
                    Authority = authority,
                    SliceParameters = "key1=value1%20with%20encoded%20space&key2=value2",
                    ClientId = MsalTestConstants.ClientId,
                    Scope = MsalTestConstants.Scope,
                    TokenCache = _cache,
                    RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null)),
                    RedirectUri = new Uri("some://uri"),
                    ExtraQueryParameters = "extra=qp"
                };

                var request = new InteractiveRequest(
                    httpManager,
                    PlatformProxyFactory.GetPlatformProxy().CryptographyManager,
                    parameters,
                    ApiEvent.ApiIds.None,
                    MsalTestConstants.ScopeForAnotherResource.ToArray(),
                    MsalTestConstants.DisplayableId,
                    UIBehavior.SelectAccount,
                    ui);
                Task<AuthenticationResult> task = request.RunAsync(CancellationToken.None);
                task.Wait();
                var result = task.Result;
                Assert.IsNotNull(result);
                Assert.AreEqual(1, _cache.tokenCacheAccessor.RefreshTokenCount);
                Assert.AreEqual(1, _cache.tokenCacheAccessor.AccessTokenCount);
                Assert.AreEqual(result.AccessToken, "some-access-token");
            }
        }

        [TestMethod]
        [TestCategory("InteractiveRequestTests")]
        public void NoCacheLookup()
        {
            var authority = Authority.CreateAuthority(MsalTestConstants.AuthorityHomeTenant, false);
            _cache = new TokenCache()
            {
                ClientId = MsalTestConstants.ClientId
            };

            var atItem = new MsalAccessTokenCacheItem(
                MsalTestConstants.ProductionPrefNetworkEnvironment,
                MsalTestConstants.ClientId,
                "Bearer",
                MsalTestConstants.Scope.AsSingleString(),
                MsalTestConstants.Utid,
                null,
                new DateTimeOffset(DateTime.UtcNow + TimeSpan.FromSeconds(3599)),
                MockHelpers.CreateClientInfo());

            string atKey = atItem.GetKey().ToString();
            atItem.Secret = atKey;
            _cache.tokenCacheAccessor.SaveAccessToken(atItem);

            var ui = new MockWebUI()
            {
                MockResult = new AuthorizationResult(
                    AuthorizationStatus.Success,
                    MsalTestConstants.AuthorityHomeTenant + "?code=some-code")
            };

            using (var httpManager = new MockHttpManager())
            {
                RequestTestsCommon.MockInstanceDiscoveryAndOpenIdRequest(httpManager);

                httpManager.AddSuccessTokenResponseMockHandlerForPost();

                var parameters = new AuthenticationRequestParameters
                {
                    Authority = authority,
                    ClientId = MsalTestConstants.ClientId,
                    Scope = MsalTestConstants.Scope,
                    TokenCache = _cache,
                    RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null)),
                    RedirectUri = new Uri("some://uri"),
                    ExtraQueryParameters = "extra=qp"
                };

                var request = new InteractiveRequest(
                    httpManager,
                    PlatformProxyFactory.GetPlatformProxy().CryptographyManager,
                    parameters,
                    ApiEvent.ApiIds.None,
                    MsalTestConstants.ScopeForAnotherResource.ToArray(),
                    MsalTestConstants.DisplayableId,
                    UIBehavior.SelectAccount,
                    ui);
                Task<AuthenticationResult> task = request.RunAsync(CancellationToken.None);
                task.Wait();
                var result = task.Result;
                Assert.IsNotNull(result);
                Assert.AreEqual(1, _cache.tokenCacheAccessor.RefreshTokenCount);
                Assert.AreEqual(2, _cache.tokenCacheAccessor.AccessTokenCount);
                Assert.AreEqual(result.AccessToken, "some-access-token");

                Assert.IsNotNull(
                    _myReceiver.EventsReceived.Find(
                        anEvent => // Expect finding such an event
                            anEvent[EventBase.EventNameKey].EndsWith("ui_event") &&
                            anEvent[UiEvent.UserCancelledKey] == "false"));
                Assert.IsNotNull(
                    _myReceiver.EventsReceived.Find(
                        anEvent => // Expect finding such an event
                            anEvent[EventBase.EventNameKey].EndsWith("api_event") &&
                            anEvent[ApiEvent.UiBehaviorKey] == "select_account"));
                Assert.IsNotNull(
                    _myReceiver.EventsReceived.Find(
                        anEvent => // Expect finding such an event
                            anEvent[EventBase.EventNameKey].EndsWith("ui_event") &&
                            anEvent[UiEvent.AccessDeniedKey] == "false"));
            }
        }

        [TestMethod]
        [TestCategory("InteractiveRequestTests")]
        public void RedirectUriContainsFragmentErrorTest()
        {
            var authority = Authority.CreateAuthority(MsalTestConstants.AuthorityHomeTenant, false);
            try
            {
                using (var httpManager = new MockHttpManager())
                {
                    var parameters = new AuthenticationRequestParameters
                    {
                        Authority = authority,
                        ClientId = MsalTestConstants.ClientId,
                        Scope = MsalTestConstants.Scope,
                        TokenCache = null,
                        RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null)),
                        RedirectUri = new Uri("some://uri#fragment=not-so-good"),
                        ExtraQueryParameters = "extra=qp"
                    };

                    new InteractiveRequest(
                        httpManager,
                        PlatformProxyFactory.GetPlatformProxy().CryptographyManager,
                        parameters,
                        ApiEvent.ApiIds.None,
                        MsalTestConstants.ScopeForAnotherResource.ToArray(),
                        (string)null,
                        UIBehavior.ForceLogin,
                        new MockWebUI());
                    Assert.Fail("ArgumentException should be thrown here");
                }
            }
            catch (ArgumentException ae)
            {
                Assert.IsTrue(ae.Message.Contains(MsalErrorMessage.RedirectUriContainsFragment));
            }
        }

        [TestMethod]
        [TestCategory("InteractiveRequestTests")]
        public void OAuthClient_FailsWithServiceExceptionWhenItCannotParseJsonResponse()
        {
            var authority = Authority.CreateAuthority(MsalTestConstants.AuthorityHomeTenant, false);

            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddMockHandler(
                    new MockHttpMessageHandler
                    {
                        Method = HttpMethod.Get,
                        ResponseMessage = MockHelpers.CreateTooManyRequestsNonJsonResponse() // returns a non json response
                    });

                var parameters = new AuthenticationRequestParameters
                {
                    Authority = authority,
                    ClientId = MsalTestConstants.ClientId,
                    Scope = MsalTestConstants.Scope,
                    TokenCache = null,
                    RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null)),
                    RedirectUri = new Uri("some://uri"),
                };

                var ui = new MockWebUI();

                var request = new InteractiveRequest(
                    httpManager,
                    PlatformProxyFactory.GetPlatformProxy().CryptographyManager,
                    parameters,
                    ApiEvent.ApiIds.None,
                    MsalTestConstants.ScopeForAnotherResource.ToArray(),
                    MsalTestConstants.DisplayableId,
                    UIBehavior.SelectAccount,
                    ui);

                try
                {
                    request.ExecuteAsync(CancellationToken.None).Wait();

                    Assert.Fail("MsalException should have been thrown here");
                }
                catch (Exception exc)
                {
                    var serverEx = exc.InnerException as MsalServiceException;
                    Assert.IsNotNull(serverEx);
                    Assert.AreEqual(429, serverEx.StatusCode);
                    Assert.AreEqual(MockHelpers.TooManyRequestsContent, serverEx.ResponseBody);
                    Assert.AreEqual(MockHelpers.TestRetryAfterDuration, serverEx.Headers.RetryAfter.Delta);
                    Assert.AreEqual(CoreErrorCodes.NonParsableOAuthError, serverEx.ErrorCode);
                }
            }
        }

        [TestMethod]
        [TestCategory("InteractiveRequestTests")]
        public void OAuthClient_FailsWithServiceExceptionWhenItCanParseJsonResponse()
        {
            var authority = Authority.CreateAuthority(MsalTestConstants.AuthorityHomeTenant, false);

            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddMockHandler(
                    new MockHttpMessageHandler
                    {
                        Method = HttpMethod.Get,
                        ResponseMessage = MockHelpers.CreateTooManyRequestsJsonResponse() // returns a non json response
                    });

                var parameters = new AuthenticationRequestParameters
                {
                    Authority = authority,
                    ClientId = MsalTestConstants.ClientId,
                    Scope = MsalTestConstants.Scope,
                    TokenCache = null,
                    RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null)),
                    RedirectUri = new Uri("some://uri"),
                };

                var ui = new MockWebUI();

                var request = new InteractiveRequest(
                    httpManager,
                    PlatformProxyFactory.GetPlatformProxy().CryptographyManager,
                    parameters,
                    ApiEvent.ApiIds.None,
                    MsalTestConstants.ScopeForAnotherResource.ToArray(),
                    MsalTestConstants.DisplayableId,
                    UIBehavior.SelectAccount,
                    ui);

                try
                {
                    request.ExecuteAsync(CancellationToken.None).Wait();

                    Assert.Fail("MsalException should have been thrown here");
                }
                catch (Exception exc)
                {
                    var serverEx = exc.InnerException as MsalServiceException;
                    Assert.IsNotNull(serverEx);
                    Assert.AreEqual(429, serverEx.StatusCode);
                    Assert.AreEqual(MockHelpers.TestRetryAfterDuration, serverEx.Headers.RetryAfter.Delta);
                    Assert.AreEqual("Server overload", serverEx.ErrorCode);
                }
            }
        }

        [TestMethod]
        [TestCategory("InteractiveRequestTests")]
        public void VerifyAuthorizationResultTest()
        {
            var authority = Authority.CreateAuthority(MsalTestConstants.AuthorityHomeTenant, false);

            using (var httpManager = new MockHttpManager())
            {
                RequestTestsCommon.MockInstanceDiscoveryAndOpenIdRequest(httpManager);

                var webUi = new MockWebUI()
                {
                    MockResult = new AuthorizationResult(
                        AuthorizationStatus.ErrorHttp,
                        MsalTestConstants.AuthorityHomeTenant + "?error=" + OAuth2Error.LoginRequired)
                };

                var parameters = new AuthenticationRequestParameters
                {
                    Authority = authority,
                    ClientId = MsalTestConstants.ClientId,
                    Scope = MsalTestConstants.Scope,
                    TokenCache = null,
                    RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null)),
                    RedirectUri = new Uri("some://uri"),
                    ExtraQueryParameters = "extra=qp"
                };

                var request = new InteractiveRequest(
                    httpManager,
                    PlatformProxyFactory.GetPlatformProxy().CryptographyManager,
                    parameters,
                    ApiEvent.ApiIds.None,
                    MsalTestConstants.ScopeForAnotherResource.ToArray(),
                    (string)null,
                    UIBehavior.ForceLogin,
                    webUi);
                try
                {
                    request.ExecuteAsync(CancellationToken.None).Wait();
                    Assert.Fail("MsalException should have been thrown here");
                }
                catch (Exception exc)
                {
                    Assert.IsTrue(exc.InnerException is MsalUiRequiredException);
                    Assert.AreEqual(
                        MsalUiRequiredException.NoPromptFailedError,
                        ((MsalUiRequiredException)exc.InnerException).ErrorCode);
                }

                webUi = new MockWebUI
                {
                    MockResult = new AuthorizationResult(
                        AuthorizationStatus.ErrorHttp,
                        MsalTestConstants.AuthorityHomeTenant + "?error=invalid_request&error_description=some error description")
                };

                request = new InteractiveRequest(
                    httpManager,
                    PlatformProxyFactory.GetPlatformProxy().CryptographyManager,
                    parameters,
                    ApiEvent.ApiIds.None,
                    MsalTestConstants.ScopeForAnotherResource.ToArray(),
                    (string)null,
                    UIBehavior.ForceLogin,
                    webUi);

                try
                {
                    request.ExecuteAsync(CancellationToken.None).Wait(CancellationToken.None);
                    Assert.Fail("MsalException should have been thrown here");
                }
                catch (Exception exc)
                {
                    Assert.IsTrue(exc.InnerException is MsalException);
                    Assert.AreEqual("invalid_request", ((MsalException)exc.InnerException).ErrorCode);
                    Assert.AreEqual("some error description", ((MsalException)exc.InnerException).Message);
                }
            }
        }

        [TestMethod]
        [TestCategory("InteractiveRequestTests")]
        public void DuplicateQueryParameterErrorTest()
        {
            var authority = Authority.CreateAuthority(MsalTestConstants.AuthorityHomeTenant, false);

            var parameters = new AuthenticationRequestParameters
            {
                Authority = authority,
                ClientId = MsalTestConstants.ClientId,
                Scope = MsalTestConstants.Scope,
                TokenCache = null,
                RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null)),
                RedirectUri = new Uri("some://uri"),
                ExtraQueryParameters = "extra=qp&prompt=login"
            };

            using (var httpManager = new MockHttpManager())
            {
                RequestTestsCommon.MockInstanceDiscoveryAndOpenIdRequest(httpManager);

                var request = new InteractiveRequest(
                    httpManager,
                    PlatformProxyFactory.GetPlatformProxy().CryptographyManager,
                    parameters,
                    ApiEvent.ApiIds.None,
                    MsalTestConstants.ScopeForAnotherResource.ToArray(),
                    null,
                    UIBehavior.ForceLogin,
                    new MockWebUI());

                try
                {
                    request.ExecuteAsync(CancellationToken.None).Wait();
                    Assert.Fail("MsalException should be thrown here");
                }
                catch (Exception exc)
                {
                    Assert.IsTrue(exc.InnerException is MsalException);
                    Assert.AreEqual(
                        MsalClientException.DuplicateQueryParameterError,
                        ((MsalException)exc.InnerException).ErrorCode);
                }
            }
        }
    }
}