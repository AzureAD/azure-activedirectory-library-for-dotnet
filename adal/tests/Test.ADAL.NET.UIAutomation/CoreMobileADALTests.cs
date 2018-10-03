using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Test.Microsoft.Identity.Core.UIAutomation.infrastructure;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Test.ADAL.NET.UIAutomation
{
    /// <summary>
    /// Contains the core test functionality that will be used by Android and iOS tests
    /// </summary>
	public class CoreMobileADALTests
    {
        private const string UiAutomationTestClientId = "3c1e0e0d-b742-45ba-a35e-01c664e14b16";
        private const string MSIDLAB4ClientId = "4b0db8c2-9f26-4417-8bde-3f0e3656f8e0";
        private const string MSGraph = "https://graph.microsoft.com";
        private const string Exchange = "https://outlook.office365.com/";
        private const string UiAutomationTestResource = "ae55a6cc-da5e-42f8-b75d-c37e41a1a0d9";
        private const string AcquireTokenID = "acquireToken";
        private const string AcquireTokenSilentID = "acquireTokenSilent";
        private const string clientIdEntryID = "clientIdEntry";
        private const string resourceEntryID = "resourceEntry";
        private const string secondPageID = "secondPage";
        private const string ClearCacheID = "clearCache";
        private const string SaveID = "saveButton";
        private const string WebUPNInputID = "i0116";
        private const string AdfsV4WebPasswordID = "passwordInput";
        private const string AdfsV4WebSubmitID = "submitButton";
        private const string WebPasswordID = "i0118";
        private const string WebSubmitID = "idSIButton9";
        private const string TestResultID = "testResult";
        private const string TestResultSuccsesfulMessage = "Result: Success";
        private const string TestResultFailureMessage = "Result: Failure";

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenInteractiveTest(ITestController controller)
		{
            AcquireTokenInteractivly(controller);
            VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenSilentTest(ITestController controller)
        {
            AcquireTokenInteractivly(controller);
            VerifyResult(controller);

            //Enter 2nd Resource
            controller.EnterText(resourceEntryID, Exchange, false);
            controller.DismissKeyboard();

            //Acquire token silently
            controller.Tap(AcquireTokenSilentID);

            VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenADFSvXInteractiveTest(ITestController controller, FederationProvider federationProvider, bool isFederated)
        {
            AcquireTokenInteractivly(controller);
            VerifyResult(controller);
        }

        private static void AcquireTokenInteractivly(ITestController controller)
        {
            var user = prepareForAuthentication(controller);
            SetInputData(controller, UiAutomationTestClientId, MSGraph);
            PerformSignInFlow(controller, user);
        }

        private static IUser prepareForAuthentication(ITestController controller)
        {
            controller.Tap(secondPageID);

            //Clear Cache
            controller.Tap(ClearCacheID);

            //Get User from Lab
            return controller.GetUser(
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false,
                    IsExternalUser = false
                });
        }

        private static void SetInputData(ITestController controller, string ClientID, string Resource)
        {
            //Enter ClientID
            controller.EnterText(clientIdEntryID, ClientID, false);
            controller.DismissKeyboard();

            //Enter Resource
            controller.EnterText(resourceEntryID, Resource, false);
            controller.DismissKeyboard();
        }

        private static void PerformSignInFlow(ITestController controller, IUser user)
        {
            string passwordInputID = string.Empty;
            string signInButtonID = string.Empty;

            switch (user.FederationProvider)
            {
                case FederationProvider.AdfsV4:
                    passwordInputID = AdfsV4WebPasswordID;
                    signInButtonID = AdfsV4WebSubmitID;
                    break;
                default:
                    passwordInputID = WebPasswordID;
                    signInButtonID = WebSubmitID;
                    break;
            }

            //Acquire token flow
            controller.Tap(AcquireTokenID);
            //i0116 = UPN text field on AAD sign in endpoint
            controller.EnterText(WebUPNInputID, 20, user.Upn, true);
            //idSIButton9 = Sign in button
            controller.Tap(WebSubmitID, true);
            //i0118 = password text field
            controller.EnterText(passwordInputID, ((LabUser)user).GetPassword(), true);
            controller.Tap(signInButtonID, true);
        }

        private static void VerifyResult(ITestController controller)
        {
            //There may be a delay in the amount of time it takes for an authentication request to complete.
            //Thus this method will check the result once a second for 10 seconds.
            bool done = false;
            int attempts = 0;
            int maximumAttempts = 20;

            while (!done)
            {
                Thread.Sleep(1000);
                attempts++;

                //Test results are put into a label that is checked for messages
                var result = controller.GetText(TestResultID);
                if (result.Contains(TestResultSuccsesfulMessage))
                {
                    return;
                }
                else if (result.Contains(TestResultFailureMessage))
                {
                    Assert.Fail();
                    return;
                }
                else if (attempts > maximumAttempts)
                    done = true;
            }
        }
    }
}
