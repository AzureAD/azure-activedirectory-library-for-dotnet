// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Identity.Test.LabInfrastructure
{
    public class LabUser
    {
        [JsonProperty("objectId")]
        public Guid ObjectId { get; set; }

        [JsonProperty("userType")]
        public UserType UserType { get; set; }

        [JsonProperty("upn")]
        public string Upn { get; set; }

        [JsonProperty("credentialVaultKeyName")]
        public string CredentialUrl { get; set; }

        public LabUser HomeUser { get; set; }

        [JsonProperty("external")]
        public bool IsExternal { get; set; }

        [JsonProperty("mfa")]
        public bool IsMfa { get; set; }

        [JsonProperty("mam")]
        public bool IsMam { get; set; }

        [JsonProperty("licenses")]
        public ISet<string> Licenses { get; set; }

        [JsonProperty("isFederated")]
        public bool IsFederated { get; set; }

        [JsonProperty("federationProvider")]
        public FederationProvider FederationProvider { get; set; }

        [JsonProperty("tenantId")]
        public string CurrentTenantId { get; set; }

        [JsonProperty("hometenantId")]
        public string HomeTenantId { get; set; }

        [JsonProperty("homeUPN")]
        public string HomeUPN { get; set; }

        [JsonProperty("appid")]
        public string AppId { get; set; }

        [JsonProperty("labname")]
        public string LabName { get; set; }

        public string TenantId { get; set; }

        private string _password = null;

        public string GetOrFetchPassword()
        {
            if (_password == null)
            {
                _password = LabUserHelper.FetchUserPassword(LabName);
            }

            return _password;
        }

        public void InitializeHomeUser()
        {
            HomeUser = new LabUser();
            var labHomeUser = HomeUser;

            labHomeUser.ObjectId = ObjectId;
            labHomeUser.UserType = UserType;
            labHomeUser.CredentialUrl = CredentialUrl;
            labHomeUser.HomeUser = labHomeUser;
            labHomeUser.IsExternal = IsExternal;
            labHomeUser.IsMfa = IsMfa;
            labHomeUser.IsMam = IsMam;
            labHomeUser.Licenses = Licenses;
            labHomeUser.IsFederated = IsFederated;
            labHomeUser.FederationProvider = FederationProvider;
            labHomeUser.HomeTenantId = HomeTenantId;
            labHomeUser.HomeUPN = HomeUPN;
            labHomeUser.CurrentTenantId = HomeTenantId;
            labHomeUser.Upn = HomeUPN;
        }
    }
}
