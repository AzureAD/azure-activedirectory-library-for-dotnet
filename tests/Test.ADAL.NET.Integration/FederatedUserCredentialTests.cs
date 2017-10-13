using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Cache;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Test.ADAL.Common;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace Test.ADAL.NET.Integration
{
    [TestClass]
    [DeploymentItem("TestMex.xml")]
    [DeploymentItem("WsTrustResponse.xml")]
    [DeploymentItem("WsTrustResponse13.xml")]
    public class FederatedUserCredentialTests
    {
        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();
            InstanceDiscovery.InstanceCache.Clear();
            HttpMessageHandlerFactory.AddMockHandler(MockHelpers.CreateInstanceDiscoveryMockHandler());
        }

        [TestMethod]
        [Description("Test for AcquireToken with empty cache")]
        public async Task AcquireTokenWithEmptyCache_GetsTokenFromServiceTestAsync()
        {
            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            await context.Authenticator.UpdateFromTemplateAsync(null);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"ver\":\"1.0\",\"account_type\":\"federated\",\"domain_name\":\"microsoft.com\"," +
                                                "\"federation_protocol\":\"WSTrust\",\"federation_metadata_url\":" +
                                                "\"https://msft.sts.microsoft.com/adfs/services/trust/mex\"," +
                                                "\"federation_active_auth_url\":\"https://msft.sts.microsoft.com/adfs/services/trust/2005/usernamemixed\"" +
                                                ",\"cloud_instance_name\":\"login.microsoftonline.com\"}")
                },
                QueryParams = new Dictionary<string, string>()
                {
                    {"api-version", "1.0"}
                }
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://msft.sts.microsoft.com/adfs/services/trust/mex")
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("TestMex.xml"))
                }
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://msft.sts.microsoft.com/adfs/services/trust/2005/usernamemixed")
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("WsTrustResponse.xml"))
                }
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://login.microsoftonline.com/common/oauth2/token")
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"grant_type", "urn:ietf:params:oauth:grant-type:saml1_1-bearer"},
                    {"scope", "openid"}
                }
            });

            // Call acquire token
            AuthenticationResult result = await context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                new UserPasswordCredential(TestConstants.DefaultDisplayableId, TestConstants.DefaultPassword));

            Assert.IsNotNull(result);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            // All mocks are consumed
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }

        [TestMethod]
        [Description("Provide only username of Federated user. Result in integrated auth call with no password xml tag.")]
        public async Task ProvideUsernameOfFederatedUser_NoPasswordXmlTagTestAsync()
        {
            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            await context.Authenticator.UpdateFromTemplateAsync(null);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"ver\":\"1.0\",\"account_type\":\"federated\",\"domain_name\":\"microsoft.com\"," +
                                                "\"federation_protocol\":\"WSTrust\",\"federation_metadata_url\":" +
                                                "\"https://msft.sts.microsoft.com/adfs/services/trust/mex\"," +
                                                "\"federation_active_auth_url\":\"https://msft.sts.microsoft.com/adfs/services/trust/2005/usernamemixed\"" +
                                                ",\"cloud_instance_name\":\"login.microsoftonline.com\"}")
                },
                QueryParams = new Dictionary<string, string>()
                {
                    {"api-version", "1.0"}
                }
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://msft.sts.microsoft.com/adfs/services/trust/mex")
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("TestMex.xml"))
                }
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://msft.sts.microsoft.com/adfs/services/trust/13/windowstransport")
            {
                Method = HttpMethod.Post,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText("WsTrustResponse13.xml"))
                }
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://login.microsoftonline.com/common/oauth2/token")
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"grant_type", "urn:ietf:params:oauth:grant-type:saml1_1-bearer"},
                    {"scope", "openid"}
                }
            });

            // Call acquire token
            AuthenticationResult result = await context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                new UserCredential(TestConstants.DefaultDisplayableId));

            Assert.IsNotNull(result);
            Assert.AreEqual("some-access-token", result.AccessToken);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);

            // All mocks are consumed
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }


        [TestMethod]
        [Description("No password provided and Mex does not return integrated auth endpoint.")]
        public async Task NoPasswordAndMexEndpointFailsToResolveTestAsync()
        {
            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            await context.Authenticator.UpdateFromTemplateAsync(null);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"ver\":\"1.0\",\"account_type\":\"federated\",\"domain_name\":\"microsoft.com\"," +
                                                "\"federation_protocol\":\"WSTrust\",\"federation_metadata_url\":" +
                                                "\"https://msft.sts.microsoft.com/adfs/services/trust/mex\"," +
                                                "\"federation_active_auth_url\":\"https://msft.sts.microsoft.com/adfs/services/trust/2005/usernamemixed\"" +
                                                ",\"cloud_instance_name\":\"login.microsoftonline.com\"}")
                },
                QueryParams = new Dictionary<string, string>()
                {
                    {"api-version", "1.0"}
                }
            });

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://msft.sts.microsoft.com/adfs/services/trust/mex")
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(File.ReadAllText("TestMex.xml"))
                }
            });

            string emptyPassword = null;
            UserPasswordCredential userPasswordCredential = new UserPasswordCredential(TestConstants.DefaultDisplayableId, emptyPassword);

            // Call acquire token
            var result = AssertException.TaskThrows<AdalException>(() =>
            context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                userPasswordCredential));

            // Check inner exception
            Assert.AreEqual(" Response status code does not indicate success: 404 (NotFound).", result.InnerException.Message);

            // All mocks are consumed
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }

        [TestMethod]
        [Description("Test for AcquireToken with valid token in cache")]
        public async Task AcquireTokenWithValidTokenInCache_ReturnsCachedTokenTestAsync()
        {
            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            await context.Authenticator.UpdateFromTemplateAsync(null);

            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityCommonTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "existing-access-token",
                    DateTimeOffset.UtcNow + TimeSpan.FromMinutes(100))
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://login.microsoftonline.com/common/oauth2/token")
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage(),
                PostData = new Dictionary<string, string>()
                {
                    {"resource", TestConstants.DefaultResource},
                    {"client_id", TestConstants.DefaultClientId},
                    {"grant_type", "refresh_token"},
                    {"scope", "openid"}
                }
            });

            // Call acquire token
            AuthenticationResult result = await context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                new UserPasswordCredential(TestConstants.DefaultDisplayableId, TestConstants.DefaultPassword));

            Assert.IsNotNull(result);
            Assert.AreEqual("existing-access-token", result.AccessToken);

            // There should be one cached entry.
            Assert.AreEqual(1, context.TokenCache.Count);
            Assert.IsNotNull(result.UserInfo);
        }

        [TestMethod]
        [Description("Mex endpoint fails to resolve and results in a 404")]
        public async Task MexEndpointFailsToResolveTestAsync()
        {
            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            await context.Authenticator.UpdateFromTemplateAsync(null);

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"ver\":\"1.0\",\"account_type\":\"federated\",\"domain_name\":\"microsoft.com\"," +
                                                "\"federation_protocol\":\"WSTrust\",\"federation_metadata_url\":" +
                                                "\"https://msft.sts.microsoft.com/adfs/services/trust/mex\"," +
                                                "\"federation_active_auth_url\":\"https://msft.sts.microsoft.com/adfs/services/trust/2005/usernamemixed\"" +
                                                ",\"cloud_instance_name\":\"login.microsoftonline.com\"}")
                },
                QueryParams = new Dictionary<string, string>()
                {
                    {"api-version", "1.0"}
                }
            });

            // Mock for Mex endpoint fails to resolve and results in 404
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler("https://msft.sts.microsoft.com/adfs/services/trust/mex")
            {
                Method = HttpMethod.Get,
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(File.ReadAllText("TestMex.xml"))
                }
            });

            // Call acquire token
            var result = AssertException.TaskThrows<AdalException>(() =>
            context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                new UserPasswordCredential(TestConstants.DefaultDisplayableId, TestConstants.DefaultPassword)));

            // Check inner exception
            Assert.AreEqual(" Response status code does not indicate success: 404 (NotFound).", result.InnerException.Message);

            // All mocks are consumed
            Assert.AreEqual(0, HttpMessageHandlerFactory.MockHandlersCount());
        }
    }
}