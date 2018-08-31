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

using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Test.Microsoft.Identity.Core.Unit;
using Test.Microsoft.Identity.Core.Unit.Mocks;

namespace Test.MSAL.NET.Unit
{
    [TestClass]
    [DeploymentItem("valid_cert.pfx")]
    public class JsonWebTokenTests
    {
        //private PlatformParameters platformParameters;
        TokenCache cache;
        private MyReceiver _myReceiver = new MyReceiver();

        MockHttpMessageHandler X5CMockHandler = new MockHttpMessageHandler()
        {
            Method = HttpMethod.Post,
            ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(TestConstants.Scope.AsSingleString(),
                    MockHelpers.CreateIdToken(TestConstants.UniqueId, TestConstants.DisplayableId),
                    MockHelpers.CreateClientInfo(TestConstants.Uid, TestConstants.Utid + "more")),
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

        MockHttpMessageHandler EmptyX5CMockHandler = new MockHttpMessageHandler()
        {
            Method = HttpMethod.Post,
            ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(TestConstants.Scope.AsSingleString(),
                    MockHelpers.CreateIdToken(TestConstants.UniqueId, TestConstants.DisplayableId),
                    MockHelpers.CreateClientInfo(TestConstants.Uid, TestConstants.Utid + "more")),
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
            ModuleInitializer.ForceModuleInitializationTestOnly();
            cache = new TokenCache();
            Authority.ValidatedAuthorities.Clear();
            HttpClientFactory.ReturnHttpClientForMocks = true;
            HttpMessageHandlerFactory.ClearMockHandlers();
            Telemetry.GetInstance().RegisterReceiver(_myReceiver.OnEvents);

            AadInstanceDiscovery.Instance.Cache.Clear();
            AddMockResponseForInstanceDisovery();

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Get,
                ResponseMessage = MockHelpers.CreateOpenIdConfigurationResponse(TestConstants.AuthorityHomeTenant)
            });
        }

        internal void AddMockResponseForInstanceDisovery()
        {
            HttpMessageHandlerFactory.AddMockHandler(
                MockHelpers.CreateInstanceDiscoveryMockHandler(
                    TestConstants.GetDiscoveryEndpoint(TestConstants.AuthorityCommonTenant)));
        }

        [TestMethod]
        [Description("Test for client assertion with X509 public certificate using sendCertificate")]
        public async Task JsonWebTokenWithX509PublicCertSendCertificateTestAsync()
        {
            var certificate = new X509Certificate2("valid_cert.pfx", TestConstants.DefaultPassword);
            var clientAssertion = new ClientAssertionCertificate(certificate);
            var clientCredential = new ClientCredential(clientAssertion);
            var app = new ConfidentialClientApplication(TestConstants.ClientId, TestConstants.RedirectUri, clientCredential, cache, cache);
            app.ValidateAuthority = false;

            //Check for x5c claim
            HttpMessageHandlerFactory.AddMockHandler(X5CMockHandler);
            AuthenticationResult result = await (app as IConfidentialClientApplicationWithCertificate).AcquireTokenForClientWithCertificateAsync(TestConstants.Scope);
            Assert.IsNotNull(result.AccessToken);

            //Check for empty x5c claim
            HttpMessageHandlerFactory.AddMockHandler(EmptyX5CMockHandler);
            result = await app.AcquireTokenForClientAsync(TestConstants.Scope);
            Assert.IsNotNull(result.AccessToken);
        }

        [TestMethod]
        [Description("Test for client assertion with X509 public certificate using sendCertificate")]
        public async Task JsonWebTokenWithX509PublicCertSendCertificateOnBehalfOfTestAsync()
        {
            var certificate = new X509Certificate2("valid_cert.pfx", TestConstants.DefaultPassword);
            var clientAssertion = new ClientAssertionCertificate(certificate);
            var clientCredential = new ClientCredential(clientAssertion);
            var app = new ConfidentialClientApplication(TestConstants.ClientId, TestConstants.RedirectUri, clientCredential, cache, cache);
            app.ValidateAuthority = false;
            var userAssertion = new UserAssertion(TestConstants.DefaultAccessToken);

            //Check for x5c claim
            HttpMessageHandlerFactory.AddMockHandler(X5CMockHandler);
            AuthenticationResult result = await (app as IConfidentialClientApplicationWithCertificate).AcquireTokenOnBehalfOfWithCertificateAsync(TestConstants.Scope, userAssertion);
            Assert.IsNotNull(result.AccessToken);

            //Check for empty x5c claim
            HttpMessageHandlerFactory.AddMockHandler(EmptyX5CMockHandler);
            result = await app.AcquireTokenOnBehalfOfAsync(TestConstants.Scope, userAssertion);
            Assert.IsNotNull(result.AccessToken);
        }
    }
}