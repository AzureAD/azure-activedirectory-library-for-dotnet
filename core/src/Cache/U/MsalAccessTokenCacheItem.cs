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
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.OAuth2;

namespace Microsoft.Identity.Core.Cache.U
{
    [DataContract]
    internal class MsalAccessTokenCacheItem : MsalCredentialCacheItemBase
    {
        internal MsalAccessTokenCacheItem(string userIdentifier, string environment, string clientId, string secret,
            string realm, string target, string cachedAt, string expiresOn)
            : base(userIdentifier, environment, clientId, secret)
        {
            if (string.IsNullOrEmpty(target))
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (string.IsNullOrEmpty(cachedAt))
            {
                throw new ArgumentNullException(nameof(cachedAt));
            }
            if (string.IsNullOrEmpty(expiresOn))
            {
                throw new ArgumentNullException(nameof(expiresOn));
            }

            this.CredentialType = CredentialType.AccessToken;
            this.Realm = realm;
            this.Target = target;
            this.CachedAt = cachedAt;
            this.ExpiresOn = expiresOn;
        }

        [DataMember(Name = "realm")]
        string Realm { get; set; }

        [DataMember(Name = "target", IsRequired = true)]
        string Scopes { get; set; }

        [DataMember(Name = "cached_at", IsRequired = true)]
        string CachedAt { get; set; }

        [DataMember(Name = "expires_on", IsRequired = true)]
        string ExpiresOn { get; set; }

        [DataMember(Name = "extended_expires_on")]
        string ExtendedExpiresOn { get; set; }

        [DataMember(Name = "authority")]
        string Authority { get; set; }
    }
}
