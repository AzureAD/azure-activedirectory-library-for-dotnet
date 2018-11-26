using Microsoft.Identity.AutomationTests;
using Microsoft.Identity.AutomationTests.Adal.Headless;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.Labs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Identity.AutomationTests.TestCategories;

namespace DesktopTests
{
    [TestClass]
    public class HeadlessAdfsV3TenantTests : HeadlessDesktopTestsBase
    {
        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(7067), Description("Acquire Token Interactive")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV3),
         TestCategory(Platform.Desktop)]
        public void AcquireTokenTest_AdfsV3_Office_OnPremise_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(7067), Description("Acquire Token Interactive for federated user")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV3),
         TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenTest_AdfsV3_Office_Federated_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod, Ignore] // #TODO: E2E test is failing 
        [WorkItem(7079), Description("Silent Auth By Refreshing Token")]
        [TestCategory(Api.AcquireTokenSilentByRefreshToken), TestCategory(BehaveAs.Office),
         TestCategory(Federation.AdfsV3), TestCategory(Platform.Desktop)]
        public void AcquireTokenSilentByRefreshToken_AdfsV3_Office_OnPremise_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenSilentByRefreshTokenTest(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("ProductUI")]
        [TestMethod, Ignore] // #TODO: 266255 No test account available for ADFSv3 cross-tenant test 
        [WorkItem(47101), Description("Silent Auth for guest tenant authority")]
        [TestCategory(Api.AcquireTokenSilentWithCrossTenant), TestCategory(BehaveAs.Office),
         TestCategory(Federation.AdfsV3), TestCategory(Platform.Desktop)]
        public void AcquireTokenSilentWithCrossTenantTest_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenSilentCrossTenant(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestMethod]
        [WorkItem(47102), Description("Username/Password Auth with empty cache")]
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office),
         TestCategory(Federation.AdfsV3), TestCategory(UserAttributeCollection.Federated),
         TestCategory(Platform.Desktop)]
        public void AcquireTokenUserCredential_AdfsV3_Office_Federated_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AquireToken_Using_UserCredentials(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(47102), Description("Username/Password Auth with empty cache")]
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office),
         TestCategory(Federation.AdfsV3), TestCategory(Platform.Desktop)]
        public void AcquireTokenUserCredential_AdfsV3_Office_OnPremise_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AquireToken_Using_UserCredentials(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod, Ignore] // #TODO: Fix 189811 and re-enable this test
        [WorkItem(47104), Description("Hidden webview Auth with Prompt=never for federated user")]
        [TestCategory(Api.AcquireTokenPromptNever), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV3),
         TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenPromptNever_AdfsV3_Office_Federated_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTestWithPromptNever(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("NoProductUI")]
        [TestCategory("Headless")]
        [TestMethod, Ignore] // #TODO: Fix 189811 and re-enable this test
        [WorkItem(47104), Description("Hidden webview Auth with Prompt=never")]
        [TestCategory(Api.AcquireTokenPromptNever), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV3),
         TestCategory(Platform.Desktop)]
        public void AcquireTokenPromptNever_AdfsV3_Office_OnPremise_Headless()
        {
            GetNonAppiumCommonTests().AcquireTokenTestWithPromptNever(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(47105), Description("Domain/AAD joined - Integrated Auth - Empty cache")]
        [TestCategory(Api.AcquireTokenPromptNever), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV3),
         TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenDomainJoined_ADFSv3_Office_Federated_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AquireTokenDomainJoined(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }

        [TestCategory("ProductUI")]
        [TestCategory("Headless")]
        [TestMethod]
        [WorkItem(47106), Description("Domain/AAD joined - Integrated Auth (Token in cache)")]
        [TestCategory(Api.AcquireTokenPromptNever), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV3),
         TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        public void AcquireTokenDomainJoinedCache_ADFSv3_Office_Federated_Headless()
        {
            GetNonAppiumDesktopSpecificTests().AquireTokenDomainJoinedCache(
                ResourceType.Graph,
                ApplicationType.Office,
                new UserQueryParameters
                {
                    FederationProvider = FederationProvider.AdfsV3,
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });
        }
    }
}