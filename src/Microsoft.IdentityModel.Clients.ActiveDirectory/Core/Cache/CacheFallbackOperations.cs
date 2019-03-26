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
            string upn,
            RequestContext requestContext)
        {
            try
            {
                string environment = new Uri(authority).Host;

                List<MsalAccountCacheItem> accounts = new List<MsalAccountCacheItem>();
                foreach (MsalAccountCacheItem accountItem in tokenCacheAccessor.GetAllAccounts())
                {
                    if (accountItem != null && accountItem.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase))
                    {
                        accounts.Add(accountItem);
                    }
                }
                if (accounts.Count > 0)
                {
                    foreach (MsalRefreshTokenCacheItem rtCacheItem in tokenCacheAccessor.GetAllRefreshTokens())
                    {
                        //TODO - authority check needs to be updated for alias check
                        if (rtCacheItem != null && environment.Equals(rtCacheItem.Environment, StringComparison.OrdinalIgnoreCase)
                            && rtCacheItem.ClientId.Equals(clientId, StringComparison.OrdinalIgnoreCase))
                        {
                            // join refresh token cache item to corresponding account cache item to get upn
                            foreach (MsalAccountCacheItem accountCacheItem in accounts)
                            {
                                if (rtCacheItem.HomeAccountId.Equals(accountCacheItem.HomeAccountId, StringComparison.OrdinalIgnoreCase)
                                    && (string.IsNullOrEmpty(upn) || accountCacheItem.PreferredUsername.Equals(upn, StringComparison.OrdinalIgnoreCase)))
                                {
                                    return new AdalResultWrapper
                                    {
                                        Result = new AdalResult(null, null, DateTimeOffset.MinValue),
                                        RefreshToken = rtCacheItem.Secret,
                                        RawClientInfo = rtCacheItem.RawClientInfo
                                    };
                                }
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
