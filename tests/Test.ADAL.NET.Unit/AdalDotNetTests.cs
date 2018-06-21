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

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Cache;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.ClientCreds;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Flows;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Instance;
using Test.ADAL.Common;
using Test.ADAL.Common.Unit;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    [DeploymentItem("valid_cert.pfx")]
    public class AdalDotNetTests
    {
        private PlatformParameters _platformParameters;

        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.InitializeMockProvider();
            InstanceDiscovery.InstanceCache.Clear();
            HttpMessageHandlerFactory.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler(TestConstants.GetDiscoveryEndpoint(TestConstants.DefaultAuthorityCommonTenant)));
            _platformParameters = new PlatformParameters(PromptBehavior.Auto);
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken")]
        [TestCategory("AdalDotNet")]
        public async Task SmokeTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, true);
            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.IsTrue(context.Authenticator.Authority.EndsWith("/some-tenant-id/"));
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(result.ExpiresOn, result.ExtendedExpiresOn);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken with extended expires on support")]
        [TestCategory("AdalDotNet")]
        public async Task SmokeTestWithExtendedExpiresOnAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(true)
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, true);
            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.IsTrue(context.Authenticator.Authority.EndsWith("/some-tenant-id/"));
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);
            Assert.IsTrue(result.ExtendedExpiresOn.Subtract(result.ExpiresOn) > TimeSpan.FromSeconds(5));
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);
        }

        [TestMethod]
        [Description("Positive Test for ExtendedLife Feature")]
        [TestCategory("AdalDotNet")]
        public async Task ExtendedLifetimeRetryAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                 TestConstants.DefaultRedirectUri + "?code=some-code"));
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, true);
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateResiliencyMessage(HttpStatusCode.GatewayTimeout),
            });
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
            });
            context.ExtendedLifeTimeEnabled = true;
            AuthenticationResult result =
            await context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual(HttpMessageHandlerFactory.MockHandlersCount(), 0);
        }

        [TestMethod]
        [Description("Positive Test for ExtendedLife Feature returning back a stale AT")]
        [TestCategory("AdalDotNet")]
        public async Task ExtendedLifetimePositiveTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, (DateTimeOffset.UtcNow + TimeSpan.FromMinutes(180)))
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateResiliencyMessage(HttpStatusCode.GatewayTimeout),
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
            });
            context.ExtendedLifeTimeEnabled = true;
            AuthenticationResult result =
                    await context.AcquireTokenSilentAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, new UserIdentifier("unique_id", UserIdentifierType.UniqueId)).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
            Assert.AreEqual("some-access-token", result.AccessToken);
        }

        [TestMethod]
        [Description("Expiry time test for ExtendedLife Feature not returning back a stale AT")]
        [TestCategory("AdalDotNet")]
        public async Task ExtendedLifetimeExpiredTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateResiliencyMessage(HttpStatusCode.GatewayTimeout),
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateResiliencyMessage(HttpStatusCode.GatewayTimeout),
            });

            context.ExtendedLifeTimeEnabled = true;
            AuthenticationResult result =
                 await context.AcquireTokenSilentAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, new UserIdentifier("unique_id", UserIdentifierType.UniqueId)).ConfigureAwait(false);
            Assert.IsNull(result.AccessToken);

            // There should be one cached entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }


        [TestMethod]
        [Description("Test for returning back a stale AT")]
        [TestCategory("AdalDotNet")]
        public async Task ExtendedLifetimeTokenTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, (DateTimeOffset.UtcNow + TimeSpan.FromMinutes(180)))
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateResiliencyMessage(HttpStatusCode.GatewayTimeout),
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateResiliencyMessage(HttpStatusCode.InternalServerError),
            });

            //Assert.AreEqual(HttpMessageHandlerFactory.MockHandlersCount(), 2);
            context.ExtendedLifeTimeEnabled = true;
            AuthenticationResult result =
                await
                    context.AcquireTokenSilentAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        new UserIdentifier("unique_id", UserIdentifierType.UniqueId)).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.ExpiresOn <=
                           DateTime.UtcNow);
            Assert.AreEqual("some-access-token", result.AccessToken);

            // There should be one cached entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }

        [TestMethod]
        [Description("Test for returning back a stale AT in case of Network failure")]
        [TestCategory("AdalDotNet")]
        public async Task ExtendedLifetimeRequestTimeoutTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, (DateTimeOffset.UtcNow + TimeSpan.FromMinutes(180)))
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ExceptionToThrow = new TaskCanceledException("request timed out")
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ExceptionToThrow = new TaskCanceledException("request timed out")
            });
            context.ExtendedLifeTimeEnabled = true;
            AuthenticationResult result =
                await
                    context.AcquireTokenSilentAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        new UserIdentifier("unique_id", UserIdentifierType.UniqueId)).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.ExpiresOn <=
                           DateTime.UtcNow);
            Assert.AreEqual("some-access-token", result.AccessToken);

            // There should be one cached entry.
            Assert.AreEqual(1, context.TokenCache.Count);
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }

        [TestMethod]
        [Description("Test for ExtendedLifetime feature flag being set in normal(non-outage) for Client Credentials")]
        public async Task ClientCredentialExtendedExpiryFlagSetAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            var credential = new ClientCredential(TestConstants.DefaultClientId, TestConstants.DefaultClientSecret);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });

            AuthenticationResult result = await context.AcquireTokenAsync(TestConstants.DefaultResource, credential).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            // cache look up
            var result2 = await context.AcquireTokenAsync(TestConstants.DefaultResource, credential).ConfigureAwait(false);
            Assert.AreEqual(result.AccessToken, result2.AccessToken);

            // Null resource -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, credential));
            Assert.AreEqual("resource", exc.ParamName);

            // Null client credential -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientCredential)null));
            Assert.AreEqual("clientCredential", exc.ParamName);
        }

        [TestMethod]
        [Description("Test for ExtendedLifetime feature flag being not set in normal(non-outage) for Client Credentials")]
        public async Task ClientCredentialExtendedExpiryFlagNotSetAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            var credential = new ClientCredential(TestConstants.DefaultClientId, TestConstants.DefaultClientSecret);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });

            AuthenticationResult result = await context.AcquireTokenAsync(TestConstants.DefaultResource, credential).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            // cache look up
            var result2 = await context.AcquireTokenAsync(TestConstants.DefaultResource, credential).ConfigureAwait(false);
            Assert.AreEqual(result.AccessToken, result2.AccessToken);

            // Null resource -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, credential));
            Assert.AreEqual("resource", exc.ParamName);

            // Null client credential -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientCredential)null));
            Assert.AreEqual("clientCredential", exc.ParamName);
        }

        [TestMethod]
        [Description("Test for getting back access token when the extendedExpiresOn flag is set")]
        public async Task ClientCredentialExtendedExpiryPositiveTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.Client,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, (DateTimeOffset.UtcNow + TimeSpan.FromMinutes(180)))
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.DefaultAuthorityCommonTenant)
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.DefaultAuthorityCommonTenant)
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });

            var credential = new ClientCredential(TestConstants.DefaultClientId, TestConstants.DefaultClientSecret);

            context.ExtendedLifeTimeEnabled = true;
            // cache look up
            var result = await context.AcquireTokenAsync(TestConstants.DefaultResource, credential).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            // There should be one cached entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            // Null resource -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, credential));
            Assert.AreEqual("resource", exc.ParamName);

            // Null client credential -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientCredential)null));
            Assert.AreEqual("clientCredential", exc.ParamName);
        }

        [TestMethod]
        [Description("Test for ExtendedLifetime feature with the extendedExpiresOn being expired not returning back stale AT")]
        public void ClientCredentialExtendedExpiryNegativeTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityCommonTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });

            var credential = new ClientCredential(TestConstants.DefaultClientId, TestConstants.DefaultClientSecret);


            // cache look up
            var ex = AssertException.TaskThrows<AdalServiceException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, credential));
            Assert.AreEqual("Response status code does not indicate success: 504 (GatewayTimeout).", ex.InnerException.Message);

            // Null resource -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, credential));
            Assert.AreEqual("resource", exc.ParamName);

            // Null client credential -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientCredential)null));
            Assert.AreEqual("clientCredential", exc.ParamName);
        }

        [TestMethod]
        [Description("Test for ExtendedLifetime feature with the extendedExpiresOn being expired not returning back stale AT")]
        public void ClientCredentialNegativeRequestTimeoutTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityCommonTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });

            var credential = new ClientCredential(TestConstants.DefaultClientId, TestConstants.DefaultClientSecret);

            context.ExtendedLifeTimeEnabled = true;
            // cache look up
            var ex = AssertException.TaskThrows<AdalServiceException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, credential));
            Assert.AreEqual("Response status code does not indicate success: 408 (RequestTimeout).", ex.InnerException.Message);


            // Null resource -> error
            ArgumentNullException exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, credential));
            Assert.AreEqual("resource", exc.ParamName);

            // Null client credential -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientCredential)null));
            Assert.AreEqual("clientCredential", exc.ParamName);
        }

        [TestMethod]
        [Description("Test for being in outage mode and extendedExpires flag not set")]
        public void ClientCredentialExtendedExpiryNoFlagSetTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityCommonTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });

            var credential = new ClientCredential(TestConstants.DefaultClientId, TestConstants.DefaultClientSecret);

            context.ExtendedLifeTimeEnabled = false;
            // cache look up
            var ex = AssertException.TaskThrows<AdalServiceException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, credential));
            Assert.AreEqual("Response status code does not indicate success: 504 (GatewayTimeout).", ex.InnerException.Message);

            // Null resource -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, credential));
            Assert.AreEqual("resource", exc.ParamName);

            // Null client credential -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientCredential)null));
            Assert.AreEqual("clientCredential", exc.ParamName);
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken with missing redirectUri and/or userId")]
        public async Task AcquireTokenPositiveWithoutUserIdAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.AccessToken, "some-access-token");

            var exc = AssertException.TaskThrows<ArgumentException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                    TestConstants.DefaultRedirectUri, _platformParameters, null));
            Assert.IsTrue(exc.Message.StartsWith(AdalErrorMessage.SpecifyAnyUser, StringComparison.OrdinalIgnoreCase));


            // this should hit the cache
            result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, _platformParameters, UserIdentifier.AnyUser).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.AccessToken, "some-access-token");

            // There should be one cached entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }

        [TestMethod]
        [Description("Test for authority validation to AuthenticationContext")]
        public async Task AuthenticationContextAuthorityValidationTestAsync()
        {
            AuthenticationContext context = null;
            AuthenticationResult result = null;

            var ex = AssertException.Throws<ArgumentException>(() => new AuthenticationContext("https://login.contoso.com/adfs"));
            Assert.AreEqual(ex.ParamName, "validateAuthority");


            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            //whitelisted authority
            context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, true);
            result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, _platformParameters,
                        new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId)).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);

            // There should be one cached entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            //add handler to return failed discovery response
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetDiscoveryEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Get,
                ResponseMessage =
                    MockHelpers.CreateFailureResponseMessage(
                        "{\"error\":\"invalid_instance\",\"error_description\":\"AADSTS70002: Error in validating authority.\"}")
            });

            context = new AuthenticationContext("https://login.microsoft0nline.com/common");
            var adalEx = AssertException.TaskThrows<AdalException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                    TestConstants.DefaultRedirectUri, _platformParameters));

            Assert.AreEqual(adalEx.ErrorCode, AdalError.AuthorityNotInValidList);
        }

        [TestMethod]
        [Description("Negative Test for AcquireToken with invalid resource")]
        public void AcquireTokenWithInvalidResourceTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow)
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateInvalidRequestTokenResponseMessage()
            });

            var exc = AssertException.TaskThrows<AdalServiceException>(() =>
                context.AcquireTokenSilentAsync("random-resource", TestConstants.DefaultClientId));
            Assert.AreEqual(AdalError.FailedToRefreshToken, exc.ErrorCode);

            // There should be one cached entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }

        [TestMethod]
        [Description("Negative Test for AcquireToken with user canceling authentication")]
        public void AcquireTokenWithAuthenticationCanceledTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.UserCancel,
                TestConstants.DefaultRedirectUri + "?error=user_cancelled"));

            var exc = AssertException.TaskThrows<AdalServiceException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, _platformParameters));

            Assert.AreEqual(exc.ErrorCode, AdalError.AuthenticationCanceled);

            // There should be no cached entries.
            Assert.AreEqual(0, context.TokenCache.Count);
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken testing default token cache")]
        public async Task AcquireTokenPositiveWithNullCacheTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, null);
            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);
        }

        [TestMethod]
        [Description("Test for simple refresh token")]
        public void SimpleRefreshTokenTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAdfsAuthorityTenant, false, new TokenCache());
            //add simple RT to cache
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAdfsAuthorityTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User, null, null);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow)
            };

            //token request for some other resource should fail.
            var exc = AssertException.TaskThrows<AdalSilentTokenAcquisitionException>(() =>
                context.AcquireTokenSilentAsync("random-resource", TestConstants.DefaultClientId));
            Assert.AreEqual(AdalError.FailedToAcquireTokenSilently, exc.ErrorCode);
        }

        [TestMethod]
        [Description("Test for acquring token using tenant specific endpoint")]
        public async Task TenantSpecificAuthorityTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true);
            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(TestConstants.DefaultAuthorityHomeTenant, context.Authenticator.Authority);
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);
        }

        [TestMethod]
        [Description("Positive Test for Confidential Client")]
        public async Task ConfidentialClientWithX509TestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            var certificate = new ClientAssertionCertificate(TestConstants.DefaultClientId,
                new X509Certificate2("valid_cert.pfx", TestConstants.DefaultPassword));

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"}
                }
            });

            AuthenticationResult result =
                await
                    context.AcquireTokenByAuthorizationCodeAsync("some-code", TestConstants.DefaultRedirectUri,
                        certificate, TestConstants.DefaultResource).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);


            // Null auth code -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenByAuthorizationCodeAsync(null, TestConstants.DefaultRedirectUri, certificate,
                        TestConstants.DefaultResource));

            Assert.AreEqual(exc.ParamName, "authorizationCode");

            // Empty auth code -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenByAuthorizationCodeAsync(string.Empty, TestConstants.DefaultRedirectUri,
                certificate,
                TestConstants.DefaultResource));

            Assert.AreEqual(exc.ParamName, "authorizationCode");


            // Null for redirect -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenByAuthorizationCodeAsync("some-code", null, certificate,
                        TestConstants.DefaultResource));
            Assert.AreEqual(exc.ParamName, "redirectUri");

            // Null client certificate -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenByAuthorizationCodeAsync("some-code", TestConstants.DefaultRedirectUri,
                        (ClientAssertionCertificate)null, TestConstants.DefaultResource));
            Assert.AreEqual(exc.ParamName, "clientCertificate");

        }

        [TestMethod]
        [Description("Test for Client credential")]
        public async Task ClientCredentialNoCrossTenantTestAsync()
        {
            TokenCache cache = new TokenCache();
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, cache);
            var credential = new ClientCredential(TestConstants.DefaultClientId, TestConstants.DefaultClientSecret);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityGuestTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });

            AuthenticationResult result = await context.AcquireTokenAsync(TestConstants.DefaultResource, credential).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            context = new AuthenticationContext(TestConstants.DefaultAuthorityGuestTenant, cache);

            AssertException.TaskThrows<AdalException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, credential));
            Assert.AreEqual(1, cache.tokenCacheDictionary.Count);
        }

        [TestMethod]
        [Description("Test for Client credential")]
        public async Task ClientCredentialTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            var credential = new ClientCredential(TestConstants.DefaultClientId, TestConstants.DefaultClientSecret);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"client_secret", TestConstants.DefaultClientSecret},
                    {"grant_type", "client_credentials"}
                }
            });

            AuthenticationResult result = await context.AcquireTokenAsync(TestConstants.DefaultResource, credential).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            // cache look up
            var result2 = await context.AcquireTokenAsync(TestConstants.DefaultResource, credential).ConfigureAwait(false);
            Assert.AreEqual(result.AccessToken, result2.AccessToken);

            // Null resource -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, credential));
            Assert.AreEqual(exc.ParamName, "resource");

            // Null client credential -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientCredential)null));
            Assert.AreEqual(exc.ParamName, "clientCredential");
        }

        [TestMethod]
        [Description("Test for Client assertion with X509")]
        public async Task ClientAssertionWithX509TestAsync()
        {
            var certificate = new X509Certificate2("valid_cert.pfx", TestConstants.DefaultPassword);
            var clientAssertion = new ClientAssertionCertificate(TestConstants.DefaultClientId, certificate);

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            var expectedAudience = TestConstants.DefaultAuthorityCommonTenant + "oauth2/token";

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-access-token\"}")
                },
                PostData = new Dictionary<string, string>
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"grant_type", "client_credentials"},
                    {"client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"}
                },
                AdditionalRequestValidation = request =>
                {
                    var requestContent = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var formsData = EncodingHelper.ParseKeyValueList(requestContent, '&', true, null);

                    // Check presence of client_assertion in request
                    string encodedJwt;
                    Assert.IsTrue(formsData.TryGetValue("client_assertion", out encodedJwt), "Missing client_assertion from request");
                }
            });

            AuthenticationResult result = await context.AcquireTokenAsync(TestConstants.DefaultResource, clientAssertion).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            // Null resource -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, clientAssertion));
            Assert.AreEqual(exc.ParamName, "resource");

            // Null client credential -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientCredential)null));

            Assert.AreEqual(exc.ParamName, "clientCredential");
        }


        [TestMethod]
        [Description("Test for Confidential Client with self signed jwt")]
        public async Task ConfidentialClientWithJwtTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"}
                }
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://login.microsoftonline.com/some-tenant-id/oauth2/token")
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"}
                }
            });

            ClientAssertion assertion = new ClientAssertion(TestConstants.DefaultClientId, "some-assertion");
            AuthenticationResult result = await context.AcquireTokenByAuthorizationCodeAsync("some-code", TestConstants.DefaultRedirectUri, assertion, TestConstants.DefaultResource).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            result = await context.AcquireTokenByAuthorizationCodeAsync("some-code", TestConstants.DefaultRedirectUri, assertion, null).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            // Empty authorization code -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenByAuthorizationCodeAsync(string.Empty, TestConstants.DefaultRedirectUri, assertion, TestConstants.DefaultResource));
            Assert.AreEqual(exc.ParamName, "authorizationCode");


            // Null authorization code -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenByAuthorizationCodeAsync(null, TestConstants.DefaultRedirectUri, assertion, TestConstants.DefaultResource));
            Assert.AreEqual(exc.ParamName, "authorizationCode");


            // Null redirectUri -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenByAuthorizationCodeAsync("some-code", null, assertion, TestConstants.DefaultResource));
            Assert.AreEqual(exc.ParamName, "redirectUri");


            // Null client assertion -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenByAuthorizationCodeAsync("some-code", TestConstants.DefaultRedirectUri, (ClientAssertion)null, TestConstants.DefaultResource));
            Assert.AreEqual(exc.ParamName, "clientAssertion");
        }

        [TestMethod]
        [Description("Positive Test for AcquireTokenOnBehalf with client credential")]
        public async Task AcquireTokenOnBehalfAndClientCredentialTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            string accessToken = "some-access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", accessToken, DateTimeOffset.UtcNow)
            };

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId, TestConstants.DefaultClientSecret);

            // Null resource -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, clientCredential, new UserAssertion(accessToken)));
            Assert.AreEqual(exc.ParamName, "resource");


            // Null user assertion -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential, null));
            Assert.AreEqual(exc.ParamName, "userAssertion");


            // Null client credential -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientCredential)null, new UserAssertion(accessToken)));
            Assert.AreEqual(exc.ParamName, "clientCredential");


            // Valid input -> no error
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-other-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"}
                }
            });

            var result = await context.AcquireTokenAsync(TestConstants.AnotherResource, clientCredential, new UserAssertion(accessToken)).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            // All mocks are consumed
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }

        [TestMethod]
        [Description("Positive Test for AcquireTokenOnBehalf with client credential")]
        public async Task AcquireTokenOnBehalfAndClientCertificateCredentialTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            string accessToken = "some-access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", accessToken, DateTimeOffset.UtcNow)
            };

            ClientAssertionCertificate clientCredential = new ClientAssertionCertificate(TestConstants.DefaultClientId, new X509Certificate2("valid_cert.pfx", TestConstants.DefaultPassword));

            // Null resource -> error
            var exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(null, clientCredential, new UserAssertion(accessToken)));
            Assert.AreEqual(exc.ParamName, "resource");


            // Null user assertion -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential, null));
            Assert.AreEqual(exc.ParamName, "userAssertion");


            // Null client cert -> error
            exc = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.AcquireTokenAsync(TestConstants.DefaultResource, (ClientAssertionCertificate)null, new UserAssertion(accessToken)));
            Assert.AreEqual(exc.ParamName, "clientCertificate");


            // Valid input -> no error
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"some-other-token\"}")
                },
                PostData = new Dictionary<string, string>()
                {
                    {"client_id", TestConstants.DefaultClientId},
                    {"grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"}
                }
            });

            var result = await context.AcquireTokenAsync(TestConstants.AnotherResource, clientCredential, new UserAssertion(accessToken)).ConfigureAwait(false);
            Assert.IsNotNull(result.AccessToken);

            // All mocks are consumed
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }

        [TestMethod]
        [Description("Test for GetAuthorizationRequestURL")]
        public async Task GetAuthorizationRequestUrlTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant);
            Uri uri = null;

            var ex = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.GetAuthorizationRequestUrlAsync(null, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId), "extra=123"));
            Assert.AreEqual(ex.ParamName, "resource");

            uri = await context.GetAuthorizationRequestUrlAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId), "extra=123").ConfigureAwait(false);
            Assert.IsNotNull(uri);
            Assert.IsTrue(uri.AbsoluteUri.Contains("login_hint"));
            Assert.IsTrue(uri.AbsoluteUri.Contains("extra=123"));
            uri = await context.GetAuthorizationRequestUrlAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, UserIdentifier.AnyUser, null).ConfigureAwait(false);
            Assert.IsNotNull(uri);
            Assert.IsFalse(uri.AbsoluteUri.Contains("login_hint"));
            Assert.IsFalse(uri.AbsoluteUri.Contains("client-request-id="));
            context.CorrelationId = Guid.NewGuid();
            uri = await context.GetAuthorizationRequestUrlAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId), "extra").ConfigureAwait(false);
            Assert.IsNotNull(uri);
            Assert.IsTrue(uri.AbsoluteUri.Contains("client-request-id="));
        }

        [TestMethod]
        [Description("Test for GetAuthorizationRequestURL with claims")]
        public async Task GetAuthorizationRequestUrlWithClaimsTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant);
            Uri uri = null;

            var ex = AssertException.TaskThrows<ArgumentNullException>(() =>
                context.GetAuthorizationRequestUrlAsync(null, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId), "extra=123", "some"));
            Assert.AreEqual(ex.ParamName, "resource");

            uri = await context.GetAuthorizationRequestUrlAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId), "extra=123", "some").ConfigureAwait(false);
            Assert.IsNotNull(uri);
            Assert.IsTrue(uri.AbsoluteUri.Contains("login_hint"));
            Assert.IsTrue(uri.AbsoluteUri.Contains("extra=123"));
            uri = await context.GetAuthorizationRequestUrlAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, UserIdentifier.AnyUser, null, "some").ConfigureAwait(false);
            Assert.IsNotNull(uri);
            Assert.IsFalse(uri.AbsoluteUri.Contains("login_hint"));
            Assert.IsFalse(uri.AbsoluteUri.Contains("client-request-id="));
            context.CorrelationId = Guid.NewGuid();
            uri = await context.GetAuthorizationRequestUrlAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId), "extra", "some").ConfigureAwait(false);
            Assert.IsNotNull(uri);
            Assert.IsTrue(uri.AbsoluteUri.Contains("client-request-id="));
            uri = await context.GetAuthorizationRequestUrlAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, TestConstants.DefaultRedirectUri, new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId), "extra=123", "some").ConfigureAwait(false);
            Assert.IsNotNull(uri);
            Assert.IsTrue(uri.AbsoluteUri.Contains("claims"));
        }


        [TestMethod]
        [Description("Positive Test for AcquireTokenOnBehalf with client credential")]
        public void UserAssertionValidationTest()
        {
            TokenCache cache = new TokenCache();
            AuthenticationResultEx resultEx = TokenCacheTests.CreateCacheValue("id", "user1");
            resultEx.UserAssertionHash = "hash1";
            cache.tokenCacheDictionary.Add(
            new TokenCacheKey("https://login.microsoftonline.com/common/", "resource1", "client1",
                TokenSubjectType.Client, "id", "user1"), resultEx);
            RequestData data = new RequestData
            {
                Authenticator = new Authenticator("https://login.microsoftonline.com/common/", false),
                TokenCache = cache,
                Resource = "resource1",
                ClientKey = new ClientKey(new ClientCredential("client1", "something")),
                SubjectType = TokenSubjectType.Client,
                ExtendedLifeTimeEnabled = false
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://login.microsoftonline.com/common/oauth2/token")
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateFailureResponseMessage("HttpRequestException:  Response status code does not indicate success: 400 (BadRequest).")
            });

            var ex = AssertException.TaskThrows<AdalException>(() =>
                    new AcquireTokenOnBehalfHandler(data, new UserAssertion("non-existant")).RunAsync());

            Assert.AreEqual("HttpRequestException:  Response status code does not indicate success: 400 (BadRequest).", ex.Message);
        }

        [TestMethod]
        [Description("Test for returning entire HttpResponse as inner exception")]
        public void HttpErrorResponseAsInnerException()
        {
            TokenCache cache = new TokenCache();
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityCommonTenant, TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User, "unique_id", "displayable@id.com");
            cache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "something-invalid",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow)
            };

            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, cache);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityCommonTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateHttpErrorResponse()
            });

            var ex = AssertException.TaskThrows<AdalSilentTokenAcquisitionException>(() =>
                context.AcquireTokenSilentAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId, new UserIdentifier("unique_id", UserIdentifierType.UniqueId)));
            Assert.IsTrue((ex.InnerException.InnerException.InnerException).Message.Contains(TestConstants.ErrorSubCode));
        }

        [TestMethod]
        [Description("Test for ensuring ADAL returns the appropriate headers during a http failure.")]
        public async Task HttpErrorResponseWithHeadersAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                                           TestConstants.DefaultRedirectUri + "?code=some-code"));

            List<KeyValuePair<string, string>> HttpErrorResponseWithHeaders = new List<KeyValuePair<string, string>>();
            HttpErrorResponseWithHeaders.Add(new KeyValuePair<string, string>("Retry-After", "120"));
            HttpErrorResponseWithHeaders.Add(new KeyValuePair<string, string>("GatewayTimeout", "0"));
            HttpErrorResponseWithHeaders.Add(new KeyValuePair<string, string>("Forbidden", "0"));

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.DefaultAuthorityCommonTenant)
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateCustomHeaderFailureResponseMessage(HttpErrorResponseWithHeaders)
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, true);

            try
            {
                AuthenticationResult result = await context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                                              TestConstants.DefaultRedirectUri, _platformParameters).ConfigureAwait(false);
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
        }
    }
}
