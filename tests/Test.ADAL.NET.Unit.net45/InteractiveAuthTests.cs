//----------------------------------------------------------------------
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
//------------------------------------------------------------------------------

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using PromptBehavior = Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior;
using Microsoft.Identity.Core.UI;
using System.Collections.Generic;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using System.Linq;
using Microsoft.Identity.Core;


#if !NET_CORE
namespace Test.ADAL.NET.Unit.net45
{
    [TestClass]
    public class InteractiveAuthTests
    {
        private PlatformParameters _platformParameters;
        private AuthenticationContext _context;

        [TestInitialize]
        public void Initialize()
        {
            ModuleInitializer.ForceModuleInitializationTestOnly();
            InstanceDiscovery.InstanceCache.Clear();
            _platformParameters = new PlatformParameters(PromptBehavior.Auto);
        }

        internal void SetupMocks(MockHttpManager httpManager)
        {
            httpManager.AddMockHandler(
                    MockHelpers.CreateInstanceDiscoveryMockHandler(
                        AdalTestConstants.GetDiscoveryEndpoint(
                            AdalTestConstants.DefaultAuthorityCommonTenant)));
        }

        [TestCleanup()]
        public void Cleanup()
        {
            _context?.TokenCache?.Clear();
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken")]
        [TestCategory("AdalDotNet")]
        public async Task SmokeTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                AdalTestConstants.DefaultRedirectUri + "?code=some-code"));
                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
                });

                _context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.True,
                    null);

                AuthenticationResult result =
                    await
                        _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                            AdalTestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.IsTrue(_context.Authenticator.Authority.EndsWith("/some-tenant-id/"));
                Assert.AreEqual(result.AccessToken, "some-access-token");
                Assert.IsNotNull(result.UserInfo);
                Assert.AreEqual(result.ExpiresOn, result.ExtendedExpiresOn);
                Assert.AreEqual(AdalTestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
                Assert.AreEqual(AdalTestConstants.DefaultUniqueId, result.UserInfo.UniqueId);
            }
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken with extended expires on support")]
        [TestCategory("AdalDotNet")]
        public async Task SmokeTestWithExtendedExpiresOnAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                AdalTestConstants.DefaultRedirectUri + "?code=some-code"));
                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(true)
                });

                _context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.True,
                    null);

                AuthenticationResult result =
                    await
                        _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                            AdalTestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.IsTrue(_context.Authenticator.Authority.EndsWith("/some-tenant-id/"));
                Assert.AreEqual(result.AccessToken, "some-access-token");
                Assert.IsNotNull(result.UserInfo);
                Assert.IsTrue(result.ExtendedExpiresOn.Subtract(result.ExpiresOn) > TimeSpan.FromSeconds(5));
                Assert.AreEqual(AdalTestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
                Assert.AreEqual(AdalTestConstants.DefaultUniqueId, result.UserInfo.UniqueId);
            }
        }

        [TestMethod]
        [Description("Positive Test for ExtendedLife Feature")]
        [TestCategory("AdalDotNet")]
        public async Task ExtendedLifetimeRetryAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                 AdalTestConstants.DefaultRedirectUri + "?code=some-code"));
                _context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.True,
                    new TokenCache());

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateResiliencyMessage(HttpStatusCode.GatewayTimeout),
                });

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                });

                _context.ExtendedLifeTimeEnabled = true;
                AuthenticationResult result =
                await _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId, AdalTestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.AccessToken);

                _context.TokenCache.Clear();
            }
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken with missing redirectUri and/or userId")]
        public async Task AcquireTokenPositiveWithoutUserId_PKCE_Async()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                _context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                var mockWebUI = MockHelpers.ConfigureMockWebUI(
                    new AuthorizationResult(AuthorizationStatus.Success, AdalTestConstants.DefaultRedirectUri + "?code=some-code"),
                    MockHelpers.GetDefaultAuthorizationRequestParams());

                var mockHandler = new MockHttpMessageHandler(
                    AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
                };
                httpManager.AddMockHandler(mockHandler);

                AuthenticationResult result =
                    await
                        _context.AcquireTokenAsync(
                            AdalTestConstants.DefaultResource,
                            AdalTestConstants.DefaultClientId,
                            AdalTestConstants.DefaultRedirectUri,
                            _platformParameters).ConfigureAwait(false);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.AccessToken, "some-access-token");
                ValidatePkce(mockWebUI, mockHandler);

                var exc = AssertException.TaskThrows<ArgumentException>(() =>
                    _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                        AdalTestConstants.DefaultRedirectUri, _platformParameters, null));
                Assert.IsTrue(exc.Message.StartsWith(AdalErrorMessage.SpecifyAnyUser));


                // this should hit the cache
                result =
                    await
                        _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                            AdalTestConstants.DefaultRedirectUri, _platformParameters, UserIdentifier.AnyUser).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.AccessToken, "some-access-token");

                // There should be one cached entry.
                Assert.AreEqual(1, _context.TokenCache.Count);

                _context.TokenCache.Clear();
            }
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken with missing redirectUri and/or userId")]
        public void InvalidStateReturned_ThrowsException()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                _context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                MockHelpers.ConfigureMockWebUI(
                    new AuthorizationResult(AuthorizationStatus.Success, AdalTestConstants.DefaultRedirectUri + "?code=some-code"),
                    MockHelpers.GetDefaultAuthorizationRequestParams(),
                    addStateToAuthroizationResult: false);

                var ex = AssertException.TaskThrows<AdalException>(() => _context.AcquireTokenAsync(
                            AdalTestConstants.DefaultResource,
                            AdalTestConstants.DefaultClientId,
                            AdalTestConstants.DefaultRedirectUri,
                            _platformParameters));

                Assert.AreEqual(AdalError.StateMismatchError, ex.ErrorCode);
            }
        }

        private static void ValidatePkce(MockWebUI mockWebUI, MockHttpMessageHandler exchangeAuthorizationCodeHandler)
        {
            string actualPkceCodeChallenge = mockWebUI.ActualQueryParams["code_challenge"];
            string actualPkceCode = exchangeAuthorizationCodeHandler.ActualQueryOrFormsParams["code_verifier"];

            Assert.IsNotNull(actualPkceCode);
            Assert.IsNotNull(actualPkceCodeChallenge);

            string expectedPkceCodeChanllenge = PlatformProxyFactory.GetPlatformProxy()
                .CryptographyManager.CreateBase64UrlEncodedSha256Hash(actualPkceCode);
            Assert.AreEqual(expectedPkceCodeChanllenge, actualPkceCodeChallenge);
        }


        [TestMethod]
        public void ArgNull_PlatformParamters()
        {
            AssertException.Throws<ArgumentNullException>(() =>
          {
              new PlatformParameters(PromptBehavior.SelectAccount, null);
          });


            AssertException.Throws<ArgumentNullException>(() =>
            {
                new PlatformParameters(PromptBehavior.SelectAccount, (object)null);
            });
        }

        [TestMethod]
        [Description("Test for authority validation to AuthenticationContext")]
        public async Task AuthenticationContextAuthorityValidationTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                _context = null;
                AuthenticationResult result = null;

                var ex = AssertException.Throws<ArgumentException>(() => new AuthenticationContext(
                    serviceBundle,
                    "https://login.contoso.com/adfs",
                    AuthorityValidationType.NotProvided,
                    new TokenCache()));
                Assert.AreEqual(ex.ParamName, "validateAuthority");

                MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                    AdalTestConstants.DefaultRedirectUri + "?code=some-code"));
                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
                });

                //whitelisted authority
                _context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.True,
                    new TokenCache());
                _context.TokenCache.Clear(); // need to reset cache before starting utest.
                result =
                    await
                        _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                            AdalTestConstants.DefaultRedirectUri, _platformParameters,
                            new UserIdentifier(AdalTestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId)).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.AccessToken, "some-access-token");
                Assert.IsNotNull(result.UserInfo);

                // There should be one cached entry.
                Assert.AreEqual(1, _context.TokenCache.Count, "Number of items in the cache is not as expected.");

                //add handler to return failed discovery response
                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetDiscoveryEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Get,
                    ResponseMessage =
                        MockHelpers.CreateFailureResponseMessage(
                            "{\"error\":\"invalid_instance\",\"error_description\":\"AADSTS70002: Error in validating authority.\"}")
                });

                _context = new AuthenticationContext(
                    serviceBundle,
                    "https://login.microsoft0nline.com/common",
                    AuthorityValidationType.NotProvided,
                    new TokenCache());
                var adalEx = AssertException.TaskThrows<AdalException>(() =>
                    _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                        AdalTestConstants.DefaultRedirectUri, _platformParameters));

                Assert.AreEqual(adalEx.ErrorCode, AdalError.AuthorityNotInValidList);
                _context.TokenCache.Clear();
            }
        }

        [TestMethod]
        [Description("Negative Test for AcquireToken with user canceling authentication")]
        public void AcquireTokenWithAuthenticationCanceledTest()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                _context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.UserCancel,
                    AdalTestConstants.DefaultRedirectUri + "?error=user_cancelled"));

                var exc = AssertException.TaskThrows<AdalServiceException>(() =>
                    _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                            AdalTestConstants.DefaultRedirectUri, _platformParameters));

                Assert.AreEqual(exc.ErrorCode, AdalError.AuthenticationCanceled);

                // There should be no cached entries.
                Assert.AreEqual(0, _context.TokenCache.Count);
            }
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken testing default token cache")]
        public async Task AcquireTokenPositiveWithNullCacheTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                AdalTestConstants.DefaultRedirectUri + "?code=some-code"));
                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
                });

                _context = new AuthenticationContext(
                    serviceBundle, AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    null);

                AuthenticationResult result =
                    await
                        _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                            AdalTestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.AccessToken, "some-access-token");
                Assert.IsNotNull(result.UserInfo);
            }
        }

        [TestMethod]
        [Description("Test for acquiring token using tenant specific endpoint")]
        public async Task TenantSpecificAuthorityTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                AdalTestConstants.DefaultRedirectUri + "?code=some-code"));
                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityHomeTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
                });

                _context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityHomeTenant,
                    AuthorityValidationType.True,
                    null);

                AuthenticationResult result =
                    await
                        _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                            AdalTestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.AreEqual(AdalTestConstants.DefaultAuthorityHomeTenant, _context.Authenticator.Authority);
                Assert.AreEqual(result.AccessToken, "some-access-token");
                Assert.IsNotNull(result.UserInfo);
                Assert.AreEqual(AdalTestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
                Assert.AreEqual(AdalTestConstants.DefaultUniqueId, result.UserInfo.UniqueId);
            }
        }

        [TestMethod]
        [Description("Test for ensuring ADAL returns the appropriate headers during a http failure.")]
        public async Task HttpErrorResponseWithHeadersAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                                           AdalTestConstants.DefaultRedirectUri + "?code=some-code"));

                List<KeyValuePair<string, string>> HttpErrorResponseWithHeaders = new List<KeyValuePair<string, string>>();
                HttpErrorResponseWithHeaders.Add(new KeyValuePair<string, string>("Retry-After", "120"));
                HttpErrorResponseWithHeaders.Add(new KeyValuePair<string, string>("GatewayTimeout", "0"));
                HttpErrorResponseWithHeaders.Add(new KeyValuePair<string, string>("Forbidden", "0"));

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.DefaultAuthorityCommonTenant)
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateCustomHeaderFailureResponseMessage(HttpErrorResponseWithHeaders)
                });

                _context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.True,
                    new TokenCache());

                try
                {
                    AuthenticationResult result = await _context.AcquireTokenAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                                                  AdalTestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
                    Assert.Fail();
                }
                catch (Exception ex)
                {
                    if (ex is AdalServiceException adalEx)
                    {
                        foreach (KeyValuePair<string, string> header in HttpErrorResponseWithHeaders)
                        {
                            var match = adalEx.Headers.Where(x => x.Key == header.Key && x.Value.Contains(header.Value)).FirstOrDefault();
                            Assert.IsNotNull(match);
                        }
                    }
                }
                _context.TokenCache.Clear();
            }
        }
    }
}

#endif
