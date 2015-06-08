//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    /// <summary>
    ///     Contains the results of one Token acquisition operation.
    /// </summary>
    [DataContract]
    public sealed class AuthenticationResult
    {
        private const string Oauth2AuthorizationHeader = "Bearer ";

        /// <summary>
        ///     Creates result returned from AcquireToken. Except in advanced scenarios related to Token caching, you do not need
        ///     to create any instance of AuthenticationResult.
        /// </summary>
        /// <param name="tokenType">Type of the Access Token returned</param>
        /// <param name="tokensToken">The Access Token requested</param>
        /// <param name="expiresOn">The point in time in which the Access Token returned in the Token property ceases to be valid</param>
        internal AuthenticationResult(string tokenType, string token, DateTimeOffset expiresOn)
        {
            this.TokenType = tokenType;
            this.Token = token;
            this.ExpiresOn = DateTime.SpecifyKind(expiresOn.DateTime, DateTimeKind.Utc);
        }

        /// <summary>
        ///     Gets the type of the Token returned.
        /// </summary>
        [DataMember]
        public string TokenType { get; private set; }

        /// <summary>
        ///     Gets the Token requested.
        /// </summary>
        [DataMember]
        public string Token { get; internal set; }

        /// <summary>
        ///     Gets the point in time in which the Access Token returned in the Token property ceases to be valid.
        ///     This value is calculated based on current UTC time measured locally and the value expiresIn received from the
        ///     service.
        /// </summary>
        [DataMember]
        public DateTimeOffset ExpiresOn { get; internal set; }

        /// <summary>
        ///     Gets an identifier for the tenant the Token was acquired from. This property will be null if tenant information is
        ///     not returned by the service.
        /// </summary>
        [DataMember]
        public string TenantId { get; private set; }

        /// <summary>
        ///     Gets user information including user Id. Some elements in UserInfo might be null if not returned by the service.
        /// </summary>
        [DataMember]
        public UserInfo UserInfo { get; internal set; }

        /// <summary>
        ///     Gets the entire profile info if returned by the service or null if no profile info is returned.
        /// </summary>
        [DataMember]
        public string ProfileInfo { get; internal set; }

        /// <summary>
        ///     Creates authorization header from authentication result.
        /// </summary>
        /// <returns>Created authorization header</returns>
        public string CreateAuthorizationHeader()
        {
            return Oauth2AuthorizationHeader + this.Token;
        }

        internal void UpdateTenantAndUserInfo(string tenantId, string profileInfo, UserInfo userInfo)
        {
            this.TenantId = tenantId;
            this.ProfileInfo = profileInfo;
            if (userInfo != null)
            {
                this.UserInfo = new UserInfo(userInfo);
            }
        }
    }
}