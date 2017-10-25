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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace Test.ADAL.NET.Integration
{
    [TestClass]
    public class SovereignCloudsTests
    {
        private PlatformParameters platformParameters;

        const string sovereignAuthorityHost = "login.some-sovereign-cloud";

        string sovereignTenantSpesificAuthority = $"https://{sovereignAuthorityHost}/{TestConstants.SomeTenantId}/";

        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();
            platformParameters = new PlatformParameters(PromptBehavior.Auto);
            InstanceDiscovery.InstanceCache.Clear();
            HttpMessageHandlerFactory.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler());
        }

        [TestMethod]
        [Description("Sovereign user use world wide authority")]
        public async Task SovereignUserWorldWideAuthorityIntegrationTest()
        {
            // creating AuthenticationContext with common Authority
            var authenticationContext =
                new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, false, new TokenCache());

            // mock value for authentication returnedUriInput, with cloud_instance_name claim
            var authReturnedUriInputMock = TestConstants.DefaultRedirectUri + "?code=some-code" + "&" +
                                           TokenResponseClaim.CloudInstanceHost + "=" + sovereignAuthorityHost;

            MockHelpers.ConfigureMockWebUI(
                new AuthorizationResult(AuthorizationStatus.Success, authReturnedUriInputMock),
                // validate that authorizationUri passed to WebUi contains instance_aware query parameter
                new Dictionary<string, string> { { "instance_aware", "true" } });

            HttpMessageHandlerFactory.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler());

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                ResponseMessage =
                    MockHelpers.CreateSuccessTokenResponseMessage(TestConstants.DefaultUniqueId,
                    TestConstants.DefaultDisplayableId, TestConstants.DefaultResource),

                AdditionalRequestValidation = request =>
                {
                    // make sure that Sovereign authority was used for Authorization request
                    Assert.AreEqual(sovereignAuthorityHost, request.RequestUri.Authority);
                }
            });

            var authenticationResult = await authenticationContext.AcquireTokenAsync(TestConstants.DefaultResource,
                TestConstants.DefaultClientId,
                TestConstants.DefaultRedirectUri, platformParameters, UserIdentifier.AnyUser, "instance_aware=true");

            // make sure that tenant spesific sovereign Authority returned to the app in AuthenticationResult
            Assert.AreEqual(sovereignTenantSpesificAuthority, authenticationResult.Authority);

            // make sure that AuthenticationContext Authority was updated
            Assert.AreEqual(sovereignTenantSpesificAuthority, authenticationContext.Authority);

            // make sure AT was stored in the cache with tenant spesific Sovereign Authority in the key
            Assert.AreEqual(1, authenticationContext.TokenCache.tokenCacheDictionary.Count);
            Assert.AreEqual(sovereignTenantSpesificAuthority,
                authenticationContext.TokenCache.tokenCacheDictionary.Keys.FirstOrDefault().Authority);

            // all mocks are consumed
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }

        [TestMethod]
        [Description("Instance discovery call is made because authority was not already in the instance cache")]
        public async Task AuthorityNotInInstanceCache_InstanceDiscoverCallMadeTestAsync()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();

            string content = @"{
                            ""tenant_discovery_endpoint"":""https://login.microsoftonline.com/tenant/.well-known/openid-configuration"",
                            ""api-version"":""1.1"",
                            ""metadata"":[{
                                ""preferred_network"":""login.microsoftonline.com"",
                                ""preferred_cache"":""login.windows.net"",
                                ""aliases"":[
                                    ""login.microsoftonline.com"",
                                    ""login.windows.net"",
                                    ""login.microsoft.com"",
                                    ""sts.windows.net""]}]}";

            // German authority not included
            HttpMessageHandlerFactory.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler(content));

            // Assure instance cache is empty
            InstanceDiscovery.InstanceCache.Clear();
            Assert.AreEqual(0, InstanceDiscovery.InstanceCache.Count());

            // Creating AuthenticationContext with common Authority
            var authenticationContext =
                new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, false, new TokenCache());

            // Mock value for authentication returnedUriInput, with cloud_instance_name claim
            var authReturnedUriInputMock = TestConstants.DefaultRedirectUri + "?code=some-code" + "&" +
                                           TokenResponseClaim.CloudInstanceHost + "=" + sovereignAuthorityHost;

            MockHelpers.ConfigureMockWebUI(
                new AuthorizationResult(AuthorizationStatus.Success, authReturnedUriInputMock),
                // validate that authorizationUri passed to WebUi contains instance_aware query parameter
                new Dictionary<string, string> { { "instance_aware", "true" } });

            // German authority not included
            HttpMessageHandlerFactory.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler(content));
            
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                ResponseMessage =
                    MockHelpers.CreateSuccessTokenResponseMessage(TestConstants.DefaultUniqueId,
                    TestConstants.DefaultDisplayableId, TestConstants.DefaultResource),

                AdditionalRequestValidation = request =>
                {
                    // make sure that Sovereign authority was used for Authorization request
                    Assert.AreEqual(sovereignAuthorityHost, request.RequestUri.Authority);
                }
            });

            // German authority not included
            HttpMessageHandlerFactory.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler(content));

            var authenticationResult = await authenticationContext.AcquireTokenAsync(TestConstants.DefaultResource,
                TestConstants.DefaultClientId,
                TestConstants.DefaultRedirectUri, platformParameters, UserIdentifier.AnyUser, "instance_aware=true");

            Assert.AreEqual(5, InstanceDiscovery.InstanceCache.Count());

            // One mock remaining
            Assert.AreEqual(1, HttpMessageHandlerFactory.MockHandlersCount());

            // German authority not included in instance cache
            Assert.AreEqual(false, InstanceDiscovery.InstanceCache.Keys.Contains("login.microsoftonline.de"));

            var entry = await InstanceDiscovery.GetMetadataEntry("login.microsoftonline.de", true, new CallState(Guid.NewGuid())).ConfigureAwait(false);

            // German authority now included in instance cache
            Assert.AreEqual("login.microsoftonline.de", entry.PreferredNetwork);
            Assert.AreEqual(6, InstanceDiscovery.InstanceCache.Count());
            Assert.AreEqual(true, InstanceDiscovery.InstanceCache.Keys.Contains("login.microsoftonline.de"));
            Assert.AreEqual(true, InstanceDiscovery.InstanceCache.Keys.Contains("login.windows.net"));
            Assert.AreEqual(true, InstanceDiscovery.InstanceCache.Keys.Contains("login.some-sovereign-cloud"));
            Assert.AreEqual(false, InstanceDiscovery.InstanceCache.Keys.Contains("login.partner.microsoftonline.cn"));

            // All mocks are consumed
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }
    }
}