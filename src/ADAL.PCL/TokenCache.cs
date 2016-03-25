﻿//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    /// <summary>
    /// Token cache class used by <see cref="AuthenticationContext"/> to store access and refresh tokens.
    /// </summary>
    public class TokenCache
    {
        /// <summary>
        /// Notification for certain token cache interactions during token acquisition.
        /// </summary>
        /// <param name="args">Arguments related to the cache item impacted</param>
        public delegate void TokenCacheNotification(TokenCacheNotificationArgs args);

        private const int SchemaVersion = 3;

        private const string Delimiter = ":::";

        internal readonly IDictionary<TokenCacheKey, AuthenticationResultEx> tokenCacheDictionary;

        // We do not want to return near expiry tokens, this is why we use this hard coded setting to refresh tokens which are close to expiration.
        private const int ExpirationMarginInMinutes = 5;

        private volatile bool hasStateChanged;

        public ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        static TokenCache()
        {
            DefaultShared = new TokenCache
            {
                BeforeAccess = PlatformPlugin.TokenCachePlugin.BeforeAccess,
                AfterAccess = PlatformPlugin.TokenCachePlugin.AfterAccess
            };
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TokenCache()
        {
            this.tokenCacheDictionary = new ConcurrentDictionary<TokenCacheKey, AuthenticationResultEx>();
        }

        /// <summary>
        /// Constructor receiving state of the cache
        /// </summary>        
        public TokenCache(byte[] state)
            : this()
        {
            this.Deserialize(state);
        }

        /// <summary>
        /// Static token cache shared by all instances of AuthenticationContext which do not explicitly pass a cache instance during construction.
        /// </summary>
        public static TokenCache DefaultShared { get; private set; }

        /// <summary>
        /// Notification method called before any library method accesses the cache.
        /// </summary>
        public TokenCacheNotification BeforeAccess { get; set; }


        /// <summary>
        /// Notification method called before any library method writes to the cache. This notification can be used to reload
        /// the cache state from a row in database and lock that row. That database row can then be unlocked in <see cref="AfterAccess"/> notification.
        /// </summary>
        public TokenCacheNotification BeforeWrite { get; set; }

        /// <summary>
        /// Notification method called after any library method accesses the cache.
        /// </summary>
        public TokenCacheNotification AfterAccess { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether cache state has changed. ADAL methods set this flag after any change. Caller application should reset 
        /// the flag after serializing and persisting the state of the cache.
        /// </summary>
        public bool HasStateChanged
        {
            get { return this.hasStateChanged; }

            set { this.hasStateChanged = value; }
        }

        /// <summary>
        /// Gets the nunmber of items in the cache.
        /// </summary>
        public int Count
        {
            get { return this.tokenCacheDictionary.Count; }
        }

        /// <summary>
        /// Serializes current state of the cache as a blob. Caller application can persist the blob and update the state of the cache later by 
        /// passing that blob back in constructor or by calling method Deserialize.
        /// </summary>
        /// <returns>Current state of the cache as a blob</returns>
        public byte[] Serialize()
        {
            cacheLock.EnterReadLock();
            try
            {
                using (Stream stream = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(SchemaVersion);
                    PlatformPlugin.Logger.Information(null,
                        string.Format("Serializing token cache with {0} items.", this.tokenCacheDictionary.Count));
                    writer.Write(this.tokenCacheDictionary.Count);
                    foreach (KeyValuePair<TokenCacheKey, AuthenticationResultEx> kvp in this.tokenCacheDictionary)
                    {
                        writer.Write(string.Format("{1}{0}{2}{0}{3}{0}{4}", Delimiter, kvp.Key.Authority,
                            kvp.Key.Resource, kvp.Key.ClientId, (int) kvp.Key.TokenSubjectType));
                        writer.Write(kvp.Value.Serialize());
                    }

                    int length = (int) stream.Position;
                    stream.Position = 0;
                    BinaryReader reader = new BinaryReader(stream);
                    return reader.ReadBytes(length);
                }
            }
            finally
            {
                cacheLock.ExitReadLock();
            }

        }

        /// <summary>
        /// Deserializes state of the cache. The state should be the blob received earlier by calling the method Serialize.
        /// </summary>
        /// <param name="state">State of the cache as a blob</param>
        public void Deserialize(byte[] state)
        {
            cacheLock.EnterWriteLock();
            try
            {
                if (state == null)
                {
                    this.tokenCacheDictionary.Clear();
                    return;
                }

                using (Stream stream = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(state);
                    writer.Flush();
                    stream.Position = 0;

                    BinaryReader reader = new BinaryReader(stream);
                    int schemaVersion = reader.ReadInt32();
                    if (schemaVersion != SchemaVersion)
                    {
                        PlatformPlugin.Logger.Warning(null,
                            "The version of the persistent state of the cache does not match the current schema, so skipping deserialization.");
                        return;
                    }

                    this.tokenCacheDictionary.Clear();
                    int count = reader.ReadInt32();
                    for (int n = 0; n < count; n++)
                    {
                        string keyString = reader.ReadString();

                        string[] kvpElements = keyString.Split(new[] {Delimiter}, StringSplitOptions.None);
                        AuthenticationResultEx resultEx = AuthenticationResultEx.Deserialize(reader.ReadString());
                        TokenCacheKey key = new TokenCacheKey(kvpElements[0], kvpElements[1], kvpElements[2],
                            (TokenSubjectType) int.Parse(kvpElements[3]), resultEx.Result.UserInfo);

                        this.tokenCacheDictionary.Add(key, resultEx);
                    }

                    PlatformPlugin.Logger.Information(null,
                        string.Format("Deserialized {0} items to token cache.", count));
                }
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }

        }

        /// <summary>
        /// Reads a copy of the list of all items in the cache. 
        /// </summary>
        /// <returns>The items in the cache</returns>
        public virtual IEnumerable<TokenCacheItem> ReadItems()
        {
            cacheLock.EnterReadLock();
            try
            {
                TokenCacheNotificationArgs args = new TokenCacheNotificationArgs {TokenCache = this};
                this.OnBeforeAccess(args);

                List<TokenCacheItem> items =
                    this.tokenCacheDictionary.Select(kvp => new TokenCacheItem(kvp.Key, kvp.Value.Result)).ToList();

                this.OnAfterAccess(args);

                return items;

            }
            finally
            {
                cacheLock.ExitReadLock();
            }

        }

        /// <summary>
        /// Deletes an item from the cache.
        /// </summary>
        /// <param name="item">The item to delete from the cache</param>
        public virtual void DeleteItem(TokenCacheItem item)
        {
            cacheLock.EnterWriteLock();
            try
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                TokenCacheNotificationArgs args = new TokenCacheNotificationArgs
                {
                    TokenCache = this,
                    Resource = item.Resource,
                    ClientId = item.ClientId,
                    UniqueId = item.UniqueId,
                    DisplayableId = item.DisplayableId
                };

                this.OnBeforeAccess(args);
                this.OnBeforeWrite(args);

                TokenCacheKey toRemoveKey = this.tokenCacheDictionary.Keys.FirstOrDefault(item.Match);
                if (toRemoveKey != null)
                {
                    this.tokenCacheDictionary.Remove(toRemoveKey);
                    PlatformPlugin.Logger.Information(null, "One item removed successfully");
                }
                else
                {
                    PlatformPlugin.Logger.Information(null, "Item not Present in the Cache");
                }

                this.HasStateChanged = true;
                this.OnAfterAccess(args);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears the cache by deleting all the items. Note that if the cache is the default shared cache, clearing it would
        /// impact all the instances of <see cref="AuthenticationContext"/> which share that cache.
        /// </summary>
        public virtual void Clear()
        {
            cacheLock.EnterWriteLock();
            try
            {
                TokenCacheNotificationArgs args = new TokenCacheNotificationArgs {TokenCache = this};
                this.OnBeforeAccess(args);
                this.OnBeforeWrite(args);
                PlatformPlugin.Logger.Information(null,
                    String.Format("Clearing Cache :- {0} items to be removed", this.Count));
                this.tokenCacheDictionary.Clear();
                PlatformPlugin.Logger.Information(null, String.Format("Successfully Cleared Cache"));
                this.HasStateChanged = true;
                this.OnAfterAccess(args);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }

        }

        internal void OnAfterAccess(TokenCacheNotificationArgs args)
        {
            if (AfterAccess != null)
            {
                AfterAccess(args);
            }
        }

        internal void OnBeforeAccess(TokenCacheNotificationArgs args)
        {
            if (BeforeAccess != null)
            {
                BeforeAccess(args);
            }
        }

        internal void OnBeforeWrite(TokenCacheNotificationArgs args)
        {
            if (BeforeWrite != null)
            {
                BeforeWrite(args);
            }
        }

        internal AuthenticationResultEx LoadFromCache(string authority, string resource, string clientId,
            TokenSubjectType subjectType, string uniqueId, string displayableId, CallState callState)
        {
            cacheLock.EnterReadLock();
            try
            {
                PlatformPlugin.Logger.Verbose(callState, "Looking up cache for a token...");

                AuthenticationResultEx resultEx = null;

                KeyValuePair<TokenCacheKey, AuthenticationResultEx>? kvp = this.LoadSingleItemFromCache(authority,
                    resource, clientId, subjectType, uniqueId, displayableId, callState);

                if (kvp.HasValue)
                {
                    TokenCacheKey cacheKey = kvp.Value.Key;
                    resultEx = kvp.Value.Value;
                    bool tokenNearExpiry = (resultEx.Result.ExpiresOn <=
                                            DateTime.UtcNow + TimeSpan.FromMinutes(ExpirationMarginInMinutes));

                    if (tokenNearExpiry)
                    {
                        resultEx.Result.AccessToken = null;
                        PlatformPlugin.Logger.Verbose(callState,
                            "An expired or near expiry token was found in the cache");
                    }
                    else if (!cacheKey.ResourceEquals(resource))
                    {
                        PlatformPlugin.Logger.Verbose(callState,
                            string.Format(
                                "Multi resource refresh token for resource '{0}' will be used to acquire token for '{1}'",
                                cacheKey.Resource, resource));
                        var newResultEx = new AuthenticationResultEx
                        {
                            Result = new AuthenticationResult(null, null, DateTimeOffset.MinValue),
                            RefreshToken = resultEx.RefreshToken,
                            ResourceInResponse = resultEx.ResourceInResponse
                        };

                        newResultEx.Result.UpdateTenantAndUserInfo(resultEx.Result.TenantId, resultEx.Result.IdToken,
                            resultEx.Result.UserInfo);
                        resultEx = newResultEx;
                    }
                    else
                    {
                        PlatformPlugin.Logger.Verbose(callState,
                            string.Format("{0} minutes left until token in cache expires",
                                (resultEx.Result.ExpiresOn - DateTime.UtcNow).TotalMinutes));
                    }

                    if (resultEx.Result.AccessToken == null && resultEx.RefreshToken == null)
                    {
                        this.tokenCacheDictionary.Remove(cacheKey);
                        PlatformPlugin.Logger.Information(callState, "An old item was removed from the cache");
                        this.HasStateChanged = true;
                        resultEx = null;
                    }

                    if (resultEx != null)
                    {
                        PlatformPlugin.Logger.Information(callState,
                            "A matching item (access token or refresh token or both) was found in the cache");
                    }
                }
                else
                {
                    PlatformPlugin.Logger.Information(callState, "No matching token was found in the cache");
                }

                return resultEx;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        internal void StoreToCache(AuthenticationResultEx result, string authority, string resource, string clientId,
            TokenSubjectType subjectType, CallState callState)
        {
            cacheLock.EnterWriteLock();
            try
            {
                PlatformPlugin.Logger.Verbose(callState, "Storing token in the cache...");

                string uniqueId = (result.Result.UserInfo != null) ? result.Result.UserInfo.UniqueId : null;
                string displayableId = (result.Result.UserInfo != null) ? result.Result.UserInfo.DisplayableId : null;

                this.OnBeforeWrite(new TokenCacheNotificationArgs
                {
                    Resource = resource,
                    ClientId = clientId,
                    UniqueId = uniqueId,
                    DisplayableId = displayableId
                });

                TokenCacheKey tokenCacheKey = new TokenCacheKey(authority, resource, clientId, subjectType,
                    result.Result.UserInfo);
                this.tokenCacheDictionary[tokenCacheKey] = result;
                PlatformPlugin.Logger.Verbose(callState, "An item was stored in the cache");
                this.UpdateCachedMrrtRefreshTokens(result, clientId, subjectType);

                this.HasStateChanged = true;
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        private void UpdateCachedMrrtRefreshTokens(AuthenticationResultEx result, string clientId,
            TokenSubjectType subjectType)
        {
            cacheLock.EnterReadLock();
            try
            {
                if (result.Result.UserInfo != null && result.IsMultipleResourceRefreshToken)
                {
                    //pass null for authority to update the token for all the tenants
                    List<KeyValuePair<TokenCacheKey, AuthenticationResultEx>> mrrtItems =
                        this.QueryCache(null, clientId, subjectType, result.Result.UserInfo.UniqueId,
                            result.Result.UserInfo.DisplayableId)
                            .Where(p => p.Value.IsMultipleResourceRefreshToken)
                            .ToList();

                    foreach (KeyValuePair<TokenCacheKey, AuthenticationResultEx> mrrtItem in mrrtItems)
                    {
                        mrrtItem.Value.RefreshToken = result.RefreshToken;
                    }
                }
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        private KeyValuePair<TokenCacheKey, AuthenticationResultEx>? LoadSingleItemFromCache(string authority,
            string resource, string clientId, TokenSubjectType subjectType, string uniqueId, string displayableId,
            CallState callState)
        {
            cacheLock.EnterReadLock();
            try
            {
                // First identify all potential tokens.
                List<KeyValuePair<TokenCacheKey, AuthenticationResultEx>> items = this.QueryCache(authority, clientId,
                    subjectType, uniqueId, displayableId);

                List<KeyValuePair<TokenCacheKey, AuthenticationResultEx>> resourceSpecificItems =
                    items.Where(p => p.Key.ResourceEquals(resource)).ToList();

                int resourceValuesCount = resourceSpecificItems.Count();
                KeyValuePair<TokenCacheKey, AuthenticationResultEx>? returnValue = null;
                switch (resourceValuesCount)
                {
                    case 1:
                        PlatformPlugin.Logger.Information(callState,
                            "An item matching the requested resource was found in the cache");
                        returnValue = resourceSpecificItems.First();
                        break;
                    case 0:
                    {
                        // There are no resource specific tokens.  Choose any of the MRRT tokens if there are any.
                        List<KeyValuePair<TokenCacheKey, AuthenticationResultEx>> mrrtItems =
                            items.Where(p => p.Value.IsMultipleResourceRefreshToken).ToList();

                        if (mrrtItems.Any())
                        {
                            returnValue = mrrtItems.First();
                            PlatformPlugin.Logger.Information(callState,
                                "A Multi Resource Refresh Token for a different resource was found which can be used");
                        }
                    }
                        break;
                    default:
                        throw new AdalException(AdalError.MultipleTokensMatched);
                }

                // check for tokens issued to same client_id/user_id combination, but any tenant.
                if (returnValue == null)
                {
                    List<KeyValuePair<TokenCacheKey, AuthenticationResultEx>> itemsForAllTenants = this.QueryCache(
                        null, clientId, subjectType, uniqueId, displayableId);
                    if (itemsForAllTenants.Count != 0)
                    {
                        returnValue = itemsForAllTenants.First();
                    }

                    // check if the token was issued by AAD
                    if (returnValue != null &&
                        Authenticator.DetectAuthorityType(returnValue.Value.Key.Authority) != AuthorityType.ADFS)
                    {
                        //remove access token to redeem refresh token against a different tenant.
                        returnValue.Value.Value.Result.AccessToken = null;
                    }
                    else
                    {
                        returnValue = null;
                    }
                }

                return returnValue;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Queries all values in the cache that meet the passed in values, plus the 
        /// authority value that this AuthorizationContext was created with.  In every case passing
        /// null results in a wildcard evaluation.
        /// </summary>
        private List<KeyValuePair<TokenCacheKey, AuthenticationResultEx>> QueryCache(string authority, string clientId,
            TokenSubjectType subjectType, string uniqueId, string displayableId)
        {
            cacheLock.EnterReadLock();
            try
            {
                return this.tokenCacheDictionary.Where(
                    p =>
                        (string.IsNullOrWhiteSpace(authority) || p.Key.Authority == authority)
                        && (string.IsNullOrWhiteSpace(clientId) || p.Key.ClientIdEquals(clientId))
                        && (string.IsNullOrWhiteSpace(uniqueId) || p.Key.UniqueId == uniqueId)
                        && (string.IsNullOrWhiteSpace(displayableId) || p.Key.DisplayableIdEquals(displayableId))
                        && p.Key.TokenSubjectType == subjectType).ToList();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }
    }

}