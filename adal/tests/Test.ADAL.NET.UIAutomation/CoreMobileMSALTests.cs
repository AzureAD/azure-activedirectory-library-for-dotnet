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
        private const string DefaultScope = "User.Read";
        private const string AcquirePageID = "Acquire";
        private const string CachePageID = "Cache";
        private const string SettignsPageID = "Settigns";
        private const string AcquireTokenID = "acquireToken";
        private const string AcquireTokenSilentID = "acquireTokenSilent";
        private const string clientIdEntryID = "clientIdEntry";
        private const string ScopesEntryID = "scopesList";
        private const string ClearCacheID = "clearCache";
        private const string SaveID = "saveButton";
        private const string WebUPNInputID = "i0116";
        private const string AdfsV4WebPasswordID = "passwordInput";
        private const string AdfsV4WebSubmitID = "submitButton";
        private const string WebPasswordID = "i0118";
        private const string WebSubmitID = "idSIButton9";
        private const string TestResultID = "testResult";
        private const string TestResultSuccsesfulMessage = "Result: Success";

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenTest(ITestController controller)
        {
            AcquireTokenInteractivly(controller);
            VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenSilentTest(ITestController controller)
        {
            //acquire token for 1st resource
            AcquireTokenInteractivly(controller);
            VerifyResult(controller);

            //acquire token for 2nd resource with refresh token
            SetInputData(controller, UIAutomationAppV2, DefaultScope);
            controller.Tap(AcquireTokenSilentID);
            VerifyResult(controller);
        }

        /// <summary>
        /// Runs through the standard acquire token interactive flow
        /// </summary>
        /// <param name="controller">The test framework that will execute the test interaction</param>
        public static void AcquireTokenADFSvXInteractiveMSALTest(ITestController controller, FederationProvider federationProvider, bool isFederated)
        {
            AcquireTokenInteractivly(controller);
            VerifyResult(controller);
        }

        private static void AcquireTokenInteractivly(ITestController controller)
        {
            var user = prepareForAuthentication(controller);
            SetInputData(controller, UIAutomationAppV2, DefaultScope);
            PerformSignInFlow(controller, user);
        }

        private static IUser prepareForAuthentication(ITestController controller)
        {
            //Clear Cache
            controller.Tap(CachePageID);
            controller.Tap(ClearCacheID);

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
            controller.Tap(SettignsPageID);

            //Enter ClientID
            controller.EnterText(clientIdEntryID, ClientID, false);
            controller.DismissKeyboard();
            controller.Tap(SaveID);

            //Enter Scopes
            controller.Tap(AcquireTokenID);
            controller.EnterText(ScopesEntryID, scopes, false);
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
            //Test results are put into a label that is checked for messages
            Assert.IsTrue(controller.GetText(TestResultID).Contains(TestResultSuccsesfulMessage));
        }
    }
}
