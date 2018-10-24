﻿//----------------------------------------------------------------------
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

#if !NET_CORE

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.Core.Unit.Mocks;

namespace Test.MSAL.NET.Unit
{
    [TestClass]
    [DeploymentItem(@"Resources\valid_cert.pfx")]
    public class JsonWebTokenTests
    {
        TokenCache cache;
        private MyReceiver _myReceiver = new MyReceiver();
        readonly MockHttpMessageHandler X5CMockHandler = new MockHttpMessageHandler()
        {
            Method = HttpMethod.Post,
            ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(MsalTestConstants.Scope.AsSingleString(),
                    MockHelpers.CreateIdToken(MsalTestConstants.UniqueId, MsalTestConstants.DisplayableId),
                    MockHelpers.CreateClientInfo(MsalTestConstants.Uid, MsalTestConstants.Utid + "more")),
            AdditionalRequestValidation = request =>
            {
                var requestContent = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var formsData = CoreHelpers.ParseKeyValueList(requestContent, '&', true, null);

                // Check presence of client_assertion in request
                Assert.IsTrue(formsData.TryGetValue("client_assertion", out string encodedJwt), "Missing client_assertion from request");

                // Check presence of x5c cert claim. It should exist.
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(encodedJwt);
                var x5c = jsonToken.Header.Where(header => header.Key == "x5c").FirstOrDefault();
                Assert.IsTrue(x5c.Key == "x5c", "x5c should be present");
            }
        };
        readonly MockHttpMessageHandler EmptyX5CMockHandler = new MockHttpMessageHandler()
        {
            Method = HttpMethod.Post,
            ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(MsalTestConstants.Scope.AsSingleString(),
                    MockHelpers.CreateIdToken(MsalTestConstants.UniqueId, MsalTestConstants.DisplayableId),
                    MockHelpers.CreateClientInfo(MsalTestConstants.Uid, MsalTestConstants.Utid + "more")),
            AdditionalRequestValidation = request =>
            {
                var requestContent = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var formsData = CoreHelpers.ParseKeyValueList(requestContent, '&', true, null);

                // Check presence of client_assertion in request
                Assert.IsTrue(formsData.TryGetValue("client_assertion", out string encodedJwt), "Missing client_assertion from request");

                // Check presence of x5c cert claim. It should not exist.
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(encodedJwt);
                var x5c = jsonToken.Header.Where(header => header.Key == "x5c").FirstOrDefault();
                Assert.IsTrue(x5c.Key != "x5c", "x5c should not be present");
            }
        };

        [TestInitialize]
        public void TestInitialize()
        {
            cache = new TokenCache();
            Authority.ValidatedAuthorities.Clear();
            Telemetry.GetInstance().RegisterReceiver(_myReceiver.OnEvents);

            AadInstanceDiscovery.Instance.Cache.Clear();
        }

        internal void SetupMocks(MockHttpManager httpManager)
        {
            httpManager.AddInstanceDiscoveryMockHandler();
            httpManager.AddMockHandlerForTenantEndpointDiscovery(MsalTestConstants.AuthorityHomeTenant);
        }

        [TestMethod]
        [Description("Test for client assertion with X509 public certificate using sendCertificate")]
        public async Task JsonWebTokenWithX509PublicCertSendCertificateTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                SetupMocks(httpManager);
                var certificate = new X509Certificate2("valid_cert.pfx", MsalTestConstants.DefaultPassword);
                var clientAssertion = new ClientAssertionCertificate(certificate);
                var clientCredential = new ClientCredential(clientAssertion);
                var app = new ConfidentialClientApplication(
                    httpManager,
                    MsalTestConstants.ClientId,
                    ClientApplicationBase.DefaultAuthority,
                    MsalTestConstants.RedirectUri,
                    clientCredential,
                    cache,
                    cache)
                {
                    ValidateAuthority = false
                };

                //Check for x5c claim
                httpManager.AddMockHandler(X5CMockHandler);
                AuthenticationResult result =
                    await (app as IConfidentialClientApplicationWithCertificate).AcquireTokenForClientWithCertificateAsync(
                        MsalTestConstants.Scope).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                //Check for empty x5c claim
                // TODO: this was in the test before,
                // but this mock is not being called.
                // test was NOT validating that all mock queues were empty before...
                // httpManager.AddMockHandler(EmptyX5CMockHandler);

                result = await app.AcquireTokenForClientAsync(MsalTestConstants.Scope).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);
            }
        }

        [TestMethod]
        [Description("Test for client assertion with X509 public certificate using sendCertificate")]
        public async Task JsonWebTokenWithX509PublicCertSendCertificateOnBehalfOfTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                SetupMocks(httpManager);

                var certificate = new X509Certificate2("valid_cert.pfx", MsalTestConstants.DefaultPassword);
                var clientAssertion = new ClientAssertionCertificate(certificate);
                var clientCredential = new ClientCredential(clientAssertion);
                var app = new ConfidentialClientApplication(
                    httpManager,
                    MsalTestConstants.ClientId,
                    ClientApplicationBase.DefaultAuthority,
                    MsalTestConstants.RedirectUri,
                    clientCredential,
                    cache,
                    cache)
                {
                    ValidateAuthority = false
                };
                var userAssertion = new UserAssertion(MsalTestConstants.DefaultAccessToken);

                //Check for x5c claim
                httpManager.AddMockHandler(X5CMockHandler);
                AuthenticationResult result =
                    await (app as IConfidentialClientApplicationWithCertificate).AcquireTokenOnBehalfOfWithCertificateAsync(
                        MsalTestConstants.Scope,
                        userAssertion).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);

                //Check for empty x5c claim
                // TODO: this was in the test before,
                // but this mock is not being called.
                // test was NOT validating that all mock queues were empty before...
                // httpManager.AddMockHandler(EmptyX5CMockHandler);
                result = await app.AcquireTokenOnBehalfOfAsync(MsalTestConstants.Scope, userAssertion).ConfigureAwait(false);
                Assert.IsNotNull(result.AccessToken);
            }
        }
    }
}
#endif