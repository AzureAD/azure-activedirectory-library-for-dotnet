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
using System.Threading.Tasks;
using Microsoft.Identity.Core.Cache;

namespace Microsoft.Identity.Core
{
    /// <summary>
    /// Platform / OS specific logic.  No library (ADAL / MSAL) specific code should go in here. 
    /// </summary>
    internal class NetStandard11PlatformProxy : IPlatformProxy
    {
        private readonly bool _isMsal;

        public NetStandard11PlatformProxy(bool isMsal)
        {
            _isMsal = isMsal;
        }

        /// <summary>
        /// Get the user logged in 
        /// </summary>
        public  async Task<string> GetUserPrincipalNameAsync()
        {
            return await Task.Factory.StartNew(() => string.Empty).ConfigureAwait(false);
        }

        public  async Task<bool> IsUserLocalAsync(RequestContext requestContext)
        {
            return await Task.Factory.StartNew(() => false).ConfigureAwait(false);
        }

        public  bool IsDomainJoined()
        {
            return false;
        }

        public  string GetEnvironmentVariable(string variable)
        {
            return null;
        }

        public  string GetProcessorArchitecture()
        {
            return null;
        }

        public  string GetOperatingSystem()
        {
            return null;
        }

        public  string GetDeviceModel()
        {
            return null;
        }

        /// <inheritdoc />
        public void ValidateRedirectUri(Uri redirectUri, RequestContext requestContext)
        {
            if (redirectUri == null)
            {
                throw new ArgumentNullException(nameof(redirectUri));
            }
        }

        /// <inheritdoc />
        public string GetRedirectUriAsString(Uri redirectUri, RequestContext requestContext)
        {
            return redirectUri.OriginalString;
        }

        /// <inheritdoc />
        public string GetDefaultRedirectUri(string correlationId)
        {
            return Constants.DefaultRedirectUri;
        }

        /// <inheritdoc />
        public string GetProductName()
        {
            return null;
        }

        /// <inheritdoc />
        public ILegacyCachePersistence LegacyCachePersistence => new Netstandard11LegacyCachePersistence();

        /// <inheritdoc />
        public ITokenCacheAccessor TokenCacheAccessor => new TokenCacheAccessor();

        /// <inheritdoc />
        public ICryptographyManager CryptographyManager { get; } = new Netstandard11CryptographyManager();
    }
}
