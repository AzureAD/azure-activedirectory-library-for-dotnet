using Microsoft.Identity.AutomationTests;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.Labs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Identity.AutomationTests.TestCategories;

namespace DesktopTests
{
    [TestClass]
    public class HeadlessShibbolethTenantTests : HeadlessDesktopTestsBase
    {
        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(7067), Description("Acquire Token Interactive for federated user")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.Shibboleth), TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenTest_Shibboleth_Office_Federated_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.Shibboleth,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(47102), Description("Username/Password Auth with empty cache")]
        [Ignore] //Need to work on why UserCredentials not working for Federated shibboleth user
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office), TestCategory(Federation.Shibboleth), TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenUserCredential_Shibboleth_Office_Federated_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AquireToken_Using_UserCredentials(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.Shibboleth,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod, Ignore] // #TODO: Fix 189811 and re-enable this test
        [WorkItem(47104), Description("Hidden webview Auth with Prompt=never for federated user")]
        [TestCategory(Api.AcquireTokenPromptNever), TestCategory(BehaveAs.Office), TestCategory(Federation.Shibboleth), TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenPromptNever_Shibboleth_Office_Federated_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTestWithPromptNever(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.Shibboleth,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }
    }
}
