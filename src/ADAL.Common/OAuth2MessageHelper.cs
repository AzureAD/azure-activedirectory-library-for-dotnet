//----------------------------------------------------------------------
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

#if !ADAL_WINRT
using System.Security;
#endif
using System;
using System.Text;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal static partial class OAuth2MessageHelper
    {
        private const string FormsAuthParameter = "amr_values";
        private const string LoginHintParameter = "login_hint"; // login_hint is not standard oauth2 parameter
        private const string CorrelationIdParameter = OAuthHeader.CorrelationId; // correlation id is not standard oauth2 parameter
        private const string PromptParameter = "prompt"; // prompt is not standard oauth2 parameter
        private const string FormsAuthValue = "pwd";
        private const string PromptLoginValue = "login";
        private const string PromptRefreshSessionValue = "refresh_session";
        private const string ScopeOpenIdValue = "openid";

        public static RequestParameters CreateAuthorizationRequest(string resource, string clientId, Uri redirectUri, string loginHint, PromptBehavior promptBehavior, string extraQueryParameters, bool includeFormsAuthParam, CallState callState)
        {
            RequestParameters parameters = new RequestParameters();
            parameters[OAuthParameter.ResponseType] = OAuthResponseType.Code;
            parameters[OAuthParameter.Resource] = resource;
            parameters[OAuthParameter.ClientId] = clientId;
            parameters[OAuthParameter.RedirectUri] = redirectUri.AbsoluteUri;

            if (!string.IsNullOrWhiteSpace(loginHint))
            {
                parameters[LoginHintParameter] = loginHint;
            }

            if (callState != null && callState.CorrelationId != Guid.Empty)
            {
                parameters[CorrelationIdParameter] = callState.CorrelationId.ToString();
            }

            // ADFS currently ignores the parameter for now.
            if (promptBehavior == PromptBehavior.Always)
            {
                parameters[PromptParameter] = PromptLoginValue;
            }
            else if (promptBehavior == PromptBehavior.RefreshSession)
            {
                parameters[PromptParameter] = PromptRefreshSessionValue;
            }

            if (!string.IsNullOrWhiteSpace(extraQueryParameters))
            {
                if (extraQueryParameters[0] == '&')
                {
                    extraQueryParameters = extraQueryParameters.Substring(1);
                }

                parameters.ExtraQueryParameter = extraQueryParameters;
            }

            if (includeFormsAuthParam)
            {
                parameters[FormsAuthParameter] = FormsAuthValue;
            }

            AdalIdHelper.AddAsQueryParameters(parameters);

            return parameters;
        }


        public static RequestParameters CreateTokenRequest(string code, Uri redirectUri, string resource, string clientId)
        {
            RequestParameters parameters = new RequestParameters();
            parameters[OAuthParameter.GrantType] = OAuthGrantType.AuthorizationCode;
            parameters[OAuthParameter.Code] = code;
            parameters[OAuthParameter.ClientId] = clientId;
            parameters[OAuthParameter.RedirectUri] = redirectUri.AbsoluteUri;

            AddOptionalParameterResource(parameters, resource);

            return parameters;
        }

        public static RequestParameters CreateTokenRequest(string resource, string clientId, UserAssertion credential)
        {
            RequestParameters parameters = new RequestParameters();
            parameters[OAuthParameter.GrantType] = credential.AssertionType;
            parameters[OAuthParameter.Resource] = resource;
            parameters[OAuthParameter.ClientId] = clientId;
            parameters[OAuthParameter.Assertion] = Convert.ToBase64String(Encoding.UTF8.GetBytes(credential.Assertion));
        
            // To request id_token in response
            parameters[OAuthParameter.Scope] = ScopeOpenIdValue;

            return parameters;
        }

        public static RequestParameters CreateTokenRequest(string resource, string clientId, UserCredential credential)
        {
            RequestParameters parameters = new RequestParameters();
            parameters[OAuthParameter.GrantType] = OAuthGrantType.Password;
            parameters[OAuthParameter.Resource] = resource;
            parameters[OAuthParameter.ClientId] = clientId;
            parameters[OAuthParameter.Username] = credential.UserName;
#if ADAL_WINRT
            parameters[OAuthParameter.Password] = credential.Password;
#else
            if (credential.SecurePassword != null)
            {
                parameters.AddSecureParameter(OAuthParameter.Password, credential.SecurePassword);
            }
            else
            {
                parameters[OAuthParameter.Password] = credential.Password;
            }
#endif

            // To request id_token in response
            parameters[OAuthParameter.Scope] = ScopeOpenIdValue;

            return parameters;
        }

        public static RequestParameters CreateTokenRequest(string resource, string refreshToken, ClientKey clientKey, string audience)
        {
            RequestParameters parameters = new RequestParameters();
            parameters[OAuthParameter.GrantType] = OAuthGrantType.RefreshToken;
            parameters[OAuthParameter.RefreshToken] = refreshToken;

            AddClientKey(parameters, clientKey, audience);
            AddOptionalParameterResource(parameters, resource);

            return parameters;
        }

        private static void AddOptionalParameterResource(RequestParameters parameters, string resource)
        {
            if (!string.IsNullOrWhiteSpace(resource))
            {
                parameters[OAuthParameter.Resource] = resource;
            }            
        }
    }
}
