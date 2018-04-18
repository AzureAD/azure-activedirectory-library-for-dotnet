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

using System.Runtime.Serialization;

namespace Microsoft.Identity.Core.Cache.U
{
    internal class MsalAccountCacheItem : MsalCacheItemBase
    {
        internal MsalAccountCacheItem(string userIdentifier, string environment, 
            string realm, string authorityAccountId, string username, AuthorityType authorityType)
            : base(userIdentifier, environment)
        {
            if (string.IsNullOrEmpty(realm))
            {
                throw new ArgumentNullException(nameof(realm));
            }
            if (string.IsNullOrEmpty(authorityAccountId))
            {
                throw new ArgumentNullException(nameof(authorityAccountId));
            }
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            this.Realm = realm;
            this.AuthorityAccountId = authorityAccountId;
            this.Username = username;
            this.authorityType = authorityType;
        }

        [DataMember(Name = "realm")]
        string Realm { get; set; }

        [DataMember(Name = "username")]
        string Username { get; set; }

        [DataMember(Name = "authority_account_id")]
        string AuthorityAccountId { get; set; }

        [DataMember(Name = "authority_type")]
        AuthorityType AuthorityType { get; set; }

    }
}
