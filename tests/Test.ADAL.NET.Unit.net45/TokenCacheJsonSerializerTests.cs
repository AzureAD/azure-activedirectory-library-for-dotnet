// ------------------------------------------------------------------------------
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

using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Json.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Test.Microsoft.Identity.Core.Unit;

namespace Test.ADAL.NET.Unit.net45
{
    [TestClass]
    public class TokenCacheJsonSerializerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ModuleInitializer.ForceModuleInitializationTestOnly();
            InstanceDiscovery.InstanceCache.Clear();
        }


        [TestMethod]
        public void TestDeserializeEmptyBytes()
        {
            byte[] bytes = new byte[0];
            TokenCacheJsonSerializer serializer = new TokenCacheJsonSerializer(new TokenCacheAccessor());
            serializer.Deserialize(bytes);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\FociTokenCacheFromMsal3.json")]
        public void UnkownNodesTest()
        {

            string jsonFilePath = ResourceHelper.GetTestResourceRelativePath("FociTokenCacheFromMsal3.json");
            string tempPath = Path.GetTempFileName();

            try
            {
                // Copy the resource content to a temp file because we will write to it
                File.Copy(jsonFilePath, tempPath, true);
                JObject originalJson = JObject.Parse(File.ReadAllText(tempPath));

                Assert.AreEqual("1", originalJson["RefreshToken"]["my-uid.my-utid-env-refreshtoken-1--"]["family_id"].ToString());

                FileCache tokenCache = new FileCache(tempPath);
                TokenCacheNotificationArgs notificationArgs = new TokenCacheNotificationArgs();
                tokenCache.HasStateChanged = true;

                tokenCache.OnBeforeAccess(notificationArgs);

                tokenCache.OnAfterAccess(notificationArgs);
                ValidateCache(tokenCache);

                tokenCache.OnBeforeAccess(notificationArgs);
                ValidateCache(tokenCache);

                tokenCache.OnAfterAccess(notificationArgs);
                ValidateCache(tokenCache);

                // we can't validate the entire JSON file against the original because the keys might have changed
                // as ADAL does not know anything about FRTs so it will create a normal RT key from an FRT.
                JObject finalJson = JObject.Parse(File.ReadAllText(tempPath));
                Assert.AreEqual("1", finalJson["RefreshToken"]["my-uid.my-utid-env-refreshtoken-d3adb33f-c0de-ed0c-c0de-deadb33fc0d3--"]["family_id"].ToString());

                // AppMetadata and Zoo are preserved
                Assert.IsTrue(originalJson["AppMetadata"].HasValues);
                Assert.IsTrue(originalJson["Zoo"].HasValues);

                Assert.IsTrue(JToken.DeepEquals(originalJson["AppMetadata"], finalJson["AppMetadata"]));
                Assert.IsTrue(JToken.DeepEquals(originalJson["Zoo"], finalJson["Zoo"]));

                Assert.IsFalse(finalJson["Foo"].HasValues);
                Assert.IsFalse(finalJson["Bar"].HasValues);

            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\ADALv5TokenCache.json")]
        public void TokenCacheWithNoUnkownNodoes()
        {

            string jsonFilePath = ResourceHelper.GetTestResourceRelativePath("ADALv5TokenCache.json");
            string tempPath = Path.GetTempFileName();

            try
            {
                // Copy the resource content to a temp file because we will write to it
                File.Copy(jsonFilePath, tempPath, true);
                JObject originalJson = JObject.Parse(File.ReadAllText(tempPath));

                FileCache tokenCache = new FileCache(tempPath);
                TokenCacheNotificationArgs notificationArgs = new TokenCacheNotificationArgs();
                tokenCache.HasStateChanged = true;

                tokenCache.OnBeforeAccess(notificationArgs);
                tokenCache.OnAfterAccess(notificationArgs);
                tokenCache.OnBeforeAccess(notificationArgs);
                tokenCache.OnAfterAccess(notificationArgs);

                JObject finalJson = JObject.Parse(File.ReadAllText(tempPath));

                Assert.IsTrue(
                    JToken.DeepEquals(originalJson, finalJson), 
                    "There should be no semantic difference betweenthe original and the final JSON files");
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        private static void ValidateCache(FileCache tokenCache)
        {
            Assert.AreEqual(5, tokenCache.TokenCacheAccessor.AccessTokenCount);
            Assert.AreEqual(4, tokenCache.TokenCacheAccessor.RefreshTokenCount);
            Assert.AreEqual(3, tokenCache.TokenCacheAccessor.IdTokenCount);
            Assert.AreEqual(3, tokenCache.TokenCacheAccessor.AccountCount);
        }

        internal class FileCache : TokenCache
        {
            private readonly string _cacheFilePath;

            // Initializes the cache against a local file.
            // If the file is already present, it loads its content in the ADAL cache
            public FileCache(string filePath)
            {
                _cacheFilePath = filePath;
                AfterAccess = AfterAccessNotification;
                BeforeAccess = BeforeAccessNotification;

                DeserializeMsalV3(File.ReadAllBytes(_cacheFilePath));
            }

            // Triggered right before ADAL needs to access the cache.
            // Reload the cache from the persistent store in case it changed since the last access.
            private void BeforeAccessNotification(TokenCacheNotificationArgs args)
            {
                DeserializeMsalV3(File.ReadAllBytes(_cacheFilePath));
            }

            // Triggered right after ADAL accessed the cache.
            private void AfterAccessNotification(TokenCacheNotificationArgs args)
            {
                // if the access operation resulted in a cache update
                if (HasStateChanged)
                {
                    File.WriteAllBytes(_cacheFilePath, SerializeMsalV3());
                    // once the write operation took place, restore the HasStateChanged bit to false
                    HasStateChanged = false;
                }
            }
        }
    }
}
