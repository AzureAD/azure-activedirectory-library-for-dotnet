//This is a temp location for this file until the visual studio bug that prevents these tests from working in the MSAL project is resolved.
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Microsoft.Identity.Core.UIAutomation.infrastructure;

namespace Test.MSAL.NET.UIAutomation
{
    /// <summary>
    /// Contains the core test functionality that will be used by Android and iOS tests
    /// </summary>
    public static class CoreMobileMSALTests
    {

        private const string MSGraph = "https://graph.microsoft.com";
        private const string UIAutomationAppV2 = "1e245a30-49aa-43eb-b9c1-c11b072cc92b";

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenTest(ITestController controller)
        {
            var user = prepareForAuthentication(controller);

            SetInputData(controller, UIAutomationAppV2, "User.Read");

            PerformSignInFlowFlow(controller, user);

            //Verify result. Test results are put into a label
            Assert.IsTrue(controller.GetText("testResult").Contains("Result: Success"));
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenSilentTest(ITestController controller)
        {
            var user = prepareForAuthentication(controller);

            SetInputData(controller, UIAutomationAppV2, "User.Read");

            PerformSignInFlowFlow(controller, user);

            Assert.IsTrue(controller.GetText("testResult").Contains("Result: Success"));

            SetInputData(controller, UIAutomationAppV2, "User.Write");

            controller.Tap("acquireTokenSilent");

            //Verify result. Test results are put into a label
            Assert.IsTrue(controller.GetText("testResult").Contains("Result: Success"));
        }

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenADFSvXInteractiveMSALTest(ITestController controller, FederationProvider federationProvider, bool isFederated)
        {
            var user = prepareForAuthentication(controller);

            SetInputData(controller, UIAutomationAppV2, "User.Read");

            PerformSignInFlowFlow(controller, user);

            //Verify result. Test results are put into a label
            Assert.IsTrue(controller.GetText("testResult") == "Result: Success");
        }

        private static IUser prepareForAuthentication(ITestController controller)
        {
            //Clear Cache
            controller.Tap("Cache");
            controller.Tap("clearCache");

            //Get User from Lab
            return controller.GetUser(
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });
        }

        private static void SetInputData(ITestController controller, string ClientID, string scopes)
        {
            controller.Tap("Settings");

            //Enter ClientID
            controller.EnterText("clientIdEntry", ClientID, false);
            controller.DismissKeyboard();
            controller.Tap("saveButton");

            //Enter Scopes
            controller.Tap("Acquire");
            controller.EnterText("scopesList", scopes, false);
            controller.DismissKeyboard();
        }

        private static void PerformSignInFlowFlow(ITestController controller, IUser user)
        {
            string passwordInputID = "";
            string signInButtonID = "";

            switch (user.FederationProvider)
            {
                case FederationProvider.AdfsV4:
                    passwordInputID = "passwordInput";
                    signInButtonID = "submitButton";
                    break;
                default:
                    passwordInputID = "i0118";
                    signInButtonID = "idSIButton9";
                    break;
            }

            //Acquire token flow
            controller.Tap("acquireToken");
            //i0116 = UPN text field on AAD sign in endpoint
            controller.EnterText("i0116", 20, user.Upn, true);
            //idSIButton9 = Sign in button
            controller.Tap("idSIButton9", true);
            //i0118 = password text field
            controller.EnterText(passwordInputID, ((LabUser)user).GetPassword(), true);
            controller.Tap(signInButtonID, true);
        }
    }
}
