// ------------------------------------------------------------------------------
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
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Extensibility;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2;

namespace Microsoft.Identity.Core.UI
{
    internal class CustomWebUiHandler : IWebUI
    {
        private readonly ICustomWebUi _customWebUi;

        public CustomWebUiHandler(ICustomWebUi customWebUi)
        {
            _customWebUi = customWebUi;
        }

        /// <inheritdoc />
        public async Task<AuthorizationResult> AcquireAuthorizationAsync(
            Uri authorizationUri,
            Uri redirectUri,
            RequestContext requestContext)
        {
            requestContext.Logger.Info("Using custom webUI to acquire an authroization code.");

            try
            {
                requestContext.Logger.InfoPii(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Calling CustomWebUi.AcquireAuthorizationCode with authUri({0}) redirectUri({1})",
                        authorizationUri,
                        redirectUri),
                    "Calling CustomWebUi.AcquireAuthorizationCode");

                var authCodeUri = await _customWebUi.AcquireAuthorizationCodeAsync(authorizationUri, redirectUri)
                                            .ConfigureAwait(false);
                if (authCodeUri == null || String.IsNullOrWhiteSpace(authCodeUri.Query))
                {
                    throw new AdalException(
                        AdalError.CustomWebUiReturnedInvalidUri,
                        CoreErrorMessages.CustomWebUiReturnedInvalidUri);
                }

                if (authCodeUri.Authority.Equals(redirectUri.Authority, StringComparison.OrdinalIgnoreCase) &&
                    authCodeUri.AbsolutePath.Equals(redirectUri.AbsolutePath))
                {
                    IDictionary<string, string> inputQp = CoreHelpers.ParseKeyValueList(
                        authorizationUri.Query.Substring(1),
                        '&',
                        true,
                        null);

                    requestContext.Logger.Info("Redirect Uri was matched. Returning success from CustomWebUiHandler.");
                    return new AuthorizationResult(AuthorizationStatus.Success, authCodeUri.OriginalString);
                }

                throw new AdalException(
                    AdalError.CustomWebUiRedirectUriMismatch,
                    CoreErrorMessages.CustomWebUiRedirectUriMismatch(
                        authCodeUri.AbsolutePath,
                        redirectUri.AbsolutePath));
            }
            catch (OperationCanceledException)
            {

                requestContext.Logger.Info("CustomWebUi AcquireAuthorizationCode was canceled");
                return new AuthorizationResult(AuthorizationStatus.UserCancel, null);
            }
            catch (Exception ex)
            {
                requestContext.Logger.WarningPiiWithPrefix(ex, "CustomWebUi AcquireAuthorizationCode failed");
                throw;
            }
        }


        /// <inheritdoc />
        public void ValidateRedirectUri(Uri redirectUri)
        {
            RedirectUriHelper.Validate(redirectUri, usesSystemBrowser: false);
        }
    }
}
