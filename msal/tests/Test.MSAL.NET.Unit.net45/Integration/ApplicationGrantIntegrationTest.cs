using Microsoft.Identity.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Test.MSAL.NET.Unit.net45.Integration
{
    [TestClass]
    public class ApplicationGrantIntegrationTest
    {
        public const string Authority = "";
        public const string ClientId = "";
        public const string RedirectUri = "http://localhost";
        public string[] MsalScopes = { "https://graph.microsoft.com/.default" };
        private string Password = "";

        static ApplicationGrantIntegrationTest()
        {
            ILabService _labService = new LabServiceApi(new KeyVaultSecretsProvider());
            // init password here - read from lab api
        }

        [TestMethod]
        [TestCategory("ApplicationGrant_IntegrationTests")]
        [Ignore]
        public async Task ApplicationGrantIntegrationTestAsync()
        {
            var appCache = new TokenCache();
            var userCache = new TokenCache();

            var confidentialClient = new ConfidentialClientApplication(ClientId, Authority, RedirectUri,
                new ClientCredential(Password), userCache, appCache);

            var res = await confidentialClient.AcquireTokenForClientAsync(MsalScopes).ConfigureAwait(false);

            Assert.IsNotNull(res);
            Assert.IsNotNull(res.AccessToken);
            Assert.IsNull(res.IdToken);
            Assert.IsNull(res.Account);

            // make sure user cache is empty
            Assert.IsTrue(userCache.tokenCacheAccessor.GetAllAccessTokensAsString().Count == 0);
            Assert.IsTrue(userCache.tokenCacheAccessor.GetAllRefreshTokensAsString().Count == 0);
            Assert.IsTrue(userCache.tokenCacheAccessor.GetAllIdTokensAsString().Count == 0);
            Assert.IsTrue(userCache.tokenCacheAccessor.GetAllAccountsAsString().Count == 0);

            // make sure nothing was written to legacy cache
            Assert.IsNull(userCache.legacyCachePersistence.LoadCache());

            // make sure only AT entity was stored in the App msal cache
            Assert.IsTrue(appCache.tokenCacheAccessor.GetAllAccessTokensAsString().Count == 1);
            Assert.IsTrue(appCache.tokenCacheAccessor.GetAllRefreshTokensAsString().Count == 0);
            Assert.IsTrue(appCache.tokenCacheAccessor.GetAllIdTokensAsString().Count == 0);
            Assert.IsTrue(appCache.tokenCacheAccessor.GetAllAccountsAsString().Count == 0);

            Assert.IsNull(appCache.legacyCachePersistence.LoadCache());

            // passing empty password to make sure that AT returned from cache
            confidentialClient = new ConfidentialClientApplication(ClientId, Authority, RedirectUri,
                new ClientCredential("wrong_password"), userCache, appCache);

            res = await confidentialClient.AcquireTokenForClientAsync(MsalScopes).ConfigureAwait(false);

            Assert.IsNotNull(res);
            Assert.IsNotNull(res.AccessToken);
            Assert.IsNull(res.IdToken);
            Assert.IsNull(res.Account);
        }
    }
}
