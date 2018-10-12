

using Test.Microsoft.Identity.LabInfrastructure;
using NUnit.Framework;
using Test.Microsoft.Identity.Core.UIAutomation;
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
        ITestController xamarinController = new XamarinUITestController();

        public XamarinDroidADALTests(Platform platform)
        {
            this.platform = platform;
        }

        /// <summary>
        /// Initializes app and test controller before each test
        /// </summary>
        [SetUp]
        public void InitializeTest()
        {
            app = AppFactory.StartApp(platform, "com.Microsoft.XFormsDroid.ADAL");
            xamarinController.Application = app;
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveTest()
        {
            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, LabUserHelper.DefaultUserQuery);
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        [Test]
        public void AcquireTokenSilentTest()
        {
            ADALMobileTestHelper.AcquireTokenSilentTestHelper(xamarinController, LabUserHelper.DefaultUserQuery);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSv4FederatedInteractiveTest()
        {
            var user = LabUserHelper.DefaultUserQuery;
            user.FederationProvider = FederationProvider.AdfsV4;
            user.IsFederatedUser = true;

            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, user);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveADFSv4NonFederatedTest()
        {
            var user = LabUserHelper.DefaultUserQuery;
            user.FederationProvider = FederationProvider.AdfsV4;
            user.IsFederatedUser = false;

            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, user);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSv3FederatedInteractiveTest()
        {
            var user = LabUserHelper.DefaultUserQuery;
            user.FederationProvider = FederationProvider.AdfsV3;
            user.IsFederatedUser = true;

            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, user);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveADFSv3NonFederatedTest()
        {
            var user = LabUserHelper.DefaultUserQuery;
            user.FederationProvider = FederationProvider.AdfsV3;
            user.IsFederatedUser = false;

            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, user);
        }
    }
}
