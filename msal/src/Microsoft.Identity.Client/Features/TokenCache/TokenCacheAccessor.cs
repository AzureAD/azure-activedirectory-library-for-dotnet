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

using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Client.Internal.Cache;

namespace Microsoft.Identity.Client
{
    internal class TokenCacheAccessor : ITokenCacheAccessor
    {
        internal readonly IDictionary<string, string> AccessTokenCacheDictionary =
            new ConcurrentDictionary<string, string>();

        internal readonly IDictionary<string, string> RefreshTokenCacheDictionary =
            new ConcurrentDictionary<string, string>();
        
        public void SaveAccessToken(string cacheKey, string item)
        {
            AccessTokenCacheDictionary[cacheKey] = item;
        }

        public void SaveRefreshToken(string cacheKey, string item)
        {
            RefreshTokenCacheDictionary[cacheKey] = item;
        }
        
        public string GetRefreshToken(string refreshTokenKey)
        {
            if (!RefreshTokenCacheDictionary.ContainsKey(refreshTokenKey))
            {
                return null;
            }

            return RefreshTokenCacheDictionary[refreshTokenKey];
        }

        public void DeleteAccessToken(string cacheKey)
        {
            AccessTokenCacheDictionary.Remove(cacheKey);
        }

        public void DeleteRefreshToken(string cacheKey)
        {
            RefreshTokenCacheDictionary.Remove(cacheKey);
        }
        
        public ICollection<string> GetAllAccessTokensAsString()
        {
            return
                new ReadOnlyCollection<string>(
                    AccessTokenCacheDictionary.Values.ToList());
        }

        public ICollection<string> GetAllRefreshTokensAsString()
        {
            return
                new ReadOnlyCollection<string>(
                    RefreshTokenCacheDictionary.Values.ToList());
        }

        public ICollection<string> GetAllAccessTokenKeys()
        {
            return
                new ReadOnlyCollection<string>(
                    AccessTokenCacheDictionary.Keys.ToList());
        }

        public ICollection<string> GetAllRefreshTokenKeys()
        {
            return
                new ReadOnlyCollection<string>(
                    RefreshTokenCacheDictionary.Keys.ToList());
        }

        public void Clear()
        {
            foreach (var key in GetAllAccessTokenKeys())
            {
                DeleteAccessToken(key);
            }

            foreach (var key in GetAllRefreshTokenKeys())
            {
                DeleteRefreshToken(key);
            }
        }
    }
}
