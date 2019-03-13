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

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Test.Common.Core.Mocks;
using System.Net.Http;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    [DeploymentItem("Resources\\valid_cert.pfx")]
    public class JsonWebTokenTests
    {
        private void X5CMockHandler(MockHttpManager httpManager)
        {
            httpManager.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(
                    AdalTestConstants.DefaultUniqueId,
                    AdalTestConstants.DefaultDisplayableId,
                    AdalTestConstants.DefaultResource),
                AdditionalRequestValidation = request =>
                {
                    var requestContent = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var formsData = CoreHelpers.ParseKeyValueList(requestContent, '&', true, null);

                // Check presence of client_assertion in request
                Assert.IsTrue(formsData.TryGetValue("client_assertion", out string encodedJwt), "Missing client_assertion from request");

                // Check presence of x5c cert claim. It should exist.
                var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(encodedJwt);
                    Assert.IsTrue(jsonToken.Header.Any(header => header.Key == "x5c"), "x5c should be present");
                }
            });
        }

        private void EmptyX5CMockHandler(MockHttpManager httpManager)
        {
            httpManager.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(
                    AdalTestConstants.DefaultUniqueId,
                    AdalTestConstants.DefaultDisplayableId,
                    AdalTestConstants.DefaultResource),
                AdditionalRequestValidation = request =>
                {
                    var requestContent = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var formsData = CoreHelpers.ParseKeyValueList(requestContent, '&', true, null);

                    // Check presence of client_assertion in request
                    Assert.IsTrue(formsData.TryGetValue("client_assertion", out string encodedJwt), "Missing client_assertion from request");

                    // Check presence of x5c cert claim. It should not exist.
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(encodedJwt);
                    Assert.IsFalse(jsonToken.Header.Any(header => header.Key == "x5c"), "x5c should not be present");
                }
            });
        }

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
        [Description("Test for Json Web Token with client assertion and a X509 public certificate claim")]
        public async Task JsonWebTokenWithX509PublicCertClaimTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                var certificate = new X509Certificate2(
                Microsoft.Identity.Core.Unit.ResourceHelper.GetTestResourceRelativePath("valid_cert.pfx"),
                AdalTestConstants.DefaultPassword);
                var clientAssertion = new ClientAssertionCertificate(AdalTestConstants.DefaultClientId, certificate);
                var context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.TenantSpecificAuthority,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                var validCertClaim = "\"x5c\":\"" + Convert.ToBase64String(certificate.GetRawCertData());

                //Check for x5c claim
                X5CMockHandler(httpManager);
                AuthenticationResult result = await context.AcquireTokenAsync(AdalTestConstants.DefaultResource, clientAssertion, true).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                X5CMockHandler(httpManager);
                result = await context.AcquireTokenByAuthorizationCodeAsync(AdalTestConstants.DefaultAuthorizationCode, AdalTestConstants.DefaultRedirectUri, clientAssertion, AdalTestConstants.DefaultResource, true).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                X5CMockHandler(httpManager);
                result = await context.AcquireTokenAsync(AdalTestConstants.DefaultResource, clientAssertion, new UserAssertion("Access Token"), true).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                //Check for empty x5c claim
                EmptyX5CMockHandler(httpManager);
                context.TokenCache.Clear();
                result = await context.AcquireTokenAsync(AdalTestConstants.DefaultResource, clientAssertion, false).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                EmptyX5CMockHandler(httpManager);
                result = await context.AcquireTokenByAuthorizationCodeAsync(AdalTestConstants.DefaultAuthorizationCode, AdalTestConstants.DefaultRedirectUri, clientAssertion, AdalTestConstants.DefaultResource, false).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                EmptyX5CMockHandler(httpManager);
                result = await context.AcquireTokenAsync(AdalTestConstants.DefaultResource, clientAssertion, new UserAssertion("Access Token"), false).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);
            }
        }

        [TestMethod]
        [Description("Test for default client assertion without X509 public certificate claim")]
        public async Task JsonWebTokenDefaultX509PublicCertClaimTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                var certificate = new X509Certificate2(
                Microsoft.Identity.Core.Unit.ResourceHelper.GetTestResourceRelativePath("valid_cert.pfx"),
                AdalTestConstants.DefaultPassword);

                var clientAssertion = new ClientAssertionCertificate(AdalTestConstants.DefaultClientId, certificate);

                var context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.TenantSpecificAuthority,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                EmptyX5CMockHandler(httpManager);
                AuthenticationResult result = await context.AcquireTokenAsync(AdalTestConstants.DefaultResource, clientAssertion).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                EmptyX5CMockHandler(httpManager);
                result = await context.AcquireTokenByAuthorizationCodeAsync(AdalTestConstants.DefaultAuthorizationCode, AdalTestConstants.DefaultRedirectUri, clientAssertion, AdalTestConstants.DefaultResource).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                EmptyX5CMockHandler(httpManager);
                result = await context.AcquireTokenAsync(AdalTestConstants.DefaultResource, clientAssertion, new UserAssertion("Access Token")).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);
            }
        }

        [TestMethod]
        [Description("Test for client assertion with developer implemented client assertion")]
        public async Task JsonWebTokenWithDeveloperImplementedClientAssertionTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                var clientAssertion = new ClientAssertionTestImplementation();
                var context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.TenantSpecificAuthority,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                EmptyX5CMockHandler(httpManager);
                AuthenticationResult result = await context.AcquireTokenAsync(
                    AdalTestConstants.DefaultResource,
                    clientAssertion,
                    true).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                EmptyX5CMockHandler(httpManager);
                result = await context.AcquireTokenByAuthorizationCodeAsync(
                    AdalTestConstants.DefaultAuthorizationCode,
                    AdalTestConstants.DefaultRedirectUri,
                    clientAssertion,
                    AdalTestConstants.DefaultResource,
                    true).ConfigureAwait(false);

                Assert.IsNotNull(result.AccessToken);

                EmptyX5CMockHandler(httpManager);
                result = await context.AcquireTokenAsync(
                    AdalTestConstants.DefaultResource,
                    clientAssertion,
                    new UserAssertion("Access Token"),
                    true).ConfigureAwait(false);

                Assert.IsNotNull(result.AccessToken);
            }
        }
    }
}