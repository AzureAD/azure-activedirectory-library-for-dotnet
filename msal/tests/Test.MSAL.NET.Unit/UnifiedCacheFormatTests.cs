using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.Instance;
using Microsoft.Identity.Core.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Test.Microsoft.Identity.Core.Unit.Mocks;
using Test.MSAL.NET.Unit.Mocks;

namespace Test.MSAL.NET.Unit
{
    [TestClass]
    [DeploymentItem(@"Resources\AADTestData.txt")]
    [DeploymentItem(@"Resources\MSATestData.txt")]
    [DeploymentItem(@"Resources\B2CNoTenantIdTestData.txt")]
    [DeploymentItem(@"Resources\B2CWithTenantIdTestData.txt")]
    public class UnifiedCacheFormatTests
    {
        private void TestInitialize(MockHttpManager httpManager)
        {
            MyReceiver _myReceiver = new MyReceiver();

            ModuleInitializer.ForceModuleInitializationTestOnly();
            Authority.ValidatedAuthorities.Clear();
            Telemetry.GetInstance().RegisterReceiver(_myReceiver.OnEvents);

            AadInstanceDiscovery.Instance.Cache.Clear();

            httpManager.AddMockHandler(
                MockHelpers.CreateInstanceDiscoveryMockHandler(
                    TestConstants.GetDiscoveryEndpoint(TestConstants.AuthorityCommonTenant)));
        }

        private string ClientId;
        private string RequestAuthority;

        private string TokenResponse;
        private string IdTokenResponse;

        private string ExpectedAtCacheKey;
        private string ExpectedAtCacheValue;

        private string ExpectedIdTokenCacheKey;
        private string ExpectedIdTokenCacheValue;

        private string ExpectedRtCacheKey;
        private string ExpectedRtCacheValue;

        private string ExpectedAccountCacheKey;
        private string ExpectedAccountCacheValue;

