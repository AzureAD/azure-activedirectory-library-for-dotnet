using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.Labs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Identity.AutomationTests.TestCategories;

namespace DesktopTests
{
    [TestClass]
    public class ManagedTenantTests : DesktopTestsBase
    {
        private const FederationProvider FederationProvider = Microsoft.Identity.Labs.FederationProvider.None;

        [TestMethod]
        [WorkItem(7067), Description("Interactive Auth with PromptBehavior.Always")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.Managed), TestCategory(Platform.Desktop)]
        [TestCategory(TestUse.Automation)] // this test should run as CI for the test repo
        [TestCategory("Automation")]
        [TestCategory("AcquireTokenStandard")]
        public void AcquireTokenTest()
        {
            GetAppiumCommonTests().AcquireTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }
        
        [TestMethod]
        [WorkItem(7076), Description("PromptBehavior.Always twice")]
        [TestCategory(Api.AcquireTokenPromptAlwaysNoHint), TestCategory(BehaveAs.Office), TestCategory(Federation.Managed), TestCategory(Platform.Desktop)]
        [TestCategory("Automation")]
        [TestCategory("AcquireTokenPromptAlways")]
        public void AcquireTokenInteractiveWithPromptAlwaysTest()
        {
            GetAppiumCommonTests().AcquireTokenInteractiveWithPromptAlways_NoHint(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestMethod]
        [WorkItem(47102), Description("UserCredential Auth with empty cache")]
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office), TestCategory(Federation.Managed), TestCategory(Platform.Desktop)]
        [TestCategory("Automation")]
        [TestCategory("AcquireTokenUserCredential")]
        public void AcquireTokenUserCredentialTest()
        {
            GetAppiumDesktopSpecificTests().AquireToken_Using_UserCredentials(
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
