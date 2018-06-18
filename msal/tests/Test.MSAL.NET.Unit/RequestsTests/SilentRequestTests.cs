﻿//----------------------------------------------------------------------
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

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.Core.Unit.Mocks;
using Test.MSAL.NET.Unit.Mocks;

namespace Test.MSAL.NET.Unit.RequestsTests
{
    [TestClass]
    public class SilentRequestTests
    {
        TokenCache cache;

        [TestInitialize]
        public void TestInitialize()
        {
            cache = new TokenCache();
            Authority.ValidatedAuthorities.Clear();
            HttpClientFactory.ReturnHttpClientForMocks = true;
            HttpMessageHandlerFactory.ClearMockHandlers();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (cache != null)
            {
                cache.TokenCacheAccessor.AccessTokenCacheDictionary.Clear();
                cache.TokenCacheAccessor.RefreshTokenCacheDictionary.Clear();
            }
        }

        [TestMethod]
        [TestCategory("SilentRequestTests")]
        public void ConstructorTests()
        {
            Authority authority = Authority.CreateAuthority(TestConstants.AuthorityHomeTenant, false);
            TokenCache cache = new TokenCache()
            {
                ClientId = TestConstants.ClientId
            };
            AuthenticationRequestParameters parameters = new AuthenticationRequestParameters()
            {
                Authority = authority,
                ClientId = TestConstants.ClientId,
                Scope = TestConstants.Scope,
                TokenCache = cache,
                RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null))
            };

            parameters.User = null;
            try
            {
                new SilentRequest(parameters, false);
                Assert.Fail("MsalUiRequiredException should have been thrown here");
            }
            catch (MsalUiRequiredException exc)
            {
                Assert.AreEqual(exc.ErrorCode, MsalUiRequiredException.UserNullError);
            }

            parameters.User = new User()
            {
                DisplayableId = TestConstants.DisplayableId
            };
            SilentRequest request = new SilentRequest(parameters, false);
            Assert.IsNotNull(request);

            parameters.User = new User()
            {
            };
            request = new SilentRequest(parameters, false);
            Assert.IsNotNull(request);

            request = new SilentRequest(parameters, false);
            Assert.IsNotNull(request);
        }

        [TestMethod]
        [TestCategory("SilentRequestTests")]
        public void ExpiredTokenRefreshFlowTest()
        {
            Authority authority = Authority.CreateAuthority(TestConstants.AuthorityHomeTenant, false);
            TokenCache cache = new TokenCache()
            {
                ClientId = TestConstants.ClientId
            };
            TokenCacheHelper.PopulateCache(cache.TokenCacheAccessor);

            AuthenticationRequestParameters parameters = new AuthenticationRequestParameters()
            {
                Authority = authority,
                ClientId = TestConstants.ClientId,
                Scope = new[] { "some-scope1", "some-scope2" }.CreateSetFromEnumerable(),
                TokenCache = cache,
                RequestContext = new RequestContext(new MsalLogger(Guid.Empty, null)),
                User = new User()
                {
                    Identifier = TestConstants.UserIdentifier,
                    DisplayableId = TestConstants.DisplayableId
                }
            };
            
            //add mock response for tenant endpoint discovery
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Get,
                ResponseMessage = MockHelpers.CreateOpenIdConfigurationResponse(TestConstants.AuthorityHomeTenant)
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            SilentRequest request = new SilentRequest(parameters, false);
            Task<AuthenticationResult> task = request.RunAsync();
            AuthenticationResult result = task.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual("some-scope1 some-scope2", result.Scopes.AsSingleString());

            Assert.IsTrue(HttpMessageHandlerFactory.IsMocksQueueEmpty, "All mocks should have been consumed");
        }


        [TestMethod]
        [TestCategory("SilentRequestTests")]
        public void SilentRefreshFailedNullCacheTest()
        {
            Authority authority = Authority.CreateAuthority(TestConstants.AuthorityHomeTenant, false);
            cache = null;

            //add mock response for tenant endpoint discovery
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Get,
                ResponseMessage = MockHelpers.CreateOpenIdConfigurationResponse(TestConstants.AuthorityHomeTenant)
            });

            AuthenticationRequestParameters parameters = new AuthenticationRequestParameters()
            {
                Authority = authority,
                ClientId = TestConstants.ClientId,
                Scope = new[] { "some-scope1", "some-scope2" }.CreateSetFromEnumerable(),
                TokenCache = cache,
                User = new User(),
                RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null))
            };

            try
            {
                SilentRequest request = new SilentRequest(parameters, false);
                Task<AuthenticationResult> task = request.RunAsync();
                var authenticationResult = task.Result;
                Assert.Fail("MsalUiRequiredException should be thrown here");
            }
            catch (AggregateException ae)
            {
                MsalUiRequiredException exc = ae.InnerException as MsalUiRequiredException;
                Assert.IsNotNull(exc);
                Assert.AreEqual(MsalUiRequiredException.TokenCacheNullError, exc.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("SilentRequestTests")]
        public void SilentRefreshFailedNoCacheItemFoundTest()
        {
            Authority authority = Authority.CreateAuthority(TestConstants.AuthorityHomeTenant, false);
            cache = new TokenCache()
            {
                ClientId = TestConstants.ClientId
            };

            //add mock response for tenant endpoint discovery
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Get,
                ResponseMessage = MockHelpers.CreateOpenIdConfigurationResponse(TestConstants.AuthorityHomeTenant)
            });

            AuthenticationRequestParameters parameters = new AuthenticationRequestParameters()
            {
                Authority = authority,
                ClientId = TestConstants.ClientId,
                Scope = new[] { "some-scope1", "some-scope2" }.CreateSetFromEnumerable(),
                TokenCache = cache,
                User = new User(),
                RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null))
            };
            
            try
            {
                SilentRequest request = new SilentRequest(parameters, false);
                Task<AuthenticationResult> task = request.RunAsync();
                var authenticationResult = task.Result;
                Assert.Fail("MsalUiRequiredException should be thrown here");
            }
            catch (AggregateException ae)
            {
                MsalUiRequiredException exc = ae.InnerException as MsalUiRequiredException;
                Assert.IsNotNull(exc);
                Assert.AreEqual(MsalUiRequiredException.NoTokensFoundError, exc.ErrorCode);
            }
        }
    }
}
