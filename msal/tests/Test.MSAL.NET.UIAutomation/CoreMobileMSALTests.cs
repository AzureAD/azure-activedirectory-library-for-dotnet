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

namespace Test.MSAL.NET.UIAutomation
{
    /// <summary>
    /// Contains the core test functionality that will be used by Android and iOS tests
    /// </summary>
    public static class CoreMobileMSALTests
    {
        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenTest(ITestController controller, UserQueryParameters userParams)
        {
            AcquireTokenInteractivly(controller, userParams);
            VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenSilentTest(ITestController controller, UserQueryParameters userParams)
        {
            //acquire token for 1st resource
            AcquireTokenInteractivly(controller, userParams);
            VerifyResult(controller);

            //acquire token for 2nd resource with refresh token
            SetInputData(controller, UiTestConstants.UIAutomationAppV2, UiTestConstants.DefaultScope);
            controller.Tap(UiTestConstants.AcquireTokenSilentID);
            VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenADFSvXInteractiveMSALTest(ITestController controller, bool isFederated, UserQueryParameters userParams)
        {
            AcquireTokenInteractivly(controller, userParams);
            VerifyResult(controller);
        }

        private static void AcquireTokenInteractivly(ITestController controller, UserQueryParameters userParams)
        {
            var user = prepareForAuthentication(controller, userParams);
            SetInputData(controller, UiTestConstants.UIAutomationAppV2, UiTestConstants.DefaultScope);
            PerformSignInFlow(controller, user);
        }

        private static IUser prepareForAuthentication(ITestController controller, UserQueryParameters userParams)
        {
            //Clear Cache
            controller.Tap(UiTestConstants.CachePageID);
            controller.Tap(UiTestConstants.ClearCacheID);

            //Get User from Lab
            return controller.GetUser(userParams);
        }

        private static void SetInputData(ITestController controller, string ClientID, string scopes)
        {
            controller.Tap(UiTestConstants.SettignsPageID);

            //Enter ClientID
            controller.EnterText(UiTestConstants.clientIdEntryID, ClientID, false);
            controller.DismissKeyboard();
            controller.Tap(UiTestConstants.SaveID);

            //Enter Scopes
            controller.Tap(UiTestConstants.AcquireTokenID);
            controller.EnterText(UiTestConstants.ScopesEntryID, scopes, false);
            controller.DismissKeyboard();
        }

        private static void PerformSignInFlow(ITestController controller, IUser user)
        {
            string passwordInputID = string.Empty;
            string signInButtonID = string.Empty;

            switch (user.FederationProvider)
            {
                case FederationProvider.AdfsV4:
                    passwordInputID = UiTestConstants.AdfsV4WebPasswordID;
                    signInButtonID = UiTestConstants.AdfsV4WebSubmitID;
                    break;
                default:
                    passwordInputID = UiTestConstants.WebPasswordID;
                    signInButtonID = UiTestConstants.WebSubmitID;
                    break;
            }

            //Acquire token flow
            controller.Tap(UiTestConstants.AcquireTokenID);
            //i0116 = UPN text field on AAD sign in endpoint
            controller.EnterText(UiTestConstants.WebUPNInputID, 20, user.Upn, true);
            //idSIButton9 = Sign in button
            controller.Tap(UiTestConstants.WebSubmitID, true);
            //i0118 = password text field
            controller.EnterText(passwordInputID, ((LabUser)user).GetPassword(), true);
            controller.Tap(signInButtonID, true);
        }

        private static void VerifyResult(ITestController controller)
        {
            //Test results are put into a label that is checked for messages
            Assert.IsTrue(controller.GetText(UiTestConstants.TestResultID).Contains(UiTestConstants.TestResultSuccsesfulMessage));
        }
    }
}
