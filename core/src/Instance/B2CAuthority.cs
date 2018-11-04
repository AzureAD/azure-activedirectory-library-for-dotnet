//------------------------------------------------------------------------------
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.Telemetry;

namespace Microsoft.Identity.Core.Instance
{
    internal class B2CAuthority : AadAuthority
    {
        public const string Prefix = "tfp"; // The http path of B2C authority looks like "/tfp/<your_tenant_name>/..."
        public const string B2CCanonicalAuthorityTemplate = "https://{0}/{1}/{2}/{3}/";
        public const string MicrosoftOnline = "https://login.microsoftonline.com";
        public const string OpenIdConfigurationEndpoint = "v2.0/.well-known/openid-configuration";
        internal const string B2CTrustedHost = "b2clogin.com";

        internal B2CAuthority(string authority, bool validateAuthority)
            : base(authority, validateAuthority)
        {
            AuthorityType = AuthorityType.B2C;
            ValidateAuthority = false;

            Uri authorityUri = new Uri(authority);
            string[] pathSegments = authorityUri.AbsolutePath.Substring(1).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length < 3)
            {
                throw new ArgumentException(CoreErrorMessages.B2cAuthorityUriInvalidPath);
            }

            CanonicalAuthority = string.Format(CultureInfo.InvariantCulture, B2CCanonicalAuthorityTemplate, authorityUri.Authority,
                pathSegments[0], pathSegments[1], pathSegments[2]);
        }

        internal new static bool IsInTrustedHostList(string host)
        {
            bool isInList =
                !string.IsNullOrEmpty(
                    TrustedHostList.FirstOrDefault(a => string.Compare(host, a, StringComparison.OrdinalIgnoreCase) == 0));
            isInList |= host.EndsWith(B2CTrustedHost);
            return isInList;
        }

        internal override async Task UpdateCanonicalAuthorityAsync(
            IHttpManager httpManager,
            ITelemetryManager telemetryManager,
            RequestContext requestContext)
        {
            var authorityUri = new Uri(CanonicalAuthority);

            if (!IsInTrustedHostList(authorityUri.Host))
            {
                throw new ArgumentException(CoreErrorMessages.UnsupportedAuthorityValidation);
            }

            var metadata = await AadInstanceDiscovery
                             .Instance.GetMetadataEntryAsync(
                                 httpManager,
                                 telemetryManager,
                                 new Uri(UpdateCanonicalAuthorityB2C()),
                                 ValidateAuthority,
                                 requestContext)
                             .ConfigureAwait(false);

            CanonicalAuthority = UpdateHost(CanonicalAuthority, metadata.PreferredNetwork);
        }

        private string UpdateCanonicalAuthorityB2C()
        {
            Uri b2cAuthority = new Uri(CanonicalAuthority);

            string canonicalAuthorityUri = string.Format(CultureInfo.InvariantCulture, MicrosoftOnline + b2cAuthority.AbsolutePath);
            return canonicalAuthorityUri;
        }

        protected override string GetDefaultOpenIdConfigurationEndpoint()
        {
            Uri b2cAuthority = new Uri(CanonicalAuthority);

            // To support host we would need to make the call to the tenant discovery end point, 
            // which is not supported for b2clogin.com
            string authorityUri = string.Format(CultureInfo.InvariantCulture, MicrosoftOnline + b2cAuthority.AbsolutePath + OpenIdConfigurationEndpoint);
            return authorityUri;
        }

        protected override async Task<string> GetOpenIdConfigurationEndpointAsync(
            IHttpManager httpManager,
            ITelemetryManager telemetryManager,
            string userPrincipalName,
            RequestContext requestContext)
        {
            return await Task.Run(() => GetDefaultOpenIdConfigurationEndpoint()).ConfigureAwait(false);
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
