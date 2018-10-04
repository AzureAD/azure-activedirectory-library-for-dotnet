using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Test.Microsoft.Identity.Core.UIAutomation;
using Test.Microsoft.Identity.Core.UIAutomation.infrastructure;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Test.ADAL.NET.UIAutomation
{
    /// <summary>
    /// Configures environment for core/android tests to run
    /// </summary>
    [TestFixture(Platform.Android)]
    class XamarinDroidADALTests
    {
        IApp app;
        Platform platform;
        ITestController xamarinController;

        UserQueryParameters DefaultUser
        {
            get
            {
                return new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                };
            }
        }

        public XamarinDroidADALTests(Platform platform)
        {
            this.platform = platform;
        }

        /// <summary>
        /// Initializes app and test controller before each test
        /// </summary>
        [SetUp]
        public void InitializeBeforeTest()
        {
            app = AppFactory.StartApp(platform, "com.Microsoft.XFormsDroid.ADAL");
            xamarinController = new XamarinUITestController(app);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveTest()
        {
            CoreMobileADALTests.AcquireTokenInteractiveTest(xamarinController, DefaultUser);
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        [Test]
        public void AcquireTokenSilentTest()
        {
            CoreMobileADALTests.AcquireTokenSilentTest(xamarinController, DefaultUser);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSv4FederatedInteractiveTest()
        {
            var user = DefaultUser;
            user.FederationProvider = FederationProvider.AdfsV4;
            user.IsFederatedUser = true;

            CoreMobileADALTests.AcquireTokenADFSvXInteractiveTest(xamarinController, true, user);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveADFSv4NonFederatedTest()
        {
            var user = DefaultUser;
            user.FederationProvider = FederationProvider.AdfsV4;
            user.IsFederatedUser = false;

            CoreMobileADALTests.AcquireTokenADFSvXInteractiveTest(xamarinController, false, user);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSv3FederatedInteractiveTest()
        {
            var user = DefaultUser;
            user.FederationProvider = FederationProvider.AdfsV3;
            user.IsFederatedUser = true;

            CoreMobileADALTests.AcquireTokenADFSvXInteractiveTest(xamarinController, true, user);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveADFSv3NonFederatedTest()
        {
            var user = DefaultUser;
            user.FederationProvider = FederationProvider.AdfsV3;
            user.IsFederatedUser = false;

            CoreMobileADALTests.AcquireTokenADFSvXInteractiveTest(xamarinController, false, user);
        }
    }
}
