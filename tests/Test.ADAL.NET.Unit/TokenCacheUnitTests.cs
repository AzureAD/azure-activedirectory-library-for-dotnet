﻿//----------------------------------------------------------------------
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

using System.IO;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public async Task TokenCacheKeyTestAsync()
        {
            await TokenCacheTests.TokenCacheKeyTestAsync(new PlatformParameters(PromptBehavior.Auto, null));
        }

        [TestMethod]
        [Description("Test for Token Cache Operations")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheOperationsTest()
        {
            TokenCacheTests.TokenCacheOperationsTest();
        }

        [TestMethod]
        [Description("Test for Token Cache Cross-Tenant operations")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheCrossTenantOperationsTest()
        {
            TokenCacheTests.TokenCacheCrossTenantOperationsTest();
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
        [Description("Test for Token Cache backwasrd compatiblity where new attribute is added in AuthenticationResultEx")]
        [TestCategory("AdalDotNetUnit")]
        public void TokenCacheBackCompatTest()
        {
                TokenCacheTests.TokenCacheBackCompatTest(File.ReadAllBytes("oldcache.txt"));
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
