//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.Common;
using Test.ADAL.NET.Unit.Mocks;

namespace Test.ADAL.NET.Unit
{
    /// <summary>
    /// This test class executes and validates OBO scenarios where token cache may or may not 
    /// contain entries with user assertion hash. It accounts for cases where there is
    /// a single user and when there are multiple users in the cache.
    /// user assertion hash exists so that the API can deterministically identify the user
    /// in the cache when a usernae is not passed in. It also allows the API to acquire
    /// new token when a different assertion is passed for the user. this is needed because
    /// the user may have authenticated with updated claims like MFA/device auth on the client.
    /// </summary>
    [TestClass]
    public class OboFlowTests
    {
        private readonly DateTimeOffset _expirationTime = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(30);
        private readonly MockHttpWebRequestFactory _mockFactory = new MockHttpWebRequestFactory();

        private static readonly string[] _cacheNoise = { "", "different" };

        [TestInitialize]
        public void TestInitialize()
        {
            NetworkPlugin.HttpWebRequestFactory = _mockFactory;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            NetworkPlugin.SetToDefault();
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task MultiUserNoHashInCacheNoUsernamePassedInAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            foreach (var cachenoise in _cacheNoise)
            {
                TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                    TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                    cachenoise + TestConstants.DefaultUniqueId, cachenoise + TestConstants.DefaultDisplayableId);
                //cache entry has no user assertion hash
                context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer",
                    cachenoise + "some-token-in-cache", cachenoise + "some-rt", _expirationTime)
                {
                    Resource = TestConstants.DefaultResource,
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = cachenoise + TestConstants.DefaultDisplayableId,
                            UniqueId = cachenoise + TestConstants.DefaultUniqueId
                        },
                };
            }

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);
            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage()));

            // call acquire token with no username. multiple results will be found.
            // this will result in a network call because cache entry with no assertion hash is
            // treated as a cache miss.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be 2 cache entries.
            Assert.AreEqual(2, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First(x => x.UserAssertionHash != null).UserAssertionHash);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task MultiUserNoHashInCacheMatchingUsernamePassedInAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            foreach (var cachenoise in _cacheNoise)
            {
                TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                    TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                    cachenoise + TestConstants.DefaultUniqueId, cachenoise + TestConstants.DefaultDisplayableId);
                //cache entry has no user assertion hash
                context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer",
                    cachenoise + "some-token-in-cache",
                    cachenoise + "some-rt", _expirationTime)
                {
                    Resource = TestConstants.DefaultResource,
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = cachenoise + TestConstants.DefaultDisplayableId,
                            UniqueId = cachenoise + TestConstants.DefaultUniqueId
                        },
                };
            }

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);
            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage()));

            // call acquire token with matching username from cache entry. 1 result will be found
            // this will result in a network call because cache entry with no assertion 
            // hash is treated as a cache miss.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken, OAuthGrantType.JwtBearer, TestConstants.DefaultDisplayableId));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(2, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First(x => x.UserAssertionHash != null).UserAssertionHash);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task MultiUserNoHashInCacheDifferentUsernamePassedInAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            foreach (var cachenoise in _cacheNoise)
            {
                TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                    TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                    cachenoise + TestConstants.DefaultUniqueId, cachenoise + TestConstants.DefaultDisplayableId);
                //cache entry has no user assertion hash
                context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer",
                    cachenoise + "some-token-in-cache",
                    cachenoise + "some-rt", _expirationTime)
                {
                    Resource = TestConstants.DefaultResource,
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = cachenoise + TestConstants.DefaultDisplayableId,
                            UniqueId = cachenoise + TestConstants.DefaultUniqueId
                        },
                };
            }

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            string displayableId2 = "extra" + TestConstants.DefaultDisplayableId;
            string uniqueId2 = "extra" + TestConstants.DefaultUniqueId;

            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage(uniqueId2, displayableId2,
                    TestConstants.DefaultResource)));

            // call acquire token with different username than from cache entry. this will result in a network call 
            // because cache entry with no assertion hash is treated as a cache miss.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken, OAuthGrantType.JwtBearer,
                            "non-existant" + TestConstants.DefaultDisplayableId));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(displayableId2, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(3, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First(x => x.UserAssertionHash != null).UserAssertionHash);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task MultiUserWithHashInCacheNoUsernameAndMatchingAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            //cache entry has user assertion hash
            string tokenInCache = "obo-access-token";
            foreach (var cachenoise in _cacheNoise)
            {
                TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                    TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                    cachenoise + TestConstants.DefaultUniqueId, cachenoise + TestConstants.DefaultDisplayableId);

                context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer",
                    cachenoise + tokenInCache,
                    cachenoise + "some-rt", _expirationTime)
                {
                    Resource = TestConstants.DefaultResource,
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = cachenoise + TestConstants.DefaultDisplayableId,
                            UniqueId = cachenoise + TestConstants.DefaultUniqueId
                        },
                    UserAssertionHash = PlatformSpecificHelper.CreateSha256Hash(cachenoise + accessToken)
                };
            }

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            // call acquire token with no username and matching assertion hash. this will result in a cache
            // hit.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken));
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual(tokenInCache, result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(2, context.TokenCache.Count);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task MultiUserWithHashInCacheNoUsernameAndDifferentAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";

            //cache entry has user assertion hash
            string tokenInCache = "obo-access-token";
            foreach (var cachenoise in _cacheNoise)
            {
                TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                    TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                    cachenoise + TestConstants.DefaultUniqueId, cachenoise + TestConstants.DefaultDisplayableId);
                context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer",
                    cachenoise + tokenInCache,
                    cachenoise + "some-rt", _expirationTime)
                {
                    Resource = TestConstants.DefaultResource,
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = cachenoise + TestConstants.DefaultDisplayableId,
                            UniqueId = cachenoise + TestConstants.DefaultUniqueId
                        },
                    UserAssertionHash = PlatformSpecificHelper.CreateSha256Hash(cachenoise + accessToken)
                };
            }

            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage()));
            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            // call acquire token with no username and different assertion hash. this will result in a 
            // network call.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion("non-existant" + accessToken));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(2, context.TokenCache.Count);
        }


        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task MultiUserWithHashInCacheMatchingUsernameAndMatchingAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            //cache entry has user assertion hash
            string tokenInCache = "obo-access-token";
            foreach (var cachenoise in _cacheNoise)
            {
                TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                    TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                    cachenoise + TestConstants.DefaultUniqueId, cachenoise + TestConstants.DefaultDisplayableId);

                context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer",
                    cachenoise + tokenInCache,
                    cachenoise + "some-rt", _expirationTime)
                {
                    Resource = TestConstants.DefaultResource,
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = cachenoise + TestConstants.DefaultDisplayableId,
                            UniqueId = cachenoise + TestConstants.DefaultUniqueId
                        },
                    UserAssertionHash = PlatformSpecificHelper.CreateSha256Hash(cachenoise + accessToken)
                };
            }

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            // call acquire token with matching username and matching assertion hash. this will result in a cache
            // hit.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken, OAuthGrantType.JwtBearer, TestConstants.DefaultDisplayableId));
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual(tokenInCache, result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(2, context.TokenCache.Count);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task MultiUserWithHashInCacheMatchingUsernameAndDifferentAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            //cache entry has user assertion hash
            string tokenInCache = "obo-access-token";
            foreach (var cachenoise in _cacheNoise)
            {
                TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                    TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                    cachenoise + TestConstants.DefaultUniqueId, cachenoise + TestConstants.DefaultDisplayableId);

                context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer",
                    cachenoise + tokenInCache,
                    cachenoise + "some-rt", _expirationTime)
                {
                    Resource = TestConstants.DefaultResource,
                    UserInfo =
                        new UserInfo()
                        {
                            DisplayableId = cachenoise + TestConstants.DefaultDisplayableId,
                            UniqueId = cachenoise + TestConstants.DefaultUniqueId
                        },
                    UserAssertionHash = PlatformSpecificHelper.CreateSha256Hash(cachenoise + accessToken)
                };
            }

            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage()));

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            // call acquire token with matching username and different assertion hash. this will result in a cache
            // hit.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion("non-existant" + accessToken, OAuthGrantType.JwtBearer,
                            TestConstants.DefaultDisplayableId));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(2, context.TokenCache.Count);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task SingleUserNoHashInCacheNoUsernamePassedInAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            //cache entry has no user assertion hash
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer", "some-token-in-cache",
                "some-rt", _expirationTime)
            {
                Resource = TestConstants.DefaultResource,
                UserInfo =
                    new UserInfo()
                    {
                        DisplayableId = TestConstants.DefaultDisplayableId,
                        UniqueId = TestConstants.DefaultUniqueId
                    },
            };

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);
            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage()));

            // call acquire token with no username. this will result in a network call because cache entry with no assertion hash is
            // treated as a cache miss.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First().UserAssertionHash);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task SingleUserNoHashInCacheMatchingUsernamePassedInAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            //cache entry has no user assertion hash
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer", "some-token-in-cache",
                "some-rt", _expirationTime)
            {
                Resource = TestConstants.DefaultResource,
                UserInfo =
                    new UserInfo()
                    {
                        DisplayableId = TestConstants.DefaultDisplayableId,
                        UniqueId = TestConstants.DefaultUniqueId
                    },
            };

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);
            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage()));

            // call acquire token with matching username from cache entry. this will result in a network call 
            // because cache entry with no assertion hash is treated as a cache miss.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken, OAuthGrantType.JwtBearer, TestConstants.DefaultDisplayableId));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First().UserAssertionHash);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task SingleUserNoHashInCacheDifferentUsernamePassedInAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            //cache entry has no user assertion hash
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer", "some-token-in-cache",
                "some-rt", _expirationTime)
            {
                Resource = TestConstants.DefaultResource,
                UserInfo =
                    new UserInfo()
                    {
                        DisplayableId = TestConstants.DefaultDisplayableId,
                        UniqueId = TestConstants.DefaultUniqueId
                    },
            };

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);
            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage()));

            // call acquire token with different username than from cache entry. this will result in a network call 
            // because cache entry with no assertion hash is treated as a cache miss.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken, OAuthGrantType.JwtBearer,
                            "extra" + TestConstants.DefaultDisplayableId));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First().UserAssertionHash);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task SingleUserWithHashInCacheNoUsernameAndMatchingAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);

            //cache entry has user assertion hash
            string tokenInCache = "obo-access-token";
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer", tokenInCache, "some-rt",
                _expirationTime)
            {
                Resource = TestConstants.DefaultResource,
                UserInfo =
                    new UserInfo()
                    {
                        DisplayableId = TestConstants.DefaultDisplayableId,
                        UniqueId = TestConstants.DefaultUniqueId
                    },
                UserAssertionHash = PlatformSpecificHelper.CreateSha256Hash(accessToken)
            };

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            // call acquire token with no username and matching assertion hash. this will result in a cache
            // hit.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken));
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual(tokenInCache, result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First().UserAssertionHash);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task SingleUserWithHashInCacheNoUsernameAndDifferentAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);

            //cache entry has user assertion hash
            string tokenInCache = "obo-access-token";
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer", tokenInCache, "some-rt",
                _expirationTime)
            {
                Resource = TestConstants.DefaultResource,
                UserInfo =
                    new UserInfo()
                    {
                        DisplayableId = TestConstants.DefaultDisplayableId,
                        UniqueId = TestConstants.DefaultUniqueId
                    },
                UserAssertionHash = PlatformSpecificHelper.CreateSha256Hash(accessToken + "different")
            };

            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage()));

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            // call acquire token with no username and different assertion hash. this will result in a cache
            // hit.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First().UserAssertionHash);
        }


        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task SingleUserWithHashInCacheMatchingUsernameAndMatchingAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);

            //cache entry has user assertion hash
            string tokenInCache = "obo-access-token";
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer", tokenInCache, "some-rt",
                _expirationTime)
            {
                Resource = TestConstants.DefaultResource,
                UserInfo =
                    new UserInfo()
                    {
                        DisplayableId = TestConstants.DefaultDisplayableId,
                        UniqueId = TestConstants.DefaultUniqueId
                    },
                UserAssertionHash = PlatformSpecificHelper.CreateSha256Hash(accessToken)
            };
            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            // call acquire token with matching username and matching assertion hash. this will result in a cache
            // hit.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken, OAuthGrantType.JwtBearer, TestConstants.DefaultDisplayableId));
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual(tokenInCache, result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First().UserAssertionHash);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task SingleUserWithHashInCacheMatchingUsernameAndDifferentAssertionTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);

            //cache entry has user assertion hash
            string tokenInCache = "obo-access-token";
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer", tokenInCache, "some-rt",
                _expirationTime)
            {
                Resource = TestConstants.DefaultResource,
                UserInfo =
                    new UserInfo()
                    {
                        DisplayableId = TestConstants.DefaultDisplayableId,
                        UniqueId = TestConstants.DefaultUniqueId
                    },
                UserAssertionHash = PlatformSpecificHelper.CreateSha256Hash(accessToken + "different")
            };
            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage()));

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            // call acquire token with matching username and different assertion hash. this will result in a cache
            // hit.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential,
                        new UserAssertion(accessToken, OAuthGrantType.JwtBearer, TestConstants.DefaultDisplayableId));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(1, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken),
                context.TokenCache.tokenCacheDictionary.Values.First().UserAssertionHash);
        }

        [TestMethod]
        [TestCategory("OboFlowTests")]
        public async Task SingleUserWithHashInCacheMatchingUsernameAndMatchingAssertionDifferentResourceTest()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            string accessToken = "access-token";
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.UserPlusClient,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);

            //cache entry has user assertion hash
            string tokenInCache = "obo-access-token";
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResult("Bearer", tokenInCache, "some-rt",
                _expirationTime)
            {
                Resource = TestConstants.DefaultResource,
                UserInfo =
                    new UserInfo()
                    {
                        DisplayableId = TestConstants.DefaultDisplayableId,
                        UniqueId = TestConstants.DefaultUniqueId
                    },
                UserAssertionHash = PlatformSpecificHelper.CreateSha256Hash(accessToken)
            };
            _mockFactory.MockHttpWebRequestQueue.Enqueue(
                new MockHttpWebRequest(MockHelpers.CreateSuccessTokenResponseMessage(TestConstants.AnotherResource,
                    TestConstants.DefaultDisplayableId, TestConstants.DefaultUniqueId)));

            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
                TestConstants.DefaultClientSecret);

            // call acquire token with matching username and different assertion hash. this will result in a cache
            // hit.
            var result =
                await
                    context.AcquireTokenAsync(TestConstants.AnotherResource, clientCredential,
                        new UserAssertion(accessToken, OAuthGrantType.JwtBearer, TestConstants.DefaultDisplayableId));
            Assert.AreEqual(0, _mockFactory.MockHttpWebRequestQueue.Count, "all mocks should have been consumed");
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            //there should be only one cache entry.
            Assert.AreEqual(2, context.TokenCache.Count);

            //assertion hash should be stored in the cache entry.
            foreach (var value in context.TokenCache.tokenCacheDictionary.Values)
            {
                Assert.AreEqual(PlatformSpecificHelper.CreateSha256Hash(accessToken), value.UserAssertionHash);
            }
        }
    }
}
