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
using PromptBehavior = Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net;
using TokenResponseClaim = Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2.TokenResponseClaim;
using Microsoft.Identity.Core.UI;
using Test.ADAL.NET.Unit;
using System.Globalization;
using Microsoft.Identity.Core;

namespace Test.ADAL.NET.Integration
{
#if !NET_CORE // these tests use UI
    [TestClass]
    public class SovereignCloudsTests
    {
        private IPlatformParameters _platformParameters;
        private const string SovereignAuthorityHost = "login.microsoftonline.de";

        private readonly string _sovereignTenantSpecificAuthority = string.Format(
            CultureInfo.InvariantCulture,
            "https://{0}/{1}/",
            SovereignAuthorityHost,
            AdalTestConstants.SomeTenantId);

        [TestInitialize]
        public void Initialize()
        {
            _platformParameters = new PlatformParameters(PromptBehavior.SelectAccount);

            InstanceDiscovery.InstanceCache.Clear();
        }

        [TestMethod]
        [Description("Sovereign user use world wide authority")]
        public async Task SovereignUserWorldWideAuthorityIntegrationTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                // creating AuthenticationContext with common Authority
                var authenticationContext =
                new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.False,
                    new TokenCache());

                // mock value for authentication returnedUriInput, with cloud_instance_name claim
                var authReturnedUriInputMock = AdalTestConstants.DefaultRedirectUri + "?code=some-code" + "&" +
                                               TokenResponseClaim.CloudInstanceHost + "=" + SovereignAuthorityHost;

                MockHelpers.ConfigureMockWebUI(
                    new AuthorizationResult(AuthorizationStatus.Success, authReturnedUriInputMock),
                    // validate that authorizationUri passed to WebUi contains instance_aware query parameter
                    new Dictionary<string, string> { { "instance_aware", "true" } });

                httpManager.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler(AdalTestConstants.GetDiscoveryEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant)));

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityBlackforestTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage =
                        MockHelpers.CreateSuccessTokenResponseMessage(AdalTestConstants.DefaultUniqueId,
                        AdalTestConstants.DefaultDisplayableId, AdalTestConstants.DefaultResource),

                    AdditionalRequestValidation = request =>
                    {
                        // make sure that Sovereign authority was used for Authorization request
                        Assert.AreEqual(SovereignAuthorityHost, request.RequestUri.Authority);
                    }
                });

                var authenticationResult = await authenticationContext.AcquireTokenAsync(AdalTestConstants.DefaultResource,
                    AdalTestConstants.DefaultClientId,
                    AdalTestConstants.DefaultRedirectUri, _platformParameters, UserIdentifier.AnyUser, "instance_aware=true").ConfigureAwait(false);

                // make sure that tenant specific sovereign Authority returned to the app in AuthenticationResult
                Assert.AreEqual(_sovereignTenantSpecificAuthority, authenticationResult.Authority);

                // make sure that AuthenticationContext Authority was updated
                Assert.AreEqual(_sovereignTenantSpecificAuthority, authenticationContext.Authority);

                // make sure AT was stored in the cache with tenant specific Sovereign Authority in the key
                Assert.AreEqual(1, authenticationContext.TokenCache._tokenCacheDictionary.Count);
                Assert.AreEqual(_sovereignTenantSpecificAuthority,
                    authenticationContext.TokenCache._tokenCacheDictionary.Keys.FirstOrDefault()?.Authority);
            }
        }

        [TestMethod]
        [Description("Instance discovery call is made because authority was not already in the instance cache")]
        public async Task AuthorityNotInInstanceCache_InstanceDiscoverCallMadeTestAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                const string content = @"{
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

                // creating AuthenticationContext with common Authority
                var authenticationContext =
                    new AuthenticationContext(
                        serviceBundle,
                        AdalTestConstants.DefaultAuthorityCommonTenant,
                        AuthorityValidationType.False,
                        new TokenCache());

                // mock value for authentication returnedUriInput, with cloud_instance_name claim
                var authReturnedUriInputMock = AdalTestConstants.DefaultRedirectUri + "?code=some-code" + "&" +
                                               TokenResponseClaim.CloudInstanceHost + "=" + SovereignAuthorityHost;

                MockHelpers.ConfigureMockWebUI(
                    new AuthorizationResult(AuthorizationStatus.Success, authReturnedUriInputMock),
                    // validate that authorizationUri passed to WebUi contains instance_aware query parameter
                    new Dictionary<string, string> { { "instance_aware", "true" } });

                httpManager.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler(AdalTestConstants.GetDiscoveryEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant), content));

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetDiscoveryEndpoint(AdalTestConstants.DefaultAuthorityBlackforestTenant))
                {
                    Method = HttpMethod.Get,
                    ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(content)
                    }
                });

                httpManager.AddMockHandler(new MockHttpMessageHandler(AdalTestConstants.GetTokenEndpoint(AdalTestConstants.DefaultAuthorityBlackforestTenant))
                {
                    Method = HttpMethod.Post,
                    ResponseMessage =
                       MockHelpers.CreateSuccessTokenResponseMessage(AdalTestConstants.DefaultUniqueId,
                       AdalTestConstants.DefaultDisplayableId, AdalTestConstants.DefaultResource)
                });

                // Assure instance cache is empty
                Assert.AreEqual(0, InstanceDiscovery.InstanceCache.Count());

                await authenticationContext.AcquireTokenAsync(AdalTestConstants.DefaultResource,
                    AdalTestConstants.DefaultClientId,
                    AdalTestConstants.DefaultRedirectUri, _platformParameters, UserIdentifier.AnyUser, "instance_aware=true").ConfigureAwait(false);

                // make sure AT was stored in the cache with tenant specific Sovereign Authority in the key
                Assert.AreEqual(1, authenticationContext.TokenCache._tokenCacheDictionary.Count);
                Assert.AreEqual(_sovereignTenantSpecificAuthority,
                    authenticationContext.TokenCache._tokenCacheDictionary.Keys.FirstOrDefault()?.Authority);

                // DE cloud authority now included in instance cache
                Assert.AreEqual(5, InstanceDiscovery.InstanceCache.Count());
                Assert.AreEqual(true, InstanceDiscovery.InstanceCache.Keys.Contains("login.microsoftonline.de"));
                Assert.AreEqual(true, InstanceDiscovery.InstanceCache.Keys.Contains("login.windows.net"));
                Assert.AreEqual(false, InstanceDiscovery.InstanceCache.Keys.Contains("login.partner.microsoftonline.cn"));
            }
        }
    }
#endif
}