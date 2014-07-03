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
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    static partial class OAuth2Request
    {
        public static async Task<AuthorizationResult> SendAuthorizeRequestAsync(Authenticator authenticator, string resource, Uri redirectUri, string clientId, UserIdentifier userId, PromptBehavior promptBehavior, string extraQueryParameters, IWebUI webUI, CallState callState)
        {
            if (!string.IsNullOrWhiteSpace(redirectUri.Fragment))
            {
                throw new ArgumentException(AdalErrorMessage.RedirectUriContainsFragment, "redirectUri");
            }
            
            Uri authorizationUri = CreateAuthorizationUri(authenticator, resource, redirectUri, clientId, userId, promptBehavior, extraQueryParameters, await IncludeFormsAuthParamsAsync(), callState);
            return await webUI.AuthenticateAsync(authorizationUri, redirectUri, callState);
        }

        public static async Task<bool> IncludeFormsAuthParamsAsync()
        {
            return PlatformSpecificHelper.IsDomainJoined() && await PlatformSpecificHelper.IsUserLocal();
        }
    }
}