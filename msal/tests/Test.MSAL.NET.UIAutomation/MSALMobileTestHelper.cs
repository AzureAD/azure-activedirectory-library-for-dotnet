//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using Test.Microsoft.Identity.LabInfrastructure;
using NUnit.Framework;
using Test.Microsoft.Identity.Core.UIAutomation;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.UITest.Queries;

namespace Test.MSAL.UIAutomation
{
    /// <summary>
    /// Contains the core test functionality that will be used by Android and iOS tests
    /// </summary>
    public class MSALMobileTestHelper
    {
        public CoreMobileTestHelper CoreMobileTestHelper { get; set; } = new CoreMobileTestHelper();

        /// <summary>
        /// Runs through the standard acquire token flow, using the login prompt behavior
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void AcquireTokenInteractiveTestHelper(
            ITestController controller,
            LabResponse labResponse,
            string promptBehavior = CoreUiTestConstants.UIBehaviorLogin)
        {
            AcquireTokenInteractiveHelper(controller, labResponse, promptBehavior);
            CoreMobileTestHelper.VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void AcquireTokenSilentTestHelper(ITestController controller, LabResponse labResponse)
        {
            //acquire token for 1st resource
            AcquireTokenInteractiveHelper(controller, labResponse, CoreUiTestConstants.UIBehaviorLogin);
            CoreMobileTestHelper.VerifyResult(controller);

            //acquire token for 2nd resource with refresh token
            SetInputData(controller, labResponse.AppId, CoreUiTestConstants.DefaultScope, CoreUiTestConstants.UIBehaviorLogin);
            controller.Tap(CoreUiTestConstants.AcquireTokenSilentID);
            CoreMobileTestHelper.VerifyResult(controller);
        }

        private void AcquireTokenInteractiveHelper(
            ITestController controller,
            LabResponse labResponse,
            string promptBehavior)
        {
            PrepareForAuthentication(controller);
            SetInputData(controller, labResponse.AppId, CoreUiTestConstants.DefaultScope, promptBehavior);
            CoreMobileTestHelper.PerformSignInFlow(controller, labResponse.User);

            // on consent, also hit the accept button
            if (promptBehavior == CoreUiTestConstants.UIBehaviorConsent)
            {
                AppWebResult consentHeader = controller.WaitForWebElementByCssId("consentHeader").FirstOrDefault();
                Assert.IsNotNull(consentHeader);
                Assert.IsTrue(consentHeader.TextContent.Contains("Permissions requested"));

                controller.Tap(CoreUiTestConstants.WebSubmitID, XamarinSelector.ByHtmlIdAttribute);
            }
        }

        private void PrepareForAuthentication(ITestController controller)
        {
            //Clear Cache
            controller.Tap(CoreUiTestConstants.CachePageID);
            controller.Tap(CoreUiTestConstants.ClearCacheID);
        }

        private void SetInputData(
            ITestController controller,
            string clientId,
            string scopes,
            string uiBehavior)
        {
            controller.Tap(CoreUiTestConstants.SettingsPageID);

            //Enter ClientID
            controller.EnterText(CoreUiTestConstants.ClientIdEntryID, clientId, XamarinSelector.ByAutomationId);
            controller.Tap(CoreUiTestConstants.SaveID);

            //Enter Scopes
            controller.Tap(CoreUiTestConstants.AcquirePageID);
            controller.EnterText(CoreUiTestConstants.ScopesEntryID, scopes, XamarinSelector.ByAutomationId);

            SetUiBehavior(controller, uiBehavior);
        }

        public void SetUiBehavior(ITestController controller, string promptBehavior)
        {
            // Enter Prompt Behavior
            controller.Tap(CoreUiTestConstants.UiBehaviorPickerID);
            controller.Tap(promptBehavior);
            controller.Tap(CoreUiTestConstants.AcquirePageID);
        }

        private void ValidateUiBehaviorString(string uiBehavior)
        {
            var okList = new[] {
                CoreUiTestConstants.UIBehaviorConsent,
                CoreUiTestConstants.UIBehaviorLogin,
                CoreUiTestConstants.UIBehaviorSelectAccount };

            bool isInList = okList.Any(item => string.Equals(item, uiBehavior, StringComparison.InvariantCulture));

            if (!isInList)
            {
                throw new InvalidOperationException("Test Setup Error: invalid uiBehavior " + uiBehavior);
            }
        }

        /// <summary>
        /// Runs through the B2C acquire token flow with local account
        /// </summary>
        public void B2CLocalAccountAcquireTokenInteractiveTestHelper(ITestController controller, LabResponse labResponse, bool isB2CLoginAuthority)
        {
            PerformB2CSignInFlow(controller, labResponse.User, B2CIdentityProvider.Local, isB2CLoginAuthority);
        }

        /// <summary>
        /// Runs through the B2C acquire token flow with Facebook Provider
        /// </summary>
        public void B2CFacebookProviderAcquireTokenInteractiveTestHelper(ITestController controller, LabResponse labResponse, bool isB2CLoginAuthority)
        {
            PerformB2CSignInFlow(controller, labResponse.User, B2CIdentityProvider.Facebook, isB2CLoginAuthority);
        }

        /// <summary>
        /// Runs through the B2C acquire token flow with Facebook Provider
        /// and Edit Policy authority
        /// </summary>
        public void B2CFacebookProviderEditPolicyAcquireTokenInteractiveTestHelper(ITestController controller)
        {
            PerformB2CSignInEditProfileFlow(controller, B2CIdentityProvider.Facebook);
        }

        /// <summary>
        /// Runs through the B2C acquire token flow with Google Provider
        /// </summary>
        public void B2CGoogleProviderAcquireTokenInteractiveTestHelper(ITestController controller, LabResponse labResponse, bool isB2CLoginAuthority)
        {
            PerformB2CSignInFlow(controller, labResponse.User, B2CIdentityProvider.Google, isB2CLoginAuthority);
        }

        private void SetB2CAuthority(ITestController controller, bool isB2CLoginAuthority)
        {
            PrepareForAuthentication(controller);
            controller.Tap(CoreUiTestConstants.SettingsPageID);

            if (isB2CLoginAuthority)
            {
                SetB2CInputDataForB2CloginAuthority(controller);
            }
            else
            {
                SetB2CInputData(controller);
            }
        }

        /// <summary>
        /// Runs through the B2C acquire token silent flow with local account
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void B2CLocalAccountAcquireTokenSilentTest(ITestController controller, LabResponse labResponse, bool isB2CLoginAuthority)
        {
            //acquire token for 1st resource   
            B2CLocalAccountAcquireTokenInteractiveTestHelper(controller, labResponse, isB2CLoginAuthority);

            B2CSilentFlowHelper(controller);
        }

        /// <summary>
        /// Runs through the B2C acquire token silent flow with Facebook identity provider
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void B2CFacebookProviderAcquireTokenSilentTest(ITestController controller, LabResponse labResponse, bool isB2CLoginAuthority)
        {
            //acquire token for 1st resource   
            B2CFacebookProviderAcquireTokenInteractiveTestHelper(controller, labResponse, isB2CLoginAuthority);

            B2CSilentFlowHelper(controller);
        }

        /// <summary>
        /// Runs through the B2C acquire token silent flow with Google identity provider
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void B2CGoogleProviderAcquireTokenSilentTest(ITestController controller, LabResponse labResponse, bool isB2CLoginAuthority)
        {
            //acquire token for 1st resource   
            B2CGoogleProviderAcquireTokenInteractiveTestHelper(controller, labResponse, isB2CLoginAuthority);

            B2CSilentFlowHelper(controller);
        }

        private void B2CSilentFlowHelper(ITestController controller)
        {
            //verify results of AT call
            CoreMobileTestHelper.VerifyResult(controller);

            //select user
            controller.Tap(CoreUiTestConstants.SelectUser);
            //b2c does not return userinfo in token response
            controller.Tap(CoreUiTestConstants.UserMissingFromResponse);
            //acquire token silent with selected user
            controller.Tap(CoreUiTestConstants.AcquireTokenSilentID);
            CoreMobileTestHelper.VerifyResult(controller);
        }

        private void SetB2CInputData(ITestController controller)
        {
            // Select login.microsoftonline.com for authority
            SetAuthority(controller, CoreUiTestConstants.MicrosoftOnlineAuthority);
        }

        private void SetB2CInputDataForB2CloginAuthority(ITestController controller)
        {
            // Select b2clogin.com for authority
            SetAuthority(controller, CoreUiTestConstants.B2CLoginAuthority);
        }

        public void SetB2CInputDataForEditProfileAuthority(ITestController controller)
        {
            controller.Tap(CoreUiTestConstants.SettingsPageID);
            // Select Edit Profile for Authority
            SetAuthority(controller, CoreUiTestConstants.B2CEditProfileAuthority);
        }

        public void SetAuthority(ITestController controller, string authority)
        {
            // Select authority
            controller.Tap(CoreUiTestConstants.AuthorityPickerID);
            controller.Tap(authority);
        }

        public void PerformB2CLocalAccountSignInFlow(ITestController controller, LabUser user, UserInformationFieldIds userInformationFieldIds)
        {
            controller.EnterText(CoreUiTestConstants.WebUPNB2CLocalInputID, 20, user.Upn, XamarinSelector.ByHtmlIdAttribute);

            controller.EnterText(userInformationFieldIds.PasswordInputId, LabUserHelper.GetUserPassword(user), XamarinSelector.ByHtmlIdAttribute);

            controller.Tap(userInformationFieldIds.SignInButtonId, XamarinSelector.ByHtmlIdAttribute);
        }

        public void PerformB2CFacebookProviderSignInFlow(ITestController controller, LabUser user, UserInformationFieldIds userInformationFieldIds)
        {
            controller.Tap(CoreUiTestConstants.FacebookAccountID, XamarinSelector.ByHtmlIdAttribute);

            controller.EnterText(CoreUiTestConstants.WebUPNB2CFacebookInputID, 20, user.Upn, XamarinSelector.ByHtmlIdAttribute);

            controller.EnterText(userInformationFieldIds.PasswordInputId, LabUserHelper.GetUserPassword(user), XamarinSelector.ByHtmlIdAttribute);

            controller.Tap(userInformationFieldIds.SignInButtonId, XamarinSelector.ByHtmlIdAttribute);
        }

        public void PerformB2CGoogleProviderSignInFlow(ITestController controller, LabUser user, UserInformationFieldIds userInformationFieldIds)
        {
            controller.Tap(CoreUiTestConstants.GoogleAccountID, XamarinSelector.ByHtmlIdAttribute);

            controller.EnterText(CoreUiTestConstants.WebUPNB2CGoogleInputID, 20, user.Upn, XamarinSelector.ByHtmlIdAttribute);

            controller.Tap(CoreUiTestConstants.B2CGoogleNextID, XamarinSelector.ByHtmlIdAttribute);

            controller.EnterText(userInformationFieldIds.PasswordInputId, LabUserHelper.GetUserPassword(user), XamarinSelector.ByHtmlIdAttribute);

            controller.Tap(userInformationFieldIds.SignInButtonId, XamarinSelector.ByHtmlIdAttribute);
        }

        public void PerformB2CSignInFlow(ITestController controller, LabUser user, B2CIdentityProvider b2CIdentityProvider, bool isB2CLoginAuthority)
        {
            SetB2CAuthority(controller, true);

            UserInformationFieldIds userInformationFieldIds = CoreMobileTestHelper.DetermineUserInformationFieldIds(user);

            controller.Tap(CoreUiTestConstants.AcquirePageID);

            //Acquire token flow
            controller.Tap(CoreUiTestConstants.AcquireTokenID);

            switch (b2CIdentityProvider)
            {
                case B2CIdentityProvider.Local:
                    PerformB2CLocalAccountSignInFlow(controller, user, userInformationFieldIds);
                    break;
                case B2CIdentityProvider.Google:
                    PerformB2CGoogleProviderSignInFlow(controller, user, userInformationFieldIds);
                    break;

                case B2CIdentityProvider.Facebook:
                    PerformB2CFacebookProviderSignInFlow(controller, user, userInformationFieldIds);
                    break;
                default:
                    throw new InvalidOperationException("B2CIdentityProvider unknown");
            }
            CoreMobileTestHelper.VerifyResult(controller);
        }

        public void PerformB2CSignInEditProfileFlow(ITestController controller, B2CIdentityProvider b2CIdentityProvider)
        {
            SetB2CInputDataForEditProfileAuthority(controller);
            
            controller.Tap(CoreUiTestConstants.AcquirePageID);
            
            SetUiBehavior(controller, CoreUiTestConstants.UIBehaviorNoPrompt);

            //Acquire token flow
            controller.Tap(CoreUiTestConstants.AcquireTokenID);

            controller.Tap(CoreUiTestConstants.B2CEditProfileContinueID, XamarinSelector.ByHtmlIdAttribute);

            CoreMobileTestHelper.VerifyResult(controller);
        }
    }
}
