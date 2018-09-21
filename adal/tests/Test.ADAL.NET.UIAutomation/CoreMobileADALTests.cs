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

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenInteractiveTest(ITestController controller)
		{
            var user = prepareForAuthentication(controller);

            // Create a string array with the lines of text
            string[] lines = { "First line", "Second line", "Third line" };

            // Set a variable to the My Documents path.
            string mydocpath =
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(mydocpath, "WriteLines.txt")))
            {
                foreach (string line in lines)
                    outputFile.WriteLine(line);
            }

            SetInputData(controller, UiAutomationTestClientId, MSGraph);

            PerformSignInFlowFlow(controller, user);

            //Verify result. Test results are put into a label
            Assert.IsTrue(controller.GetText("testResult") == "Result: Success");
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenSilentTest(ITestController controller)
        {
            //Get User from Lab
            var user = prepareForAuthentication(controller);

            SetInputData(controller, UiAutomationTestClientId, MSGraph);

            PerformSignInFlowFlow(controller, user);

            //Enter 2nd Resource
            controller.EnterText("resourceEntry", Exchange, false);
            controller.DismissKeyboard();

            //Acquire token silently
            controller.Tap("acquireTokenSilent");

            //Verify result. Test results are put into a label
            Assert.IsTrue(controller.GetText("testResult") == "Result: Success");
        }

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenADFSvXInteractiveTest(ITestController controller, FederationProvider federationProvider, bool isFederated)
        {
            //Get User from Lab
            var user = prepareForAuthentication(controller);

            SetInputData(controller, UiAutomationTestClientId, MSGraph);

            PerformSignInFlowFlow(controller, user);

            //Verify result. Test results are put into a label
            Assert.IsTrue(controller.GetText("testResult") == "Result: Success");
        }

        private static IUser prepareForAuthentication(ITestController controller)
        {
            controller.Tap("secondPage");

            //Clear Cache
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

        private static void SetInputData(ITestController controller, string ClientID, string Resource)
        {
            //Enter ClientID
            controller.EnterText("clientIdEntry", ClientID, false);
            controller.DismissKeyboard();

            //Enter Resource
            controller.EnterText("resourceEntry", Resource, false);
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
            controller.EnterText("i0116", user.Upn, true);
            //idSIButton9 = Sign in button
            controller.Tap("idSIButton9", true);
            //i0118 = password text field
            controller.EnterText(passwordInputID, ((LabUser)user).GetPassword(), true);
            controller.Tap(signInButtonID, true);
        }
    }
}
