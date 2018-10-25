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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.Instance;
using Microsoft.Identity.Core.OAuth2;
using Microsoft.Identity.Core.Telemetry;

namespace Microsoft.Identity.Client.Internal.Requests
{
    internal abstract class RequestBase
    {
        internal AuthenticationRequestParameters AuthenticationRequestParameters { get; }
        internal TokenCache TokenCache { get; }
        private readonly ApiEvent.ApiIds _apiId;
        protected IHttpManager HttpManager { get; }
        protected ICryptographyManager CryptographyManager { get; }

        protected RequestBase(
            IHttpManager httpManager, 
            ICryptographyManager cryptographyManager, 
            AuthenticationRequestParameters authenticationRequestParameters,
            ApiEvent.ApiIds apiId)
        {
            HttpManager = httpManager;
            CryptographyManager = cryptographyManager;
            TokenCache = authenticationRequestParameters.TokenCache;
            _apiId = apiId;
            
            AuthenticationRequestParameters = authenticationRequestParameters;
            if (authenticationRequestParameters.Scope == null || authenticationRequestParameters.Scope.Count == 0)
            {
                throw new ArgumentNullException(nameof(authenticationRequestParameters.Scope));
            }

            ValidateScopeInput(authenticationRequestParameters.Scope);

            AuthenticationRequestParameters.LogState();

            // TODO: FIX THIS SINCE THIS IS PROCESS GLOBAL.  
            // #HOWDOESTHISEVENWORK?!
            Telemetry.GetInstance().ClientId = AuthenticationRequestParameters.ClientId;
        }

        private void LogRequestStarted(AuthenticationRequestParameters authenticationRequestParameters)
        {
            string messageWithPii = string.Format(
                CultureInfo.InvariantCulture,
                "=== Token Acquisition ({4}) started:\n\tAuthority: {0}\n\tScope: {1}\n\tClientId: {2}\n\tCache Provided: {3}",
                authenticationRequestParameters.Authority?.CanonicalAuthority,
                authenticationRequestParameters.Scope.AsSingleString(),
                authenticationRequestParameters.ClientId,
                TokenCache != null,
                this.GetType().Name);

            string messageWithoutPii = string.Format(
                CultureInfo.InvariantCulture,
                "=== Token Acquisition ({1}) started:\n\tCache Provided: {0}",
                TokenCache != null,
                this.GetType().Name);

            if (authenticationRequestParameters.Authority != null &&
                AadAuthority.IsInTrustedHostList(authenticationRequestParameters.Authority.Host))
            {
                messageWithoutPii += string.Format(
                    CultureInfo.CurrentCulture,
                    "\n\tAuthority Host: {0}",
                    authenticationRequestParameters.Authority.Host);
            }

            authenticationRequestParameters.RequestContext.Logger.InfoPii(messageWithPii, messageWithoutPii);
        }

        protected SortedSet<string> GetDecoratedScope(SortedSet<string> inputScope)
        {
            SortedSet<string> set = new SortedSet<string>(inputScope.ToArray());
            set.UnionWith(ScopeHelper.CreateSortedSetFromEnumerable(OAuth2Value.ReservedScopes));
            return set;
        }

        protected void ValidateScopeInput(SortedSet<string> scopesToValidate)
        {
            // Check if scope or additional scope contains client ID.
            if (scopesToValidate.Intersect(ScopeHelper.CreateSortedSetFromEnumerable(OAuth2Value.ReservedScopes)).Any())
            {
                throw new ArgumentException("MSAL always sends the scopes 'openid profile offline_access'. " +
                                            "They cannot be suppressed as they are required for the " +
                                            "library to function. Do not include any of these scopes in the scope parameter.");
            }

            if (scopesToValidate.Contains(AuthenticationRequestParameters.ClientId))
            {
                throw new ArgumentException("API does not accept client id as a user-provided scope");
            }
        }

        internal abstract Task<AuthenticationResult> ExecuteAsync(CancellationToken cancellationToken);

