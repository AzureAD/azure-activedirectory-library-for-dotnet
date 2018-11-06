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
using System.Linq;
using System;

namespace Test.MSAL.UIAutomation
{
    /// <summary>
    /// Contains the core test functionality that will be used by Android and iOS tests
    /// </summary>
    public class MSALMobileTestHelper
    {
        CoreMobileTestHelper _coreMobileTestHelper;
        ITestController _testController;

        public MSALMobileTestHelper(ITestController testController)
        {
            _testController = testController;
            _coreMobileTestHelper = new CoreMobileTestHelper(testController);
        }
        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void AcquireTokenInteractiveTestHelper(
            UserQueryParameters userParams,
            string uiBehavior = CoreUiTestConstants.UIBehviorLogin)
        {
            AcquireTokenInteractiveHelper(userParams, uiBehavior);
            _coreMobileTestHelper.VerifyResult();
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public void AcquireTokenSilentTestHelper(UserQueryParameters userParams)
        {
            //acquire token for 1st resource
            AcquireTokenInteractiveHelper(userParams, CoreUiTestConstants.UIBehviorLogin);
            _coreMobileTestHelper.VerifyResult();

            //acquire token for 2nd resource with refresh token
            SetInputData(
                CoreUiTestConstants.UIAutomationAppV2, 
                CoreUiTestConstants.DefaultScope, 
                CoreUiTestConstants.UIBehviorLogin);

            _testController.Tap(CoreUiTestConstants.AcquireTokenSilentID);
            _coreMobileTestHelper.VerifyResult();
        }

        private void AcquireTokenInteractiveHelper(
            UserQueryParameters userParams,
            string uiBehavior)
        {
            var user = PrepareForAuthentication(userParams);
            SetInputData(
                CoreUiTestConstants.UIAutomationAppV2, 
                CoreUiTestConstants.DefaultScope, 
                uiBehavior);
            _coreMobileTestHelper.PerformSignInFlow(user);
        }

        private IUser PrepareForAuthentication(UserQueryParameters userParams)
        {
            //Clear Cache
            _testController.Tap(CoreUiTestConstants.CachePageID);
            _testController.Tap(CoreUiTestConstants.ClearCacheID);

            //Get User from Lab
            return _testController.GetUser(userParams);
        }

        private void SetInputData(string clientId, string scopes, string uiBehavior)
        {
            ValidateUiBehaviorString(uiBehavior);

            _testController.Tap(CoreUiTestConstants.SettingsPageID);

            //Enter ClientID
            _testController.EnterText(CoreUiTestConstants.ClientIdEntryID, clientId, false);
            _testController.DismissKeyboard();
            _testController.Tap(CoreUiTestConstants.SaveID);

            //Enter Scopes
            _testController.Tap(CoreUiTestConstants.AcquireTokenID);
            _testController.EnterText(CoreUiTestConstants.ScopesEntryID, scopes, false);
            _testController.DismissKeyboard();

            // Set UIBehavior
            _testController.Tap(CoreUiTestConstants.UiBehaviorID);
            _testController.Tap(uiBehavior);
        }

        private void ValidateUiBehaviorString(string uiBehavior)
        {
            var okList = new[] {
                CoreUiTestConstants.UIBehviorConsent,
                CoreUiTestConstants.UIBehviorLogin,
                CoreUiTestConstants.UIBehviorSelectAccount };

            bool isInList = okList.Any(item => item.CaseInsensitiveOrdinalEquals(uiBehavior));

            if (!isInList)
            {
                throw new InvalidOperationException("Test Setup Error: invalid uiBehavior " + uiBehavior);
            }
        }
    }
}
