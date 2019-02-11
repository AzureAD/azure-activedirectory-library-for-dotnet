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

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.OAuth2;
using Microsoft.Identity.Json.Linq;

namespace Microsoft.Identity.Core.Cache
{
    internal class MsalAccessTokenCacheItem : MsalCredentialCacheItemBase
    {
        internal MsalAccessTokenCacheItem()
        {
            CredentialType = StorageJsonValues.CredentialTypeAccessToken;
        }

        internal string TenantId { get; set; }

        /// <summary>
        /// String comprised of scopes that have been lowercased and ordered.
        /// </summary>
        /// <remarks>Normalization is important when creating unique keys.</remarks>
        internal string NormalizedScopes { get; set; }
        internal string CachedAt { get; set; }
        internal string ExpiresOnUnixTimestamp { get; set; }
        internal string ExtendedExpiresOnUnixTimestamp { get; set; }
        public string UserAssertionHash { get; set; }

        internal string Authority =>
            string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}/", Environment, TenantId ?? "common");

        internal SortedSet<string> ScopeSet => ScopeHelper.ConvertStringToLowercaseSortedSet(NormalizedScopes);

        internal DateTimeOffset ExpiresOn => CoreHelpers.UnixTimestampStringToDateTime(ExpiresOnUnixTimestamp);
        internal DateTimeOffset ExtendedExpiresOn => CoreHelpers.UnixTimestampStringToDateTime(ExtendedExpiresOnUnixTimestamp);
        public bool IsExtendedLifeTimeToken { get; set; }

        internal static MsalAccessTokenCacheItem FromJsonString(string json)
        {
            return FromJObject(JObject.Parse(json));
        }

        internal static MsalAccessTokenCacheItem FromJObject(JObject j)
        {
            long cachedAt = JsonUtils.ExtractParsedIntOrZero(j, StorageJsonKeys.CachedAt);
            long expiresOn = JsonUtils.ExtractParsedIntOrZero(j, StorageJsonKeys.ExpiresOn);
            long extendedExpiresOn = JsonUtils.ExtractParsedIntOrZero(j, StorageJsonKeys.ExtendedExpiresOn);

            var item = new MsalAccessTokenCacheItem
            {
                TenantId = JsonUtils.ExtractExistingOrEmptyString(j, StorageJsonKeys.Realm),
                NormalizedScopes = JsonUtils.ExtractExistingOrEmptyString(j, StorageJsonKeys.Target),
                CachedAt = cachedAt.ToString(CultureInfo.InvariantCulture),
                ExpiresOnUnixTimestamp = expiresOn.ToString(CultureInfo.InvariantCulture),
                ExtendedExpiresOnUnixTimestamp = extendedExpiresOn.ToString(CultureInfo.InvariantCulture),
                UserAssertionHash = JsonUtils.ExtractExistingOrEmptyString(j, StorageJsonKeys.UserAssertionHash),
            };

            item.PopulateFieldsFromJObject(j);

            return item;
        }

        internal override JObject ToJObject()
        {
            var json = base.ToJObject();

            json[StorageJsonKeys.Realm] = TenantId;
            json[StorageJsonKeys.Target] = NormalizedScopes;
            json[StorageJsonKeys.UserAssertionHash] = UserAssertionHash;
            json[StorageJsonKeys.CachedAt] = CachedAt;
            json[StorageJsonKeys.ExpiresOn] = ExpiresOnUnixTimestamp;
            json[StorageJsonKeys.ExtendedExpiresOn] = ExtendedExpiresOnUnixTimestamp;

            return json;
        }

        internal string ToJsonString()
        {
            return ToJObject().ToString();
        }

        internal MsalAccessTokenCacheKey GetKey()
        {
            return new MsalAccessTokenCacheKey(
                Environment,
                TenantId,
                HomeAccountId,
                ClientId,
                NormalizedScopes);
        }

        internal MsalIdTokenCacheKey GetIdTokenItemKey()
        {
            return new MsalIdTokenCacheKey(Environment, TenantId, HomeAccountId, ClientId);
        }
    }
}
