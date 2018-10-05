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

using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.Instance;
using Microsoft.Identity.Core.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using Test.Microsoft.Identity.Core.Unit.Mocks;
using Test.MSAL.NET.Unit.Mocks;

namespace Test.MSAL.NET.Unit
{
    [TestClass]
    class AuthorityAliasesTests
    {
        private MyReceiver _myReceiver = new MyReceiver();

        [TestInitialize]
        public void TestInitialize()
        {
            ModuleInitializer.ForceModuleInitializationTestOnly();
            Authority.ValidatedAuthorities.Clear();
            HttpClientFactory.ReturnHttpClientForMocks = true;
            HttpMessageHandlerFactory.ClearMockHandlers();
            Telemetry.GetInstance().RegisterReceiver(_myReceiver.OnEvents);

            AadInstanceDiscovery.Instance.Cache.Clear();
            AddMockResponseForInstanceDisovery();
        }

        internal void AddMockResponseForInstanceDisovery()
        {
            HttpMessageHandlerFactory.AddMockHandler(
                MockHelpers.CreateInstanceDiscoveryMockHandler(
                    TestConstants.GetDiscoveryEndpoint(TestConstants.AuthorityCommonTenant)));
        }

        class TestLegacyCachePersistance : ILegacyCachePersistance
        {
            private byte[] data;
            public byte[] LoadCache()
            {
                return data;
            }

            public void WriteCache(byte[] serializedCache)
            {
                data = serializedCache;
            }
        }

        [TestMethod]
        [Description("Test authority migration")]
        public void AuthorityMigration_IntegrationTest()
        {
            // make sure that for all network calls "preferred_cache" enironment is used
            // (it is taken from metadata in instance discovery response), 
            // except very first network call - instance discovery

            // init public client app
            PublicClientApplication app = new PublicClientApplication(TestConstants.ClientId,
                string.Format(CultureInfo.InvariantCulture, "https://{0}/common", TestConstants.ProdNotPrefEnvAlias));
            app.UserTokenCache.legacyCachePersistance = new TestLegacyCachePersistance();

            // mock for openId config request
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Url = string.Format(CultureInfo.InvariantCulture, "https://{0}/common/v2.0/.well-known/openid-configuration",
                    TestConstants.ProdPrefNetworkEnv),
                Method = HttpMethod.Get,
                ResponseMessage = MockHelpers.CreateOpenIdConfigurationResponse(TestConstants.AuthorityHomeTenant)
            });

            // mock webUi authorization
            MsalMockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                app.RedirectUri + "?code=some-code"), null, TestConstants.ProdPrefNetworkEnv);

            // mock token request
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Url = string.Format(CultureInfo.InvariantCulture, "https://{0}/home/oauth2/v2.0/token",
                    TestConstants.ProdPrefNetworkEnv),
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            AuthenticationResult result = app.AcquireTokenAsync(TestConstants.Scope).Result;

            // make sure that all cache entities are stored with "preferred_cache" environment
            // (it is taken from metadata in instance discovery response)
            ValidateCacheEntitiesEnvironment(app.UserTokenCache, TestConstants.ProdPrefCacheEnv);

            // silent request targeting at, should return at from cache for any environment alias
            foreach (var envAlias in TestConstants.ProdEnvAliases)
            {
                result = app.AcquireTokenSilentAsync(TestConstants.Scope,
                    app.GetAccountsAsync().Result.First(),
                    string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}/", envAlias, TestConstants.Utid),
                    false).Result;

                Assert.IsNotNull(result);
            }

            // mock for openId config request for tenant spesific authority
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Url = string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}/v2.0/.well-known/openid-configuration",
                    TestConstants.ProdPrefNetworkEnv, TestConstants.Utid),
                Method = HttpMethod.Get,
                ResponseMessage = MockHelpers.CreateOpenIdConfigurationResponse(TestConstants.AuthorityUtidTenant)
            });

            // silent request targeting rt should find rt in cahce for authority with any environment alias
            foreach (var envAlias in TestConstants.ProdEnvAliases)
            {
                HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
                {
                    Url = string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}/oauth2/v2.0/token",
                        TestConstants.ProdPrefNetworkEnv, TestConstants.Utid),
                    Method = HttpMethod.Post,
                    PostData = new Dictionary<string, string>()
                    {
                        {"grant_type", "refresh_token"}
                    },
                    // return not retriable status code
                    ResponseMessage = MockHelpers.CreateInvalidGrantTokenResponseMessage()
                });

                try
                {
                    result = app.AcquireTokenSilentAsync(TestConstants.ScopeForAnotherResource,
                        app.GetAccountsAsync().Result.First(),
                        string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}/", envAlias, TestConstants.Utid),
                        false).Result;
                }
                catch (AggregateException ex)
                {
                    Assert.IsNotNull(ex.InnerException);
                    Assert.IsTrue(ex.InnerException is MsalUiRequiredException);
                }

                Assert.IsNotNull(result);
            }
        }

        private void ValidateCacheEntitiesEnvironment(TokenCache cache, string expectedEnvironment)
        {
            var accessTokens = cache.GetAllAccessTokensForClient(new RequestContext(new MsalLogger(Guid.NewGuid(), null)));
            foreach (var at in accessTokens)
            {
                Assert.AreEqual(expectedEnvironment, at.Environment);
            }

            var refreshTokens = cache.GetAllRefreshTokensForClient(new RequestContext(new MsalLogger(Guid.NewGuid(), null)));
            foreach (var rt in refreshTokens)
            {
                Assert.AreEqual(expectedEnvironment, rt.Environment);
            }

            var idTokens = cache.GetAllIdTokensForClient(new RequestContext(new MsalLogger(Guid.NewGuid(), null)));
            foreach (var id in idTokens)
            {
                Assert.AreEqual(expectedEnvironment, id.Environment);
            }

            var accounts = cache.GetAllAccounts(new RequestContext(new MsalLogger(Guid.NewGuid(), null)));
            foreach (var account in accounts)
            {
                Assert.AreEqual(expectedEnvironment, account.Environment);
            }

            IDictionary<AdalTokenCacheKey, AdalResultWrapper> adalCache =
                AdalCacheOperations.Deserialize(cache.legacyCachePersistance.LoadCache());

            foreach (KeyValuePair<AdalTokenCacheKey, AdalResultWrapper> kvp in adalCache)
            {
                Assert.AreEqual(expectedEnvironment, new Uri(kvp.Key.Authority).Host);
            }
        }
    }
}
