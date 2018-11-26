using Microsoft.Identity.AutomationTests;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.Labs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Identity.AutomationTests.TestCategories;

namespace DesktopTests
{
    [TestClass]
    public class HeadlessAdfsV4TenantTests : HeadlessDesktopTestsBase
    {
        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(7067), Description("Acquire Token Interactive")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV4), TestCategory(Platform.Desktop)]
        public void AcquireTokenTest_AdfsV4_Office_OnPremise_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV4,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(7067), Description("Acquire Token Interactive for federated user")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV4), TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenTest_AdfsV4_Office_Federated_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV4,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [Ignore] // #232033: E2E tests for AdfsV4-Office-Federated scenarios are failing
        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(7079), Description("Silent Auth By Refreshing Token")]
        [TestCategory(Api.AcquireTokenSilentByRefreshToken), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV4), TestCategory(Platform.Desktop)]
        public void AcquireTokenSilentByRefreshToken_AdfsV4_Office_OnPremise_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenSilentByRefreshTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV4,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [Ignore] // #232033: E2E tests for AdfsV4-Office-Federated scenarios are failing
        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(47102), Description("Username/Password Auth with empty cache")]
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV4), TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenUserCredential_AdfsV4_Office_Federated_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AquireToken_Using_UserCredentials(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV4,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(47102), Description("Username/Password Auth with empty cache")]
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV4), TestCategory(Platform.Desktop)]
        public void AcquireTokenUserCredential_AdfsV4_Office_OnPremise_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AquireToken_Using_UserCredentials(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV4,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod, Ignore] // #TODO: Fix 189811 and re-enable this test
        [WorkItem(47104), Description("Hidden webview Auth with Prompt=never for federated user")]
        [TestCategory(Api.AcquireTokenPromptNever), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV4), TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenPromptNever_AdfsV4_Office_Federated_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTestWithPromptNever(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV4,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod, Ignore] // #TODO: Fix 189811 and re-enable this test
        [WorkItem(47104), Description("Hidden webview Auth with Prompt=never")]
        [TestCategory(Api.AcquireTokenPromptNever), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV4), TestCategory(Platform.Desktop)]
        public void AcquireTokenPromptNever_AdfsV4_Office_OnPremise_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTestWithPromptNever(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV4,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }
    }
}
