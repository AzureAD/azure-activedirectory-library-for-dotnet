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

using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Core.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core.Cache
{
    [DataContract]
    internal class MsalRefreshTokenCacheItem : MsalCredentialCacheItemBase
    {
        internal MsalRefreshTokenCacheItem()
        {
            CredentialType = Cache.CredentialType.refreshtoken.ToString();
        }
        internal MsalRefreshTokenCacheItem(string environment, string clientId, MsalTokenResponse response) : 
            this(environment, clientId, response.RefreshToken, response.ClientInfo)
        {
        }

        internal MsalRefreshTokenCacheItem(string environment, string clientId, string secret, string rawClientInfo) : this()
        {
            ClientId = clientId;
            Environment = environment;
            Secret = secret;
            RawClientInfo = rawClientInfo;

            InitUserIdentifier();
        }

        internal MsalRefreshTokenCacheKey GetKey()
        {
            return new MsalRefreshTokenCacheKey(Environment, ClientId, HomeAccountId);
        }
    }
}
