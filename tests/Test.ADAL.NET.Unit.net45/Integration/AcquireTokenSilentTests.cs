//------------------------------------------------------------------------------
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
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Test.Common.Core.Mocks;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;
using Test.ADAL.NET.Unit.net45;
using Microsoft.Identity.Core;

namespace Test.ADAL.NET.Integration
{
    [TestClass]
    public class AcquireTokenSilentTests
    {
        [TestInitialize]
        public void Initialize()
        {
            TokenCache.DefaultShared.Clear();
            InstanceDiscovery.InstanceCache.Clear();
        }

        internal void SetupMocks(MockHttpManager httpManager)
        {
            httpManager.AddInstanceDiscoveryMockHandler();
        }

        [TestMethod]
        [TestCategory("AcquireTokenSilentTests")]
        public void AcquireTokenSilentServiceErrorTest()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                TokenCache cache = new TokenCache();

                AdalTokenCacheKey key = new AdalTokenCacheKey(AdalTestConstants.DefaultAuthorityCommonTenant,
                    AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId, TokenSubjectType.User, "unique_id",
                    "displayable@id.com");
                cache._tokenCacheDictionary[key] = new AdalResultWrapper
                {
                    RefreshToken = "something-invalid",
                    ResourceInResponse = AdalTestConstants.DefaultResource,
                    Result = new AdalResult("Bearer", "some-access-token", DateTimeOffset.UtcNow)
                };

                var myHttpClientFactory = new MyHttpClientFactory();

                AuthenticationContext context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    cache,
                    myHttpClientFactory);

                var ex = AssertException.TaskThrows<AdalSilentTokenAcquisitionException>(async () =>
                {
                    httpManager.AddMockHandler(
                        new MockHttpMessageHandler(
                        AdalTestConstants.GetTokenEndpoint(
                            AdalTestConstants.DefaultAuthorityCommonTenant))
                        {
                            Method = HttpMethod.Post,
                            ResponseMessage = MockHelpers.CreateInvalidGrantTokenResponseMessage()
                        });
                    await context.AcquireTokenSilentAsync(
                        AdalTestConstants.DefaultResource,
                        AdalTestConstants.DefaultClientId,
                        new UserIdentifier("unique_id", UserIdentifierType.UniqueId))
                        .ConfigureAwait(false);
                });

                Assert.AreEqual(AdalError.FailedToAcquireTokenSilently, ex.ErrorCode);
                Assert.AreEqual(AdalErrorMessage.FailedToAcquireTokenSilently, ex.Message);
                Assert.IsNotNull(ex.InnerException);
                Assert.IsTrue(ex.InnerException is AdalException);
                Assert.AreEqual(((AdalException)ex.InnerException).ErrorCode, "invalid_grant");

                // There should be one cached entry.
                Assert.AreEqual(1, context.TokenCache.Count);
            }
        }

        [TestMethod]
        [TestCategory("AcquireTokenSilentTests")]
        //292916 Ensure AcquireTokenSilent tests exist in ADAL.NET for public clients
        public async Task AcquireTokenSilentTestWithValidTokenInCacheAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                SetupMocks(httpManager);

                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);
                var context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityHomeTenant,
                    AuthorityValidationType.True,
                    new TokenCache());

                AdalTokenCacheKey key = new AdalTokenCacheKey(AdalTestConstants.DefaultAuthorityHomeTenant,
                    AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId, TokenSubjectType.User,
                    AdalTestConstants.DefaultUniqueId, AdalTestConstants.DefaultDisplayableId);
                context.TokenCache._tokenCacheDictionary[key] = new AdalResultWrapper
                {
                    RefreshToken = "some-rt",
                    ResourceInResponse = AdalTestConstants.DefaultResource,
                    Result = new AdalResult("Bearer", "existing-access-token",
                        DateTimeOffset.UtcNow + TimeSpan.FromMinutes(100))
                };

                AuthenticationResult result =
                    await
                        context.AcquireTokenSilentAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId, new UserIdentifier("unique_id", UserIdentifierType.UniqueId)).ConfigureAwait(false);

                Assert.IsNotNull(result);
                Assert.AreEqual("existing-access-token", result.AccessToken);

                // There should be one cached entry.
                Assert.AreEqual(1, context.TokenCache.Count);
            }
        }

        [TestMethod]
        [TestCategory("AcquireTokenSilentTests")]
        //292916 Ensure AcquireTokenSilent tests exist in ADAL.NET for public clients
        public void AcquireTokenSilentWithEmptyCacheTest()
        {
            var context = new AuthenticationContext(AdalTestConstants.DefaultAuthorityHomeTenant, true, new TokenCache());
            AuthenticationResult result;
            AdalSilentTokenAcquisitionException ex = AssertException.TaskThrows<AdalSilentTokenAcquisitionException>(async () =>
            result = await context.AcquireTokenSilentAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId, new UserIdentifier("unique_id", UserIdentifierType.UniqueId)).ConfigureAwait(false));

            Assert.AreEqual(ex.ErrorCode, AdalError.FailedToAcquireTokenSilently);
            Assert.AreEqual(ex.Message, AdalErrorMessage.FailedToAcquireTokenSilently);
            Assert.IsNull(ex.InnerException);
        }

        [TestMethod]
        [TestCategory("AcquireTokenSilentTests")]
        //292916 Ensure AcquireTokenSilent tests exist in ADAL.NET for public clients
        public async Task ExpiredATValidRTInCache_GetNewATRTFromServiceAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                SetupMocks(httpManager);

                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);
                
                var context = new AuthenticationContext(
                   serviceBundle,
                   AdalTestConstants.DefaultAuthorityCommonTenant,
                   AuthorityValidationType.NotProvided,
                   new TokenCache());

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                    PostData = new Dictionary<string, string>()
                {
                    { "client_id", AdalTestConstants.DefaultClientId},
                    {"grant_type", "refresh_token"},
                    {"refresh_token", "some_rt" }
                }
                });

                AdalTokenCacheKey key = new AdalTokenCacheKey(AdalTestConstants.DefaultAuthorityCommonTenant,
                    AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId, TokenSubjectType.User,
                    AdalTestConstants.DefaultUniqueId, AdalTestConstants.DefaultDisplayableId);

                context.TokenCache._tokenCacheDictionary[key] = new AdalResultWrapper
                {
                    RefreshToken = "some_rt",
                    ResourceInResponse = AdalTestConstants.DefaultResource,
                    Result = new AdalResult("Bearer", "existing-access-token",
                        DateTimeOffset.UtcNow)
                };

                AuthenticationResult result = await context.AcquireTokenSilentAsync(AdalTestConstants.DefaultResource, AdalTestConstants.DefaultClientId,
                        new UserIdentifier(AdalTestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId)).ConfigureAwait(false);
                Assert.IsNotNull(result);
            }
        }
    }
}