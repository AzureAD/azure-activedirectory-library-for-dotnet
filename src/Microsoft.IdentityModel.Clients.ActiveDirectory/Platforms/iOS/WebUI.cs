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

using Foundation;
using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform
{
    internal class WebUI : IWebUI, IDisposable
    {
        private static SemaphoreSlim returnedUriReady;
        private static AuthorizationResult authorizationResult;
        private PlatformParameters parameters;
        private nint taskId = UIApplication.BackgroundTaskInvalid;
        private NSObject didEnterBackgroundNotification, willEnterForegroundNotification;

        public WebUI(IPlatformParameters parameters)
        {
            this.parameters = parameters as PlatformParameters;
            if (this.parameters == null)
            {
                throw new ArgumentException("parameters should be of type PlatformParameters", "parameters");
            }

            this.didEnterBackgroundNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidEnterBackgroundNotification, OnMoveToBackground);
            this.willEnterForegroundNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillEnterForegroundNotification, OnMoveToForeground);
        }

        void OnMoveToBackground(NSNotification notification)
        {
            //After iOS 11.3, it is neccesary to keep a background task running while moving an app to the background in order to prevent the system from reclaiming network resources from the app. 
            //This will prevent authentication from failing while the application is moved to the background while waiting for MFA to finish.
            this.taskId = UIApplication.SharedApplication.BeginBackgroundTask(() => {
                if (this.taskId != UIApplication.BackgroundTaskInvalid)
                {
                    UIApplication.SharedApplication.EndBackgroundTask(this.taskId);
                    this.taskId = UIApplication.BackgroundTaskInvalid;
                }
            });
        }

        void OnMoveToForeground(NSNotification notification)
        {
            if (this.taskId != UIApplication.BackgroundTaskInvalid)
            {
                UIApplication.SharedApplication.EndBackgroundTask(this.taskId);
                this.taskId = UIApplication.BackgroundTaskInvalid;
            }
        }

        public async Task<AuthorizationResult> AcquireAuthorizationAsync(Uri authorizationUri, Uri redirectUri, CallState callState)
        {
            returnedUriReady = new SemaphoreSlim(0);
            Authenticate(authorizationUri, redirectUri, callState);
            await returnedUriReady.WaitAsync().ConfigureAwait(false);

            this.parameters = null;
            return authorizationResult;
        }

        public static void SetAuthorizationResult(AuthorizationResult authorizationResultInput)
        {
            authorizationResult = authorizationResultInput;
            returnedUriReady.Release();
        }

        public void Authenticate(Uri authorizationUri, Uri redirectUri, CallState callState)
        {
            try
            {
                this.parameters.CallerViewController.InvokeOnMainThread(() =>
                {
                    var navigationController =
                        new AuthenticationAgentUINavigationController(authorizationUri.AbsoluteUri,
                            redirectUri.OriginalString, CallbackMethod, this.parameters.PreferredStatusBarStyle);

                    navigationController.ModalPresentationStyle = this.parameters.ModalPresentationStyle;
                    navigationController.ModalTransitionStyle = this.parameters.ModalTransitionStyle;
                    navigationController.TransitioningDelegate = this.parameters.TransitioningDelegate;

                    this.parameters.CallerViewController.PresentViewController(navigationController, true, null);
                });
            }
            catch (Exception ex)
            {
                this.parameters = null;
                throw new AdalException(AdalError.AuthenticationUiFailed, ex);
            }
        }

        private void CallbackMethod(AuthorizationResult result)
        {
            SetAuthorizationResult(result);
        }

        public void Dispose()
        {
            this.didEnterBackgroundNotification.Dispose();
            this.willEnterForegroundNotification.Dispose();
        }
    }
}
