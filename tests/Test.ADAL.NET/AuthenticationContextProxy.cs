﻿//----------------------------------------------------------------------
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
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Test.ADAL.NET.Friend;
using Timer = System.Timers.Timer;

namespace Test.ADAL.Common
{
    public enum PageType
    {
        Dashboard,
        DashboardResponse,
        Wab,
        WabError,
        Unknown
    }

    internal partial class AuthenticationContextProxy
    {
        private const string NotSpecified = "NotSpecified";

        private readonly UserIdentifier NotSpecifiedUserId = new UserIdentifier(NotSpecified, UserIdentifierType.UniqueId);

        private static string userName;
        private static string password;
        private static SecureString securePassword;

        public AuthenticationContextProxy(string authority)
        {
            this.context = new AuthenticationContext(authority);
            this.context.CorrelationId = new Guid(FixedCorrelationId);
        }

        public AuthenticationContextProxy(string authority, bool validateAuthority)
        {
            this.context = new AuthenticationContext(authority, validateAuthority);
            this.context.CorrelationId = new Guid(FixedCorrelationId);
        }

        public AuthenticationContextProxy(string authority, bool validateAuthority, TokenCacheType tokenCacheType)
        {
            TokenCache tokenCache = null;
            if (tokenCacheType == TokenCacheType.InMemory)
            {
                tokenCache = new TokenCache();
            }

            this.context = new AuthenticationContext(authority, validateAuthority, tokenCache);
            this.context.CorrelationId = new Guid(FixedCorrelationId);
        }

        public static void InitializeTest()
        {
            ClearDefaultCache();
        }

        public static void ClearDefaultCache()
        {
            var dummyContext = new AuthenticationContext("https://dummy/dummy", false);
            dummyContext.TokenCache.Clear();
        }

        public static void SetEnvironmentVariable(string environmentVariable, string environmentVariableValue)
        {
            Environment.SetEnvironmentVariable(environmentVariable, environmentVariableValue);
        }

        public static void SetCredentials(string userNameIn, string passwordIn)
        {
            userName = userNameIn;
            password = passwordIn;
        }

        public static void SetSecureCredentials(string userNameIn, SecureString passwordIn)
        {
            userName = userNameIn;
            securePassword = passwordIn;
        }


        public static SecureString convertToSecureString(string strPassword)
        {
            var secureStr = new SecureString();
            if (strPassword.Length > 0)
            {
                foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            }
            return secureStr;
        }

        public static void Delay(int sleepMilliSeconds)
        {
            if (RecorderSettings.Mode == RecorderMode.Record || !RecorderSettings.Mock)
            {
                Thread.Sleep(sleepMilliSeconds);
            }
        }

        public void SetCorrelationId(Guid correlationId)
        {
            this.context.CorrelationId = correlationId;
        }

        public async Task<AuthenticationResultProxy> AcquireTokenAsync(string resource, string clientId, Uri redirectUri, IPlatformParameters parameters)
        {
            return await RunTaskInteractiveAsync(resource, clientId, redirectUri, parameters, UserIdentifier.AnyUser, null);
        }

        public async Task<AuthenticationResultProxy> AcquireTokenAsync(string resource, string clientId, Uri redirectUri, IPlatformParameters parameters, UserIdentifier userId)
        {
            return await RunTaskInteractiveAsync(resource, clientId, redirectUri, parameters, userId, null);
        }

        public async Task<AuthenticationResultProxy> AcquireTokenAsync(string resource, string clientId, Uri redirectUri, IPlatformParameters parameters, UserIdentifier userId, string extraQueryParameters)
        {
            return await RunTaskInteractiveAsync(resource, clientId, redirectUri, parameters, userId, extraQueryParameters);
        }

        private async Task<AuthenticationResultProxy> RunTaskAsync(Task<AuthenticationResult> task)
        {
            AuthenticationResultProxy resultProxy;

            try
            {
                AuthenticationResult result = await task;
                resultProxy = GetAuthenticationResultProxy(result);
            }
            catch (Exception ex)
            {
                resultProxy = GetAuthenticationResultProxy(ex);            
            }

            return resultProxy;
        }

        private async Task<AuthenticationResultProxy> RunTaskInteractiveAsync(string resource, string clientId, Uri redirectUri, IPlatformParameters authorizationParameters, UserIdentifier userId, string extraQueryParameters, int retryCount = 0)
        {
            AuthenticationResultProxy resultProxy;
            bool exceptionOccured = false;

            PromptBehavior promptBehavior = (authorizationParameters as PlatformParameters).PromptBehavior;

            try
            {
                AuthenticationResult result = null;
                using (Timer abortTest = new Timer(10 * 1000)) // 10 seconds for test execution
                {
                    using (Timer uiSupply = new Timer(1500))
                    {
                        if (userName != null || password != null)
                        {
                            uiSupply.Elapsed += UiSupplyEventHandler;
                        }

                        abortTest.Elapsed += (sender, e) => UiAbortEventHandler(sender, e, uiSupply);

                        uiSupply.Start();
                        abortTest.Start();

                        if (userId != null && !ReferenceEquals(userId, UserIdentifier.AnyUser) && userId.Id == NotSpecified)
                        {
                            result = await context.AcquireTokenAsync(resource, clientId, redirectUri, new PlatformParameters(promptBehavior, null));
                        }
                        else
                        {
                            if (extraQueryParameters == NotSpecified)
                            {
                                result = await context.AcquireTokenAsync(resource, clientId, redirectUri, new PlatformParameters(promptBehavior, null), userId);
                            }
                            else
                            {
                                result = await context.AcquireTokenAsync(resource, clientId, redirectUri, new PlatformParameters(promptBehavior, null), userId, extraQueryParameters);
                            }
                        }

                        abortTest.Stop();
                        uiSupply.Stop();
                    }
                }

                resultProxy = GetAuthenticationResultProxy(result);
            }
            catch (Exception ex)
            {
                resultProxy = GetAuthenticationResultProxy(ex);
                if (resultProxy.ExceptionStatusCode == 503 && retryCount < 5)
                {
                    Thread.Sleep(3000);
                    Log.Comment(string.Format(CultureInfo.CurrentCulture, " Retry #{0}...", retryCount + 1));
                    exceptionOccured = true;
                }
            }

            if (exceptionOccured)
            {
                return await RunTaskInteractiveAsync(resource, clientId, redirectUri, authorizationParameters, userId, extraQueryParameters, retryCount + 1);                
            }

            return resultProxy;
        }