        public async Task<AuthenticationResult> RunAsync(CancellationToken cancellationToken)
        {
            LogRequestStarted(AuthenticationRequestParameters);
            string accountId = AuthenticationRequestParameters.Account?.HomeAccountId?.Identifier;
            var apiEvent = InitializeApiEvent(accountId);

            using (CoreTelemetryService.CreateTelemetryHelper(
                AuthenticationRequestParameters.RequestContext.TelemetryRequestId,
                apiEvent,
                shouldFlush: true))
            {
                try
                {
                    var authenticationResult = await ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    LogReturnedToken(authenticationResult);

                    apiEvent.TenantId = authenticationResult.TenantId;
                    apiEvent.AccountId = authenticationResult.UniqueId;
                    apiEvent.WasSuccessful = true;
                    return authenticationResult;
                }
                catch (MsalException ex)
                {
                    apiEvent.ApiErrorCode = ex.ErrorCode;
                    AuthenticationRequestParameters.RequestContext.Logger.ErrorPii(ex);
                    throw;
                }
                catch (Exception ex)
                {
                    AuthenticationRequestParameters.RequestContext.Logger.ErrorPii(ex);
                    throw;
                }
            }
        }

        protected virtual void EnrichTelemetryApiEvent(ApiEvent apiEvent)
        {
            // todo: in base classes have them override this to add their properties/fields to this...
            //IsConfidentialClient = IsConfidentialClient,
            //UiBehavior = GetUIBehaviorPromptValue(),
        }

        private ApiEvent InitializeApiEvent(string accountId)
        {
            AuthenticationRequestParameters.RequestContext.TelemetryRequestId = Telemetry.GetInstance().GenerateNewRequestId();
            var apiEvent = new ApiEvent(AuthenticationRequestParameters.RequestContext.Logger)
            {
                ApiId = _apiId,
                ValidationStatus = AuthenticationRequestParameters.ValidateAuthority.ToString(),
                AccountId = accountId ?? "",
                CorrelationId = AuthenticationRequestParameters.RequestContext.Logger.CorrelationId.ToString(),
                RequestId = AuthenticationRequestParameters.RequestContext.TelemetryRequestId,
                WasSuccessful = false
            };

            if (AuthenticationRequestParameters.LoginHint != null)
            {
                apiEvent.LoginHint = AuthenticationRequestParameters.LoginHint;
            }

            if (AuthenticationRequestParameters.Authority != null)
            {
                apiEvent.Authority = new Uri(AuthenticationRequestParameters.Authority.CanonicalAuthority);
                apiEvent.AuthorityType = AuthenticationRequestParameters.Authority.AuthorityType.ToString();
            }

            // Give derived classes the ability to add or modify fields in the telemetry as needed.
            EnrichTelemetryApiEvent(apiEvent);

            return apiEvent;
        }

        protected AuthenticationResult CacheTokenResponseAndCreateAuthenticationResult(MsalTokenResponse msalTokenResponse)
        {
            // developer passed in user object.
            AuthenticationRequestParameters.RequestContext.Logger.Info("checking client info returned from the server..");

            ClientInfo fromServer = null;

            if (!AuthenticationRequestParameters.IsClientCredentialRequest)
            {
                //client_info is not returned from client credential flows because there is no user present.
                fromServer = ClientInfo.CreateFromJson(msalTokenResponse.ClientInfo);
            }

            if (fromServer!= null && AuthenticationRequestParameters.ClientInfo != null)
            {
                if (!fromServer.UniqueObjectIdentifier.Equals(AuthenticationRequestParameters.ClientInfo.UniqueObjectIdentifier, StringComparison.OrdinalIgnoreCase) ||
                    !fromServer.UniqueTenantIdentifier.Equals(AuthenticationRequestParameters.ClientInfo.UniqueTenantIdentifier, StringComparison.OrdinalIgnoreCase))
                {
                    AuthenticationRequestParameters.RequestContext.Logger.Error("Returned user identifiers do not match the sent user identifier");

                    AuthenticationRequestParameters.RequestContext.Logger.ErrorPii(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Returned user identifiers (uid:{0} utid:{1}) does not match the sent user identifier (uid:{2} utid:{3})",
                            fromServer.UniqueObjectIdentifier, 
                            fromServer.UniqueTenantIdentifier,
                            AuthenticationRequestParameters.ClientInfo.UniqueObjectIdentifier,
                            AuthenticationRequestParameters.ClientInfo.UniqueTenantIdentifier),
                        string.Empty);

                    throw new MsalClientException(MsalError.UserMismatch, MsalErrorMessage.UserMismatchSaveToken);
                }
            }

