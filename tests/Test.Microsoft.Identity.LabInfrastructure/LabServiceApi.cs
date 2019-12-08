// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Microsoft.Identity.Test.LabInfrastructure
{
    /// <summary>
    /// Wrapper for new lab service API
    /// </summary>
    public class LabServiceApi : ILabService
    {
        private string _labAccessAppId;
        private string _labAccessClientSecret;
        private string _labApiAccessToken;

        public LabServiceApi()
        {
            KeyVaultSecretsProvider _keyVaultSecretsProvider = new KeyVaultSecretsProvider();
            _labAccessAppId = _keyVaultSecretsProvider.GetMsidLabSecret("LabVaultAppID").Value;
            _labAccessClientSecret = _keyVaultSecretsProvider.GetMsidLabSecret("LabVaultAppSecret").Value;
        }

        private Task<string> RunQueryAsync(UserQuery query)
        {
            IDictionary<string, string> queryDict = new Dictionary<string, string>();

            //Disabled for now until there are tests that use it.
            queryDict.Add(LabApiConstants.MobileAppManagementWithConditionalAccess, LabApiConstants.False);
            queryDict.Add(LabApiConstants.MobileDeviceManagementWithConditionalAccess, LabApiConstants.False);

            //Building user query
            if (query.FederationProvider != null)
            {
                queryDict.Add(LabApiConstants.FederationProvider, query.FederationProvider.ToString());
            }

            queryDict.Add(LabApiConstants.MobileAppManagement, query.IsMamUser != null && (bool)(query.IsMamUser) ? LabApiConstants.True : LabApiConstants.False);
            queryDict.Add(LabApiConstants.MultiFactorAuthentication, query.IsMfaUser != null && (bool)(query.IsMfaUser) ? LabApiConstants.MfaOnAll : LabApiConstants.None);

            if (query.Licenses != null && query.Licenses.Count > 0)
            {
                queryDict.Add(LabApiConstants.License, query.Licenses.ToArray().ToString());
            }

            queryDict.Add(LabApiConstants.FederatedUser, query.IsFederatedUser != null && (bool)(query.IsFederatedUser) ? LabApiConstants.True : LabApiConstants.False);

            if (query.UserType != null)
            {
                queryDict.Add(LabApiConstants.UserType, query.UserType.ToString());
            }

            queryDict.Add(LabApiConstants.External, query.IsExternalUser != null && (bool)(query.IsExternalUser) ? LabApiConstants.True : LabApiConstants.False);

            return SendLabRequestAsync(LabApiConstants.LabEndPoint, queryDict);
        }

        /// <summary>
        /// Returns a test user account for use in testing.
        /// </summary>
        /// <param name="query">Any and all parameters that the returned user should satisfy.</param>
        /// <returns>Users that match the given query parameters.</returns>
        public async Task<LabResponse> GetLabResponseAsync(UserQuery query)
        {
            var response = await GetLabResponseFromApiAsync(query).ConfigureAwait(false);

            if (response == null)
            {
                throw new LabUserNotFoundException(query, "No lab user with specified parameters exists");
            }

            return response;
        }

        private async Task<LabResponse> GetLabResponseFromApiAsync(UserQuery query)
        {
            //Fetch user
            string result = await RunQueryAsync(query).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new LabUserNotFoundException(query, "No lab user with specified parameters exists");
            }

            LabUser[] userResponses = JsonConvert.DeserializeObject<LabUser[]>(result);
            if (userResponses.Length > 1)
            {
                throw new InvalidOperationException(
                    "Test Setup Error: Not expecting the lab to return multiple users for a query." +
                    " Please have rewrite the query so that it returns a single user.");
            }

            var user = userResponses[0];

            var appResponse = await GetLabResponseAsync(LabApiConstants.LabAppEndpoint + user.AppId).ConfigureAwait(false);
            LabApp[] labApps = JsonConvert.DeserializeObject<LabApp[]>(appResponse);

            var labInfoResponse = await GetLabResponseAsync(LabApiConstants.LabInfoEndpoint + user.LabName).ConfigureAwait(false);
            Lab[] labs = JsonConvert.DeserializeObject<Lab[]>(labInfoResponse);

            user.TenantId = labs[0].TenantId;
            user.FederationProvider = labs[0].FederationProvider;

            return new LabResponse
            {
                User = user,
                App = labApps[0],
                Lab = labs[0]
            };
        }

        private async Task<string> SendLabRequestAsync(string requestUrl, IDictionary<string, string> queryDict)
        {
            UriBuilder uriBuilder = new UriBuilder(requestUrl)
            {
                Query = string.Join("&", queryDict.Select(x => x.Key + "=" + x.Value.ToString()))
            };

            return await GetLabResponseAsync(uriBuilder.ToString()).ConfigureAwait(false);
        }

        private async Task<string> GetLabResponseAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(_labApiAccessToken))
                _labApiAccessToken = await LabAuthenticationHelper.GetAccessTokenForLabAPIAsync(
                    _labAccessAppId, 
                    _labAccessClientSecret).ConfigureAwait(false);

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add(
                    "Authorization", 
                    string.Format(
                        CultureInfo.InvariantCulture, 
                        "bearer {0}", 
                        _labApiAccessToken));
                return await httpClient.GetStringAsync(address).ConfigureAwait(false);
            }
        }

        public async Task<LabResponse> CreateTempLabUserAsync()
        {
            IDictionary<string, string> queryDict = new Dictionary<string, string>
            {
                { "code", "HC1Tud9RHGK12VoBPH3sbeyyPHfjmACKbyq8bFlhIiEwpMbWYR4zTQ==" },
                { "userType", "Basic" }
            };

            string result = await SendLabRequestAsync(LabApiConstants.CreateLabUser, queryDict).ConfigureAwait(false);
            return CreateLabResponseFromResultStringAsync(result).Result;
        }

        public async Task<string> GetUserSecretAsync(string lab)
        {
            IDictionary<string, string> queryDict = new Dictionary<string, string>
            {
                { "secret", lab }
            };

            string result = await SendLabRequestAsync(LabApiConstants.LabUserCredentialEndpoint, queryDict).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<LabCredentialResponse>(result).Secret;
        }

        private async Task<LabResponse> CreateLabResponseFromResultStringAsync(string result)
        {
            LabUser[] userResponses = JsonConvert.DeserializeObject<LabUser[]>(result);
            if (userResponses.Length > 1)
            {
                throw new InvalidOperationException(
                    "Test Setup Error: Not expecting the lab to return multiple users for a query." +
                    " Please have rewrite the query so that it returns a single user.");
            }

            var user = userResponses[0];

            var appResponse = await GetLabResponseAsync(LabApiConstants.LabAppEndpoint + user.AppId).ConfigureAwait(false);
            LabApp[] labApps = JsonConvert.DeserializeObject<LabApp[]>(appResponse);

            var labInfoResponse = await GetLabResponseAsync(LabApiConstants.LabInfoEndpoint + user.LabName).ConfigureAwait(false);
            Lab[] labs = JsonConvert.DeserializeObject<Lab[]>(labInfoResponse);

            user.TenantId = labs[0].TenantId;
            user.FederationProvider = labs[0].FederationProvider;

            return new LabResponse
            {
                User = user,
                App = labApps[0],
                Lab = labs[0]
            };
        }
    }
}