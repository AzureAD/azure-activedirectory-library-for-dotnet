using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Cache;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.ClientCreds;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Flows;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.NET.Common;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class BrokerParametersTests
    {
        private const string Authority = "https://login.microsoftonline.com/test";
        private static readonly string CanonicalizedAuthority = Authenticator.CanonicalizeUri(Authority);

        private const string ExtraQueryParameters = "testQueryParameters";
        private const string Claims = "testClaims";
        private const string Resource = "testResource";

        private const string ClientId = "testClientId";
        private const string ClientSecret = "testClientSecret";

        private const string UniqueUserId = "testUniqueUserId";

        private readonly RequestData _requestData = new RequestData
        {
            Authenticator = new Authenticator(Authority, false),
            Resource = Resource,
            ClientKey = new ClientKey(new ClientCredential(ClientId, ClientSecret)),
            SubjectType = TokenSubjectType.Client,
            ExtendedLifeTimeEnabled = false
        };

        [TestMethod]
        [Description("Test setting of brokerParameters by AcquireTokenInteractiveHandler constructor")]
        public void AcquireTokenInteractiveHandlerConstructor_InitializeBrokerParameters()
        {
            var acquireTokenInteractiveHandler = new AcquireTokenInteractiveHandler(_requestData,
                TestConstants.DefaultRedirectUri, null, UserIdentifier.AnyUser,
                ExtraQueryParameters, null, Claims);

            Assert.AreEqual(11, acquireTokenInteractiveHandler.brokerParameters.Count);

            var brokerParams = acquireTokenInteractiveHandler.brokerParameters;

            Assert.AreEqual(CanonicalizedAuthority, brokerParams["authority"]);
            Assert.AreEqual(Resource, brokerParams["resource"]);
            Assert.AreEqual(ClientId, brokerParams["client_id"]);

            Assert.AreEqual(acquireTokenInteractiveHandler.CallState.CorrelationId.ToString(), brokerParams["correlation_id"]);
            Assert.AreEqual(AdalIdHelper.GetAdalVersion(), brokerParams["client_version"]);
            Assert.AreEqual("NO", brokerParams["force"]);
            Assert.AreEqual(string.Empty, brokerParams["username"]);
            Assert.AreEqual(UserIdentifierType.OptionalDisplayableId.ToString(), brokerParams["username_type"]);

            Assert.AreEqual(TestConstants.DefaultRedirectUri, brokerParams["redirect_uri"]);

            Assert.AreEqual(ExtraQueryParameters, brokerParams["extra_qp"]);
            Assert.AreEqual(Claims, brokerParams["claims"]);
        }

        [TestMethod]
        [Description("Test setting of brokerParameters by AcquireTokenSilentHandler constructor")]
        public void AcquireTokenSilentHandlerConstructor_InitializeBrokerParameters()
        {
            var acquireTokenSilentHandler = new AcquireTokenSilentHandler(_requestData, new UserIdentifier(UniqueUserId, UserIdentifierType.UniqueId), null);

            Assert.AreEqual(8, acquireTokenSilentHandler.brokerParameters.Count);

            var brokerParams = acquireTokenSilentHandler.brokerParameters;

            Assert.AreEqual(CanonicalizedAuthority, brokerParams["authority"]);
            Assert.AreEqual(Resource, brokerParams["resource"]);
            Assert.AreEqual(ClientId, brokerParams["client_id"]);
            Assert.AreEqual(acquireTokenSilentHandler.CallState.CorrelationId.ToString(), brokerParams["correlation_id"]);
            Assert.AreEqual(AdalIdHelper.GetAdalVersion(), brokerParams["client_version"]);
            Assert.AreEqual(UniqueUserId, brokerParams["username"]);
            Assert.AreEqual(UserIdentifierType.UniqueId.ToString(), brokerParams["username_type"]);

            Assert.IsTrue(brokerParams.ContainsKey("silent_broker_flow"));
        }
    }
}
