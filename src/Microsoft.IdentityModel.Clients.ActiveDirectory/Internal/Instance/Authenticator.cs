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

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Helpers;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Instance
{
    internal enum AuthorityType
    {
        AAD,
        ADFS
    }

    internal class Authenticator
    {
        private const string TenantlessTenantName = "Common";

        private bool _updatedFromTemplate;

        private static readonly Regex TenantNameRegex = new Regex(Regex.Escape(TenantlessTenantName), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private void Init(IServiceBundle serviceBundle, string authority, bool validateAuthority)
        {
            Authority = EnsureUrlEndsWithForwardSlash(authority);

            AuthorityType = DetectAuthorityType(Authority);

            if (AuthorityType != AuthorityType.AAD && validateAuthority)
            {
                throw new ArgumentException(AdalErrorMessage.UnsupportedAuthorityValidation, "validateAuthority");
            }

            ValidateAuthority = validateAuthority;
            ServiceBundle = serviceBundle;
        }

        public Authenticator(IServiceBundle serviceBundle, string authority, bool validateAuthority)
        {
            Init(serviceBundle, authority, validateAuthority);
        }

        public async Task UpdateAuthorityAsync(IServiceBundle serviceBundle, string authority, RequestContext requestContext)
        {
            Init(serviceBundle, authority, ValidateAuthority);

            _updatedFromTemplate = false;
            await UpdateFromTemplateAsync(requestContext).ConfigureAwait(false);
        }

        public string Authority { get; private set; }

        public string GetAuthorityHost()
        {
            return !string.IsNullOrWhiteSpace(Authority) ? new Uri(Authority).Host : null;
        }
        
        public AuthorityType AuthorityType { get; private set; }

        public bool ValidateAuthority { get; private set; }

        public bool IsTenantless { get; private set; }

        public string AuthorizationUri { get; set; }

        public string DeviceCodeUri { get; set; }

        public string TokenUri { get; private set; }

        public string UserRealmUriPrefix { get; private set; }

        public string SelfSignedJwtAudience { get; private set; }

        public Guid CorrelationId { get; set; }
        internal IServiceBundle ServiceBundle { get; private set; }

        public async Task UpdateFromTemplateAsync(RequestContext requestContext)
        {
            if (!_updatedFromTemplate)
            {
                var authorityUri = new Uri(Authority);
                var host = authorityUri.Host;
                var authority = authorityUri.Authority;

                // The authority could be https://{AzureAD host name}/{tenantid} OR https://{Dsts host name}/dstsv2/{tenantid}
                // Detecting the tenantId using the last segment of the url
                string tenant = authorityUri.Segments[authorityUri.Segments.Length - 1].TrimEnd('/');
                if (AuthorityType == AuthorityType.AAD)
                {
                    var metadata = await ServiceBundle.InstanceDiscovery.GetMetadataEntryAsync(authorityUri, ValidateAuthority, requestContext).ConfigureAwait(false);
                    host = metadata.PreferredNetwork;
                    // All the endpoints will use this updated host, and it affects future network calls, as desired.
                    // The Authority remains its original host, and will be used in TokenCache later.
                }
                else
                {
                    ServiceBundle.InstanceDiscovery.AddMetadataEntry(host);
                }

                AuthorizationUri = UriBuilderExtensions.GetHttpsUriWithOptionalPort(ServiceBundle.InstanceDiscovery.FormatAuthorizeEndpoint(host, tenant), authorityUri.Port);
                DeviceCodeUri = UriBuilderExtensions.GetHttpsUriWithOptionalPort(host, tenant, "oauth2/devicecode", authorityUri.Port);
                TokenUri = UriBuilderExtensions.GetHttpsUriWithOptionalPort(host, tenant, "oauth2/token", authorityUri.Port);
                UserRealmUriPrefix = EnsureUrlEndsWithForwardSlash(
                                     UriBuilderExtensions.GetHttpsUriWithOptionalPort(host, string.Empty, "common/userrealm", authorityUri.Port));

                IsTenantless = (string.Compare(tenant, TenantlessTenantName, StringComparison.OrdinalIgnoreCase) == 0);
                SelfSignedJwtAudience = this.TokenUri;
                _updatedFromTemplate = true;
            }
        }

        public void UpdateTenantId(string tenantId)
        {
            if (IsTenantless && !string.IsNullOrWhiteSpace(tenantId))
            {
                ReplaceTenantlessTenant(tenantId);
                _updatedFromTemplate = false;
            }
        }

        internal static AuthorityType DetectAuthorityType(string authority)
        {
            if (string.IsNullOrWhiteSpace(authority))
            {
                throw new ArgumentNullException("authority");
            }

            if (!Uri.IsWellFormedUriString(authority, UriKind.Absolute))
            {
                throw new ArgumentException(AdalErrorMessage.AuthorityInvalidUriFormat, "authority");
            }

            var authorityUri = new Uri(authority);
            if (authorityUri.Scheme != "https")
            {
                throw new ArgumentException(AdalErrorMessage.AuthorityUriInsecure, "authority");
            }

            string path = authorityUri.AbsolutePath.Substring(1);
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(AdalErrorMessage.AuthorityUriInvalidPath, "authority");
            }

            string firstPath = path.Substring(0, path.IndexOf("/", StringComparison.Ordinal));
            AuthorityType authorityType = IsAdfsAuthority(firstPath) ? AuthorityType.ADFS : AuthorityType.AAD;

            return authorityType;
        }

        internal static string EnsureUrlEndsWithForwardSlash(string uri)
        {
            if (!string.IsNullOrWhiteSpace(uri) && !uri.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                uri = uri + "/";
            }

            return uri;
        }

        private static bool IsAdfsAuthority(string firstPath)
        {
            return string.Compare(firstPath, "adfs", StringComparison.OrdinalIgnoreCase) == 0;
        }

        private void ReplaceTenantlessTenant(string tenantId)
        {
            Authority = TenantNameRegex.Replace(Authority, tenantId, 1);
        }
    }
}
