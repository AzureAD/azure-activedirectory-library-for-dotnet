using Microsoft.Identity.AutomationTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.LabInfrastructure;
using static Microsoft.Identity.AutomationTests.TestCategories;

namespace DesktopTests
{
    [TestClass]
    public class AdfsV3TenantTests : DesktopTestsBase
    {
        [TestMethod]
        [WorkItem(7067), Description("Acquire Token Interactive")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV3),
         TestCategory(Platform.Desktop)]
        [TestCategory("Automation")]
        [TestCategory("AcquireTokenStandard")]
        public void AcquireTokenTest_AdfsV3_Office_NotFederated()
        {
            GetAppiumCommonTests().AcquireTokenTest(
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

        [TestMethod]
        [WorkItem(7067), Description("Acquire Token Interactive for federated user")]
        [TestCategory(Api.AcquireToken), TestCategory(BehaveAs.Office), TestCategory(Federation.AdfsV3),
         TestCategory(UserAttributeCollection.Federated), TestCategory(Platform.Desktop)]
        [TestCategory("Automation")]
        [TestCategory("AcquireTokenStandard")]
        public void AcquireTokenTest_AdfsV3_Office_Federated()
        {
            GetAppiumCommonTests().AcquireTokenTest(
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

        [TestMethod]
        [WorkItem(7076), Description("PromptBehavior.Always twice for federated user")]
        [TestCategory(Api.AcquireTokenPromptAlwaysNoHint), TestCategory(BehaveAs.Office),
         TestCategory(Federation.AdfsV3), TestCategory(UserAttributeCollection.Federated),
         TestCategory(Platform.Desktop)]
        [TestCategory("Automation")]
        [TestCategory("Automation Federated")]
        [TestCategory("AcquireTokenPromptAlways")]
        public void AcquireTokenInteractiveWithPromptAlways_AdfsV3_Office_Federated()
        {
            GetAppiumCommonTests().AcquireTokenInteractiveWithPromptAlways_NoHint(
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

        [TestMethod]
        [WorkItem(7076), Description("PromptBehavior.Always twice")]
        [TestCategory(Api.AcquireTokenPromptAlwaysNoHint), TestCategory(BehaveAs.Office),
         TestCategory(Federation.AdfsV3), TestCategory(Platform.Desktop)]
        [TestCategory("Automation")]
        [TestCategory("Automation Not Federated")]
        [TestCategory("AcquireTokenPromptAlways")]
        public void AcquireTokenInteractiveWithPromptAlways_AdfsV3_Office_NotFederated()
        {
            GetAppiumCommonTests().AcquireTokenInteractiveWithPromptAlways_NoHint(
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

        [TestMethod]
        [WorkItem(47102), Description("Username/Password Auth with empty cache")]
        [TestCategory(Api.AcquireTokenWithUserCredentials), TestCategory(BehaveAs.Office),
         TestCategory(Federation.AdfsV3), TestCategory(Platform.Desktop)]
        [TestCategory("Automation")]
        [TestCategory("AcquireTokenUserCredential")]
        public void AcquireTokenUserCredential_AdfsV3_Office_NotFederated()
        {
            GetAppiumDesktopSpecificTests().AquireToken_Using_UserCredentials(
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
    }
}