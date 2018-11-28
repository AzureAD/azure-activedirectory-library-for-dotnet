﻿// ------------------------------------------------------------------------------
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Core.OAuth2;

namespace Microsoft.Identity.Core.Instance
{
    internal class AdfsAuthority : Authority
    {
        private const string DefaultRealm = "http://schemas.microsoft.com/rel/trusted-realm";
        private readonly HashSet<string> _validForDomainsList = new HashSet<string>();

        public AdfsAuthority(IServiceBundle serviceBundle, string authority, bool validateAuthority)
            : base(serviceBundle, authority, validateAuthority)
        {
            AuthorityType = AuthorityType.Adfs;
        }

        protected override bool ExistsInValidatedAuthorityCache(string userPrincipalName)
        {
            if (string.IsNullOrEmpty(userPrincipalName))
            {
                throw ServiceBundle.ExceptionFactory.GetClientException(
                    CoreErrorCodes.UpnRequired,
                    CoreErrorMessages.UpnRequiredForAuthroityValidation);
            }

            if (ServiceBundle.ValidatedAuthoritiesCache.TryGetValue(CanonicalAuthority, out Authority authority))
            {
                var auth = (AdfsAuthority)authority;
                return auth._validForDomainsList.Contains(GetDomainFromUpn(userPrincipalName));
            }

            return false;
        }

        protected override async Task<string> GetOpenIdConfigurationEndpointAsync(
            string userPrincipalName,
            RequestContext requestContext)
        {
            if (ValidateAuthority)
            {
                var drsResponse = await GetMetadataFromEnrollmentServerAsync(
                                          userPrincipalName, 
                                          requestContext)
                                      .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(drsResponse.Error))
                {
                    ServiceBundle.ExceptionFactory.GetServiceException(
                        drsResponse.Error,
                        drsResponse.ErrorDescription,
                        ExceptionDetail.FromDrsResponse(drsResponse));
                }

                if (drsResponse.IdentityProviderService?.PassiveAuthEndpoint == null)
                {
                    throw ServiceBundle.ExceptionFactory.GetServiceException(
                        CoreErrorCodes.MissingPassiveAuthEndpoint,
                        CoreErrorMessages.CannotFindTheAuthEndpont,
                        ExceptionDetail.FromDrsResponse(drsResponse));
                }

                string resource = string.Format(CultureInfo.InvariantCulture, CanonicalAuthority);
                string webfingerUrl = string.Format(
                    CultureInfo.InvariantCulture,
                    "https://{0}/adfs/.well-known/webfinger?rel={1}&resource={2}",
                    drsResponse.IdentityProviderService.PassiveAuthEndpoint.Host,
                    DefaultRealm,
                    resource);

                var httpResponse =
                    await ServiceBundle.HttpManager.SendGetAsync(new Uri(webfingerUrl), null, requestContext).ConfigureAwait(false);

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw ServiceBundle.ExceptionFactory.GetServiceException(
                        CoreErrorCodes.InvalidAuthority,
                        CoreErrorMessages.AuthorityValidationFailed,
                        httpResponse);
                }

                var wfr = OAuth2Client.CreateResponse<AdfsWebFingerResponse>(httpResponse, requestContext, false);
                if (wfr.Links.FirstOrDefault(
                        a => a.Rel.Equals(DefaultRealm, StringComparison.OrdinalIgnoreCase) &&
                             a.Href.Equals(resource, StringComparison.OrdinalIgnoreCase)) == null)
                {
                    throw ServiceBundle.ExceptionFactory.GetClientException(
                        CoreErrorCodes.InvalidAuthority,
                        CoreErrorMessages.InvalidAuthorityOpenId);
                }
            }

            return GetDefaultOpenIdConfigurationEndpoint();
        }

        protected override string GetDefaultOpenIdConfigurationEndpoint()
        {
            return CanonicalAuthority + ".well-known/openid-configuration";
        }

        protected override void AddToValidatedAuthorities(string userPrincipalName)
        {
            var authorityInstance = this;
            if (ServiceBundle.ValidatedAuthoritiesCache.TryGetValue(CanonicalAuthority, out Authority authority))
            {
                authorityInstance = (AdfsAuthority)authority;
            }

            authorityInstance._validForDomainsList.Add(GetDomainFromUpn(userPrincipalName));
            ServiceBundle.ValidatedAuthoritiesCache.TryAddValue(CanonicalAuthority, authorityInstance);
        }

        private async Task<DrsMetadataResponse> GetMetadataFromEnrollmentServerAsync(
            string userPrincipalName,
            RequestContext requestContext)
        {
            try
            {
                //attempt to connect to on-premise enrollment server first.
                return await QueryEnrollmentServerEndpointAsync(
                           string.Format(
                               CultureInfo.InvariantCulture,
                               "https://enterpriseregistration.{0}/enrollmentserver/contract",
                               GetDomainFromUpn(userPrincipalName)),
                           requestContext).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                requestContext.Logger.InfoPiiWithPrefix(
                    exc,
                    "On-Premise ADFS enrollment server endpoint lookup failed. Error - ");
            }

            return await QueryEnrollmentServerEndpointAsync(
                       string.Format(
                           CultureInfo.InvariantCulture,
                           "https://enterpriseregistration.windows.net/{0}/enrollmentserver/contract",
                           GetDomainFromUpn(userPrincipalName)),
                       requestContext).ConfigureAwait(false);
        }

        private async Task<DrsMetadataResponse> QueryEnrollmentServerEndpointAsync(
            string endpoint, RequestContext requestContext)
        {
            var client = new OAuth2Client(ServiceBundle.HttpManager, ServiceBundle.TelemetryManager);
            client.AddQueryParameter("api-version", "1.0");
            return await client.ExecuteRequestAsync<DrsMetadataResponse>(new Uri(endpoint), HttpMethod.Get, requestContext)
                               .ConfigureAwait(false);
        }

        private string GetDomainFromUpn(string upn)
        {
            if (!upn.Contains("@"))
            {
                throw new ArgumentException("userPrincipalName does not contain @ character.");
            }

            return upn.Split('@')[1];
        }

        internal override string GetTenantId()
        {
            throw new NotImplementedException();
        }

        internal override void UpdateTenantId(string tenantId)
        {
            throw new NotImplementedException();
        }
    }
}