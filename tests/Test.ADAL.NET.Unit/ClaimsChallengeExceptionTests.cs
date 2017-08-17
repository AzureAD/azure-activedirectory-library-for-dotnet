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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.Common;
using Test.ADAL.NET.Unit.Mocks;
using System.Security.Cryptography.X509Certificates;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class ClaimsChallengeExceptionTests
    {
        private readonly MockHttpWebRequestFactory _mockFactory = new MockHttpWebRequestFactory();

        public static string claims = "{\\\"access_token\\\":{\\\"polids\\\":{\\\"essential\\\":true,\\\"values\\\":[\\\"5ce770ea-8690-4747-aa73-c5b3cd509cd4\\\"]}}}";

        public static string responseContent = "{\"error\":\"interaction_required\",\"claims\":\"" + claims + "\"}";

        [TestInitialize]
        public void Initialize()
        {
            NetworkPlugin.HttpWebRequestFactory = _mockFactory;
        }

        [TestMethod]
        [TestCategory("ClaimsChallengeExceptionTests")]
        [Description("Test for claims challenge exception with client credential")]
        public async Task AdalClaimsChallengeExceptionThrownWithAcquireTokenClientCredentialWhenClaimsChallengeRequiredTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
            TestConstants.DefaultClientSecret);

            _mockFactory.MockHttpWebRequestQueue.Enqueue(
               new MockHttpWebRequest(MockHelpers.CreateClaimsChallengeAndInteractionRequiredResponseMessage()));
            try
            {
               var result = await context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential);
            }
            catch (AdalClaimsChallengeException ex)
            {
                Assert.AreEqual(claims.Replace("\\",""), ex.Claims);
                return;
            }

            Assert.Fail("Expected exception was not thrown");            
        }

        [TestMethod]
        [TestCategory("ClaimsChallengeExceptionTests")]
        [Description("Test for claims challenge exception with client credential and user assertion")]
        public async Task AdalClaimsChallengeExceptionThrownWithAcquireTokenClientCredentialUserAssertionWhenClaimsChallengeRequiredTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, new TokenCache());
            ClientCredential clientCredential = new ClientCredential(TestConstants.DefaultClientId,
            TestConstants.DefaultClientSecret);
            string accessToken = "some-access-token";

            _mockFactory.MockHttpWebRequestQueue.Enqueue(
               new MockHttpWebRequest(MockHelpers.CreateClaimsChallengeAndInteractionRequiredResponseMessage()));
            try
            {
                var result = await context.AcquireTokenAsync(TestConstants.DefaultResource, clientCredential, new UserAssertion(accessToken));
            }
            catch (AdalClaimsChallengeException ex)
            {
                Assert.AreEqual(claims.Replace("\\", ""), ex.Claims);
                return;
            }

            Assert.Fail("Expected exception was not thrown");
        }

        [TestMethod]
        [TestCategory("ClaimsChallengeExceptionTests")]
        [Description("Test for claims challenge exception with client assertion")]
        public async Task AdalClaimsChallengeExceptionThrownWithClientAssertionWhenClaimsChallengeRequiredTestAsync()
        {
            var certificate = new X509Certificate2("valid_cert.pfx", TestConstants.DefaultPassword);
            var clientAssertion = new ClientAssertionCertificate(TestConstants.DefaultClientId, certificate);
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());

            _mockFactory.MockHttpWebRequestQueue.Enqueue(
               new MockHttpWebRequest(MockHelpers.CreateClaimsChallengeAndInteractionRequiredResponseMessage()));
            try
            {
                var result = await context.AcquireTokenAsync(TestConstants.DefaultResource, clientAssertion);
            }
            catch (AdalClaimsChallengeException ex)
            {
                Assert.AreEqual(claims.Replace("\\", ""), ex.Claims);
                return;
            }

            Assert.Fail("Expected exception was not thrown");
        }

        [TestMethod]
        [TestCategory("ClaimsChallengeExceptionTests")]
        [Description("Test for claims challenge exception with auth code")]
        public async Task AdalClaimsChallengeExceptionThrownWithAuthCodeWhenClaimsChallengeRequiredTestAsync()
        {
            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            ClientAssertion clientAssertion = new ClientAssertion(TestConstants.DefaultClientId, "some-assertion");

            _mockFactory.MockHttpWebRequestQueue.Enqueue(
               new MockHttpWebRequest(MockHelpers.CreateClaimsChallengeAndInteractionRequiredResponseMessage()));
            try
            {
                var result = await context.AcquireTokenByAuthorizationCodeAsync("some-code", TestConstants.DefaultRedirectUri, clientAssertion, TestConstants.DefaultResource);
            }
            catch (AdalClaimsChallengeException ex)
            {
                Assert.AreEqual(claims.Replace("\\", ""), ex.Claims);
                return;
            }

            Assert.Fail("Expected exception was not thrown");
        }
    }
}