            IdToken idToken = IdToken.Parse(msalTokenResponse.IdToken);

            AuthenticationRequestParameters.TenantUpdatedCanonicalAuthority = Authority.UpdateTenantId(
                AuthenticationRequestParameters.Authority.CanonicalAuthority, idToken?.TenantId);

            if (TokenCache != null)
            {
                AuthenticationRequestParameters.RequestContext.Logger.Info("Saving Token Response to cache..");

                var tuple = TokenCache.SaveAccessAndRefreshToken(AuthenticationRequestParameters, msalTokenResponse);
                return new AuthenticationResult(tuple.Item1, tuple.Item2);
            }
            else
            {
                return new AuthenticationResult(
                    new MsalAccessTokenCacheItem(
                        AuthenticationRequestParameters.Authority.Host,
                        AuthenticationRequestParameters.ClientId, 
                        msalTokenResponse,
                        idToken?.TenantId),
                    new MsalIdTokenCacheItem(
                        AuthenticationRequestParameters.Authority.Host,
                        AuthenticationRequestParameters.ClientId, 
                        msalTokenResponse, 
                        idToken?.TenantId));
            }
        }

        // this method used to do this...  when we find derived classes that still call base() they should just call REsolveAuthorityEndpointsAsync...
        //internal virtual async Task PreTokenRequestAsync(CancellationToken cancellationToken)
        //{
        //    await ResolveAuthorityEndpointsAsync().ConfigureAwait(false);
        //}

        internal async Task ResolveAuthorityEndpointsAsync()
        {
            await AuthenticationRequestParameters.Authority.UpdateCanonicalAuthorityAsync
                (HttpManager, AuthenticationRequestParameters.RequestContext).ConfigureAwait(false);

            await AuthenticationRequestParameters.Authority
                .ResolveEndpointsAsync(HttpManager, AuthenticationRequestParameters.LoginHint,
                    AuthenticationRequestParameters.RequestContext)
                .ConfigureAwait(false);
        }

        protected async Task<MsalTokenResponse> SendTokenRequestAsync(
            IDictionary<string, string> additionalBodyParameters, 
            CancellationToken cancellationToken)
        {
            OAuth2Client client = new OAuth2Client(HttpManager);
            client.AddBodyParameter(OAuth2Parameter.ClientId, AuthenticationRequestParameters.ClientId);
            client.AddBodyParameter(OAuth2Parameter.ClientInfo, "1");
            foreach (var entry in AuthenticationRequestParameters.ToParameters())
            {
                client.AddBodyParameter(entry.Key, entry.Value);
            }

            client.AddBodyParameter(OAuth2Parameter.Scope,
                GetDecoratedScope(AuthenticationRequestParameters.Scope).AsSingleString());

            foreach (var kvp in additionalBodyParameters)
            {
                client.AddBodyParameter(kvp.Key, kvp.Value);
            }

            return await SendHttpMessageAsync(client).ConfigureAwait(false);
        }

        private async Task<MsalTokenResponse> SendHttpMessageAsync(OAuth2Client client)
        {
            UriBuilder builder = new UriBuilder(AuthenticationRequestParameters.Authority.TokenEndpoint);
            builder.AppendQueryParameters(AuthenticationRequestParameters.SliceParameters);
            MsalTokenResponse msalTokenResponse =
                await client
                    .GetTokenAsync(builder.Uri,
                        AuthenticationRequestParameters.RequestContext)
                    .ConfigureAwait(false);

            if (string.IsNullOrEmpty(msalTokenResponse.Scope))
            {
                msalTokenResponse.Scope = AuthenticationRequestParameters.Scope.AsSingleString();
                AuthenticationRequestParameters.RequestContext.Logger.Info("ScopeSet was missing from the token response, so using developer provided scopes in the result");
            }

            return msalTokenResponse;
        }

        private void LogReturnedToken(AuthenticationResult result)
        {
            if (result.AccessToken != null)
            {
                AuthenticationRequestParameters.RequestContext.Logger.Info(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "=== Token Acquisition finished successfully. An access token was returned with Expiration Time: {0} ===",
                        result.ExpiresOn));
            }
        }
    }
}
