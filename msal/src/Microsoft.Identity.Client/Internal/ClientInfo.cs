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
using System.Runtime.Serialization;

namespace Microsoft.Identity.Client.Internal
{
    internal class ClientInfoClaim
    {
        public const string UniqueIdentifier = "uid";
        public const string UnqiueTenantIdentifier = "utid";
    }

    [DataContract]
    internal class ClientInfo
    {
        [DataMember(Name = ClientInfoClaim.UniqueIdentifier, IsRequired = false)]
        public string UniqueIdentifier { get; set; }

        [DataMember(Name = ClientInfoClaim.UnqiueTenantIdentifier, IsRequired = false)]
        public string UniqueTenantIdentifier { get; set; }

        public static ClientInfo CreateFromJson(string clientInfo)
        {
            if (string.IsNullOrEmpty(clientInfo))
            {
                throw new MsalClientException(MsalClientException.JsonParseError, "client info is null");
            }

            try
            {
                return JsonHelper.DeserializeFromJson<ClientInfo>(Base64UrlHelpers.DecodeToBytes(clientInfo));
            }
            catch (Exception exc)
            {
                throw new MsalClientException(MsalClientException.JsonParseError,
                    "Failed to parse the returned client info.", exc);
            }
        }
        public static ClientInfo CreateFromEncodedString(string encodedUserIdentiier)
        {
            if (string.IsNullOrEmpty(encodedUserIdentiier))
            {
                return null;
            }

            string[] artifacts = encodedUserIdentiier.Split('.');

            if (artifacts.Length == 0)
            {
                return null;
            }

            return new ClientInfo()
            {
                UniqueIdentifier = Base64UrlHelpers.DecodeToString(artifacts[0]),
                UniqueTenantIdentifier = Base64UrlHelpers.DecodeToString(artifacts[1]),
            };
        }
    }
}
