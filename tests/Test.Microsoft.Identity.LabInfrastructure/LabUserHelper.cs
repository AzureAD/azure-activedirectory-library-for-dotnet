// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Identity.Test.LabInfrastructure
{
    public static class LabUserHelper
    {
        static LabServiceApi _labService;
        static KeyVaultSecretsProvider _keyVaultSecretsProvider;
        private static readonly IDictionary<UserQuery, LabResponse> _userCache =
            new Dictionary<UserQuery, LabResponse>();


        static LabUserHelper()
        {
            _keyVaultSecretsProvider = new KeyVaultSecretsProvider();
            _labService = new LabServiceApi();
        }


        public static async Task<LabResponse> GetLabUserDataAsync(UserQuery query)
        {
            if (_userCache.ContainsKey(query))
            {
                Debug.WriteLine("User cache hit");
                return _userCache[query];
            }

            var user = await _labService.GetLabResponseAsync(query).ConfigureAwait(false);
            if (user == null)
            {
                throw new LabUserNotFoundException(query, "Found no users for the given query.");
            }

            Debug.WriteLine("User cache miss");
            _userCache.Add(query, user);

            return user;
        }

        public static Task<LabResponse> GetDefaultUserAsync()
        {
            return GetLabUserDataAsync(UserQuery.DefaultUserQuery);
        }

        public static string FetchUserPassword(string userLabName)
        {
            if (string.IsNullOrWhiteSpace(userLabName))
            {
                throw new InvalidOperationException("Error: lab name is not set on user. Password retrieval failed.");
            }

            if (_keyVaultSecretsProvider == null)
            {
                throw new InvalidOperationException("Error: Keyvault secrets provider is not set");
            }

            try
            {
                return _labService.GetUserSecretAsync(userLabName).Result;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Test setup: cannot get the user password. See inner exception.", e);
            }
        }

        public static Task<LabResponse> GetAdfsUserAsync(FederationProvider federationProvider, bool federated = true)
        {
            var query = UserQuery.DefaultUserQuery;
            query.FederationProvider = federationProvider;
            query.IsFederatedUser = true;
            query.IsFederatedUser = federated;
            return GetLabUserDataAsync(query);
        }
    }
}