        private readonly RequestContext requestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null));

        private void IntitTestData(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                var configJson = JsonConvert.DeserializeObject<JObject>(json);

                ClientId = configJson.GetValue("client_id").ToString();
                RequestAuthority = configJson.GetValue("authority").ToString();

                TokenResponse = configJson.GetValue("token_response").ToString();
                IdTokenResponse = configJson.GetValue("id_token_response").ToString();

                ExpectedAtCacheKey = configJson.GetValue("at_cache_key").ToString();
                ExpectedAtCacheValue = configJson.GetValue("at_cache_value").ToString();

                ExpectedIdTokenCacheKey = configJson.GetValue("id_token_cache_key").ToString();
                ExpectedIdTokenCacheValue = configJson.GetValue("id_token_cache_value").ToString();

                ExpectedRtCacheKey = configJson.GetValue("rt_cache_key").ToString();
                ExpectedRtCacheValue = configJson.GetValue("rt_cache_value").ToString();

                ExpectedAccountCacheKey = configJson.GetValue("account_cache_key").ToString();
                ExpectedAccountCacheValue = configJson.GetValue("account_cache_value").ToString();

                var idTokenSecret = CreateIdToken(IdTokenResponse);

                TokenResponse = string.Format
                    (CultureInfo.InvariantCulture, "{" + TokenResponse + "}", idTokenSecret);

                ExpectedIdTokenCacheValue = string.Format
                    (CultureInfo.InvariantCulture, "{" + ExpectedIdTokenCacheValue + "}", idTokenSecret);
            }
        }

        public static string CreateIdToken(string idToken)
        {
            return string.Format
                (CultureInfo.InvariantCulture, "someheader.{0}.somesignature", Base64UrlHelpers.Encode(idToken));
        }

        [TestMethod]
        [Description("Test unified token cache")]
        public void AAD_CacheFormatValidationTest()
        {
            IntitTestData("AADTestData.txt");
            RunCacheFormatValidation();
        }

        [TestMethod]
        [Description("Test unified token cache")]
        public void MSA_CacheFormatValidationTest()
        {
            IntitTestData("MSATestData.txt");
            RunCacheFormatValidation();
        }

        [TestMethod]
        [Description("Test unified token cache")]
        public void B2C_NoTenantId_CacheFormatValidationTest()
        {
            IntitTestData("B2CNoTenantIdTestData.txt");
            RunCacheFormatValidation();
        }

        [TestMethod]
        [Description("Test unified token cache")]
        public void B2C_WithTenantId_CacheFormatValidationTest()
        {
            IntitTestData("B2CWithTenantIdTestData.txt");
            RunCacheFormatValidation();
        }

        public void RunCacheFormatValidation()
        {
            using (var httpManager = new MockHttpManager())
            {
                TestInitialize(httpManager);

                PublicClientApplication app = new PublicClientApplication(httpManager, ClientId, RequestAuthority);
                MockWebUI ui = new MockWebUI()
                {
                    MockResult = new AuthorizationResult(AuthorizationStatus.Success,
                        TestConstants.AuthorityHomeTenant + "?code=some-code")
                };
                MsalMockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                    app.RedirectUri + "?code=some-code"));

                //add mock response for tenant endpoint discovery
                httpManager.AddMockHandler(new MockHttpMessageHandler
                {
                    Method = HttpMethod.Get,
                    ResponseMessage = MockHelpers.CreateOpenIdConfigurationResponse(TestConstants.AuthorityHomeTenant)
                });
                httpManager.AddMockHandler(new MockHttpMessageHandler
                {
                    Method = HttpMethod.Post,
                    ResponseMessage = MockHelpers.CreateSuccessResponseMessage(TokenResponse)
                });

                AuthenticationResult result = app.AcquireTokenAsync(TestConstants.Scope).Result;
                Assert.IsNotNull(result);

                ValidateAt(app.UserTokenCache);
                ValidateRt(app.UserTokenCache);
                ValidateIdToken(app.UserTokenCache);
                ValidateAccount(app.UserTokenCache);
            }
        }

        private void ValidateAt(TokenCache cache)
        {
            var atList = cache.GetAllAccessTokenCacheItems(requestContext);
            Assert.IsTrue(atList.Count == 1);

            var actualPayload = JsonConvert.DeserializeObject<JObject>(atList.First());
            var expectedPayload = JsonConvert.DeserializeObject<JObject>(ExpectedAtCacheValue);

            foreach (KeyValuePair<string, JToken> prop in expectedPayload)
            {
                string[] timeProperties = { "extended_expires_on", "expires_on", "cached_at" };

                var propName = prop.Key;
                var expectedPropValue = prop.Value;
                var actualPropValue = actualPayload.GetValue(propName);
                if (timeProperties.Contains(propName))
                {
                    if (!"extended_expires_on".Equals(propName))
                    {
                        Assert.IsTrue(actualPayload.GetValue(propName).Type == JTokenType.String);
                    }
                }
                else
                {
                    Assert.AreEqual(expectedPropValue, actualPropValue);
                }
            }
            var atCacheItem = cache.GetAllAccessTokensForClient(requestContext).First();
            var atCacheItemKeyActual = atCacheItem.GetKey().ToString();
            Assert.AreEqual(ExpectedAtCacheKey, atCacheItemKeyActual);
        }

        private void ValidateRt(TokenCache cache)
        {
            ValidateCacheEntityValue
                (ExpectedRtCacheValue, cache.GetAllRefreshTokenCacheItems(requestContext));

            var rtCacheItem = cache.GetAllRefreshTokensForClient(requestContext).First();
            var rtCacheItemKeyActual = rtCacheItem.GetKey().ToString();

            Assert.AreEqual(ExpectedRtCacheKey, rtCacheItemKeyActual);
        }

        private void ValidateIdToken(TokenCache cache)
        {
            ValidateCacheEntityValue
                (ExpectedIdTokenCacheValue, cache.GetAllIdTokenCacheItems(requestContext));

            var idTokenCacheItem = cache.GetAllIdTokensForClient(requestContext).First();
            var idTokenCacheItemKeyActual = idTokenCacheItem.GetKey().ToString();

            Assert.AreEqual(ExpectedIdTokenCacheKey, idTokenCacheItemKeyActual);
        }

        private void ValidateAccount(TokenCache cache)
        {
            ValidateCacheEntityValue
                (ExpectedAccountCacheValue, cache.GetAllAccountCacheItems(requestContext));

            var accountCacheItem = cache.GetAllAccounts(requestContext).First();
            var accountCacheItemKeyActual = accountCacheItem.GetKey().ToString();

            Assert.AreEqual(ExpectedAccountCacheKey, accountCacheItemKeyActual);
        }

        private void ValidateCacheEntityValue(string expectedEntityValue, ICollection<string> entities)
        {
            Assert.IsTrue(entities.Count == 1);

            var actualPayload = JsonConvert.DeserializeObject<JObject>(entities.First());
            var expectedPayload = JsonConvert.DeserializeObject<JObject>(expectedEntityValue);

            Assert.AreEqual(expectedPayload.Count, actualPayload.Count);

            foreach (KeyValuePair<string, JToken> prop in expectedPayload)
            {
                var propName = prop.Key;
                var expectedPropValue = prop.Value;
                var actualPropValue = actualPayload.GetValue(propName);

                Assert.AreEqual(expectedPropValue, actualPropValue);
            }
        }
    }
}
