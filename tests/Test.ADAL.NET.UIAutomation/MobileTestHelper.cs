// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Test.Microsoft.Identity.Core.UIAutomation;
using Microsoft.Identity.Test.LabInfrastructure;

namespace Test.ADAL.UIAutomation
{
    /// <summary>
    /// Contains the core test functionality that will be used by Android and iOS tests
    /// </summary>
	public class MobileTestHelper
    {
        CoreMobileTestHelper CoreMobileTestHelper = new CoreMobileTestHelper();

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void AcquireTokenInteractiveTestHelper(ITestController controller, LabResponse labResponse)
        {
            AcquireTokenInteractiveHelper(controller, labResponse);
            CoreMobileTestHelper.VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void AcquireTokenSilentTestHelper(ITestController controller, LabResponse labResponse)
        {
            AcquireTokenInteractiveHelper(controller, labResponse);
            CoreMobileTestHelper.VerifyResult(controller);

            //Enter 2nd Resource
            controller.EnterText(
                CoreUiTestConstants.ResourceEntryID, 
                CoreUiTestConstants.Exchange, 
                XamarinSelector.ByAutomationId);

            //Acquire token silently
            controller.Tap(CoreUiTestConstants.AcquireTokenSilentID);

            CoreMobileTestHelper.VerifyResult(controller);
        }

        public void AcquireTokenInteractiveHelper(ITestController controller, LabResponse labResponse)
        {
            PrepareForAuthentication(controller);
            SetInputData(controller, labResponse.App.AppId, CoreUiTestConstants.MSGraph);
            CoreMobileTestHelper.PerformSignInFlow(controller, labResponse.User);
        }

        public void AcquireTokenWithPromptBehaviorAlwaysHelper(ITestController controller, LabResponse labResponse)
        {
            PrepareForAuthentication(controller);
            SetInputData(controller, labResponse.App.AppId, CoreUiTestConstants.MSGraph);

            // AcquireToken promptBehavior.Auto to get a token in the cache 
            SetPromptBehavior(controller, CoreUiTestConstants.PromptBehaviorAuto);
            CoreMobileTestHelper.PerformSignInFlow(controller, labResponse.User);

            // AcquireToken promptBehavior.Always. Even with a token, the UI should be shown 
            SetPromptBehavior(controller, CoreUiTestConstants.PromptBehaviorAlways);
            CoreMobileTestHelper.PerformSignInFlow(controller, labResponse.User);

            // AcquireToken promptBehavior.Auto. No UI should be shown. 
            SetPromptBehavior(controller, CoreUiTestConstants.PromptBehaviorAuto);
            CoreMobileTestHelper.PerformSignInFlowWithoutUI(controller);
            CoreMobileTestHelper.VerifyResult(controller);
        }

        private void PrepareForAuthentication(ITestController controller)
        {
            //Navigate to second page
            controller.Tap(CoreUiTestConstants.SecondPageID);

            //Clear Cache
            controller.Tap(CoreUiTestConstants.ClearCacheID);
        }

        private void SetInputData(ITestController controller, string clientID, string resource)
        {
            //Enter ClientID
            controller.EnterText(CoreUiTestConstants.ClientIdEntryID, clientID, XamarinSelector.ByAutomationId);

            //Enter Resource
            controller.EnterText(CoreUiTestConstants.ResourceEntryID, resource, XamarinSelector.ByAutomationId);
        }

        private void SetPromptBehavior(ITestController controller, string promptBehavior)
        {
            //Select PromptBehavior
            controller.EnterText(CoreUiTestConstants.PromptBehaviorEntryID, promptBehavior, XamarinSelector.ByAutomationId);
        }
    }
}
