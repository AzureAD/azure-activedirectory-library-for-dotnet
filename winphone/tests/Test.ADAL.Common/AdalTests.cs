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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Test.ADAL.WinPhone.UnitTest;

namespace Test.ADAL.Common
{
    enum TestType
    {
        DotNet,
        WinRT,
        WinPhone
    }

    internal partial class AdalTests
    {
        private const string SecondCallExtraQueryParameter = "secondcall";
        private const string ThirdCallExtraQueryParameter = "thirdcall";

        public static TestType TestType { get; set; }

        public static void InitializeTest()
        {
            AuthenticationContextProxy.InitializeTest();
        }

        public async static void AcquireTokenPositive(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            var factory = new ReplayerHttpWebRequestFactory();
            factory.UserProvidedHttpWebRequest = new ReplayerHttpWebRequest("https://login.windows.net/aaltests.onmicrosoft.com");
            factory.UserProvidedHttpWebRequest.Success = true;
            factory.UserProvidedHttpWebRequest.ResponseValue = AdalConstants.AadSuccessToken;
            NetworkPlugin.HttpWebRequestFactory = factory;
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);
        }

        public async static void AcquireTokenPositiveWithoutRedirectUriOrUserId(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(sts.ValidUserId, sts.ValidPassword);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);

            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);
            VerifySuccessResult(sts, result);

            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, null);
            VerifySuccessResult(sts, result);

            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, string.Empty);
            VerifySuccessResult(sts, result);
        }

        public static async Task AcquireTokenPositiveByRefreshTokenTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            var factory = new ReplayerHttpWebRequestFactory();
            factory.UserProvidedHttpWebRequest = new ReplayerHttpWebRequest("https://login.windows.net/aaltests.onmicrosoft.com");
            factory.UserProvidedHttpWebRequest.Success = true;
            factory.UserProvidedHttpWebRequest.ResponseValue = AdalConstants.AadSuccessToken;
            NetworkPlugin.HttpWebRequestFactory = factory;
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            result = await context.AcquireTokenByRefreshTokenAsync(result.RefreshToken, sts.ValidClientId, (string)null);
            VerifySuccessResult(sts, result, true, false);

            result = await context.AcquireTokenByRefreshTokenAsync(result.RefreshToken, sts.ValidClientId, sts.ValidResource);
            if (sts.Type == StsType.ADFS)
            {
                VerifyErrorResult(result, Sts.InvalidArgumentError, "multiple resource");                
            }
            else
            {
                VerifySuccessResult(sts, result, true, false);
            }
        }

        public static async void AcquireTokenWithRedirectUriTest(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);

            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.InvalidExistingRedirectUri, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, null);

            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.InvalidNonExistingRedirectUri, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, null);

            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, new Uri(sts.ValidNonExistingRedirectUri.AbsoluteUri + "#fragment"), sts.ValidUserId);
            VerifyErrorResult(result, Sts.InvalidArgumentError, "redirectUri");
            VerifyErrorResult(result, Sts.InvalidArgumentError, "fragment");

            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, null, sts.ValidUserId);
            if (TestType != TestType.WinRT)
            {
                VerifyErrorResult(result, Sts.InvalidArgumentError, "redirectUri");
            }
            else
            {
                // Winrt can send null redirecturi
                VerifySuccessResult(sts, result);
            }

            AuthenticationContextProxy.ClearDefaultCache();
            EndBrowserDialogSession();
            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientIdWithExistingRedirectUri, sts.ValidExistingRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            AuthenticationContextProxy.ClearDefaultCache();

            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidNonExistentRedirectUriClientId, sts.ValidNonExistingRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);            
        }

        public static async void AcquireTokenWithInvalidAuthority(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync("https://www.live.com/login", false);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, null);

            context = await AuthenticationContextProxy.CreateProxyAsync(sts.InvalidAuthority, false);
            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationUiFailedError, null);

            if (sts.Type != StsType.ADFS)
            {
                Uri uri = new Uri(sts.Authority);
                context = await AuthenticationContextProxy.CreateProxyAsync(string.Format("{0}://{1}/non_existing_tenant", uri.Scheme, uri.Authority));
                result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
                VerifyErrorResult(result, Sts.AuthenticationCanceledError, null);
            }
        }

        public static async void AcquireTokenWithInvalidResource(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.InvalidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifyErrorResult(result, Sts.InvalidResourceError, "resource");

            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            result = context.ContinueAcquireToken(sts.ValidResource.ToUpper(), sts.ValidClientId.ToUpper(), sts.ValidDefaultRedirectUri, result.UserInfo.UserId.ToUpper());
            VerifySuccessResult(sts, result);
        }

        public static async void AcquireTokenWithInvalidClientId(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.InvalidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, null);
        }

        public static async void AcquireTokenWithIncorrectUserCredentialTest(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(sts.InvalidUserId, "invalid_password");
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, (string)null, "incorrect_user");
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, "canceled");
        }

        public static async void AcquireTokenWithAuthenticationCanceledTest(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(null, null);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, "cancel_authentication@test.com");
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, "canceled");
        }

        public static async void AcquireTokenPositiveWithDefaultCacheTest(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            List<AuthenticationResultProxy> results = AcquireTokenPositiveWithCache(sts, context);
            Verify.IsTrue(AreDateTimeOffsetsEqual(results[0].ExpiresOn, results[1].ExpiresOn), "AuthenticationResult.ExpiresOn." + " results[0]: " + results[0].ExpiresOn + ", results[1]: " + results[1].ExpiresOn);

            Log.Comment("Waiting 2 seconds before next token request...");
            AuthenticationContextProxy.Delay(2000);   // 2 seconds delay
            AuthenticationResultProxy resultWithoutUser = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, null, SecondCallExtraQueryParameter);
            VerifyExpiresOnAreEqual(results[0], resultWithoutUser);
        }

        public static async void AcquireTokenPositiveWithNullCache(Sts sts)
        {
            var context = await AuthenticationContextProxy.CreateProxyAsync(
                sts.Authority,
                sts.ValidateAuthority,
                TokenCacheStoreType.Null);
            var factory = new ReplayerHttpWebRequestFactory();
            factory.UserProvidedHttpWebRequest = new ReplayerHttpWebRequest("https://login.windows.net/aaltests.onmicrosoft.com");
            factory.UserProvidedHttpWebRequest.Success = true;
            factory.UserProvidedHttpWebRequest.ResponseValue = AdalConstants.AadSuccessToken;
            NetworkPlugin.HttpWebRequestFactory = factory;
            List<AuthenticationResultProxy> results = AcquireTokenPositiveWithCache(sts, context);
            Verify.AreNotEqual(results[0].ExpiresOn, results[1].ExpiresOn, "AuthenticationResult.ExpiresOn");
        }

        public static async void AcquireTokenPositiveWithInMemoryCache(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority, TokenCacheStoreType.InMemory);
            List<AuthenticationResultProxy> results = AcquireTokenPositiveWithCacheExpectingEqualResults(sts, context);
            Verify.IsTrue(AreDateTimeOffsetsEqual(results[0].ExpiresOn, results[1].ExpiresOn), "AuthenticationResult.ExpiresOn");
        }

        public static async void UserInfoTest(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            NetworkPlugin.AdalWabResultHandler = new MockAdalWabResultHandler(sts.Authority+"?code=AwABAAAAvPM1KaPlrEqdFSBzjqfTGEqB2qiVJz262oC8P62ECTquJzAX9kSEgRteZAK0BH3_1ZIg-MMDRX-4QDVQiEG_apiTCM_Sy86cuXY6RfML1xl8gkKJQPsAEXauak13OubGII7orHspp4N1XxTKDpjutUTZg5ZeN97n-eH7xif0v2wybSwr2YB126jnP0jdbQNG0rsjkXmhJNQa9o__A7tzOrZC7dmI9psraTRUhInwD1dKoKPjiqh58nUtA3D6KEZZcAyCaZfkVFRESgtGs0aaoezD6PrxEMeKKUL2xk5N14BcVziej-_angRA_sbyZnZtwNK7S1np916U7G198LHbMl7r3fow85FNZLXw4N3_jcV_uB5bK69SmJ9vpBQtagJXaQd4CpcoLG8KFUPbqudABSAA", 0, WebAuthenticationStatus.Success);

            var factory = new ReplayerHttpWebRequestFactory();
            factory.UserProvidedHttpWebRequest = new ReplayerHttpWebRequest("https://login.windows.net/aaltests.onmicrosoft.com");
            factory.UserProvidedHttpWebRequest.Success = true;
            factory.UserProvidedHttpWebRequest.ResponseValue = AdalConstants.AadSuccessToken;
            NetworkPlugin.HttpWebRequestFactory = factory;
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);
            if (sts.Type == StsType.AAD)
            {
                Verify.IsTrue(result.UserInfo.IsUserIdDisplayable);
                Verify.AreEqual(sts.ValidUserId, result.UserInfo.UserId);
                Verify.IsNotNull(result.UserInfo.GivenName);
                Verify.IsNotNull(result.UserInfo.FamilyName);
            }

            Log.Comment("Waiting 2 seconds before next token request...");
            AuthenticationContextProxy.Delay(2000);   // 2 seconds delay
            AuthenticationContextProxy.SetCredentials(null, null);
            AuthenticationResultProxy result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, result.UserInfo.UserId, SecondCallExtraQueryParameter);
            ValidateAuthenticationResultsAreEqual(result, result2);

            AuthenticationContextProxy.SetCredentials(null, null);
            result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);
            Verify.AreEqual(result.AccessToken, result2.AccessToken);

            SetCredential(sts);
            result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId, ThirdCallExtraQueryParameter);
            VerifySuccessResult(sts, result2);
            if (result.UserInfo.IsUserIdDisplayable)
            {
                ValidateAuthenticationResultsAreEqual(result, result2);
            }
            else
            {
                VerifyExpiresOnAreNotEqual(result, result2);
            }
        }

        public static async Task MultiResourceRefreshTokenTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);
            AuthenticationResultProxy result2 = await context.AcquireTokenByRefreshTokenAsync(result.RefreshToken, sts.ValidClientId, sts.ValidResource2);

            if (sts.Type == StsType.ADFS)
            {
                Verify.IsFalse(result.IsMultipleResourceRefreshToken);
                VerifyErrorResult(result2, Sts.InvalidArgumentError, "multiple resources");
            }
            else
            {
                Verify.IsTrue(result.IsMultipleResourceRefreshToken);
                VerifySuccessResult(sts, result2, true, false);
                Verify.IsTrue(result.IsMultipleResourceRefreshToken);
            }

            result2 = context.ContinueAcquireToken(sts.ValidResource2, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result2);
            if (sts.Type == StsType.ADFS)
            {
                Verify.IsFalse(result.IsMultipleResourceRefreshToken);
            }
            else
            {
                Verify.IsTrue(result.IsMultipleResourceRefreshToken);                
            }

            if (sts.Type == StsType.AAD)
            {
                result2 = context.ContinueAcquireToken(sts.ValidResource3, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
                VerifySuccessResult(sts, result2);
                Verify.IsTrue(result.IsMultipleResourceRefreshToken);
            }
        }

        public static async void TenantlessTest(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.TenantlessAuthority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);
            Verify.IsNotNull(result.TenantId);

            AuthenticationContextProxy.SetCredentials(null, null);
            AuthenticationResultProxy result2 = context.ContinueAcquireToken(
                sts.ValidResource, 
                sts.ValidClientId,
                sts.ValidDefaultRedirectUri, 
                sts.ValidUserId);

            ValidateAuthenticationResultsAreEqual(result, result2);

            SetCredential(sts);
            context = await AuthenticationContextProxy.CreateProxyAsync(sts.TenantlessAuthority.Replace("Common", result.TenantId), sts.ValidateAuthority, TokenCacheStoreType.Null);
            result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result2);
        }

        public static async Task InstanceDiscoveryErrorTestAsync(Sts sts)
        {
            try
            {
                AuthenticationContext ctx =
                    AuthenticationContext.CreateAsync("https://sdfvsedf.windows.net/hfjrhf", true).GetResults();
                Verify.Fail("AuthenticationContext create should have failed");
            }
            catch (Exception ex)
            {
            }
        }

        //public static async Task InstanceDiscoveryTestAsync(Sts sts)
        //{
        //    SetCredential(sts);
        //    var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
        //    AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
        //    VerifySuccessResult(sts, result);

        //    AuthenticationContextProxy.SetEnvironmentVariable("ExtraQueryParameter", string.Empty);

        //    // PROD discovery endpoint knows about PPE as well, so this passes discovery and fails later as refresh token is invalid for PPE.
        //    context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority.Replace("windows.net", "windows-ppe.net"), sts.ValidateAuthority);
        //    result = await context.AcquireTokenByRefreshTokenAsync(result.RefreshToken, sts.ValidClientId, sts.ValidResource);
        //    VerifyErrorResult(result, "invalid_grant", "Refresh Token");

        //    context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority.Replace("windows.net", "windows.unknown"), sts.ValidateAuthority);
        //    result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
        //    VerifyErrorResult(result, "authority_not_in_valid_list", "invalid_instance");
        //}

        public static async void ForcePromptTest(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            AuthenticationContextProxy.SetCredentials(null, null);
            AuthenticationResultProxy result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, 
                (sts.Type == StsType.ADFS) ? result.UserInfo.UserId : sts.ValidUserId);
            VerifySuccessResult(sts, result2);
            Verify.AreEqual(result2.AccessToken, result.AccessToken);

            AuthenticationContextProxy.SetCredentials(sts.ValidUserId, sts.ValidPassword);
            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);
            VerifySuccessResult(sts, result);
            Verify.AreNotEqual(result2.AccessToken, result.AccessToken);
        }

        public static async void AcquireTokenPositiveWithFederatedTenant(Sts sts, bool domainJoined)
        {
            string userId = sts.ValidUserId;

            AuthenticationContextProxy.SetCredentials(userId, sts.ValidPassword);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, false, TokenCacheStoreType.Null);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, userId);
            VerifySuccessResult(sts, result);

            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, null);
            VerifySuccessResult(sts, result);
        }

        public static async Task WebExceptionAccessTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);
            result = await context.AcquireTokenByRefreshTokenAsync(result.RefreshToken, sts.InvalidClientId);
            VerifyErrorResult(result, "invalid_request", "AADSTS90011");
            Verify.IsNotNull(result.Exception);
            Verify.IsNotNull(result.Exception.InnerException);
            Verify.IsTrue(result.Exception.InnerException is WebException);
            using (StreamReader sr = new StreamReader(((WebException)(result.Exception.InnerException)).Response.GetResponseStream()))
            {
                string streamBody = sr.ReadToEnd();
                Verify.IsTrue(streamBody.Contains("AADSTS90011"));
            }
        }

        internal static async void AcquireTokenWithPromptBehaviorNeverTest(Sts sts)
        {
            // Should not be able to get a token silently on first try.
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId,
                sts.ValidDefaultRedirectUri);
            VerifyErrorResult(result, Sts.UserInteractionRequired, null);

            AuthenticationContextProxy.SetCredentials(sts.ValidUserId, sts.ValidPassword);
            // Obtain a token interactively.
            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            AuthenticationContextProxy.SetCredentials(null, null);
            // Now there should be a token available in the cache so token should be available silently.
            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);
            VerifySuccessResult(sts, result);

            // Clear the cache and silent auth should work via session cookies.
            AuthenticationContextProxy.ClearDefaultCache();
            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);
            VerifySuccessResult(sts, result);

            // Clear the cache and cookies and silent auth should fail.
            AuthenticationContextProxy.ClearDefaultCache();
            EndBrowserDialogSession();
            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);
            VerifyErrorResult(result, Sts.UserInteractionRequired, null);                
        }

        internal static async void MultiUserCacheTest(Sts sts)
        {
            Log.Comment("Acquire token for user1 interactively");
            AuthenticationContextProxy.SetCredentials(null, sts.ValidPassword);            
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserId, result.UserInfo.UserId);

            Log.Comment("Acquire token for user1 returning cached token");
            AuthenticationContextProxy.SetCredentials(null, null);            
            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserId, result.UserInfo.UserId);

            Log.Comment("Fail to acquire token for user2 via cookie for user1");
            AuthenticationResultProxy result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId2);
            VerifyErrorResult(result2, "user_mismatch", null);

            Log.Comment("Clear cookie and acquire token for user2 interactively");
            EndBrowserDialogSession();
            AuthenticationContextProxy.SetCredentials(null, sts.ValidPassword2);            
            result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId2);
            VerifySuccessResultAndTokenContent(sts, result2);
            Verify.AreEqual(sts.ValidUserId2, result2.UserInfo.UserId);

            Log.Comment("Acquire token for user1 returning cached token");
            AuthenticationContextProxy.SetCredentials(null, null);
            result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserId, result.UserInfo.UserId);

            Log.Comment("Acquire token for user2 returning cached token");
            AuthenticationContextProxy.SetCredentials(null, null);
            result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId2);
            VerifySuccessResultAndTokenContent(sts, result2);
            Verify.AreEqual(sts.ValidUserId2, result2.UserInfo.UserId);

            Log.Comment("Acquire token for user1 and resource2 using cached multi resource refresh token");
            AuthenticationContextProxy.SetCredentials(null, null);
            result = context.ContinueAcquireToken(sts.ValidResource2, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserId, result.UserInfo.UserId);

            Log.Comment("Acquire token for user2 and resource2 using cached multi resource refresh token");
            AuthenticationContextProxy.SetCredentials(null, null);
            result2 = context.ContinueAcquireToken(sts.ValidResource2, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId2);
            VerifySuccessResultAndTokenContent(sts, result2);
            Verify.AreEqual(sts.ValidUserId2, result2.UserInfo.UserId);
        }

        internal static async void SwitchUserTest(Sts sts)
        {
            Log.Comment("Acquire token for user1 interactively");
            AuthenticationContextProxy.SetCredentials(null, sts.ValidPassword);
            var context = await AuthenticationContextProxy.CreateProxyAsync(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserId, result.UserInfo.UserId);

            Log.Comment("Fail to acquire token for user2 via cookie for user1");
            AuthenticationResultProxy result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId2);
            VerifyErrorResult(result2, "user_mismatch", null);

            Log.Comment("Acquire token via cookie for user1 without user");
            result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserId, result.UserInfo.UserId);

            Log.Comment("Acquire token for user2 via force prompt and user");
            AuthenticationContextProxy.SetCredentials(sts.ValidUserId2, sts.ValidPassword2);
            result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId2);
            VerifySuccessResultAndTokenContent(sts, result2);
            Verify.AreEqual(sts.ValidUserId2, result2.UserInfo.UserId);

            Log.Comment("Acquire token for user2 via force prompt");
            AuthenticationContextProxy.SetCredentials(sts.ValidUserId2, sts.ValidPassword2);
            result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);
            VerifySuccessResultAndTokenContent(sts, result2);
            Verify.AreEqual(sts.ValidUserId2, result2.UserInfo.UserId);

            Log.Comment("Fail to acquire token without user while tokens for two users in the cache");
            result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);
            VerifyErrorResult(result2, "multiple_matching_tokens_detected", null);
        }

        public static void VerifyExpiresOnAreEqual(AuthenticationResultProxy result, AuthenticationResultProxy result2)
        {
            Verify.IsTrue(AreDateTimeOffsetsEqual(result.ExpiresOn, result2.ExpiresOn), "AuthenticationResult.ExpiresOn");
        }

        public static void VerifyExpiresOnAreNotEqual(AuthenticationResultProxy result, AuthenticationResultProxy result2)
        {
            Verify.IsFalse(AreDateTimeOffsetsEqual(result.ExpiresOn, result2.ExpiresOn), "AuthenticationResult.ExpiresOn");
        }

        public static bool AreDateTimeOffsetsEqual(DateTimeOffset time1, DateTimeOffset time2)
        {
            bool equal = (time1.Ticks / 10000 == time2.Ticks / 10000);
            if (!equal)
            {
                Log.Comment("DateTimeOffsets with ticks {0} and {1} are not equal", time1.Ticks, time2.Ticks);
            }

            return equal;
        }

        public static List<AuthenticationResultProxy> AcquireTokenPositiveWithCache(Sts sts, AuthenticationContextProxy context)
        {
            AuthenticationResultProxy result = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            Log.Comment("Waiting 2 seconds before next token request...");
            AuthenticationContextProxy.Delay(2000);   // 2 seconds delay

            AuthenticationResultProxy result2;
            if (result.UserInfo.IsUserIdDisplayable)
                result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, result.UserInfo.UserId, SecondCallExtraQueryParameter);
            else
                result2 = context.ContinueAcquireToken(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri);

            VerifySuccessResult(sts, result2);

            return new List<AuthenticationResultProxy> { result, result2 };
        }

        public static void EndBrowserDialogSession()
        {
            const int INTERNET_OPTION_END_BROWSER_SESSION = 42;
            NativeMethods.InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
        }

        public static void VerifySuccessResult(Sts sts, AuthenticationResultProxy result, bool supportRefreshToken = true, bool supportUserInfo = true)
        {
            Log.Comment("Verifying success result...");
            if (result.Status == AuthenticationStatusProxy.Failed)
            {
                Log.Comment(string.Format("Unexpected '{0}' error from service: {1}", result.Error, result.ErrorDescription));
            }

            Verify.AreEqual(AuthenticationStatusProxy.Succeeded, result.Status, "AuthenticationResult.Status");
            Verify.IsNotNull(result.AccessToken, "AuthenticationResult.AccessToken");
            if (supportRefreshToken)
            {
                Verify.IsNotNull(result.RefreshToken, "AuthenticationResult.RefreshToken");
            }
            else
            {
                Verify.IsNull(result.RefreshToken, "AuthenticationResult.RefreshToken");
            }

            Verify.IsNull(result.Error, "AuthenticationResult.Error");
            Verify.IsNull(result.ErrorDescription, "AuthenticationResult.ErrorDescription");

            if (sts.Type != StsType.ADFS && supportUserInfo)
            {
                Action<string, string, bool> ValidateUserInfo = (string field, string caption, bool required) =>
                {
                    if (string.IsNullOrEmpty(field))
                    {
                        if (required)
                        {
                            Log.Error("No " + caption);
                        }
                        else
                        {
                            Log.Warning("No " + caption);                            
                        }
                    }
                    else
                    {
                        Log.Comment(field, caption);
                    }
                };

                ValidateUserInfo(result.TenantId, "tenant id", true);
                ValidateUserInfo(result.UserInfo.UserId, "user id", true);
                ValidateUserInfo(result.UserInfo.GivenName, "given name", false);
                ValidateUserInfo(result.UserInfo.FamilyName, "family name", false);
            }

            long expiresIn = (long)(result.ExpiresOn - DateTime.UtcNow).TotalSeconds;
            Log.Comment("Verifying token expiration...");
            Verify.IsGreaterThanOrEqual(expiresIn, (long)0, "Token ExpiresOn");
        }

        public static void VerifyErrorResult(AuthenticationResultProxy result, string error, string errorDescriptionKeyword, int innerStatuCode = 0)
        {
            Log.Comment(string.Format("Verifying error result '{0}':'{1}'...", result.Error, result.ErrorDescription));
            Verify.AreEqual(AuthenticationStatusProxy.Failed, result.Status);
            Verify.IsNull(result.AccessToken);
            Verify.IsNotNull(result.Error);
            Verify.IsNotNull(result.ErrorDescription);
            Verify.IsFalse(result.ErrorDescription.Contains("+"), "Error description should not be in URL form encoding!");
            Verify.IsFalse(result.ErrorDescription.Contains("%2"), "Error description should not be in URL encoding!");

            if (error != null)
            {
                Verify.AreEqual(error, result.Error);
            }

            if (errorDescriptionKeyword != null)
            {
                VerifyErrorDescriptionContains(result.ErrorDescription, errorDescriptionKeyword);
            }

            if (innerStatuCode != 0)
            {
                Verify.AreEqual(innerStatuCode, result.ExceptionInnerStatusCode);
            }
        }

        private static List<AuthenticationResultProxy> AcquireTokenPositiveWithCacheExpectingEqualResults(Sts sts, AuthenticationContextProxy context)
        {
            List<AuthenticationResultProxy> results = AcquireTokenPositiveWithCache(sts, context);

            Verify.AreEqual(results[0].AccessToken, results[1].AccessToken, "AuthenticationResult.AccessToken");
            Log.Comment(string.Format("First ExpiresOn: {0}", results[0].ExpiresOn));
            Log.Comment(string.Format("Second ExpiresOn: {0}", results[1].ExpiresOn));
            return results;
        }

        private static void VerifyErrorDescriptionContains(string errorDescription, string keyword)
        {
            Log.Comment(string.Format("Verifying error description '{0}'...", errorDescription));
            Verify.IsGreaterThanOrEqual(errorDescription.IndexOf(keyword, StringComparison.OrdinalIgnoreCase), 0);
        }

        private static void ValidateAuthenticationResultsAreEqual(AuthenticationResultProxy result, AuthenticationResultProxy result2)
        {
            Verify.AreEqual(result.AccessToken, result2.AccessToken, "AuthenticationResult.AccessToken");
            Verify.AreEqual(result.RefreshToken, result2.RefreshToken, "AuthenticationResult.RefreshToken");
            Verify.AreEqual(result.UserInfo.UserId, result2.UserInfo.UserId);
            Verify.AreEqual(result.UserInfo.IsUserIdDisplayable, result2.UserInfo.IsUserIdDisplayable);
            Verify.AreEqual(result.UserInfo.GivenName, result2.UserInfo.GivenName);
            Verify.AreEqual(result.UserInfo.FamilyName, result2.UserInfo.FamilyName);
            Verify.AreEqual(result.TenantId, result2.TenantId);
        }
        
        private static void SetCredential(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(sts.Type == StsType.ADFS ? sts.ValidUserId : null, sts.ValidPassword);            
        }

        private static class NativeMethods
        {
            [DllImport("wininet.dll", SetLastError = true)]
            public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
        }
    }
}
