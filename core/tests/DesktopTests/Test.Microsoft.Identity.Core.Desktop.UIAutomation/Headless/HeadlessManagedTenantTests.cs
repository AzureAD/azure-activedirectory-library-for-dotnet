using Microsoft.Identity.AutomationTests;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.LabInfrastructure;
using static Microsoft.Identity.AutomationTests.TestCategories;

namespace DesktopTests
{
    [TestClass]
    public class HeadlessManagedTenantTests : HeadlessDesktopTestsBase
    {
        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(7067), Description("Interactive Auth with PromptBehavior.Always")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.Managed), TestCategory(Platform.Desktop)]
        [TestCategory(TestUse.AutomationSanity)] // this test should run as CI for the test repo
        public void AcquireTokenTest_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(7079), Description("Silent Auth By Refreshing Token")]
        [TestCategory(Api.AcquireTokenSilentByRefreshToken), TestCategory(BehaveAs.Office), TestCategory(Federation.Managed), TestCategory(Platform.Desktop)]
        public void AcquireTokenSilentByRefreshTokenTest_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenSilentByRefreshTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("NoProductUI"), TestCategory("TestUI")] // the product code is UI-less but the test has UI when validating
        [TestCategory("Headless")]
        [TestMethod] //, Ignore] // #TODO: Fix 189831 and re-enable this test
        [WorkItem(81040), Description("DeviceCode registration followed by AcquireToken")]
        [TestCategory(Api.AcquireTokenWithDeviceAuthCode), TestCategory(BehaveAs.Office), TestCategory(Federation.Managed), TestCategory(Platform.Desktop)]
        public void AcquireTokenWithDeviceAuthCode_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AcquireTokenByDeviceAuth(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(47101), Description("Silent Auth for guest tenant authority")]
        [TestCategory(Api.AcquireTokenSilentWithCrossTenant), TestCategory(BehaveAs.Office), TestCategory(Federation.Managed), TestCategory(Platform.Desktop)]
        public void AcquireTokenSilentWithCrossTenantTest_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenSilentCrossTenant(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(47102), Description("UserCredential Auth with empty cache")]
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office), TestCategory(Federation.Managed), TestCategory(Platform.Desktop)]
        public void AcquireTokenUserCredentialTest_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AquireToken_Using_UserCredentials(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod, Ignore] // #TODO: Fix 189811 and re-enable this test
        [WorkItem(47104), Description("Hidden webview Auth with Prompt=never")]
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office), TestCategory(Federation.Managed), TestCategory(Platform.Desktop)]
        public void AcquireTokenPromptNeverTest_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTestWithPromptNever(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }
    }
}
