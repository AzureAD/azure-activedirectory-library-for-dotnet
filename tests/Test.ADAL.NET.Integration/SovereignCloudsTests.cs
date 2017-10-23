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
using System.Net;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class SovereignCloudsTests
    {
        private PlatformParameters platformParameters;

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
            const string sovereignAuthorityHost = "login.some-sovereign-cloud";

            var sovereignTenantSpesificAuthority = $"https://{sovereignAuthorityHost}/{TestConstants.SomeTenantId}/";

            // creating AuthenticationContext with common Authority
            var authenticationContext =
                new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, false, new TokenCache());

            // mock value for authentication returnedUriInput, with cloud_instance_name claim
            var authReturnedUriInputMock = TestConstants.DefaultRedirectUri + "?code=some-code" + "&" +
                                           TokenResponseClaim.CloudInstanceHost + "=" + sovereignAuthorityHost;

            MockHelpers.ConfigureMockWebUI(
                new AuthorizationResult(AuthorizationStatus.Success, authReturnedUriInputMock),
                // validate that authorizationUri passed to WebUi contains instance_aware query parameter
                new Dictionary<string, string> {{"instance_aware", "true"}});

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        @"{  
                            ""tenant_discovery_endpoint"":""https://login.microsoftonline.com/v1/.well-known/openid-configuration"",
                            ""api-version"":""1.1"",
                            ""metadata"":[
                                {
                                ""preferred_network"":""login.microsoftonline.com"",
                                ""preferred_cache"":""login.windows.net"",
                                ""aliases"":[
                                    ""login.microsoftonline.com"",
                                    ""login.windows.net"",
                                    ""login.microsoft.com"",
                                    ""sts.windows.net""]},
                                {
                                ""preferred_network"":""login.partner.microsoftonline.cn"",
                                ""preferred_cache"":""login.partner.microsoftonline.cn"",
                                ""aliases"":[
                                    ""login.partner.microsoftonline.cn"",
                                    ""login.chinacloudapi.cn""]},
                                {
                                ""preferred_network"":""login.microsoftonline.de"",
                                ""preferred_cache"":""login.microsoftonline.de"",
                                ""aliases"":[
                                     ""login.microsoftonline.de""]},
                                {  
                                ""preferred_network"":""login.microsoftonline.us"",
                                ""preferred_cache"":""login.microsoftonline.us"",
                                ""aliases"":[
                                    ""login.microsoftonline.us"",
                                    ""login.usgovcloudapi.net""]},
                                {  
                                ""preferred_network"":""login -us.microsoftonline.com"",
                                ""preferred_cache"":""login -us.microsoftonline.com"",
                                ""aliases"":[
                                    ""login -us.microsoftonline.com""]}]}"
                    )
                }
            });

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
    }
}
