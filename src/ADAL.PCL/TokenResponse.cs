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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class TokenResponseClaim
    {
        public const string Code = "code";
        public const string TokenType = "token_type";
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
        public const string Scope = "scope";
        public const string IdToken = "id_token";
        public const string ProfileInfo = "profile_info";
        public const string CreatedOn = "created_on";
        public const string ExpiresOn = "expires_on";
        public const string ExpiresIn = "expires_in";
        public const string Error = "error";
        public const string ErrorDescription = "error_description";
        public const string ErrorCodes = "error_codes";
    }

    [DataContract]
    internal class TokenResponse
    {
        private const string CorrelationIdClaim = "correlation_id";

        [DataMember(Name = TokenResponseClaim.TokenType, IsRequired = false)]
        public string TokenType { get; set; }

        [DataMember(Name = TokenResponseClaim.AccessToken, IsRequired = false)]
        public string AccessToken { get; set; }

        [DataMember(Name = TokenResponseClaim.RefreshToken, IsRequired = false)]
        public string RefreshToken { get; set; }

        [DataMember(Name = TokenResponseClaim.Scope, IsRequired = false)]
        public string Scope { get; set; }

        [DataMember(Name = TokenResponseClaim.ProfileInfo, IsRequired = false)]
        public string ProfileInfoString { get; set; }

        [DataMember(Name = TokenResponseClaim.IdToken, IsRequired = false)]
        public string IdTokenString { get; set; }

        [DataMember(Name = TokenResponseClaim.CreatedOn, IsRequired = false)]
        public long CreatedOn { get; set; }

        [DataMember(Name = TokenResponseClaim.ExpiresOn, IsRequired = false)]
        public long ExpiresOn { get; set; }

        [DataMember(Name = TokenResponseClaim.ExpiresIn, IsRequired = false)]
        public long ExpiresIn { get; set; }

        [DataMember(Name = TokenResponseClaim.Error, IsRequired = false)]
        public string Error { get; set; }

        [DataMember(Name = TokenResponseClaim.ErrorDescription, IsRequired = false)]
        public string ErrorDescription { get; set; }

        [DataMember(Name = TokenResponseClaim.ErrorCodes, IsRequired = false)]
        public string[] ErrorCodes { get; set; }

        [DataMember(Name = CorrelationIdClaim, IsRequired = false)]
        public string CorrelationId { get; set; }

        public static TokenResponse CreateFromErrorResponse(IHttpWebResponse webResponse)
        {
            if (webResponse == null)
            {
                return new TokenResponse
                {
                    Error = AdalError.ServiceReturnedError,
                    ErrorDescription = AdalErrorMessage.ServiceReturnedError
                };
            }

            Stream responseStream = webResponse.ResponseStream;

            if (responseStream == null)
            {
                return new TokenResponse
                {
                    Error = AdalError.Unknown,
                    ErrorDescription = AdalErrorMessage.Unknown
                };
            }

            TokenResponse tokenResponse;

            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof (TokenResponse));
                tokenResponse = ((TokenResponse) serializer.ReadObject(responseStream));

                // Reset stream position to make it possible for application to read HttpRequestException body again
                responseStream.Position = 0;
            }
            catch (SerializationException)
            {
                responseStream.Position = 0;
                tokenResponse = new TokenResponse
                {
                    Error = (webResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                        ? AdalError.ServiceUnavailable
                        : AdalError.Unknown,
                    ErrorDescription = ReadStreamContent(responseStream)
                };
            }

            return tokenResponse;
        }

        public List<AuthenticationResultEx> GetResults()
        {
            List<AuthenticationResultEx> results = new List<AuthenticationResultEx>();

            if (this.AccessToken != null || this.IdTokenString != null)
            {
                if (!string.IsNullOrEmpty(this.AccessToken))
                {
                    results.Add(this.GetResult(this.AccessToken, this.Scope));
                }

                if (!string.IsNullOrEmpty(this.IdTokenString))
                {
                    results.Add(this.GetResult(this.IdTokenString, "openid"));
                }
            }
            else if (this.Error != null)
            {
                throw new AdalServiceException(this.Error, this.ErrorDescription);
            }
            else
            {
                throw new AdalServiceException(AdalError.Unknown, AdalErrorMessage.Unknown);
            }

            return results;
        }

        private AuthenticationResultEx GetResult(string token, string scope)
        {
            DateTimeOffset expiresOn = DateTime.UtcNow + TimeSpan.FromSeconds(this.ExpiresIn);
            var result = new AuthenticationResult(this.TokenType, token, expiresOn);

            ProfileInfo profileInfo = ProfileInfo.Parse(this.ProfileInfoString);
            if (profileInfo != null)
            {
                string tenantId = profileInfo.TenantId;
                string uniqueId = null;
                string displayableId = null;

                if (!string.IsNullOrWhiteSpace(profileInfo.ObjectId))
                {
                    uniqueId = profileInfo.ObjectId;
                }
                else if (!string.IsNullOrWhiteSpace(profileInfo.Subject))
                {
                    uniqueId = profileInfo.Subject;
                }

                if (!string.IsNullOrWhiteSpace(profileInfo.UPN))
                {
                    displayableId = profileInfo.UPN;
                }
                else if (!string.IsNullOrWhiteSpace(profileInfo.Email))
                {
                    displayableId = profileInfo.Email;
                }

                string givenName = profileInfo.GivenName;
                string familyName = profileInfo.FamilyName;
                string identityProvider = profileInfo.IdentityProvider ?? profileInfo.Issuer;
                DateTimeOffset? passwordExpiresOffest = null;
                if (profileInfo.PasswordExpiration > 0)
                {
                    passwordExpiresOffest = DateTime.UtcNow + TimeSpan.FromSeconds(profileInfo.PasswordExpiration);
                }

                Uri changePasswordUri = null;
                if (!string.IsNullOrEmpty(profileInfo.PasswordChangeUrl))
                {
                    changePasswordUri = new Uri(profileInfo.PasswordChangeUrl);
                }

                result.UpdateTenantAndUserInfo(tenantId, this.ProfileInfoString,
                    new UserInfo
                    {
                        UniqueId = uniqueId,
                        DisplayableId = displayableId,
                        GivenName = givenName,
                        FamilyName = familyName,
                        IdentityProvider = identityProvider,
                        PasswordExpiresOn = passwordExpiresOffest,
                        PasswordChangeUrl = changePasswordUri
                    });
            }

            return new AuthenticationResultEx
            {
                Result = result,
                RefreshToken = this.RefreshToken,
                // This is only needed for AcquireTokenByAuthorizationCode in which parameter resource is optional and we need
                // to get it from the STS response.
                ScopeInResponse = ADALScopeHelper.CreateArrayFromSingleString(scope)
            };
        }

        private static string ReadStreamContent(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}