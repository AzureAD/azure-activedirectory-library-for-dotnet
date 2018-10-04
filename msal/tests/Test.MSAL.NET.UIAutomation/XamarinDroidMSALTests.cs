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
        [Test]
        public void AcquireTokenTest()
        {
            CoreMobileMSALTests.AcquireTokenTest(xamarinController, DefaultUser);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenSilentTest()
        {
            CoreMobileMSALTests.AcquireTokenSilentTest(xamarinController, DefaultUser);
        }

        /// <summary>
        /// Runs through the standard acquire token ADFSV4 Federated flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSV4InteractiveFederated()
        {
            var user = DefaultUser;
            user.FederationProvider = FederationProvider.AdfsV4;
            user.IsFederatedUser = true;

            CoreMobileMSALTests.AcquireTokenADFSvXInteractiveMSALTest(xamarinController, true, user);
        }

        /// <summary>
        /// Runs through the standard acquire token ADFSV3 Federated flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSV3InteractiveFederated()
        {
            var user = DefaultUser;
            user.FederationProvider = FederationProvider.AdfsV3;
            user.IsFederatedUser = true;

            CoreMobileMSALTests.AcquireTokenADFSvXInteractiveMSALTest(xamarinController, true, user);
        }

        /// <summary>
        /// Runs through the standard acquire token ADFSV4 Non-Federated flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSV4InteractiveNonFederated()
        {
            var user = DefaultUser;
            user.FederationProvider = FederationProvider.AdfsV4;
            user.IsFederatedUser = false;

            CoreMobileMSALTests.AcquireTokenADFSvXInteractiveMSALTest(xamarinController, false, user);
        }

        /// <summary>
        /// Runs through the standard acquire token ADFSV3 Non-Federated flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSV3InteractiveNonFederated()
        {
            var user = DefaultUser;
            user.FederationProvider = FederationProvider.AdfsV3;
            user.IsFederatedUser = false;

            CoreMobileMSALTests.AcquireTokenADFSvXInteractiveMSALTest(xamarinController, false, user);
        }
    }
}
