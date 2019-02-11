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

using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Core.Helpers;

namespace Microsoft.Identity.Core
{
    internal class TokenCacheAccessor : ITokenCacheAccessor
    {
        internal readonly IDictionary<string, MsalAccessTokenCacheItem> AccessTokenCacheDictionary =
            new ConcurrentDictionary<string, MsalAccessTokenCacheItem>();

        internal readonly IDictionary<string, MsalRefreshTokenCacheItem> RefreshTokenCacheDictionary =
            new ConcurrentDictionary<string, MsalRefreshTokenCacheItem>();

        internal readonly IDictionary<string, MsalIdTokenCacheItem> IdTokenCacheDictionary =
            new ConcurrentDictionary<string, MsalIdTokenCacheItem>();

        internal readonly IDictionary<string, MsalAccountCacheItem> AccountCacheDictionary =
            new ConcurrentDictionary<string, MsalAccountCacheItem>();

        /// <inheritdoc />
        public int RefreshTokenCount => RefreshTokenCacheDictionary.Count;

        /// <inheritdoc />
        public int AccessTokenCount => AccessTokenCacheDictionary.Count;

        /// <inheritdoc />
        public int AccountCount => AccountCacheDictionary.Count;

        /// <inheritdoc />
        public int IdTokenCount => IdTokenCacheDictionary.Count;

        /// <inheritdoc />
        public void ClearRefreshTokens()
        {
            RefreshTokenCacheDictionary.Clear();
        }

        /// <inheritdoc />
        public void ClearAccessTokens()
        {
            AccessTokenCacheDictionary.Clear();
        }

        public void SaveAccessToken(MsalAccessTokenCacheItem item)
        {
            AccessTokenCacheDictionary[item.GetKey().ToString()] = item;
        }

        public void SaveRefreshToken(MsalRefreshTokenCacheItem item)
        {
            RefreshTokenCacheDictionary[item.GetKey().ToString()] = item;
        }

        public void SaveIdToken(MsalIdTokenCacheItem item)
        {
            IdTokenCacheDictionary[item.GetKey().ToString()] = item;
        }

        public void SaveAccount(MsalAccountCacheItem item)
        {
            AccountCacheDictionary[item.GetKey().ToString()] = item;
        }
        
        public ICollection<MsalAccessTokenCacheItem> GetAllAccessTokens()
        {
            return
                new ReadOnlyCollection<MsalAccessTokenCacheItem>(
                    AccessTokenCacheDictionary.Values.ToList());
        }

        public ICollection<MsalRefreshTokenCacheItem> GetAllRefreshTokens()
        {
            return
                new ReadOnlyCollection<MsalRefreshTokenCacheItem>(
                    RefreshTokenCacheDictionary.Values.ToList());
        }

        public ICollection<MsalIdTokenCacheItem> GetAllIdTokens()
        {
            return
                new ReadOnlyCollection<MsalIdTokenCacheItem>(
                   IdTokenCacheDictionary.Values.ToList());
        }

        public ICollection<MsalAccountCacheItem> GetAllAccounts()
        {
            return
                new ReadOnlyCollection<MsalAccountCacheItem>(
                   AccountCacheDictionary.Values.ToList());
        }

        public void SetKeychainSecurityGroup(string keychainSecurityGroup)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            AccessTokenCacheDictionary.Clear();
            RefreshTokenCacheDictionary.Clear();
            IdTokenCacheDictionary.Clear();
            AccountCacheDictionary.Clear();
        }
    }
}
