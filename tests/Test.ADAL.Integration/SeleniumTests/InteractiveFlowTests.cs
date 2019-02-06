using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Test.ADAL.Integration.Infrastructure;
using Test.ADAL.NET.Common;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Test.ADAL.Integration.SeleniumTests
{
    [TestClass]
    public class InteractiveFlowTests
    {
        private readonly TimeSpan _seleniumTimeout = TimeSpan.FromMinutes(2);

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

        //[TestInitialize]
        //public void TestInitialize()
        //{
        //    TestCommon.ResetStateAndInitMsal();
        //}

        #endregion

        [TestMethod]
        public async Task InteractiveAuth_DefaultUserAsync()
        {
            // Arrange
            LabResponse labResponse = LabUserHelper.GetDefaultUser();
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task Interactive_AdfsV3_NotFederatedAsync()
        {
            // Arrange
            UserQueryParameters query = new UserQueryParameters
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
        public async Task Interactive_AdfsV3_FederatedAsync()
        {
            // Arrange
            UserQueryParameters query = new UserQueryParameters
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
        public async Task Interactive_AdfsV2_FederatedAsync()
        {
            // Arrange
            UserQueryParameters query = new UserQueryParameters
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
        public async Task Interactive_AdfsV4_NotFederatedAsync()
        {
            // Arrange
            UserQueryParameters query = new UserQueryParameters
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
        public async Task Interactive_AdfsV4_FederatedAsync()
        {
            // Arrange
            UserQueryParameters query = new UserQueryParameters
            {
                FederationProvider = FederationProvider.AdfsV4,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = true
            };

            LabResponse labResponse = LabUserHelper.GetLabUserData(query);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        private async Task RunTestForUserAsync(LabResponse labResponse)
        {
            Action<IWebDriver> seleniumLogic = (driver) =>
            {
                Trace.WriteLine("Starting Selenium automation");
                driver.PerformLogin(labResponse.User);
            };

            WebUIFactoryProvider.WebUIFactory = new SeleniumWebUIFactory(seleniumLogic, _seleniumTimeout);

            //PublicClientApplication pca = PublicClientApplicationBuilder.Create(labResponse.AppId)
            //                                                            .WithRedirectUri(SeleniumWebUIFactory.FindFreeLocalhostRedirectUri())
            //                                                            .BuildConcrete();

            AuthenticationContext context = new AuthenticationContext(AdalTestConstants.DefaultAuthorityCommonTenant);

            // Act
            //AuthenticationResult result = await pca.AcquireTokenAsync(new[] { "user.read" }).ConfigureAwait(false);
            AuthenticationResult result = await context.AcquireTokenAsync(AdalTestConstants.MSGraph, labResponse.AppId, new Uri(SeleniumWebUIFactory.FindFreeLocalhostRedirectUri()), null);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.AccessToken));
        }
    }
}
