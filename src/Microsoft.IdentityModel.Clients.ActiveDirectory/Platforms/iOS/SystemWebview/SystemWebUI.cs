﻿//------------------------------------------------------------------------------
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
using System.Threading.Tasks;
using SafariServices;
using UIKit;
using System.Threading;
using Microsoft.Identity.Core.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Identity.Core.UI.SystemWebview
{
    internal class SystemWebUI : WebviewBase, IDisposable
    {
        public RequestContext RequestContext { get; set; }

        public async override Task<AuthorizationResult> AcquireAuthorizationAsync(Uri authorizationUri, Uri redirectUri,
            RequestContext requestContext)
        {
            viewController = null;
            InvokeOnMainThread(() =>
            {
                UIWindow window = UIApplication.SharedApplication.KeyWindow;
                viewController = CoreUIParent.FindCurrentViewController(window.RootViewController);
            });

            returnedUriReady = new SemaphoreSlim(0);
            Authenticate(authorizationUri, redirectUri, requestContext);
            await returnedUriReady.WaitAsync().ConfigureAwait(false);

            //dismiss safariviewcontroller
            viewController.InvokeOnMainThread(() =>
            {
                safariViewController?.DismissViewController(false, null);
            });

            return authorizationResult;
        }

        public void Authenticate(Uri authorizationUri, Uri redirectUri, RequestContext requestContext)
        {
            try
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(12, 0))
                {
                    asWebAuthenticationSession = new AuthenticationServices.ASWebAuthenticationSession(new NSUrl(authorizationUri.AbsoluteUri),
                        redirectUri.Scheme, (callbackUrl, error) =>
                        {
                            if (error != null)
                            {
                                ProcessCompletionHandlerError(error);
                            }
                            else
                            {
                                ContinueAuthentication(callbackUrl.ToString());
                            }
                        });

                    asWebAuthenticationSession.Start();
                }

                else if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    sfAuthenticationSession = new SFAuthenticationSession(new NSUrl(authorizationUri.AbsoluteUri),
                        redirectUri.Scheme, (callbackUrl, error) =>
                        {
                            if (error != null)
                            {
                                ProcessCompletionHandlerError(error);
                            }
                            else
                            {
                                ContinueAuthentication(callbackUrl.ToString());
                            }
                        });

                    sfAuthenticationSession.Start();
                }

                else
                {
                    safariViewController = new SFSafariViewController(new NSUrl(authorizationUri.AbsoluteUri), false);
                    safariViewController.Delegate = this;
                    viewController.InvokeOnMainThread(() =>
                    {
                        viewController.PresentViewController(safariViewController, false, null);
                    });
                }
            }
            catch (Exception ex)
            {
                requestContext.Logger.ErrorPii(ex);
                throw AdalExceptionFactory.GetClientException(
                    CoreErrorCodes.AuthenticationUiFailedError,
                    "Failed to invoke SFSafariViewController",
                    ex);
            }
        }

        public void ProcessCompletionHandlerError(NSError error)
        {
            if (returnedUriReady != null)
            {
                // The authorizationResult is set on the class and sent back to the InteractiveRequest
                // There it's processed in VerifyAuthorizationResult() and an MsalClientException
                // will be thrown.
                authorizationResult = new AuthorizationResult(AuthorizationStatus.UserCancel, null);
                returnedUriReady.Release();
            }
        }

        [Export("safariViewControllerDidFinish:")]
        public void DidFinish(SFSafariViewController controller)
        {
            controller.DismissViewController(true, null);

            if (returnedUriReady != null)
            {
                authorizationResult = new AuthorizationResult(AuthorizationStatus.UserCancel, null);
                returnedUriReady.Release();
            }
        }

        public override void ValidateRedirectUri(Uri redirectUri)
        {
            RedirectUriHelper.Validate(redirectUri, usesSystemBrowser: true);
        }
    }
}