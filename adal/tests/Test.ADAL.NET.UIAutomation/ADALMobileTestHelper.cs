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
using Test.Microsoft.Identity.Core.UIAutomation;

namespace Test.ADAL.UIAutomation
{
    /// <summary>
    /// Contains the core test functionality that will be used by Android and iOS tests
    /// </summary>
	public class ADALMobileTestHelper
    {
        private ITestController _testController;
        private CoreMobileTestHelper _coreMobileTestHelper;

        public ADALMobileTestHelper(ITestController testController)
        {
            _testController = testController;
            _coreMobileTestHelper = new CoreMobileTestHelper(testController);
        }

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void AcquireTokenInteractiveTestHelper(UserQueryParameters userParams)
        {
            AcquireTokenInteractiveHelper(userParams);
            _coreMobileTestHelper.VerifyResult();
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void AcquireTokenSilentTestHelper(UserQueryParameters userParams)
        {
            AcquireTokenInteractiveHelper(userParams);
            _coreMobileTestHelper.VerifyResult();

            //Enter 2nd Resource
            _testController.EnterText(CoreUiTestConstants.ResourceEntryID, CoreUiTestConstants.Exchange, false);
            _testController.DismissKeyboard();

            //Acquire token silently
            _testController.Tap(CoreUiTestConstants.AcquireTokenSilentID);

            _coreMobileTestHelper.VerifyResult();
        }

        public void AcquireTokenInteractiveHelper(UserQueryParameters userParams)
        {
            var user = PrepareForAuthentication(userParams);
            SetInputData(CoreUiTestConstants.MSIDLAB4ClientId, CoreUiTestConstants.MSGraph);
            _coreMobileTestHelper.PerformSignInFlow(user);
        }

        public void AcquireTokenWithPromptBehaviorAlwaysHelper(UserQueryParameters userParams)
        {
            var user = PrepareForAuthentication(userParams);
            SetInputData(CoreUiTestConstants.MSIDLAB4ClientId, CoreUiTestConstants.MSGraph);

            // AcquireToken promptBehavior.Auto to get a token in the cache 
            SetPromptBehavior(CoreUiTestConstants.PromptBehaviorAuto);
            _coreMobileTestHelper.PerformSignInFlow(user);

            // AcquireToken promptBehavior.Always. Even with a token, the UI should be shown 
            SetPromptBehavior(CoreUiTestConstants.PromptBehaviorAlways);
            _coreMobileTestHelper.PerformSignInFlow(user);

            // AcquireToken promptBehavior.Auto. No UI should be shown. 
            SetPromptBehavior(CoreUiTestConstants.PromptBehaviorAuto);
            _coreMobileTestHelper.PerformSignInFlowWithoutUI();
            _coreMobileTestHelper.VerifyResult();
        }

        private IUser PrepareForAuthentication(UserQueryParameters userParams)
        {
            //Navigate to second page
            _testController.Tap(CoreUiTestConstants.SecondPageID);

            //Clear Cache
            _testController.Tap(CoreUiTestConstants.ClearCacheID);

            //Get User from Lab
            return _testController.GetUser(userParams);
        }

        private void SetInputData(string clientID, string resource)
        {
            //Enter ClientID
            _testController.EnterText(CoreUiTestConstants.ClientIdEntryID, clientID, false);
            _testController.DismissKeyboard();

            //Enter Resource
            _testController.EnterText(CoreUiTestConstants.ResourceEntryID, resource, false);
            _testController.DismissKeyboard();
        }

        private void SetPromptBehavior(string promptBehavior)
        {
            //Select PromptBehavior 
            _testController.EnterText(CoreUiTestConstants.PromptBehaviorEntryID, promptBehavior, false);
            _testController.DismissKeyboard();
        }
    }
}
