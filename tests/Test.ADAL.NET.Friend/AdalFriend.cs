﻿//----------------------------------------------------------------------
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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Test.ADAL.NET.Friend
{
    public static class AdalFriend
    {
        public static ClientAssertion CreateJwt(X509Certificate2 cert, string issuer, string aud)
        {
            ClientAssertionCertificate certificate = new ClientAssertionCertificate(issuer, cert);

            JsonWebToken jwtToken = new JsonWebToken(certificate, aud);
            return jwtToken.Sign(certificate);
        }

        public static string AcquireAccessCode(AuthenticationContext context, string resource, string clientId, Uri redirectUri, UserIdentifier userId)
        {
            var handler = new AcquireTokenInteractiveHandler(context.Authenticator, context.TokenCache, resource, clientId, redirectUri, PromptBehavior.Auto, userId, null,
                context.CreateWebAuthenticationDialog(PromptBehavior.Auto), true, null);
            handler.CallState = null;
            context.Authenticator.AuthorizationUri = context.Authority + "oauth2/authorize";
            handler.AcquireAuthorization();
            return handler.authorizationResult.Code;
        }

        public static void UpdateTokenExpiryOnTokenCache(TokenCache cache, DateTimeOffset newExpiry)
        {
            var cacheDictionary = cache.tokenCacheDictionary;

            var key = cacheDictionary.Keys.First();
            cache.tokenCacheDictionary[key].ExpiresOn = newExpiry; 
            var value = cacheDictionary.Values.First();
            cache.Clear();
            cacheDictionary.Add(key, value);        
        }
    }
}
