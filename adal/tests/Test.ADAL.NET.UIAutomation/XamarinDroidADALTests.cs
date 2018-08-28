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
            CoreMobileADALTests.AcquireTokenInteractiveTest(xamarinController);
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        [Test]
        public void AcquireTokenSilentTest()
        {
            CoreMobileADALTests.AcquireTokenSilentTest(xamarinController);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSv4FederatedInteractiveTest()
        {
            CoreMobileADALTests.AcquireTokenADFSv4FederatedInteractiveTest(xamarinController);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveADFSv4NonFederatedTest()
        {
            CoreMobileADALTests.AcquireTokenInteractiveADFSv4NonFederatedTest(xamarinController);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSv3FederatedInteractiveTest()
        {
            CoreMobileADALTests.AcquireTokenADFSv3FederatedInteractiveTest(xamarinController);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveADFSv3NonFederatedTest()
        {
            CoreMobileADALTests.AcquireTokenInteractiveADFSv3NonFederatedTest(xamarinController);
        }
    }
}
