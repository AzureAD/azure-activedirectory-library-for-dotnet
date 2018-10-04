using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Test.Microsoft.Identity.Core.UIAutomation.infrastructure;
using Test.Microsoft.Identity.Core.UIAutomation;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Test.ADAL.NET.UIAutomation
{
    /// <summary>
    /// Contains the core test functionality that will be used by Android and iOS tests
    /// </summary>
	public class CoreMobileADALTests
    {
        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenInteractiveTest(ITestController controller, UserQueryParameters userParams)
		{
            AcquireTokenInteractivly(controller, userParams);
            VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenSilentTest(ITestController controller, UserQueryParameters userParams)
        {
            AcquireTokenInteractivly(controller, userParams);
            VerifyResult(controller);

            //Enter 2nd Resource
            controller.EnterText(UiTestConstants.resourceEntryID, UiTestConstants.Exchange, false);
            controller.DismissKeyboard();

            //Acquire token silently
            controller.Tap(UiTestConstants.AcquireTokenSilentID);

            VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenADFSvXInteractiveTest(ITestController controller, bool isFederated, UserQueryParameters userParams)
        {
            AcquireTokenInteractivly(controller, userParams);
            VerifyResult(controller);
        }

        private static void AcquireTokenInteractivly(ITestController controller, UserQueryParameters userParams)
        {
            var user = prepareForAuthentication(controller, userParams);
            SetInputData(controller, UiTestConstants.UiAutomationTestClientId, UiTestConstants.MSGraph);
            PerformSignInFlow(controller, user);
        }

        private static IUser prepareForAuthentication(ITestController controller, UserQueryParameters userParams)
        {
            //Navigate to second page
            controller.Tap(UiTestConstants.secondPageID);

            //Clear Cache
            controller.Tap(UiTestConstants.ClearCacheID);

            //Get User from Lab
            return controller.GetUser(userParams);
        }

        private static void SetInputData(ITestController controller, string ClientID, string Resource)
        {
            //Enter ClientID
            controller.EnterText(UiTestConstants.clientIdEntryID, ClientID, false);
            controller.DismissKeyboard();

            //Enter Resource
            controller.EnterText(UiTestConstants.resourceEntryID, Resource, false);
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