        private async Task<AuthenticationResultProxy> AcquireAccessCodeAsync(string resource, string clientId, Uri redirectUri, UserIdentifier userId, string extraQueryParameters, int retryCount = 0)
        {
            AuthenticationResultProxy resultProxy;
            bool exceptionOccured = false;

            try
            {
                using (Timer abortTest = new Timer(10 * 1000)) // 10 seconds for test execution
                {
                    using (Timer uiSupply = new Timer(1500))
                    {
                        if (userName != null || password != null)
                        {
                            uiSupply.Elapsed += UiSupplyEventHandler;
                        }

                        abortTest.Elapsed += (sender, e) => UiAbortEventHandler(sender, e, uiSupply);

                        uiSupply.Start();
                        abortTest.Start();

                        string authorizationCode = await AdalFriend.AcquireAccessCodeAsync(this.context, resource, clientId,
                            redirectUri, userId);
                        return new AuthenticationResultProxy() { AccessToken = authorizationCode };
                    }
                }
            }
            catch (Exception ex)
            {
                resultProxy = GetAuthenticationResultProxy(ex);
                if (resultProxy.ExceptionStatusCode == 503 && retryCount < 5)
                {
                    Thread.Sleep(3000);
                    Log.Comment(string.Format(CultureInfo.CurrentCulture, " Retry #{0}...", retryCount + 1));
                    exceptionOccured = true;
                }
            }

            if (exceptionOccured)
            {
                return await AcquireAccessCodeAsync(resource, clientId, redirectUri, userId, extraQueryParameters, retryCount + 1);
            }

            return resultProxy;
        }

        public async Task<string> AcquireAccessCodeAsync(string resource, string clientId, Uri redirectUri, UserIdentifier userId)
        {
            AuthenticationResultProxy result = await AcquireAccessCodeAsync(resource, clientId, redirectUri, userId, null);
            return result.AccessToken;
        }

        public delegate void UiSupplyDelegate(WindowsFormsWebAuthenticationDialog dialog);

        private void UiSupplyEventHandler(object sender, ElapsedEventArgs e)
        {
            WindowsFormsWebAuthenticationDialog webAuthenticationDialog = this.GetWebAuthenticationDialog(5000);
            webAuthenticationDialog.BeginInvoke(new UiSupplyDelegate(UiSupply), webAuthenticationDialog);
        }

        private void UiSupply(WindowsFormsWebAuthenticationDialog webAuthenticationDialog)
        {
            if (webAuthenticationDialog != null)
            {
                WebBrowser webBrowser = ((WindowsFormsWebAuthenticationDialog)webAuthenticationDialog).WebBrowser;
                DialogHandler handler = new DialogHandler();
                handler.EnterInput(userName, password);

                UISupplier supplier = new UISupplier();
                UISupplier.Results result = supplier.SupplyUIStep(webBrowser, userName, password);
                if (result == UISupplier.Results.Error)
                {
                    ((Form)webBrowser.Parent.Parent).Close();
                }
            }
        }

        private void UiAbortEventHandler(object sender, ElapsedEventArgs e, Timer uiSupply)
        {
            Log.Comment("Test execution timeout");
            WindowsFormsWebAuthenticationDialog webAuthenticationDialog = this.GetWebAuthenticationDialog(1000);
            if (webAuthenticationDialog != null)
            {
                ((Form)(webAuthenticationDialog).WebBrowser.Parent.Parent).Close();
            }

            uiSupply.Stop();
        }

        private WindowsFormsWebAuthenticationDialog GetWebAuthenticationDialog(int totalWaitMilliseconds)
        {
            WindowsFormsWebAuthenticationDialog webAuthenticationDialog = null;
            const int EachWaitMilliseconds = 200;
            do
            {
                try
                {
                    webAuthenticationDialog = Enumerable.OfType<WindowsFormsWebAuthenticationDialog>(Application.OpenForms).Single();
                }
                catch (InvalidOperationException)
                {
                    Verify.Fail("Unable to find auth dialog");
                }

                if (webAuthenticationDialog == null)
                {
                    Thread.Sleep(EachWaitMilliseconds);
                    totalWaitMilliseconds -= EachWaitMilliseconds;
                }
            }
            while (totalWaitMilliseconds > 0 && webAuthenticationDialog == null);

            return webAuthenticationDialog;
        }
    }
}
