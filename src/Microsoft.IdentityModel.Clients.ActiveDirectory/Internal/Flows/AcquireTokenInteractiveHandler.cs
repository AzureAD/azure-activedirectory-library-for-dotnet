//----------------------------------------------------------------------
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
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.UI;
using Microsoft.Identity.Core.Cache;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;
using Microsoft.Identity.Core.Http;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Flows
{
    internal class AcquireTokenInteractiveHandler : AcquireTokenHandlerBase
    {
        internal AuthorizationResult authorizationResult;

        private readonly Uri redirectUri;

        private readonly string redirectUriRequestParameter;

        private readonly IPlatformParameters authorizationParameters;

        private readonly string extraQueryParameters;

        private readonly IWebUI webUi;

        private readonly UserIdentifier userId;

        private readonly string claims;


        public AcquireTokenInteractiveHandler(
            RequestData requestData, 
            Uri redirectUri, 
            IPlatformParameters platformParameters,
            UserIdentifier userId, 
            string extraQueryParameters, 
            //IWebUI webUI, 
            string claims)
            : base(requestData)
        {
            this.redirectUri = ComputeAndValidateRedirectUri(redirectUri, ClientKey?.ClientId);
            redirectUriRequestParameter = PlatformProxyFactory.GetPlatformProxy().GetBrokerOrRedirectUri(this.redirectUri);

            authorizationParameters = platformParameters;
            this.userId = userId ?? throw new ArgumentNullException(nameof(userId), AdalErrorMessage.SpecifyAnyUser);

            if (!string.IsNullOrEmpty(extraQueryParameters) && extraQueryParameters[0] == '&')
            {
                extraQueryParameters = extraQueryParameters.Substring(1);
            }

            this.extraQueryParameters = extraQueryParameters;
            webUi = CreateWebUIOrNull(platformParameters);
            UniqueId = userId.UniqueId;
            DisplayableId = userId.DisplayableId;
            UserIdentifierType = userId.Type;
            SupportADFS = true;

            if (!string.IsNullOrEmpty(claims))
            {
                LoadFromCache = false;
                RequestContext.Logger.Verbose("Claims present. Skip cache lookup.");
                this.claims = claims;
            }
            else
            {
                var platformInformation = new PlatformInformation();
                LoadFromCache = requestData.TokenCache != null && platformParameters != null && platformInformation.GetCacheLoadPolicy(platformParameters);
            }

            brokerParameters[BrokerParameter.Force] = "NO";
            if (userId != UserIdentifier.AnyUser)
            {
                brokerParameters[BrokerParameter.Username] = userId.Id;
            }
            else
            {
                brokerParameters[BrokerParameter.Username] = string.Empty;
            }
            brokerParameters[BrokerParameter.UsernameType] = userId.Type.ToString();

            brokerParameters[BrokerParameter.RedirectUri] = this.redirectUri.AbsoluteUri;
            brokerParameters[BrokerParameter.ExtraQp] = extraQueryParameters;
            brokerParameters[BrokerParameter.Claims] = claims;
            brokerHelper.PlatformParameters = authorizationParameters;
        }

        private IWebUI CreateWebUIOrNull(IPlatformParameters parameters)
        {
            if (parameters == null)
            {
                return null;
            }

            if (!(parameters is PlatformParameters parametersObj))
            {
                throw new ArgumentException("Objects implementing IPlatformParameters should be of type PlatformParameters");
            }

            return WebUIFactoryProvider.WebUIFactory.CreateAuthenticationDialog(
                parametersObj.GetCoreUIParent(), 
                base.RequestContext);
        }

        private static Uri ComputeAndValidateRedirectUri(Uri redirectUri, string clientId)
        {
            // ADAL mostly does not provide defaults for the redirect URI, currently only for UWP for broker support
            if (redirectUri == null)
            {
                string defaultUriAsString = PlatformProxyFactory.GetPlatformProxy().GetDefaultRedirectUri(clientId);

                if (!string.IsNullOrWhiteSpace(defaultUriAsString))
                {
                    return new Uri(defaultUriAsString);
                }
            }

            RedirectUriHelper.Validate(redirectUri);

            return redirectUri;
        }

        private static string ReplaceHost(string original, string newHost)
        {
            return new UriBuilder(original) { Host = newHost }.Uri.ToString();
        }

        protected internal /* internal for test only */ override async Task PreTokenRequestAsync()
        {
            await base.PreTokenRequestAsync().ConfigureAwait(false);

            // We do not have async interactive API in .NET, so we call this synchronous method instead.
            await AcquireAuthorizationAsync().ConfigureAwait(false);
            VerifyAuthorizationResult();

            if(!string.IsNullOrEmpty(authorizationResult.CloudInstanceHost))
            {
                var updatedAuthority = ReplaceHost(Authenticator.Authority, authorizationResult.CloudInstanceHost);

                await UpdateAuthorityAsync(updatedAuthority).ConfigureAwait(false);
            }
        }

        internal async Task AcquireAuthorizationAsync()
        {
            Uri authorizationUri = CreateAuthorizationUri();
            authorizationResult = await webUi.AcquireAuthorizationAsync(authorizationUri, redirectUri, RequestContext).ConfigureAwait(false);
        }

        internal async Task<Uri> CreateAuthorizationUriAsync(Guid correlationId)
        {
            RequestContext.Logger.CorrelationId = correlationId;
            await Authenticator.UpdateFromTemplateAsync(RequestContext).ConfigureAwait(false);
            return CreateAuthorizationUri();
        }
        protected override void AddAditionalRequestParameters(DictionaryRequestParameters requestParameters)
        {
            requestParameters[OAuthParameter.GrantType] = OAuthGrantType.AuthorizationCode;
            requestParameters[OAuthParameter.Code] = authorizationResult.Code;
            requestParameters[OAuthParameter.RedirectUri] = redirectUriRequestParameter;
        }

        protected override async Task PostTokenRequestAsync(AdalResultWrapper resultEx)
        {
            await base.PostTokenRequestAsync(resultEx).ConfigureAwait(false);
            if ((DisplayableId == null && UniqueId == null) || UserIdentifierType == UserIdentifierType.OptionalDisplayableId)
            {
                return;
            }

            string uniqueId = (resultEx.Result.UserInfo != null && resultEx.Result.UserInfo.UniqueId != null) ? resultEx.Result.UserInfo.UniqueId : "NULL";
            string displayableId = (resultEx.Result.UserInfo != null) ? resultEx.Result.UserInfo.DisplayableId : "NULL";

            if (UserIdentifierType == UserIdentifierType.UniqueId && string.Compare(uniqueId, UniqueId, StringComparison.Ordinal) != 0)
            {
                throw new AdalUserMismatchException(UniqueId, uniqueId);
            }

            if (UserIdentifierType == UserIdentifierType.RequiredDisplayableId && string.Compare(displayableId, DisplayableId, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new AdalUserMismatchException(DisplayableId, displayableId);
            }
        }

        private Uri CreateAuthorizationUri()
        {
            string loginHint = null;

            if (!userId.IsAnyUser
                && (userId.Type == UserIdentifierType.OptionalDisplayableId
                    || userId.Type == UserIdentifierType.RequiredDisplayableId))
            {
                loginHint = userId.Id;
            }

            IRequestParameters requestParameters = CreateAuthorizationRequest(loginHint);

            return new Uri(new Uri(Authenticator.AuthorizationUri), "?" + requestParameters);
        }

        private DictionaryRequestParameters CreateAuthorizationRequest(string loginHint)
        {
            var authorizationRequestParameters = new DictionaryRequestParameters(Resource, ClientKey)
            {
                [OAuthParameter.ResponseType] = OAuthResponseType.Code,
                [OAuthParameter.HasChrome] = "1",
                [OAuthParameter.RedirectUri] = redirectUriRequestParameter,
                [OAuthParameter.ResponseMode] = OAuthResponseMode.FormPost
            };

#if DESKTOP
            // Added form_post as a way to request to ensure we can handle large requests for dsts scenarios
#endif

            if (!string.IsNullOrWhiteSpace(loginHint))
            {
                authorizationRequestParameters[OAuthParameter.LoginHint] = loginHint;
            }

            if (!string.IsNullOrWhiteSpace(claims))
            {
                authorizationRequestParameters["claims"] = claims;
            }

            if (RequestContext != null && RequestContext.Logger.CorrelationId != Guid.Empty)
            {
                authorizationRequestParameters[OAuthParameter.CorrelationId] = RequestContext.Logger.CorrelationId.ToString();
            }

            if (authorizationParameters != null)
            {
                var platformInformation = new PlatformInformation();
                platformInformation.AddPromptBehaviorQueryParameter(authorizationParameters, authorizationRequestParameters);
            }
            
                IDictionary<string, string> adalIdParameters = AdalIdHelper.GetAdalIdParameters();
                foreach (KeyValuePair<string, string> kvp in adalIdParameters)
                {
                    authorizationRequestParameters[kvp.Key] = kvp.Value;
                }
            

            if (!string.IsNullOrWhiteSpace(extraQueryParameters))
            {
                // Checks for extraQueryParameters duplicating standard parameters
                Dictionary<string, string> kvps = EncodingHelper.ParseKeyValueList(extraQueryParameters, '&', false, RequestContext);
                foreach (KeyValuePair<string, string> kvp in kvps)
                {
                    if (authorizationRequestParameters.ContainsKey(kvp.Key))
                    {
                        throw new AdalException(AdalError.DuplicateQueryParameter, string.Format(CultureInfo.CurrentCulture, AdalErrorMessage.DuplicateQueryParameterTemplate, kvp.Key));
                    }
                }

                authorizationRequestParameters.ExtraQueryParameter = extraQueryParameters;
            }

            return authorizationRequestParameters;
        }

        private void VerifyAuthorizationResult()
        {
            if (authorizationResult.Error == OAuthError.LoginRequired)
            {
                throw new AdalException(AdalError.UserInteractionRequired);
            }

            if (authorizationResult.Status != AuthorizationStatus.Success)
            {
                throw new AdalServiceException(authorizationResult.Error, authorizationResult.ErrorDescription);
            }
        }

        protected override void UpdateBrokerParameters(IDictionary<string, string> parameters)
        {
            Uri uri = new Uri(authorizationResult.Code);
            string query = EncodingHelper.UrlDecode(uri.Query);
            Dictionary<string, string> kvps = EncodingHelper.ParseKeyValueList(query, '&', false, RequestContext);
            parameters["username"] = kvps["username"];
        }

        protected override bool BrokerInvocationRequired()
        {
            if (authorizationResult != null
                && !string.IsNullOrEmpty(authorizationResult.Code)
                && authorizationResult.Code.StartsWith("msauth://", StringComparison.OrdinalIgnoreCase))
            {
                brokerParameters[BrokerParameter.BrokerInstallUrl] = authorizationResult.Code;
                return true;
            }

            return false;
        }
    }
}