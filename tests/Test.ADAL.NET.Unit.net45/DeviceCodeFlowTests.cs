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

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Test.Common.Core.Mocks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class DeviceCodeFlowTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ModuleInitializer.ForceModuleInitializationTestOnly();
            InstanceDiscovery.InstanceCache.Clear();
        }

        internal void SetupMocks(MockHttpManager httpManager)
        {
            httpManager.AddInstanceDiscoveryMockHandler();
        }

        [TestMethod]
        public async Task PositiveTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                DeviceCodeResult dcr = new DeviceCodeResult()
                {
                    ClientId = AdalTestConstants.DefaultClientId,
                    Resource = AdalTestConstants.DefaultResource,
                    DeviceCode = "device-code",
                    ExpiresOn = (DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10)),
                    Interval = 5,
                    Message = "get token here",
                    UserCode = "user-code",
                    VerificationUrl = "https://login.microsoftonline.com/home.oauth2/token"
                };

                httpManager.AddMockHandler(new MockHttpMessageHandler()
                {
                    Method = HttpMethod.Post,
                    Url = "https://login.microsoftonline.com/home/oauth2/token",
                    ResponseMessage = MockHelpers.CreateFailureResponseMessage("{\"error\":\"authorization_pending\"," +
                                                                               "\"error_description\":\"AADSTS70016: Pending end-user authorization." +
                                                                               "\\r\\nTrace ID: f6c2c73f-a21d-474e-a71f-d8b121a58205\\r\\nCorrelation ID: " +
                                                                               "36fe3e82-442f-4418-b9f4-9f4b9295831d\\r\\nTimestamp: 2015-09-24 19:51:51Z\"," +
                                                                               "\"error_codes\":[70016],\"timestamp\":\"2015-09-24 19:51:51Z\",\"trace_id\":" +
                                                                               "\"f6c2c73f-a21d-474e-a71f-d8b121a58205\",\"correlation_id\":" +
                                                                               "\"36fe3e82-442f-4418-b9f4-9f4b9295831d\"}")
                });

                httpManager.AddMockHandler(new MockHttpMessageHandler()
                {
                    Method = HttpMethod.Post,
                    Url = "https://login.microsoftonline.com/home/oauth2/token",
                    ResponseMessage =
                        MockHelpers.CreateSuccessTokenResponseMessage(AdalTestConstants.DefaultUniqueId,
                            AdalTestConstants.DefaultDisplayableId, AdalTestConstants.DefaultResource)
                });

                TokenCache cache = new TokenCache();
                AuthenticationContext ctx = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityHomeTenant,
                    AuthorityValidationType.NotProvided,
                    cache);

                AuthenticationResult result = await ctx.AcquireTokenByDeviceCodeAsync(dcr).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.AreEqual("some-access-token", result.AccessToken);
            }
        }

        [TestMethod]
        public void TestDeviceCodeCancel()
        {
            using (var httpManager = new MockHttpManager())
            {
                DeviceCodeResult dcr = new DeviceCodeResult()
                {
                    ClientId = AdalTestConstants.DefaultClientId,
                    Resource = AdalTestConstants.DefaultResource,
                    DeviceCode = "device-code",
                    ExpiresOn = (DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10)),
                    Interval = 5,
                    Message = "get token here",
                    UserCode = "user-code",
                    VerificationUrl = "https://login.microsoftonline.com/home.oauth2/token"
                };

                MockHttpMessageHandler mockMessageHandler = new MockHttpMessageHandler()
                {
                    Method = HttpMethod.Post,
                    Url = "https://login.microsoftonline.com/home/oauth2/token",
                    ResponseMessage = MockHelpers.CreateFailureResponseMessage("{\"error\":\"authorization_pending\"," +
                                                                               "\"error_description\":\"AADSTS70016: Pending end-user authorization." +
                                                                               "\\r\\nTrace ID: f6c2c73f-a21d-474e-a71f-d8b121a58205\\r\\nCorrelation ID: " +
                                                                               "36fe3e82-442f-4418-b9f4-9f4b9295831d\\r\\nTimestamp: 2015-09-24 19:51:51Z\"," +
                                                                               "\"error_codes\":[70016],\"timestamp\":\"2015-09-24 19:51:51Z\",\"trace_id\":" +
                                                                               "\"f6c2c73f-a21d-474e-a71f-d8b121a58205\",\"correlation_id\":" +
                                                                               "\"36fe3e82-442f-4418-b9f4-9f4b9295831d\"}")
                };

                TokenCache cache = new TokenCache();
                AuthenticationContext ctx = new AuthenticationContext(AdalTestConstants.DefaultAuthorityHomeTenant, cache);

                var cancellationSource = new CancellationTokenSource();
                // We setup the cancel before calling the RunAsync operation since we don't check the cancel
                // until later and the mock network calls run insanely fast for us to timeout for them.
                cancellationSource.Cancel();

                AssertException.TaskThrows<OperationCanceledException>(() => ctx.AcquireTokenByDeviceCodeAsync(dcr, cancellationSource.Token));
            }
        }

        [TestMethod]
        public async Task AdfsPositiveTestAsync()
        {
            await CreateAdfsDeviceCodeTestAsync(AdalTestConstants.DefaultAdfsAuthorityTenant).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task AdfsWithTenantIdPositiveTestAsync()
        {
            await CreateAdfsDeviceCodeTestAsync(AdalTestConstants.AdfsAuthorityWithTenant).ConfigureAwait(false); // adfs should never have a tenantId
        }

        [TestMethod]
        public async Task AdfsPostMethodTestAsync()
        {
            await CreateAdfsPostMethodTestAsync(AdalTestConstants.DefaultAdfsAuthorityTenant).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task AdfsWithTenantIdPostMethodTestAsync()
        {
            await CreateAdfsPostMethodTestAsync(AdalTestConstants.AdfsAuthorityWithTenant).ConfigureAwait(false); // adfs should never have a tenantId
        }

        private async Task CreateAdfsPostMethodTestAsync(string authority)
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                httpManager.AddMockHandler(new MockHttpMessageHandler()
                {
                    Method = HttpMethod.Post,
                    Url = "https://login.contoso.com/adfs/oauth2/devicecode",
                    ResponseMessage = MockHelpers.CreateSuccessDeviceCodeResponseMessage()
                });

                AuthenticationContext context = new AuthenticationContext(
                    serviceBundle,
                    authority,
                    AuthorityValidationType.False,
                    null);

                DeviceCodeResult dcr = await context.AcquireDeviceCodeAsync(
                    AdalTestConstants.DefaultResource,
                    AdalTestConstants.DefaultClientId)
                    .ConfigureAwait(false);

                Assert.IsNotNull(dcr);
                Assert.AreEqual("some-user-code", dcr.UserCode);

                Assert.AreEqual(authority, context.Authority);
                CheckAdfsEndpoints(authority, context.Authenticator);
            }
        }

        private async Task CreateAdfsDeviceCodeTestAsync(string authority)
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                DeviceCodeResult dcr = new DeviceCodeResult()
                {
                    ClientId = AdalTestConstants.DefaultClientId,
                    Resource = AdalTestConstants.DefaultResource,
                    DeviceCode = "device-code",
                    ExpiresOn = (DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10)),
                    Interval = 5,
                    Message = "get token here",
                    UserCode = "user-code",
                    VerificationUrl = "https://contoso.com/adfs/oauth2/deviceauth"
                };

                httpManager.AddMockHandler(new MockHttpMessageHandler()
                {
                    Method = HttpMethod.Post,
                    Url = "https://login.contoso.com/adfs/oauth2/token",
                    ResponseMessage = MockHelpers.CreateFailureResponseMessage("{\"error\":\"authorization_pending\"," +
                                                                               "\"error_description\":\"AADSTS70016: Pending end-user authorization." +
                                                                               "\\r\\nTrace ID: f6c2c73f-a21d-474e-a71f-d8b121a58205\\r\\nCorrelation ID: " +
                                                                               "36fe3e82-442f-4418-b9f4-9f4b9295831d\\r\\nTimestamp: 2015-09-24 19:51:51Z\"," +
                                                                               "\"error_codes\":[70016],\"timestamp\":\"2015-09-24 19:51:51Z\",\"trace_id\":" +
                                                                               "\"f6c2c73f-a21d-474e-a71f-d8b121a58205\",\"correlation_id\":" +
                                                                               "\"36fe3e82-442f-4418-b9f4-9f4b9295831d\"}")
                });

                httpManager.AddMockHandler(new MockHttpMessageHandler()
                {
                    Method = HttpMethod.Post,
                    Url = "https://login.contoso.com/adfs/oauth2/token",
                    ResponseMessage =
                        MockHelpers.CreateSuccessTokenResponseMessage(AdalTestConstants.DefaultUniqueId,
                            AdalTestConstants.DefaultDisplayableId, AdalTestConstants.DefaultResource)
                });

                TokenCache cache = new TokenCache();
                AuthenticationContext context = new AuthenticationContext(
                    serviceBundle,
                    authority,
                    AuthorityValidationType.False,
                    cache);
                AuthenticationResult result = await context.AcquireTokenByDeviceCodeAsync(dcr).ConfigureAwait(false);
                Assert.IsNotNull(result);
                Assert.AreEqual("some-access-token", result.AccessToken);

                CheckAdfsEndpoints(authority, context.Authenticator);
            }
        }

        private void CheckAdfsEndpoints(string authority, Authenticator authenticator)
        {
            Assert.AreEqual(authority, authenticator.Authority);
            Assert.AreEqual(AuthorityType.ADFS, authenticator.AuthorityType);
            Assert.AreEqual("https://login.contoso.com/adfs/oauth2/authorize", authenticator.AuthorizationUri);
            Assert.AreEqual("https://login.contoso.com/adfs/oauth2/devicecode", authenticator.DeviceCodeUri);
            Assert.AreEqual("https://login.contoso.com/adfs/oauth2/token", authenticator.SelfSignedJwtAudience);
            Assert.AreEqual("https://login.contoso.com/adfs/oauth2/token", authenticator.TokenUri);
        }
    }
}
