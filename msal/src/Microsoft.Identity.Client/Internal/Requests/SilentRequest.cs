﻿// ------------------------------------------------------------------------------
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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.OAuth2;
using Microsoft.Identity.Core.Telemetry;

namespace Microsoft.Identity.Client.Internal.Requests
{
    internal class SilentRequest : RequestBase
    {
        public SilentRequest(
            IHttpManager httpManager,
            ICryptographyManager cryptographyManager,
            ITelemetryManager telemetryManager,
            AuthenticationRequestParameters authenticationRequestParameters,
            ApiEvent.ApiIds apiId,
            bool forceRefresh)
            : base(httpManager, cryptographyManager, telemetryManager, authenticationRequestParameters, apiId)
        {
            ForceRefresh = forceRefresh;
        }

        public bool ForceRefresh { get; }

        internal override async Task<AuthenticationResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            if (TokenCache == null)
            {
                throw new MsalUiRequiredException(
                    MsalUiRequiredException.TokenCacheNullError,
                    "Token cache is set to null. Silent requests cannot be executed.");
            }

            // Look for access token.
            var msalAccessTokenItem =
                await TokenCache.FindAccessTokenAsync(AuthenticationRequestParameters).ConfigureAwait(false);

            MsalIdTokenCacheItem msalIdTokenItem = null;

            if (msalAccessTokenItem != null)
            {
                msalIdTokenItem = TokenCache.GetIdTokenCacheItem(
                    msalAccessTokenItem.GetIdTokenItemKey(),
                    AuthenticationRequestParameters.RequestContext);
            }

            if (ForceRefresh)
            {
                msalAccessTokenItem = null;
            }

            if (msalAccessTokenItem != null)
            {
                return new AuthenticationResult(msalAccessTokenItem, msalIdTokenItem);
            }

            var msalRefreshTokenItem =
                await TokenCache.FindRefreshTokenAsync(AuthenticationRequestParameters).ConfigureAwait(false);

            if (msalRefreshTokenItem == null)
            {
                AuthenticationRequestParameters.RequestContext.Logger.Verbose("No Refresh Token was found in the cache");

                throw new MsalUiRequiredException(
                    MsalUiRequiredException.NoTokensFoundError,
                    "No Refresh Token found in the cache");
            }

            AuthenticationRequestParameters.RequestContext.Logger.Verbose("Refreshing access token...");
            await ResolveAuthorityEndpointsAsync().ConfigureAwait(false);
            var msalTokenResponse = await SendTokenRequestAsync(GetBodyParameters(msalRefreshTokenItem.Secret), cancellationToken)
                                        .ConfigureAwait(false);

            if (msalTokenResponse.RefreshToken == null)
            {
                msalTokenResponse.RefreshToken = msalRefreshTokenItem.Secret;
                AuthenticationRequestParameters.RequestContext.Logger.Info(
                    "Refresh token was missing from the token refresh response, so the refresh token in the request is returned instead");
            }

            return CacheTokenResponseAndCreateAuthenticationResult(msalTokenResponse);
        }

        private Dictionary<string, string> GetBodyParameters(string refreshTokenSecret)
        {
            var dict = new Dictionary<string, string>
            {
                [OAuth2Parameter.GrantType] = OAuth2GrantType.RefreshToken,
                [OAuth2Parameter.RefreshToken] = refreshTokenSecret
            };

            return dict;
        }
    }
}