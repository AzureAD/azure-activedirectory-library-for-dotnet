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
using Security;
using Foundation;
using Microsoft.Identity.Core.Cache;
using System.Globalization;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Identity.Core
{
    internal class iOSTokenCacheAccessor : ITokenCacheAccessor
    {
        public const string CacheKeyDelimiter = "-";

        static Dictionary<string, int> AuthorityTypeToAttrType = new Dictionary<string, int>()
        {
            {AuthorityType.AAD.ToString(), 1001},
            {AuthorityType.MSA.ToString(), 1002},
            {AuthorityType.MSSTS.ToString(), 1003},
            {AuthorityType.OTHER.ToString(), 1004},
        };

        enum CredentialAttrType
        {
            AccessToken = 2001,
            RefreshToken = 2002,
            IdToken = 2003,
            Password = 2004
        }

        private const bool _defaultSyncSetting = false;
        private const SecAccessible _defaultAccessiblityPolicy = SecAccessible.AfterFirstUnlockThisDeviceOnly;

        private const string DefaultKeychainGroup = "com.microsoft.adalcache";
        // Identifier for the keychain item used to retrieve current team ID
        private const string TeamIdKey = "DotNetTeamIDHint";
        private RequestContext _requestContext;

        private string keychainGroup;

        public iOSTokenCacheAccessor()
        {
            keychainGroup = GetTeamId() + '.' + DefaultKeychainGroup;
        }

        public iOSTokenCacheAccessor(RequestContext requestContext) : this()
        {
            _requestContext = requestContext;
        }

        private string GetBundleId()
        {
            return NSBundle.MainBundle.BundleIdentifier;
        }

        public void SetiOSKeychainSecurityGroup(string keychainSecurityGroup)
        {
            if (keychainSecurityGroup == null)
            {
                keychainGroup = GetBundleId();
            }
            else
            {
                keychainGroup = GetTeamId() + '.' + keychainSecurityGroup;
            }
        }

        [Obsolete("Use SetiOSKeychainSecurityGroup instead (See https://aka.ms/adal-net-ios-keychain-access)", false)]
        public void SetKeychainSecurityGroup(string keychainSecurityGroup)
        {
            if (keychainSecurityGroup == null)
            {
                keychainGroup = GetBundleId();
            }
            else
            {
                keychainGroup = keychainSecurityGroup;
            }
        }

        private string GetTeamId()
        {
            var queryRecord = new SecRecord(SecKind.GenericPassword)
            {
                Service = "",
                Account = TeamIdKey,
                Accessible = SecAccessible.Always
            };

            SecRecord match = SecKeyChain.QueryAsRecord(queryRecord, out SecStatusCode resultCode);

            if (resultCode == SecStatusCode.ItemNotFound)
            {
                SecKeyChain.Add(queryRecord);
                match = SecKeyChain.QueryAsRecord(queryRecord, out resultCode);
            }

            if (resultCode == SecStatusCode.Success)
            {
                return match.AccessGroup.Split('.')[0];
            }

            throw AdalExceptionFactory.GetClientException(
                CoreErrorCodes.CannotAccessPublisherKeyChain,
                CoreErrorMessages.CannotAccessPublisherKeyChain);
        }

        public void SaveAccessToken(MsalAccessTokenCacheItem item)
        {
            var key = item.GetKey();

            var account = key.GetiOSAccountKey();
            var service = key.GetiOSServiceKey();
            var generic = key.GetiOSGenericKey();
            var type = (int)CredentialAttrType.AccessToken;

            var value = item.ToJsonString();

            Save(account, service, generic, type, value);
        }

        public void SaveRefreshToken(MsalRefreshTokenCacheItem item)
        {
            var key = item.GetKey();
            var account = key.GetiOSAccountKey();
            var service = key.GetiOSServiceKey();
            var generic = key.GetiOSGenericKey();

            var type = (int)CredentialAttrType.RefreshToken;

            var value = item.ToJsonString();

            Save(account, service, generic, type, value);
        }

        public void SaveIdToken(MsalIdTokenCacheItem item)
        {
            var key = item.GetKey();
            var account = key.GetiOSAccountKey();
            var service = key.GetiOSServiceKey();
            var generic = key.GetiOSGenericKey();

            var type = (int)CredentialAttrType.IdToken;

            var value = item.ToJsonString();

            Save(account, service, generic, type, value);
        }

        public void SaveAccount(MsalAccountCacheItem item)
        {
            var key = item.GetKey();
            var account = key.GetiOSAccountKey();
            var service = key.GetiOSServiceKey();
            var generic = key.GetiOSGenericKey();

            var type = AuthorityTypeToAttrType[item.AuthorityType];

            var value = item.ToJsonString();

            Save(account, service, generic, type, value);
        }

        public ICollection<MsalAccessTokenCacheItem> GetAllAccessTokens()
        {
            return GetValues((int)CredentialAttrType.AccessToken).Select(MsalAccessTokenCacheItem.FromJsonString).ToList();
        }

        public ICollection<MsalRefreshTokenCacheItem> GetAllRefreshTokens()
        {
            return GetValues((int)CredentialAttrType.RefreshToken).Select(MsalRefreshTokenCacheItem.FromJsonString).ToList();
        }

        public ICollection<MsalIdTokenCacheItem> GetAllIdTokens()
        {
            return GetValues((int)CredentialAttrType.IdToken).Select(MsalIdTokenCacheItem.FromJsonString).ToList();
        }

        public ICollection<MsalAccountCacheItem> GetAllAccounts()
        {
            return GetValues(AuthorityTypeToAttrType[AuthorityType.MSSTS.ToString()]).Select(MsalAccountCacheItem.FromJsonString).ToList();
        }

        private ICollection<string> GetValues(int type)
        {
            var queryRecord = new SecRecord(SecKind.GenericPassword)
            {
                CreatorType = type,
                AccessGroup = keychainGroup
            };

            SecRecord[] records = SecKeyChain.QueryAsRecord(queryRecord, Int32.MaxValue, out SecStatusCode resultCode);

            ICollection<string> res = new List<string>();

            if (resultCode == SecStatusCode.Success)
            {
                foreach (var record in records)
                {
                    string str = record.ValueData.ToString(NSStringEncoding.UTF8);
                    res.Add(str);
                }
            }

            return res;
        }

        private SecStatusCode Save(string account, string service, string generic, int type, string value)
        {
            SecRecord recordToSave = CreateRecord(account, service, generic, type, value);

            var secStatusCode = Update(recordToSave);

            if (secStatusCode == SecStatusCode.ItemNotFound)
            {
                secStatusCode = SecKeyChain.Add(recordToSave);
            }

            if (secStatusCode == SecStatusCode.MissingEntitlement)
            {
                throw AdalExceptionFactory.GetClientException(
                CoreErrorCodes.MissingEntitlements,
                string.Format(
                    CultureInfo.InvariantCulture,
                    CoreErrorMessages.MissingEntitlements,
                    recordToSave.AccessGroup));
            }

            return secStatusCode;
        }

        private SecRecord CreateRecord(string account, string service, string generic, int type, string value)
        {
            return new SecRecord(SecKind.GenericPassword)
            {
                Account = account,
                Service = service,
                Generic = generic,
                CreatorType = type,
                ValueData = NSData.FromString(value, NSStringEncoding.UTF8),
                AccessGroup = keychainGroup,
                Accessible = _defaultAccessiblityPolicy,
                Synchronizable = _defaultSyncSetting,
            };
        }

        private SecStatusCode Update(SecRecord updatedRecord)
        {
            var currentRecord = new SecRecord(SecKind.GenericPassword)
            {
                Account = updatedRecord.Account,
                Service = updatedRecord.Service,
                CreatorType = updatedRecord.CreatorType,
                AccessGroup = keychainGroup
            };
            var attributesToUpdate = new SecRecord()
            {
                ValueData = updatedRecord.ValueData
            };

            return SecKeyChain.Update(currentRecord, attributesToUpdate);
        }

        private void RemoveAll(int type)
        {
            var queryRecord = new SecRecord(SecKind.GenericPassword)
            {
                CreatorType = type,
                AccessGroup = keychainGroup
            };
            SecKeyChain.Remove(queryRecord);
        }

        public void Clear()
        {
            RemoveAll((int)CredentialAttrType.AccessToken);
            RemoveAll((int)CredentialAttrType.RefreshToken);
            RemoveAll((int)CredentialAttrType.IdToken);

            RemoveAll(AuthorityTypeToAttrType[AuthorityType.MSSTS.ToString()]);
        }

        /// <inheritdoc />
        public int RefreshTokenCount => throw new NotImplementedException();

        /// <inheritdoc />
        public int AccessTokenCount => throw new NotImplementedException();

        /// <inheritdoc />
        public int AccountCount => throw new NotImplementedException();

        /// <inheritdoc />
        public int IdTokenCount => throw new NotImplementedException();

        /// <inheritdoc />
        public void ClearRefreshTokens()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ClearAccessTokens()
        {
            throw new NotImplementedException();
        }
    }
}