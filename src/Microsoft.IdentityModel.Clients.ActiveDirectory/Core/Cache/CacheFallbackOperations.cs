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
using System.Linq;

namespace Microsoft.Identity.Core.Cache
{
    internal class CacheFallbackOperations
    {
        public static void WriteMsalRefreshToken(ITokenCacheAccessor tokenCacheAccessor,
            AdalResultWrapper resultWrapper, string authority, string clientId, string displayableId,
             string givenName, string familyName, string objectId)
        {
            if (string.IsNullOrEmpty(resultWrapper.RawClientInfo))
            {
                CoreLoggerBase.Default.Info("Client Info is missing. Skipping MSAL refresh token cache write");
                return;
            }

            if (string.IsNullOrEmpty(resultWrapper.RefreshToken))
            {
                CoreLoggerBase.Default.Info("Refresh Token is missing. Skipping MSAL refresh token cache write");
                return;
            }

            if (string.IsNullOrEmpty(resultWrapper.Result.IdToken))
            {
                CoreLoggerBase.Default.Info("Id Token is missing. Skipping MSAL refresh token cache write");
                return;
            }

            try
            {
                MsalRefreshTokenCacheItem rtItem = new MsalRefreshTokenCacheItem
                    (new Uri(authority).Host, clientId, resultWrapper.RefreshToken, resultWrapper.RawClientInfo);
                tokenCacheAccessor.SaveRefreshToken(rtItem);

                MsalAccountCacheItem accountCacheItem = new MsalAccountCacheItem
                    (new Uri(authority).Host, objectId, resultWrapper.RawClientInfo, null, displayableId, resultWrapper.Result.TenantId,
                        givenName, familyName);
                tokenCacheAccessor.SaveAccount(accountCacheItem);
            }
            catch (Exception ex)
            {
                CoreLoggerBase.Default.WarningPiiWithPrefix(
                    ex,
                    "An error occurred while writing ADAL refresh token to the cache in MSAL format. "
                    + "For details please see https://aka.ms/net-cache-persistence-errors. ");
            }
        }

        public static AdalResultWrapper FindMsalEntryForAdal(
            ITokenCacheAccessor tokenCacheAccessor,
            string authority,
            string clientId,
            string displayableId,
            string uniqueId)
        {
            try
            {
                string environment = new Uri(authority).Host;

                List<MsalAccountCacheItem> accounts = tokenCacheAccessor
                    .GetAllAccounts()
                    .Where(accountItem => accountItem != null &&
                           accountItem.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (accounts.Any())
                {
                    var rtItems = tokenCacheAccessor
                        .GetAllRefreshTokens()
                        .Where(rt => environment.Equals(rt.Environment, StringComparison.OrdinalIgnoreCase) &&
                               rt.ClientId.Equals(clientId, StringComparison.OrdinalIgnoreCase));

                    //TODO - authority check needs to be updated for alias check

                    foreach (var rt in rtItems)
                    {
                        // join refresh token cache item to corresponding account cache item to get upn
                        foreach (var account in accounts)
                        {
                            if (rt.HomeAccountId.Equals(account.HomeAccountId, StringComparison.OrdinalIgnoreCase) &&
                                (string.IsNullOrEmpty(displayableId) || account.PreferredUsername.Equals(displayableId, StringComparison.OrdinalIgnoreCase)) &&
                                (string.IsNullOrEmpty(uniqueId) || account.LocalAccountId.Equals(uniqueId, StringComparison.OrdinalIgnoreCase)))
                            {
                                return new AdalResultWrapper
                                {
                                    Result = new AdalResult(null, null, DateTimeOffset.MinValue),
                                    RefreshToken = rt.Secret,
                                    RawClientInfo = rt.RawClientInfo
                                };
                            }
                        }
                    }
                }
            
            }
            catch (Exception ex)
            {
                CoreLoggerBase.Default.WarningPiiWithPrefix(ex, "An error occurred while searching for refresh tokens in MSAL format in the cache for ADAL. " +
                             "For details please see https://aka.ms/net-cache-persistence-errors. ");
            }

            return null;
        }
    }
}
