using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;
using Test.ADAL.NET.Unit;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace Test.ADAL.NET.Integration
{
    [TestClass]
    [DeploymentItem("WsTrustResponse.xml")]
    public class FederatedUserCredentialTests
    {
        [DeploymentItem("WsTrustResponse.xml")]
        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();
            InstanceDiscovery.InstanceCache.Clear();
            HttpMessageHandlerFactory.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler());
        }

        [TestMethod]
        [Description("Positive Test for AcquireToken with empty cache")]
        public async Task AcquireTokenWithEmptyCachePositiveTestAsync()
        {
            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            await context.Authenticator.UpdateFromTemplateAsync(null);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"ver\":\"1.0\",\"account_type\":\"Federated\",\"domain_name\":\"microsoft.com\"," +
                                                "\"federation_protocol\":\"WSTrust\",\"federation_metadata_url\":" +
                                                "\"https://msft.sts.microsoft.com/adfs/services/trust/mex\"," +
                                                "\"federation_active_auth_url\":\"https://msft.sts.microsoft.com/adfs/services/trust/2005/usernamemixed\"" +
                                                ",\"cloudinstancename\":\"login.microsoftonline.com\"}")
                },
                QueryParams = new Dictionary<string, string>()
                {
                    {"api-version", "1.0"}
                }
            });

            UserRealmDiscoveryResponse userRealmResponse = await UserRealmDiscoveryResponse.CreateByDiscoveryAsync(context.Authenticator.UserRealmUri, TestConstants.DefaultDisplayableId, CallState.Default);

            WsTrustAddress address = new WsTrustAddress()
            {
                Uri = new Uri("https://some/address/usernamemixed"),
                Version = WsTrustVersion.WsTrust13
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("WsTrustResponse.xml"))
                }
            });

            WsTrustResponse wsTrustResponse = await WsTrustRequest.SendRequestAsync(address, new UserPasswordCredential(TestConstants.DefaultDisplayableId, TestConstants.DefaultPassword), null, userRealmResponse.CloudAudienceUrn);
            Assert.IsNotNull(wsTrustResponse.Token);
        }
    }
}

