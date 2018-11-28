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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Identity.Client;
using Xamarin.Forms;

namespace XForms
{
    public partial class App : Application
    {
        public static PublicClientApplication MsalPublicClient;
        public static UIParent UIParent { get; set; }
        public const string DefaultClientId = "4b0db8c2-9f26-4417-8bde-3f0e3656f8e0";
        public const string B2cClientId = "e3b9ad76-9763-4827-b088-80c7a7888f79";

        public static string RedirectUriOnAndroid = Microsoft.Identity.Core.Constants.DefaultRedirectUri; // will not work with system browser
        public static string RedirectUriOnIos = "adaliosxformsapp://com.yourcompany.xformsapp";
        public const string RedirectUriB2C = "msale3b9ad76-9763-4827-b088-80c7a7888f79://auth";

        public const string DefaultAuthority = "https://login.microsoftonline.com/common";
        public const string B2cAuthority = "https://login.microsoftonline.com/tfp/msidlabb2c.onmicrosoft.com/B2C_1_SISOPolicy/";
        public const string B2CLoginAuthority = "https://msidlabb2c.b2clogin.com/tfp/msidlabb2c.onmicrosoft.com/B2C_1_SISOPolicy/";
        public const string B2CEditProfilePolicyAuthority = "https://msidlabb2c.b2clogin.com/tfp/msidlabb2c.onmicrosoft.com/B2C_1_ProfileEditPolicy/";

        public static string[] DefaultScopes = {"User.Read"};
        public static string[] B2cScopes = { "https://msidlabb2c.onmicrosoft.com/msidlabb2capi/read" };

        public const bool DefaultValidateAuthority = true;

        public static string Authority = DefaultAuthority;
        public static bool ValidateAuthority = DefaultValidateAuthority;

        public static string ClientId = DefaultClientId;

        public static string[] Scopes = DefaultScopes;

        public App()
        {
            MainPage = new NavigationPage(new XForms.MainPage());

            InitPublicClient();

            Logger.LogCallback = delegate (LogLevel level, string message, bool containsPii)
            {
                Device.BeginInvokeOnMainThread(() => { LogPage.AddToLog("[" + level + "]" + " - " + message, containsPii); });
            };
            Logger.Level = LogLevel.Verbose;
            Logger.PiiLoggingEnabled = true;
        }

        public static void InitPublicClient()
        {
            MsalPublicClient = new PublicClientApplication(ClientId, Authority);

            // Let Android set its own redirect uri
            switch (Device.RuntimePlatform)
            {
                case "iOS":
                    MsalPublicClient.RedirectUri = RedirectUriOnIos;
                    break;
                case "Android":
                    MsalPublicClient.RedirectUri = RedirectUriOnAndroid;
                    break;
            }

            MsalPublicClient.ValidateAuthority = ValidateAuthority;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
