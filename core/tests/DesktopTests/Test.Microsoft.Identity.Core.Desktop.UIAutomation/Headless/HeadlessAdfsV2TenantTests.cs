using Microsoft.Identity.AutomationTests;
using Microsoft.Identity.AutomationTests.Adal.Headless;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.Labs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Identity.AutomationTests.TestCategories;

namespace DesktopTests
{
    [TestClass]
    public class HeadlessAdfsV2TenantTests : HeadlessDesktopTestsBase
    {

        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(7067), Description("Acquire Token Interactive for federated user")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV2), TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenTest_AdfsV2_Federated_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV2,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(47102), Description("Username/Password Auth with empty cache")]
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV2), TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenUserCredential_AdfsV2_Federated_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AquireToken_Using_UserCredentials(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV2,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod, Ignore] // #TODO: Fix 189811 and re-enable this test
        [WorkItem(47104), Description("Hidden webview Auth with Prompt=never for federated user")]
        [TestCategory(Api.AcquireTokenPromptNever), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV2), TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenPromptNever_AdfsV2_Federated_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTestWithPromptNever(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV2,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }
    }
}
