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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Test.ADAL.Common
{
    internal partial class AuthenticationContextProxy
    {
        private const string FixedCorrelationId = "2ddbba59-1a04-43fb-b363-7fb0ae785030";
        private readonly AuthenticationContext context;

        public async Task<AuthenticationResultProxy> AcquireTokenSilentAsync(string[] scope, string clientId)
        {
            return await RunTaskAsync(this.context.AcquireTokenSilentAsync(scope, clientId));
        }

        public async Task<AuthenticationResultProxy> AcquireTokenSilentAsync(string[] scope, string clientId,
            UserIdentifier userId)
        {
            return await RunTaskAsync(this.context.AcquireTokenSilentAsync(scope, clientId, userId));
        }

        public async Task<AuthenticationResultProxy> AcquireTokenSilentAsync(string[] scope,
            ClientCredential clientCredential, UserIdentifier userId)
        {
            return await RunTaskAsync(this.context.AcquireTokenSilentAsync(scope, clientCredential, userId));
        }

        public async Task<AuthenticationResultProxy> AcquireTokenSilentAsync(string[] scope,
            ClientAssertion clientAssertion, UserIdentifier userId)
        {
            return await RunTaskAsync(this.context.AcquireTokenSilentAsync(scope, clientAssertion, userId));
        }

        public async Task<AuthenticationResultProxy> AcquireTokenSilentAsync(string[] scope,
            ClientAssertionCertificate clientCertificate, UserIdentifier userId)
        {
            return await RunTaskAsync(this.context.AcquireTokenSilentAsync(scope, clientCertificate, userId));
        }

        public async Task<AuthenticationResultProxy> AcquireTokenByAuthorizationCodeAsync(string authorizationCode,
            Uri redirectUri, ClientCredential credential, string[] scope)
        {
            return
                await
                    RunTaskAsync(this.context.AcquireTokenByAuthorizationCodeAsync(authorizationCode, redirectUri,
                        credential, scope));
        }

        public async Task<AuthenticationResultProxy> AcquireTokenByAuthorizationCodeAsync(string authorizationCode,
            Uri redirectUri, ClientAssertionCertificate certificate, string[] scope)
        {
            return
                await
                    RunTaskAsync(this.context.AcquireTokenByAuthorizationCodeAsync(authorizationCode, redirectUri,
                        certificate, scope));
        }

        public async Task<AuthenticationResultProxy> AcquireTokenByAuthorizationCodeAsync(string authorizationCode,
            Uri redirectUri, ClientAssertion credential, string[] scope)
        {
            return
                await
                    RunTaskAsync(this.context.AcquireTokenByAuthorizationCodeAsync(authorizationCode, redirectUri,
                        credential, scope));
        }

        internal void VerifySingleItemInCache(AuthenticationResultProxy result, StsType stsType)
        {
            List<TokenCacheItem> items = this.context.TokenCache.ReadItems().ToList();
            Verify.AreEqual(1, items.Count);
            Verify.AreEqual(result.Token, items[0].Token);
            Verify.AreEqual(result.ProfileInfo ?? string.Empty, items[0].ProfileInfo ?? string.Empty);
            Verify.IsTrue(stsType == StsType.ADFS || items[0].ProfileInfo != null);
        }

        private static AuthenticationResultProxy GetAuthenticationResultProxy(AuthenticationResult result)
        {
            return new AuthenticationResultProxy
            {
                Status = AuthenticationStatusProxy.Success,
                Token = result.Token,
                TokenType = result.TokenType,
                ExpiresOn = result.ExpiresOn,
                ProfileInfo = result.ProfileInfo,
                TenantId = result.TenantId,
                UserInfo = result.UserInfo
            };
        }

        private static AuthenticationResultProxy GetAuthenticationResultProxy(Exception ex)
        {
            var output = new AuthenticationResultProxy
            {
                ErrorDescription = ex.Message,
            };

            output.Status = AuthenticationStatusProxy.ClientError;
            if (ex is ArgumentNullException)
            {
                output.Error = AdalError.InvalidArgument;
            }
            else if (ex is ArgumentException)
            {
                output.Error = AdalError.InvalidArgument;
            }
            else if (ex is AdalServiceException)
            {
                output.Error = ((AdalServiceException) ex).ErrorCode;
                output.ExceptionStatusCode = ((AdalServiceException) ex).StatusCode;
                output.ExceptionServiceErrorCodes = ((AdalServiceException) ex).ServiceErrorCodes;
                output.Status = AuthenticationStatusProxy.ServiceError;
            }
            else if (ex is AdalException)
            {
                output.Error = ((AdalException) ex).ErrorCode;
            }
            else
            {
                output.Error = AdalError.AuthenticationFailed;
            }

            output.Exception = ex;

            return output;
        }
    }
}