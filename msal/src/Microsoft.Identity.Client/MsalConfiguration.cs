// ------------------------------------------------------------------------------
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
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Identity.Client.CacheV2;
using Microsoft.Identity.Core.Http;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// This class is used to configure the construction of a PublicClientApplication or ConfidentialClientApplication object.
    /// To construct this object, you should use the MsalConfigurationBuilder.
    /// </summary>
    internal sealed class MsalConfiguration
    {
        private readonly List<MsalAuthorityInfo> _authorityInfos = new List<MsalAuthorityInfo>();

        internal MsalConfiguration()
        {
            // used if we're de-serializing from json
            InitializeDefaults();
        }

        /// <summary>
        /// ClientId of the application being referenced.
        /// </summary>
        public string ClientId { get; internal set; }

        /// <summary>
        /// Redirect URI of the application being referenced.
        /// </summary>
        public string RedirectUri { get; internal set; }


        // TODO: need to figure out #if blocks for this since ClientCredential doesn't exist in mobile scenarios.
        ///// <summary>
        ///// ClientCredential for ConfidentialClientApplication scenarios.
        ///// </summary>
        //public ClientCredential ClientCredential { get; internal set; }

        /// <summary>
        /// AuthorizationUserAgent being referenced.
        /// </summary>
        public AuthorizationUserAgent AuthorizationUserAgent { get; internal set; }

        /// <summary>
        /// The configured valid authorities to be used.
        /// </summary>
        public IEnumerable<MsalAuthorityInfo> Authorities => _authorityInfos.AsEnumerable();

        // Logging

        /// <summary>
        /// The desired logging level.
        /// </summary>
        public LogLevel LoggingLevel { get; internal set; }

        /// <summary>
        /// True if PII logging should be enabled, false to only have scrubbed logging.
        /// </summary>
        public bool EnablePii { get; internal set; }

        // Http

        /// <summary>
        /// A HttpClientFactory to inject HttpClient object usage within MSAL.  If this is not specified,
        /// MSAL will use its internal factory.
        /// </summary>
        public IMsalHttpClientFactory HttpClientFactory { get; internal set; }

        /// <summary>
        /// The connection timeout in milliseconds for HTTP connections.
        /// </summary>
        public int HttpConnectionTimeoutMilliseconds { get; internal set; }

        /// <summary>
        /// The read timeout in milliseconds for HTTP connections.
        /// </summary>
        public int HttpConnectionReadTimeoutMilliseconds { get; internal set; }

        /// <summary>
        /// The value used during token expiration status calculations.
        /// </summary>
        public int TokenExpirationBufferMilliseconds { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public ITokenCache TokenCache { get; internal set; }

        /// <summary>
        /// Only used on ConfidentialClientApplication
        /// </summary>
        public ITokenCache UserTokenCache { get; internal set; }

        /// <summary>
        /// Used internally for tests to override the HttpManager.
        /// </summary>
        internal IHttpManager HttpManager { get; set; }

        private void InitializeDefaults()
        {
        }

        internal void AddAuthorityInfo(MsalAuthorityInfo authorityInfo)
        {
            _authorityInfos.Add(authorityInfo);
        }
    }
}