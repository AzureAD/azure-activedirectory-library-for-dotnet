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
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using Test.ADAL.Common;
using System.Net;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Cache;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Http;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace Test.ADAL.NET.Integration
{
    [TestClass]
    public class PromptBehaviorTests
    {
        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.InitializeMockProvider();
            ResetInstanceDiscovery();
        }

        public void ResetInstanceDiscovery()
        {
            InstanceDiscovery.InstanceCache.Clear();
            HttpMessageHandlerFactory.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler(TestConstants.GetDiscoveryEndpoint(TestConstants.DefaultAuthorityCommonTenant)));
        }
        
        [TestMethod]
        [Description("Test for PromptBehavior.Auto, prompts only if necessary")]
        public async Task AutoPromptBehaviorTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"grant_type", "authorization_code"}
                }
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true, new TokenCache());
            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Auto)).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.AreEqual(TestConstants.DefaultAuthorityHomeTenant, context.Authenticator.Authority);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);
            // There should be one cached entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }
        
        [TestMethod]
        [Description("Test for calling promptBehavior.Auto when cache already has an access token")]
        public async Task AutoPromptBehaviorWithTokenInCacheTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true, new TokenCache());

            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "existing-access-token",
                    DateTimeOffset.UtcNow + TimeSpan.FromMinutes(100))
            };

            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Auto)).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.AreEqual("existing-access-token", result.AccessToken);

            // There should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }

        [TestMethod]
        [Description("Test for calling promptBehavior.Auto when cache has an expired access token, but a good refresh token")]
        public async Task AutoPromptBehaviorWithExpiredAccessTokenAndGoodRefreshTokenInCacheTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true, new TokenCache());

            await context.TokenCache.StoreToCacheAsync(new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "existing-access-token", DateTimeOffset.UtcNow)
                {
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = TestConstants.DefaultDisplayableId,
                            UniqueId = TestConstants.DefaultUniqueId
                        }
                },
            },
            TestConstants.DefaultAuthorityHomeTenant, TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
            new CallState(new Guid())).ConfigureAwait(false);
            ResetInstanceDiscovery();

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"grant_type", "refresh_token"}
                }
            });

            AuthenticationResult result =
                await
                    context.AcquireTokenSilentAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                    new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId),
                    new PlatformParameters(PromptBehavior.Auto)).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.AreEqual("some-access-token", result.AccessToken);

            // There should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }

        [TestMethod]
        [Description("Test for Force Prompt with PromptBehavior.Always")]
        public async Task ForcePromptForAlwaysPromptBehaviorTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true, new TokenCache());

            await context.TokenCache.StoreToCacheAsync(new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "existing-access-token", DateTimeOffset.UtcNow)
                {
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = TestConstants.DefaultDisplayableId,
                            UniqueId = TestConstants.DefaultUniqueId
                        }
                },
            },
            TestConstants.DefaultAuthorityHomeTenant, TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
            new CallState(new Guid())).ConfigureAwait(false);
            ResetInstanceDiscovery();

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"grant_type", "authorization_code"}
                }
            });

            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Always)).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.AreEqual("some-access-token", result.AccessToken);

            // There should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }
        
        [TestMethod]
        [Description("Test for Force Prompt with PromptBehavior.SelectAccount")]
        public async Task ForcePromptForSelectAccountPromptBehaviorTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                    TestConstants.DefaultRedirectUri + "?code=some-code"),
                // validate that authorizationUri passed to WebUi contains prompt=select_account query parameter
                new Dictionary<string, string> { { "prompt", "select_account" } });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true, new TokenCache());

            await context.TokenCache.StoreToCacheAsync(new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "existing-access-token",
                    DateTimeOffset.UtcNow + TimeSpan.FromMinutes(100))
                {
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = TestConstants.DefaultDisplayableId,
                            UniqueId = TestConstants.DefaultUniqueId
                        }
                },
            },
            TestConstants.DefaultAuthorityHomeTenant, TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
            new CallState(new Guid())).ConfigureAwait(false);
            ResetInstanceDiscovery();

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>
                {
                    {"grant_type", "authorization_code"}
                }
            });

            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.SelectAccount)).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.AreEqual(TestConstants.DefaultAuthorityHomeTenant, context.Authenticator.Authority);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);

            // There should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }
        
        [TestMethod]
        [Description("Test for Force Prompt with PromptBehavior.Never")]
        public void ForcePromptForNeverPromptBehaviorTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());

            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
               TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
               TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "existing-access-token", DateTimeOffset.UtcNow)
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateInvalidRequestTokenResponseMessage()
            });

            var exc = AssertException.TaskThrows<AdalServiceException>(() =>
            context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Never)));

            Assert.AreEqual(AdalError.FailedToRefreshToken, exc.ErrorCode);
            // There should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }
        
        [TestMethod]
        [Description("Test for Force Prompt with PromptBehavior.RefreshSession")]
        public async Task ForcePromptForRefreshSessionPromptBehaviorTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"),
                // validate that authorizationUri passed to WebUi contains prompt=refresh_session query parameter
                new Dictionary<string, string> { { "prompt", "refresh_session" } });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true, new TokenCache());

            await context.TokenCache.StoreToCacheAsync(new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "existing-access-token", DateTimeOffset.UtcNow)
                {
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = TestConstants.DefaultDisplayableId,
                            UniqueId = TestConstants.DefaultUniqueId
                        }
                },
            },
            TestConstants.DefaultAuthorityHomeTenant, TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
            new CallState(new Guid())).ConfigureAwait(false);
            ResetInstanceDiscovery();

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler(TestConstants.GetTokenEndpoint(TestConstants.DefaultAuthorityHomeTenant))
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"grant_type", "authorization_code"}
                }
            });

            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.RefreshSession)).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.AreEqual("some-access-token", result.AccessToken);

            // There should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }
    }
}
