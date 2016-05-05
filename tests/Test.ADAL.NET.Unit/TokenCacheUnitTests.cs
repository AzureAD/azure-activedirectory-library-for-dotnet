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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.Common;
using Test.ADAL.Common.Unit;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    [DeploymentItem("oldcache.txt")]
    public class TokenCacheUnitTests
    {
        [TestMethod]
        [Description("Test to store in default token cache")]
        [TestCategory("AdalDotNetUnit")]
        public void DefaultTokenCacheTest()
        {
            TokenCacheTests.DefaultTokenCacheTest();
        }

        [TestMethod]
        [Description("Test for TokenCache")]
        [TestCategory("AdalDotNetUnit")]
        public async Task TokenCacheKeyTest()
        {
            await TokenCacheTests.TokenCacheKeyTestAsync();
        }

        [TestMethod]
        [Description("Test for Token Cache Operations")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheOperationsTest()
        {
            TokenCacheTests.TokenCacheOperationsTest();
        }

        [TestMethod]
        [Description("Test for Token Cache Capacity")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheCapacityTest()
        {
            TokenCacheTests.TokenCacheCapacityTest();
        }

        [TestMethod]
        [Description("Test for Token Cache Serialization")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheSerializationTest()
        {
            TokenCacheTests.TokenCacheSerializationTest();
        }

        [TestMethod]
        [Description("Test for Multiple User tokens found, hash fallback test")]
        [TestCategory("AdalDotNetUnit")]
        public void MultipleUserAssertionHashTest()
        {
            TokenCacheKey key = new TokenCacheKey("https://localhost/MockSts/", "resource1", "client1", TokenSubjectType.User, null, "user1");
            TokenCacheKey key2 = new TokenCacheKey("https://localhost/MockSts/", "resource1", "client1", TokenSubjectType.User, null, "user2");
            AuthenticationResult value = TokenCacheTests.CreateCacheValue(null, "user1");
            value.UserAssertionHash = "hash1";
            AuthenticationResult value2 = TokenCacheTests.CreateCacheValue(null, "user2");
            value2.UserAssertionHash = "hash2";

            TokenCache cache = new TokenCache();
            cache.tokenCacheDictionary[key] = value;
            cache.tokenCacheDictionary[key2] = value2;
            CacheQueryData data = new CacheQueryData()
            {
                AssertionHash = "hash1",
                Authority = "https://localhost/MockSts/",
                Resource = "resource1",
                ClientId = "client1",
                SubjectType = TokenSubjectType.User,
                UniqueId = null,
                DisplayableId = null
            };

            AuthenticationResult result = cache.LoadFromCache(data, null);
            TokenCacheTests.VerifyAuthenticationResultsAreEqual(value, result);

            data.AssertionHash = "hash2";
            result = cache.LoadFromCache(data, null);
            TokenCacheTests.VerifyAuthenticationResultsAreEqual(value2, result);

            data.AssertionHash = null;

            try
            {
                cache.LoadFromCache(data, null);
            }
            catch (Exception exc)
            {
                Verify.IsTrue(exc is AdalException);
                Verify.AreEqual(((AdalException)exc).ErrorCode, AdalError.MultipleTokensMatched);
            }
        }

        [TestMethod]
         [Description("Test for Token Cache backwasrd compatiblity where new attribute is added in AuthenticationResult")]
         [TestCategory("AdalDotNetUnit")]
         public void TokenCacheBackCompatTest()
        {
            TokenCache cache = new TokenCache(File.ReadAllBytes("oldcache.txt"));
            Verify.IsNotNull(cache);
            foreach (var value in cache.tokenCacheDictionary.Values)
            {
                Verify.IsNull(value.UserAssertionHash);
            }
        }

        [TestMethod]
        [Description("Positive Test for Parallel stores on cache")]
        [TestCategory("AdalDotNet.Unit")]
        public void ParallelStoreTest()
        {
            TokenCacheTests.ParallelStorePositiveTest(File.ReadAllBytes("oldcache.txt"));
        }

    }
}
