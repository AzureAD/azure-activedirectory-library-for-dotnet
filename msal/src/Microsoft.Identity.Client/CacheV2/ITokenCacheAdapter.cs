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
using System.Threading.Tasks;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Core.OAuth2;

namespace Microsoft.Identity.Client.CacheV2
{
    internal interface ITokenCacheAdapter
    {
        ITokenCache TokenCache { get; set; }
        IEnumerable<IAccount> GetAccounts(string authority, bool validateAuthority, RequestContext requestContext);
        void RemoveAccount(IAccount account, RequestContext requestContext);

        bool TryReadCache(
            AuthenticationRequestParameters authenticationRequestParameters,
            out MsalTokenResponse msalTokenResponse,
            out IAccount account);

        AuthenticationResult SaveAccessAndRefreshToken(
            AuthenticationRequestParameters authenticationRequestParameters,
            MsalTokenResponse msalTokenResponse);

        Task<MsalAccessTokenCacheItem> FindAccessTokenAsync(AuthenticationRequestParameters authenticationRequestParameters);
        MsalIdTokenCacheItem GetIdTokenCacheItem(MsalIdTokenCacheKey msalIdTokenCacheKey, RequestContext requestContext);
        Task<MsalRefreshTokenCacheItem> FindRefreshTokenAsync(AuthenticationRequestParameters authenticationRequestParameters);

        void SetKeychainSecurityGroup(string keychainSecurityGroup);
        ICollection<string>  GetAllAccessTokenCacheItems(RequestContext requestContext);
        ICollection<MsalAccessTokenCacheItem> GetAllAccessTokensForClient(RequestContext requestContext);
        ICollection<MsalAccountCacheItem> GetAllAccounts(RequestContext requestContext);
        ICollection<string> GetAllAccountCacheItems(RequestContext requestContext);
        ICollection<MsalIdTokenCacheItem> GetAllIdTokensForClient(RequestContext requestContext);
        ICollection<string> GetAllIdTokenCacheItems(RequestContext requestContext);
        ICollection<MsalRefreshTokenCacheItem> GetAllRefreshTokensForClient(RequestContext requestContext);
        ICollection<string> GetAllRefreshTokenCacheItems(RequestContext requestContext);
        void RemoveMsalAccount(IAccount user, RequestContext requestContext);
    }
}