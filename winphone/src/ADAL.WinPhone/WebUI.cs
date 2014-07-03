﻿//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;

using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class WebUI : IWebUI
    {
        private readonly bool useCorporateNetwork;

        public WebUI(bool useCorporateNetwork)
        {
            this.useCorporateNetwork = useCorporateNetwork;
        }

        public void Authenticate(Uri authorizationUri, Uri redirectUri, CallState callState, IDictionary<string, object> headersMap)
        {
            ValueSet set = new ValueSet();
            foreach (string key in headersMap.Keys)
            {
                set[key] = headersMap[key];
            }

            WebAuthenticationOptions options = WebAuthenticationOptions.None;

            if (redirectUri.AbsoluteUri == WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri)
            {
                if (this.useCorporateNetwork)
                {
                    // SSO Mode with CorporateNetwork
                    options = WebAuthenticationOptions.UseCorporateNetwork;
                }
                else
                {                
                    // SSO Mode
                    options = WebAuthenticationOptions.None;
                }

                try
                {
                    WebAuthenticationBroker.AuthenticateAndContinue(authorizationUri, null, set, options);
                }
                catch (FileNotFoundException ex)
                {
                    throw new ActiveDirectoryAuthenticationException(ActiveDirectoryAuthenticationError.AuthenticationUiFailed, ex);
                }
            }
            else if (redirectUri.Scheme == "ms-app")
            {
                throw new ArgumentException(ActiveDirectoryAuthenticationErrorMessage.RedirectUriAppIdMismatch, "redirectUri");
            }
            else
            {
                try
                {
                    // Non-SSO Mode
                    WebAuthenticationBroker.AuthenticateAndContinue(authorizationUri, redirectUri, set, options);
                }
                catch (FileNotFoundException ex)
                {
                    throw new ActiveDirectoryAuthenticationException(ActiveDirectoryAuthenticationError.AuthenticationUiFailed, ex);
                }
            }
        }
    }
}
