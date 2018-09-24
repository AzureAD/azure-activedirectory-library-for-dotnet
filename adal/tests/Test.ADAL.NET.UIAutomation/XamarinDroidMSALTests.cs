//This is a temp location for this file until the visual studio bug that prevents these tests from working in the MSAL project is resolved.
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Microsoft.Identity.Core.UIAutomation;
using Test.Microsoft.Identity.Core.UIAutomation.infrastructure;
using Xamarin.UITest;

namespace Test.MSAL.NET.UIAutomation
{
    /// <summary>
    /// Configures environment for core/android tests to run
    /// </summary>
    [TestFixture(Platform.Android)]
    class XamarinMSALDroidTests
    {
        IApp app;
        Platform platform;
        ITestController xamarinController;

        public XamarinMSALDroidTests(Platform platform)
        {
            this.platform = platform;
        }

        /// <summary>
        /// Initializes app and test controller before each test
        /// </summary>
        [SetUp]
        public void InitializeBeforeTest()
        {
            app = AppFactory.StartApp(platform, "com.Microsoft.XFormsDroid.MSAL");
            xamarinController = new XamarinUITestController(app);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        //[Test]
        public void AcquireTokenTestMSAL()
        {
            CoreMobileMSALTests.AcquireTokenTest(xamarinController);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        //[Test]
        public void AcquireTokenSilentTestMSAL()
        {
            CoreMobileMSALTests.AcquireTokenSilentTest(xamarinController);
        }

        /// <summary>
        /// Runs through the standard acquire token ADFSV4 Federated flow
        /// </summary>
        //[Test]
        public void AcquireTokenADFSV4InteractiveFederatedMSAL()
        {
            CoreMobileMSALTests.AcquireTokenADFSvXInteractiveMSALTest(xamarinController, FederationProvider.AdfsV4, true);
        }

        /// <summary>
        /// Runs through the standard acquire token ADFSV3 Federated flow
        /// </summary>
        //[Test]
        public void AcquireTokenADFSV3InteractiveFederatedMSAL()
        {
            CoreMobileMSALTests.AcquireTokenADFSvXInteractiveMSALTest(xamarinController, FederationProvider.AdfsV3, true);
        }

        /// <summary>
        /// Runs through the standard acquire token ADFSV4 Non-Federated flow
        /// </summary>
        //[Test]
        public void AcquireTokenADFSV4InteractiveNonFederatedMSAL()
        {
            CoreMobileMSALTests.AcquireTokenADFSvXInteractiveMSALTest(xamarinController, FederationProvider.AdfsV3, false);
        }

        /// <summary>
        /// Runs through the standard acquire token ADFSV3 Non-Federated flow
        /// </summary>
        //[Test]
        public void AcquireTokenADFSV3InteractiveNonFederatedMSAL()
        {
            CoreMobileMSALTests.AcquireTokenADFSvXInteractiveMSALTest(xamarinController, FederationProvider.AdfsV3, false);
        }
    }
}
