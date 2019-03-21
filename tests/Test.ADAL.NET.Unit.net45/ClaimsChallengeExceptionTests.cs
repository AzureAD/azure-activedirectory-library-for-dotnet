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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Test.ADAL.NET.Common.Mocks;
using Test.ADAL.NET.Common;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Test.Microsoft.Identity.Core.Unit;
using Microsoft.Identity.Core;
using Microsoft.Identity.Test.Common.Core.Mocks;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class ClaimsChallengeExceptionTests
    {
        public static string claims = "{\\\"access_token\\\":{\\\"polids\\\":{\\\"essential\\\":true,\\\"values\\\":[\\\"5ce770ea-8690-4747-aa73-c5b3cd509cd4\\\"]}}}";

        public static string responseContent = "{\"error\":\"interaction_required\",\"claims\":\"" + claims + "\"}";

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
        [Description("Test for inner exception in claims challenge exception")]
        public void InnerExceptionIncludedWithAdalClaimsChallengeExceptionTest()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                var context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                var credential = new ClientCredential(AdalTestConstants.DefaultClientId, AdalTestConstants.DefaultClientSecret);

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(responseContent)
                    }
                });

                var result = AssertException.TaskThrows<AdalClaimChallengeException>(() =>
                context.AcquireTokenAsync(AdalTestConstants.DefaultResource, credential));

                // Check inner exception
                Assert.AreEqual("Response status code does not indicate success: 400 (BadRequest).", result.InnerException.Message);
                Assert.AreEqual("interaction_required", result.ErrorCode);
                Assert.AreEqual(claims.Replace("\\", ""), result.Claims);
            }
        }

        [TestMethod]
        [Description("Test for claims challenge exception with client credential")]
        public void AdalClaimsChallengeExceptionThrownWithAcquireTokenClientCredentialWhenClaimsChallengeRequiredTest()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                var context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                var credential = new ClientCredential(AdalTestConstants.DefaultClientId, AdalTestConstants.DefaultClientSecret);

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(responseContent)
                    }
                });

                var result = AssertException.TaskThrows<AdalClaimChallengeException>(() =>
                context.AcquireTokenAsync(AdalTestConstants.DefaultResource, credential));
                Assert.AreEqual(claims.Replace("\\", ""), result.Claims);
            }
        }

        [TestMethod]
        [Description("Test for claims challenge exception with client credential and user assertion")]
        public void AdalClaimsChallengeExceptionThrownWithAcquireTokenClientCredentialUserAssertionWhenClaimsChallengeRequiredTest()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                var context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                var credential = new ClientCredential(AdalTestConstants.DefaultClientId, AdalTestConstants.DefaultClientSecret);
                string accessToken = "some-access-token";

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(responseContent)
                    }
                });

                var result = AssertException.TaskThrows<AdalClaimChallengeException>(() =>
                context.AcquireTokenAsync(AdalTestConstants.DefaultResource, credential, new UserAssertion(accessToken)));
                Assert.AreEqual(claims.Replace("\\", ""), result.Claims);
            }
        }

        [TestMethod]
        [Description("Test for claims challenge exception with client assertion")]
        public void AdalClaimsChallengeExceptionThrownWithClientAssertionWhenClaimsChallengeRequiredTest()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                var certificate = new X509Certificate2(
                ResourceHelper.GetTestResourceRelativePath("valid_cert.pfx"),
                AdalTestConstants.DefaultPassword);
                var clientAssertion = new ClientAssertionCertificate(AdalTestConstants.DefaultClientId, certificate);
                var context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(responseContent)
                    }
                });

                var result = AssertException.TaskThrows<AdalClaimChallengeException>(() =>
                context.AcquireTokenAsync(AdalTestConstants.DefaultResource, clientAssertion));
                Assert.AreEqual(claims.Replace("\\", ""), result.Claims);
            }
        }

        [TestMethod]
        [Description("Test for claims challenge exception with auth code")]
        public void AdalClaimsChallengeExceptionThrownWithAuthCodeWhenClaimsChallengeRequiredTest()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                SetupMocks(httpManager);

                var context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.NotProvided,
                    new TokenCache());

                ClientAssertion clientAssertion = new ClientAssertion(AdalTestConstants.DefaultClientId, "some-assertion");

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(responseContent)
                    }
                });

                var result = AssertException.TaskThrows<AdalClaimChallengeException>(() =>
                context.AcquireTokenByAuthorizationCodeAsync("some-code", AdalTestConstants.DefaultRedirectUri, clientAssertion, AdalTestConstants.DefaultResource));
                Assert.AreEqual(claims.Replace("\\", ""), result.Claims);
            }
        }
    }
}
