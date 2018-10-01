//------------------------------------------------------------------------------
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
using System.Text;

namespace Microsoft.Identity.Core.Cache
{
    /// <summary>
    /// An object representing the key of the token cache AT dictionary. The 
    /// format of the key is not important for this library, as long as it is unique.
    /// </summary>
    internal class MsalAccessTokenCacheKey
    {
        private readonly string _environment;
        private readonly string _homeAccountId;
        private readonly string _clientId;
        private readonly string _scopes;
        private readonly string _tenantId;

        internal MsalAccessTokenCacheKey(
            string environment,
            string tenantId,
            string userIdentifier,
            string clientId,
            string scopes)
        {
            if (string.IsNullOrEmpty(environment))
            {
                throw new ArgumentNullException(nameof(environment));
            }

            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

           _environment = environment;
           _homeAccountId = userIdentifier;
           _clientId = clientId;
           _scopes = scopes;
           _tenantId = tenantId;
        }


        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append((_homeAccountId ?? "") + MsalCacheConstants.CacheKeyDelimiter);
            stringBuilder.Append(this._environment + MsalCacheConstants.CacheKeyDelimiter);
            stringBuilder.Append(MsalCacheConstants.AccessToken + MsalCacheConstants.CacheKeyDelimiter);
            stringBuilder.Append(_clientId + MsalCacheConstants.CacheKeyDelimiter);
            stringBuilder.Append((_tenantId ?? "") + MsalCacheConstants.CacheKeyDelimiter);
            stringBuilder.Append(_scopes);

            return stringBuilder.ToString();
        }


        #region UWP

        /// <summary>
        /// Gets a key that is smaller than 255 characters, which is a limitation for 
        /// UWP storage. This is done by hashing the scopes and env.
        /// </summary>
        /// <remarks>
        /// accountId - two guids plus separator - 73 chars        
        /// "accesstoken" string - 11 chars
        /// clientid - a guid - 36 chars
        /// tenantid - a guid - 36 chars
        /// scopes and env - a sha256 string - 44 chars
        /// delimiters - 4 chars
        /// total: 204 chars
        /// </remarks>
        public string GetFixedSizeKey()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append((_homeAccountId ?? "") + MsalCacheConstants.CacheKeyDelimiter);
            
            stringBuilder.Append(MsalCacheConstants.AccessToken + MsalCacheConstants.CacheKeyDelimiter);
            stringBuilder.Append(_clientId + MsalCacheConstants.CacheKeyDelimiter);
            stringBuilder.Append((_tenantId ?? "") + MsalCacheConstants.CacheKeyDelimiter);

            // can't use scopes and env because they are of variable length
            stringBuilder.Append(CoreCryptographyHelpers.CreateSha256Hash(_scopes + _environment));

            return stringBuilder.ToString();
        }
        #endregion


        #region iOS

        public string GetiOSAccountKey()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(_homeAccountId ?? "");
            stringBuilder.Append(MsalCacheConstants.CacheKeyDelimiter);

            stringBuilder.Append(_environment);

            return stringBuilder.ToString();
        }


        public string GetiOSServiceKey()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(MsalCacheConstants.AccessToken);
            stringBuilder.Append(MsalCacheConstants.CacheKeyDelimiter);

            stringBuilder.Append(_clientId);
            stringBuilder.Append(MsalCacheConstants.CacheKeyDelimiter);

            stringBuilder.Append(_tenantId ?? "");
            stringBuilder.Append(MsalCacheConstants.CacheKeyDelimiter);

            stringBuilder.Append(_scopes);
            stringBuilder.Append(MsalCacheConstants.CacheKeyDelimiter);

            return stringBuilder.ToString();
        }

        public string GetiOSGenericKey()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(MsalCacheConstants.AccessToken);
            stringBuilder.Append(MsalCacheConstants.CacheKeyDelimiter);

            stringBuilder.Append(_clientId);
            stringBuilder.Append(MsalCacheConstants.CacheKeyDelimiter);

            stringBuilder.Append(_tenantId ?? "");

            return stringBuilder.ToString();
        }

        #endregion
    }
}