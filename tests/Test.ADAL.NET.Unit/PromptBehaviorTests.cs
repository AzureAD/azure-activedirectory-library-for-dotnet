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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.NET.Unit.Mocks;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using System.Net.Http;
using Test.ADAL.Common;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class PromptBehaviorTests
    {
        private PlatformParameters platformParameters;

        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();
        }

        #region PromptBehavior Tests for each value of prompt behavior
        // PromptBehavior Auto Test Case
        [TestMethod]
        [Description("Test for Force Prompt with PromptBehavior.Auto")]
        public async Task ForcePromptForAutoPromptBehaviorTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true);
            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Auto));

            Assert.IsNotNull(result);
            Assert.AreEqual(TestConstants.DefaultAuthorityHomeTenant, context.Authenticator.Authority);
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);
        }

        // Test for calling promptBehavior.auto when cache already has an access token
        [TestMethod]
        [Description("Test for calling promptBehavior.Auto when cache already has an access token")]
        public async Task AutoPromptBehaviorWithTokenInCacheTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true);
            context.TokenCache.Clear();

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
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Auto));

            Assert.IsNotNull(result);
            Assert.AreEqual(TestConstants.DefaultAuthorityHomeTenant, context.Authenticator.Authority);
            Assert.AreEqual("existing-access-token", result.AccessToken);
            Assert.IsNotNull(result.UserInfo);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }

        // PromptBehavior Always Test Case
        [TestMethod]
        [Description("Test for Force Prompt with PromptBehavior.Always")]
        public async Task ForcePromptForAlwaysPromptBehaviorTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"grant_type", "authorization_code"}
                }
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true);

            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "existing-access-token", DateTimeOffset.UtcNow)
            };

            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Always));

            Assert.IsNotNull(result);
            Assert.AreEqual(TestConstants.DefaultAuthorityHomeTenant, context.Authenticator.Authority);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }

        // PromptBehavior SelectAccount Test Case
        [TestMethod]
        [Description("Test for Force Prompt with PromptBehavior.SelectAccount")]
        public async Task ForcePromptForSelectAccountPromptBehaviorTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                    TestConstants.DefaultRedirectUri + "?code=some-code"),
                // validate that authorizationUri passed to WebUi contains prompt=select_account query parameter
                new Dictionary<string, string> { { "prompt", "select_account" } });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>
                {
                    {"grant_type", "authorization_code"}
                }
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true);
            context.TokenCache.Clear();

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
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.SelectAccount));

            Assert.IsNotNull(result);
            Assert.AreEqual(TestConstants.DefaultAuthorityHomeTenant, context.Authenticator.Authority);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }

        // PromptBehavior Never Test Case
        [TestMethod]
        [Description("Test for Force Prompt with PromptBehavior.Never")]
        public void ForcePromptForNeverPromptBehaviorTestAsync()
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

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateInvalidRequestTokenResponseMessage()
            });

            var exc = AssertException.TaskThrows<AdalServiceException>(() =>
            context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Never)));

            Assert.AreEqual(AdalError.FailedToRefreshToken, exc.ErrorCode);
            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }

        // PromptBehavior RefreshSession Test Cases
        [TestMethod]
        [Description("Test for Force Prompt with PromptBehavior.RefreshSession")]
        public async Task ForcePromptForRefreshSessionPromptBehaviorTestAsync()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"),
                // validate that authorizationUri passed to WebUi contains prompt=select_account query parameter
                new Dictionary<string, string> { { "prompt", "refresh_session" } });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"grant_type", "authorization_code"}
                }
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true);

            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "existing-access-token", DateTimeOffset.UtcNow)
            };

            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.RefreshSession));

            Assert.IsNotNull(result);
            Assert.AreEqual(TestConstants.DefaultAuthorityHomeTenant, context.Authenticator.Authority);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);
        }
        #endregion

        #region Prompt behavior specific tests

        // Test for calling promptBehavior.auto when cache already has an expired access token, but good refresh token

        // Test for calling prompBehavior.always when cache already has an access token

        #endregion
    }
}
