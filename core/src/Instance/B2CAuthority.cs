﻿//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted free of charge to any person obtaining a copy
// of this software and associated documentation files(the "Software") to deal
// in the Software without restriction including without limitation the rights
// to use copy modify merge publish distribute sublicense and / or sell
// copies of the Software and to permit persons to whom the Software is
// furnished to do so subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND EXPRESS OR
// IMPLIED INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM DAMAGES OR OTHER
// LIABILITY WHETHER IN AN ACTION OF CONTRACT TORT OR OTHERWISE ARISING FROM
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core.Instance
{
    internal class B2CAuthority : AadAuthority
    {
        public const string Prefix = "tfp"; // The http path of B2C authority looks like "/tfp/<your_tenant_name>/..."
        public const string B2CCanonicalAuthorityTemplate = "https://{0}/{1}/{2}/{3}/";
        public const string OpenIdConfigurationEndpoint = "v2.0/.well-known/openid-configuration";
        public const string B2CTrustedHost = "b2clogin.com";

        internal B2CAuthority(IServiceBundle serviceBundle, string authority, bool validateAuthority)
            : base(serviceBundle, authority, validateAuthority)
        {
            AuthorityType = AuthorityType.B2C;
            // Setting this to false as B2C authorities are customer specific
            validateAuthority = false;

            Uri authorityUri = new Uri(authority);
            string[] pathSegments = authorityUri.AbsolutePath.Substring(1).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length < 3)
            {
                throw new ArgumentException(CoreErrorMessages.B2cAuthorityUriInvalidPath);
            }

            CanonicalAuthority = string.Format(CultureInfo.InvariantCulture, B2CCanonicalAuthorityTemplate, authorityUri.Authority,
                pathSegments[0], pathSegments[1], pathSegments[2]);
        }

        internal override async Task UpdateCanonicalAuthorityAsync(
            RequestContext requestContext)
        {
            Uri b2cHost = new Uri(CanonicalAuthority);
            if (IsB2CLoginHost(b2cHost.Host))
            {
                return;
            }
            else
            {
                var metadata = await ServiceBundle.AadInstanceDiscovery
                                        .GetMetadataEntryAsync(
                                     new Uri(CanonicalAuthority),
                                     ValidateAuthority,
                                     requestContext)
                                 .ConfigureAwait(false);

                CanonicalAuthority = UpdateHost(CanonicalAuthority, metadata.PreferredNetwork);
            }
        }

        private bool IsB2CLoginHost(string host)
        {
            if (host.EndsWith(B2CTrustedHost, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        protected override string GetDefaultOpenIdConfigurationEndpoint()
        {
            return string.Format(CultureInfo.InvariantCulture, new Uri(CanonicalAuthority).AbsoluteUri + OpenIdConfigurationEndpoint);
        }

        protected override async Task<string> GetOpenIdConfigurationEndpointAsync(
            string userPrincipalName,
            RequestContext requestContext)
        {
            return await Task.FromResult(GetDefaultOpenIdConfigurationEndpoint()).ConfigureAwait(false);
        }

        internal override string GetTenantId()
        {
            return new Uri(CanonicalAuthority).Segments[2].TrimEnd('/');
        }

        internal override void UpdateTenantId(string tenantId)
        {
            Uri authorityUri = new Uri(CanonicalAuthority);
            var segments = authorityUri.Segments;

            var b2cPrefix = segments[1].TrimEnd('/');
            var b2cPolicy = segments[3].TrimEnd('/');

            CanonicalAuthority = string.Format(CultureInfo.InvariantCulture, B2CCanonicalAuthorityTemplate,
                authorityUri.Authority, b2cPrefix, tenantId, b2cPolicy);
        }
    }
}