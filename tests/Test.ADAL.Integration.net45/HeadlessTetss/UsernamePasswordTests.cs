using Microsoft.Identity.Test.LabInfrastructure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Test.ADAL.NET.Common;

namespace Test.ADAL.Integration.net45.HeadlessTetss
{
    [TestClass]
    public class UsernamePasswordTests
    {
        #region MSTest Hooks
        /// <summary>
        /// Initialized by MSTest (do not make private or readonly)
        /// </summary>
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        #endregion

        [TestMethod]
        public async Task ROPC_DefaultUserAsync()
        {
            // Arrange
            LabResponse labResponse = LabUserHelper.GetDefaultUser();
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV3_NotFederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV4,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = false
            };

            LabResponse labResponse = LabUserHelper.GetLabUserData(query);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV3_FederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV4,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = true
            };

            LabResponse labResponse = LabUserHelper.GetLabUserData(query);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV2_FederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV2,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = true
            };


            LabResponse labResponse = LabUserHelper.GetLabUserData(query);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV4_NotFederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV4,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = false
            };

            LabResponse labResponse = LabUserHelper.GetLabUserData(query);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV4_FederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV4,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = true
            };

            LabResponse labResponse = LabUserHelper.GetLabUserData(query);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }


        private static async Task RunTestForUserAsync(LabResponse labResponse)
        {
            var user = labResponse.User;

            var context = new AuthenticationContext(AdalTestConstants.DefaultAuthorityCommonTenant);
            var authResult = await context.AcquireTokenAsync(
                AdalTestConstants.MSGraph,
                labResponse.AppId,
                new UserPasswordCredential(user.Upn, user.Password))
                .ConfigureAwait(false);

            Assert.IsNotNull(authResult.AccessToken);
        }
    }
}
