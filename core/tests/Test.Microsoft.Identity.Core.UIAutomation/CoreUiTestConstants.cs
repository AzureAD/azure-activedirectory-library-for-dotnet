﻿//----------------------------------------------------------------------
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

namespace Test.Microsoft.Identity.Core.UIAutomation
{
    public static class CoreUiTestConstants
    {
        //Applications
        public const string UiAutomationTestClientId = "3c1e0e0d-b742-45ba-a35e-01c664e14b16";
        public const string MSIDLAB4ClientId = "4b0db8c2-9f26-4417-8bde-3f0e3656f8e0"; // clientId is multi-tenant
        public const string UIAutomationAppV2 = "1e245a30-49aa-43eb-b9c1-c11b072cc92b";

        //Resources
        public const string MSGraph = "https://graph.microsoft.com";
        public const string Exchange = "https://outlook.office365.com/";
        public const string UiAutomationTestResource = "ae55a6cc-da5e-42f8-b75d-c37e41a1a0d9";

        //ADAL & MSAL test app
        public const string AcquireTokenID = "acquireToken";
        public const string AcquireTokenWithPromptBehaviorAlwaysID = "acquireTokenPromptBehaviorAlways";
        public const string AcquireTokenSilentID = "acquireTokenSilent";
        public const string ClientIdEntryID = "clientIdEntry";
        public const string ResourceEntryID = "resourceEntry";
        public const string PromptBehaviorEntryID = "promptBehaviorEntry";
        public const string PromptBehaviorAuto = "auto";
        public const string PromptBehaviorAlways = "always";
        public const string SecondPageID = "secondPage";
        public const string ClearCacheID = "clearCache";
        public const string SaveID = "saveButton";
        public const string WebUPNInputID = "i0116";
        public const string WebUPNB2CLocalInputID = "logonIdentifier";
        public const string AdfsV4WebPasswordID = "passwordInput";
        public const string AdfsV4WebSubmitID = "submitButton";
        public const string WebPasswordID = "i0118";
        public const string WebSubmitID = "idSIButton9";
        public const string TestResultID = "testResult";
        public const string TestResultSuccsesfulMessage = "Result: Success";
        public const string TestResultFailureMessage = "Result: Failure";

        //MSAL test app
        public const string DefaultScope = "User.Read";
        public const string AcquirePageID = "Acquire";
        public const string CachePageID = "Cache";
        public const string SettingsPageID = "Settings";
        public const string ScopesEntryID = "scopesList";
        public const string UiBehaviorPickerID = "uiBehavior";
        public const string SelectUser = "userList";
        public const string UserNotSelected = "not selected";
        public const string UserMissingFromResponse = "Missing from the token response";
        public const string RedirectUriOnAndroid = "urn:ietf:wg:oauth:2.0:oob";
        public const string RedirectUriEntryID = "redirectUriEntry";

        //MSAL B2C
        public static string B2cScopes = "https://sometenant.onmicrosoft.com/some/scope";
        public const string AuthorityPickerID = "b2cAuthorityPicker";
        public const string B2CWebSubmitID = "next";
        public const string B2CWebPasswordID = "password";
        public const string B2CLoginAuthority = "b2clogin.com";
        public const string MicrosoftOnlineAuthority = "login.microsoftonline.com";
        public const string NonB2CAuthority = "non-b2c authority";

        // these should match the product enum values
        public const string UIBehaviorConsent = "consent";
        public const string UIBehaviorSelectAccount = "select_account";
        public const string UIBehaviorLogin = "login";

        //Test Constants
        public const int ResultCheckPolliInterval = 1000;
        public const int MaximumResultCheckRetryAttempts = 20;

    }
}
