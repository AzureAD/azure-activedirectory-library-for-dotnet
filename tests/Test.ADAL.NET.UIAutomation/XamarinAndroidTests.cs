// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Test.LabInfrastructure;
using NUnit.Framework;
using Test.Microsoft.Identity.Core.UIAutomation;
using Xamarin.UITest;

//NOTICE! Inorder to run UI automation tests for xamarin locally, you may need to upgrade nunit to 3.0 and above for this project and the core ui Automation project.
//It is set to 2.6.4 because that is the maximum version that appcenter can support.
//There is an error in visual studio that can prevent the NUnit test framework from loading the test dll properly.
//Remember to return the version back to 2.6.4 before commiting to prevent appcenter from failing

namespace Test.ADAL.UIAutomation
{
    /// <summary>
    /// Configures environment for core/android tests to run
    /// </summary>
    [TestFixture(Platform.Android)]
    public class XamarinAndroidTests
    {
        IApp app;
        Platform platform;
        ITestController xamarinController = new XamarinUITestController();
        MobileTestHelper _adalMobileTestHelper = new MobileTestHelper();

        public XamarinAndroidTests(Platform platform)
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
            _adalMobileTestHelper.AcquireTokenInteractiveTestHelper(
                xamarinController,
                LabUserHelper.GetDefaultUserAsync().GetAwaiter().GetResult());
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow 
        /// </summary>
        [Test]
        public void AcquireTokenSilentTest()
        {
            _adalMobileTestHelper.AcquireTokenSilentTestHelper(
                xamarinController,
                LabUserHelper.GetDefaultUserAsync().GetAwaiter().GetResult());
        }

        /// <summary>
        /// Runs through the standard acquire token flow with prompt behavior set to always
        /// The user is always prompted for credentials, 
        /// even if a token exists in the cache, and if the user has a session. 
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveWithPromptAlwaysTest()
        {
            _adalMobileTestHelper.AcquireTokenWithPromptBehaviorAlwaysHelper(
                xamarinController,
                LabUserHelper.GetDefaultUserAsync().GetAwaiter().GetResult());
        }

        /// <summary>
        /// Runs through the standard acquire token flow with a ADFS V4 account
        /// </summary>
        [Test]
        public void AcquireTokenADFSv4FederatedInteractiveTest()
        {
            _adalMobileTestHelper.AcquireTokenInteractiveTestHelper(
                xamarinController,
                LabUserHelper.GetAdfsUserAsync(FederationProvider.AdfsV4).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Runs through the standard acquire token flow with a non federated ADFS V4 account
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveADFSv4NonFederatedTest()
        {
            _adalMobileTestHelper.AcquireTokenInteractiveTestHelper(
                xamarinController,
                LabUserHelper.GetAdfsUserAsync(FederationProvider.AdfsV4, false).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        [Ignore("Disabled, breaks because cannot get a correct test lab user because the lab API query is wrong.")]
        public void AcquireTokenADFSv3FederatedInteractiveTest()
        {
            _adalMobileTestHelper.AcquireTokenInteractiveTestHelper(
                xamarinController,
                LabUserHelper.GetAdfsUserAsync(FederationProvider.AdfsV3).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        [Ignore("Disabled, breaks because cannot get a correct test lab user because the lab API query is wrong.")]
        public void AcquireTokenInteractiveADFSv3NonFederatedTest()
        {
            _adalMobileTestHelper.AcquireTokenInteractiveTestHelper(
                xamarinController,
                LabUserHelper.GetAdfsUserAsync(FederationProvider.AdfsV3, false).GetAwaiter().GetResult());
        }
    }
}
