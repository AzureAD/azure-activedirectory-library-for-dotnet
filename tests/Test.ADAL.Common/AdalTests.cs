﻿//----------------------------------------------------------------------
//------------------------------------------------------------------------------
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
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Test.ADAL.Common
{
    enum TestType
    {
        DotNet,
        WinRT
    }

    internal partial class AdalTests
    {
        private const string SecondCallExtraQueryParameter = "secondcall";
        private const string ThirdCallExtraQueryParameter = "thirdcall";

        public static TestType TestType { get; set; }

        public static IPlatformParameters PlatformParameters { get; set; }

        public static void InitializeTest()
        {
            AuthenticationContextProxy.InitializeTest();
        }

        public static async Task AcquireTokenPositiveTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);
        }

        public static async Task NonHttpsURLNegativeTest(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(sts.ValidUserName, sts.ValidPassword);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);

            // Obtain a token interactively.
            AuthenticationResultProxy result =await  context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, UserIdentifier.AnyUser);
            VerifyErrorDescriptionContains(result.ErrorDescription, "Non-HTTPS url redirect is not supported in webview");
        }

        public static async Task AcquireTokenPositiveWithoutRedirectUriOrUserIdTestAsync(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(sts.ValidUserName, sts.ValidPassword);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);

            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters);
            VerifySuccessResult(sts, result);

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, null);
            VerifyErrorResult(result, Sts.InvalidArgumentError, "userId");
            VerifyErrorResult(result, Sts.InvalidArgumentError, "UserIdentifier.AnyUser");

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, UserIdentifier.AnyUser);
            VerifySuccessResult(sts, result);
        }

        public static async Task AuthenticationContextAuthorityValidationTestAsync(Sts sts)
        {
            SetCredential(sts);
            AuthenticationContextProxy context = null;
            AuthenticationResultProxy result = null;
            try
            {
                context = new AuthenticationContextProxy(sts.InvalidAuthority, true);
                Verify.AreNotEqual(sts.Type, StsType.ADFS);
                result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
                VerifyErrorResult(result, Sts.AuthorityNotInValidList, "authority");
            }
            catch (ArgumentException ex)
            {
                Verify.AreEqual(sts.Type, StsType.ADFS);
                Verify.AreEqual(ex.ParamName, "validateAuthority");
            }
#if TEST_ADAL_WINPHONE_UNIT
            catch (AdalServiceException ex)
            {
                Verify.AreNotEqual(sts.Type, StsType.ADFS);
                Verify.AreEqual(ex.ErrorCode, Sts.AuthorityNotInValidList);
                Verify.IsTrue(ex.Message.Contains("authority"));
            }
#endif

            context = new AuthenticationContextProxy(sts.InvalidAuthority, false);
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationUiFailedError, "authentication dialog");
            context = new AuthenticationContextProxy(sts.Authority, false);
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            if (sts.Type != StsType.ADFS)
            {
                context = new AuthenticationContextProxy(sts.Authority, true);
                result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
                VerifySuccessResult(sts, result);
            }

            try
            {
                context = new AuthenticationContextProxy(sts.InvalidAuthority);
                Verify.AreNotEqual(sts.Type, StsType.ADFS);
                result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
                VerifyErrorResult(result, Sts.AuthorityNotInValidList, "authority");
            }
            catch (ArgumentException ex)
            {
                Verify.AreEqual(sts.Type, StsType.ADFS);
                Verify.AreEqual(ex.ParamName, "validateAuthority");
            }
#if TEST_ADAL_WINPHONE_UNIT
            catch (AdalServiceException ex)
            {
                Verify.AreNotEqual(sts.Type, StsType.ADFS);
                Verify.AreEqual(ex.ErrorCode, Sts.AuthorityNotInValidList);
                Verify.IsTrue(ex.Message.Contains("authority"));
            }
#endif

            context = new AuthenticationContextProxy(sts.Authority + "/extraPath1/extraPath2", sts.ValidateAuthority);
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);            
        }

        public static async Task AcquireTokenWithRedirectUriTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);

            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.InvalidExistingRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, null);

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.InvalidNonExistingRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, null);

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, new Uri(sts.ValidNonExistingRedirectUri.OriginalString + "#fragment"), PlatformParameters, sts.ValidUserId);
            VerifyErrorResult(result, Sts.InvalidArgumentError, "redirectUri");
            VerifyErrorResult(result, Sts.InvalidArgumentError, "fragment");

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, null, PlatformParameters, sts.ValidUserId);
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
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientIdWithExistingRedirectUri, sts.ValidExistingRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            AuthenticationContextProxy.ClearDefaultCache();

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidNonExistentRedirectUriClientId, sts.ValidNonExistingRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);            
        }

        public static async Task AcquireTokenWithInvalidAuthorityTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy("https://www.outlook.com/login", false);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationUiFailedError, null);

            context = new AuthenticationContextProxy(sts.InvalidAuthority, false);
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationUiFailedError, null);

            if (sts.Type != StsType.ADFS)
            {
                Uri uri = new Uri(sts.Authority);
                context = new AuthenticationContextProxy(string.Format(CultureInfo.CurrentCulture, " {0}://{1}/non_existing_tenant", uri.Scheme, uri.Authority));
                result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
                VerifyErrorResult(result, Sts.AuthenticationCanceledError, null);
            }
        }

        public static async Task AcquireTokenWithInvalidResourceTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.InvalidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifyErrorResult(result, Sts.InvalidResourceError, "resource");

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            result = await context.AcquireTokenAsync(sts.ValidResource.ToUpper(), sts.ValidClientId.ToUpper(), sts.ValidDefaultRedirectUri, PlatformParameters, 
                (sts.Type == StsType.AAD) ? new UserIdentifier(sts.ValidUserName, UserIdentifierType.RequiredDisplayableId) : UserIdentifier.AnyUser);
            VerifySuccessResult(sts, result);

            result = await context.AcquireTokenAsync(sts.ValidResource.ToUpper(), sts.ValidClientId.ToUpper(), sts.ValidDefaultRedirectUri, PlatformParameters,
                (result.UserInfo != null) ? new UserIdentifier(result.UserInfo.UniqueId, UserIdentifierType.UniqueId) : UserIdentifier.AnyUser);
            VerifySuccessResult(sts, result);
        }

        public static async Task AcquireTokenWithInvalidClientIdTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.InvalidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, null);
        }

        public static async Task AcquireTokenWithIncorrectUserCredentialTestAsync(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(sts.InvalidUserName, "invalid_password");
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, UserIdentifier.AnyUser, "incorrect_user");
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, "canceled");
        }

        public static async Task AcquireTokenWithAuthenticationCanceledTestAsync(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(null, null);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, new UserIdentifier("cancel_authentication@test.com", UserIdentifierType.OptionalDisplayableId));
            VerifyErrorResult(result, Sts.AuthenticationCanceledError, "canceled");
        }

        public static async Task AcquireTokenPositiveWithDefaultCacheTestAsync(Sts sts)
        {
            AuthenticationContextProxy.ClearDefaultCache();

            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            List<AuthenticationResultProxy> results = await AcquireTokenPositiveWithCacheAsync(sts, context);
            VerifyExpiresOnAreEqual(results[0], results[1]);

            EndBrowserDialogSession();
            Log.Comment("Waiting 2 seconds before next token request...");
            AuthenticationContextProxy.Delay(2000);   // 2 seconds delay
            AuthenticationResultProxy resultWithoutUser = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, UserIdentifier.AnyUser, SecondCallExtraQueryParameter);
            VerifyExpiresOnAreEqual(results[0], resultWithoutUser);

            context.VerifySingleItemInCache(results[0], sts.Type);
        }

        public static async Task AcquireTokenPositiveWithNullCacheTestAsync(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(sts.ValidUserName, sts.ValidPassword);
            var context = new AuthenticationContextProxy(
                sts.Authority,
                sts.ValidateAuthority,
                TokenCacheType.Null);
            List<AuthenticationResultProxy> results = await AcquireTokenPositiveWithCacheAsync(sts, context);
            VerifyExpiresOnAreNotEqual(results[0], results[1]);
        }

        public static async Task AcquireTokenPositiveWithInMemoryCacheTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority, TokenCacheType.InMemory);
            List<AuthenticationResultProxy> results = await AcquireTokenPositiveWithCacheExpectingEqualResultsAsync(sts, context);
            VerifyExpiresOnAreEqual(results[0], results[1]);
        }

        public static async Task UserInfoTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            AuthenticationResultProxy result2;
            if (sts.Type == StsType.AAD)
            {
                Verify.AreEqual(sts.ValidUserName, result.UserInfo.DisplayableId);
                Verify.IsNotNullOrEmptyString(result.UserInfo.UniqueId);
                Verify.IsNotNullOrEmptyString(result.UserInfo.GivenName);
                Verify.IsNotNullOrEmptyString(result.UserInfo.FamilyName);

                EndBrowserDialogSession();
                Log.Comment("Waiting 2 seconds before next token request...");
                AuthenticationContextProxy.Delay(2000);   // 2 seconds delay
                AuthenticationContextProxy.SetCredentials(null, null);
                result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters,
                    new UserIdentifier(result.UserInfo.DisplayableId, UserIdentifierType.OptionalDisplayableId), 
                    SecondCallExtraQueryParameter);
                ValidateAuthenticationResultsAreEqual(result, result2);
            }

            AuthenticationContextProxy.SetCredentials(null, null);
            result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters);
            Verify.AreEqual(result.AccessToken, result2.AccessToken);

            SetCredential(sts);
            result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId, ThirdCallExtraQueryParameter);
            VerifySuccessResult(sts, result2);
            if (result.UserInfo != null)
            {
                ValidateAuthenticationResultsAreEqual(result, result2);
            }
            else
            {
                VerifyExpiresOnAreNotEqual(result, result2);
            }

            EndBrowserDialogSession();
            Log.Comment("Waiting 2 seconds before next token request...");
            AuthenticationContextProxy.Delay(2000);   // 2 seconds delay
            AuthenticationContextProxy.SetCredentials(sts.ValidUserName, sts.ValidPassword);
            result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.InvalidRequiredUserId, SecondCallExtraQueryParameter);
            VerifyErrorResult(result2, "user_mismatch", null);
        }

        public static async Task MultiResourceRefreshTokenTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            AuthenticationResultProxy result2 = await context.AcquireTokenAsync(sts.ValidResource2, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result2);

            if (sts.Type == StsType.AAD)
            {
                result2 = await context.AcquireTokenAsync(sts.ValidResource3, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
                VerifySuccessResult(sts, result2);
            }
        }

        public static async Task TenantlessTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.TenantlessAuthority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);
            Verify.IsNotNullOrEmptyString(result.TenantId);

            AuthenticationContextProxy.SetCredentials(null, null);
            AuthenticationResultProxy result2 = await context.AcquireTokenAsync(
                sts.ValidResource, 
                sts.ValidClientId,
                sts.ValidDefaultRedirectUri,
                PlatformParameters, 
                sts.ValidUserId);

            ValidateAuthenticationResultsAreEqual(result, result2);

            SetCredential(sts);
            context = new AuthenticationContextProxy(sts.TenantlessAuthority.Replace("Common", result.TenantId), sts.ValidateAuthority, TokenCacheType.Null);
            result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result2);
        }

        public static async Task InstanceDiscoveryTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            AuthenticationContextProxy.SetEnvironmentVariable("ExtraQueryParameter", string.Empty);

            // PROD discovery endpoint knows about PPE as well, so this passes discovery and fails later as refresh token is invalid for PPE.
            context = new AuthenticationContextProxy(sts.Authority.Replace("windows.net", "windows-ppe.net"), sts.ValidateAuthority);
            result = await context.AcquireTokenAsync(sts.ValidResource, new ClientCredential(sts.ValidClientId, sts.ValidPassword));
            VerifyErrorResult(result, "invalid_request", "No service namespace");

            try
            {
                context = new AuthenticationContextProxy(sts.Authority.Replace("windows.net", "windows.unknown"), sts.ValidateAuthority);
                result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
                VerifyErrorResult(result, "authority_not_in_valid_list", "authority");
            }
#if TEST_ADAL_WINPHONE_UNIT
            catch (AdalServiceException ex)
            {
                Verify.AreNotEqual(sts.Type, StsType.ADFS);
                Verify.AreEqual(ex.ErrorCode, Sts.AuthorityNotInValidList);
                Verify.IsTrue(ex.Message.Contains("authority"));
            }
#endif
            finally
            {
                
            }
        }

        public static async Task AcquireTokenPositiveWithFederatedTenantTestAsync(Sts sts)
        {
            var userId = sts.ValidUserId;

            AuthenticationContextProxy.SetCredentials(userId.Id, sts.ValidPassword);
            var context = new AuthenticationContextProxy(sts.Authority, false, TokenCacheType.Null);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, userId);
            VerifySuccessResult(sts, result);

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, UserIdentifier.AnyUser);
            VerifySuccessResult(sts, result);
        }

#if TEST_ADAL_NET
        public static async Task AcquireTokenNonInteractivePositiveTestAsync(Sts sts)
        {
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            UserCredentialProxy credential = new UserCredentialProxy(sts.ValidUserName, sts.ValidPassword);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, credential);
            VerifySuccessResult(sts, result);
            Verify.IsNotNull(result.UserInfo);
            Verify.IsNotNullOrEmptyString(result.UserInfo.UniqueId);
            Verify.IsNotNullOrEmptyString(result.UserInfo.DisplayableId);

            AuthenticationContextProxy.Delay(2000);

            // Test token cache
            AuthenticationResultProxy result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, credential);
            VerifySuccessResult(sts, result2);
            VerifyExpiresOnAreEqual(result, result2);
        }
#endif

        public static async Task InnerExceptionAccessTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);
            result = await context.AcquireTokenSilentAsync(sts.ValidResource, sts.InvalidClientId);
            VerifyErrorResult(result, "failed_to_acquire_token_silently", null);
            Verify.IsNotNull(result.Exception);
        }

        public static async Task ExtraQueryParametersTestAsync(Sts sts)
        {
            SetCredential(sts);
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority, TokenCacheType.Null);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId, null);
            VerifySuccessResult(sts, result);

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId, "redirect_uri=123");
            VerifyErrorResult(result, "duplicate_query_parameter", "redirect_uri");

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId, "resource=123&dummy=dummy_value#$%^@%^^%");
            VerifyErrorResult(result, "duplicate_query_parameter", "resource");

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId, "client_id=123");
            VerifyErrorResult(result, "duplicate_query_parameter", "client_id");   

            EndBrowserDialogSession();
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId, "login_hint=123");
            VerifyErrorResult(result, "duplicate_query_parameter", "login_hint");   

            EndBrowserDialogSession();
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId, "login_hintx=123");
            VerifySuccessResult(sts, result);

            EndBrowserDialogSession();
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, UserIdentifier.AnyUser, "login_hint=" + sts.ValidUserName);
            VerifySuccessResult(sts, result);

            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId, string.Empty);
            VerifySuccessResult(sts, result);
        }

        internal static async Task MultiUserCacheTestAsync(Sts sts)
        {
            Log.Comment("Acquire token for user1 interactively");
            AuthenticationContextProxy.SetCredentials(null, sts.ValidPassword);            
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserName, result.UserInfo.DisplayableId);

            Log.Comment("Acquire token for user1 returning cached token");
            AuthenticationContextProxy.SetCredentials(null, null);
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserName, result.UserInfo.DisplayableId);

            Log.Comment("Clear cookie and acquire token for user2 interactively");
            EndBrowserDialogSession();
            AuthenticationContextProxy.SetCredentials(null, sts.ValidPassword2);
            AuthenticationResultProxy result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidRequiredUserId2);
            VerifySuccessResultAndTokenContent(sts, result2);
            Verify.AreEqual(sts.ValidUserName2, result2.UserInfo.DisplayableId);

            Log.Comment("Acquire token for user1 returning cached token");
            AuthenticationContextProxy.SetCredentials(null, null);
            result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserName, result.UserInfo.DisplayableId);

            Log.Comment("Acquire token for user2 returning cached token");
            AuthenticationContextProxy.SetCredentials(null, null);
            result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidRequiredUserId2);
            VerifySuccessResultAndTokenContent(sts, result2);
            Verify.AreEqual(sts.ValidUserName2, result2.UserInfo.DisplayableId);

            Log.Comment("Acquire token for user1 and resource2 using cached multi resource refresh token");
            AuthenticationContextProxy.SetCredentials(null, null);
            result = await context.AcquireTokenAsync(sts.ValidResource2, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResultAndTokenContent(sts, result);
            Verify.AreEqual(sts.ValidUserName, result.UserInfo.DisplayableId);

            Log.Comment("Acquire token for user2 and resource2 using cached multi resource refresh token");
            AuthenticationContextProxy.SetCredentials(null, null);
            result2 = await context.AcquireTokenAsync(sts.ValidResource2, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidRequiredUserId2);
            VerifySuccessResultAndTokenContent(sts, result2);
            Verify.AreEqual(sts.ValidUserName2, result2.UserInfo.DisplayableId);
        }

#if TEST_ADAL_NET
        public static async Task ResourceOwnerCredentialsTestAsync(Sts sts)
        {
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            UserCredentialProxy credential = new UserCredentialProxy(sts.ValidUserName, sts.ValidPassword);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, credential);
            VerifySuccessResult(sts, result);
            Verify.IsNotNull(result.UserInfo);
            
            // TODO: Figure out if we should we use mixed case user name to run tests?
            // Verify.AreNotEqual(result.UserInfo.DisplayableId, result.UserInfo.DisplayableId.ToLower());
            
            AuthenticationContextProxy.Delay(2000);   // 2 seconds delay
            AuthenticationResultProxy result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, credential);
            VerifySuccessResult(sts, result2);
            Verify.IsTrue(AreDateTimeOffsetsEqual(result.ExpiresOn, result2.ExpiresOn));
        }

        public static async Task MixedCaseUserNameTestAsync(Sts sts)
        {
            var context = new AuthenticationContextProxy(sts.Authority, sts.ValidateAuthority);
            UserCredentialProxy credential = new UserCredentialProxy(sts.ValidUserName3, sts.ValidPassword3);
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, credential);
            VerifySuccessResult(sts, result);
            Verify.IsNotNull(result.UserInfo);
            Verify.AreNotEqual(result.UserInfo.DisplayableId, result.UserInfo.DisplayableId.ToLower());
            AuthenticationContextProxy.Delay(2000);   // 2 seconds delay
            AuthenticationResultProxy result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, credential);
            VerifySuccessResult(sts, result2);
            Verify.IsTrue(AreDateTimeOffsetsEqual(result.ExpiresOn, result2.ExpiresOn));
        }
#endif
                
        public static void VerifyExpiresOnAreEqual(AuthenticationResultProxy result, AuthenticationResultProxy result2)
        {
            bool equal = AreDateTimeOffsetsEqual(result.ExpiresOn, result2.ExpiresOn);

            if (!equal)
            {
                Log.Comment(result.ExpiresOn.ToString("R") + " <> " + result2.ExpiresOn.ToString("R"));
            }

            Verify.IsTrue(equal, "AuthenticationResult.ExpiresOn");
        }

        public static void VerifyExpiresOnAreNotEqual(AuthenticationResultProxy result, AuthenticationResultProxy result2)
        {
            bool equal = AreDateTimeOffsetsEqual(result.ExpiresOn, result2.ExpiresOn);

            if (equal)
            {
                Log.Comment(result.ExpiresOn.ToString("R") + " <> " + result2.ExpiresOn.ToString("R"));
            }

            Verify.IsFalse(equal, "AuthenticationResult.ExpiresOn");
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

        public static async Task<List<AuthenticationResultProxy>> AcquireTokenPositiveWithCacheAsync(Sts sts, AuthenticationContextProxy context)
        {
            AuthenticationResultProxy result = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, sts.ValidUserId);
            VerifySuccessResult(sts, result);

            Log.Comment("Waiting 2 seconds before next token request...");
            AuthenticationContextProxy.Delay(2000);   // 2 seconds delay

            AuthenticationResultProxy result2;
            if (result.UserInfo != null)
                result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters, new UserIdentifier(result.UserInfo.DisplayableId, UserIdentifierType.OptionalDisplayableId), SecondCallExtraQueryParameter);
            else
                result2 = await context.AcquireTokenAsync(sts.ValidResource, sts.ValidClientId, sts.ValidDefaultRedirectUri, PlatformParameters);

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
            if (result.Status != AuthenticationStatusProxy.Success)
            {
                Log.Comment(string.Format(CultureInfo.CurrentCulture, " Unexpected '{0}' error from service: {1}", result.Error, result.ErrorDescription));
            }

            Verify.AreEqual(AuthenticationStatusProxy.Success, result.Status, "AuthenticationResult.Status");
            Verify.IsNotNullOrEmptyString(result.AccessToken, "AuthenticationResult.AccessToken");

            Verify.IsNullOrEmptyString(result.Error, "AuthenticationResult.Error");
            Verify.IsNullOrEmptyString(result.ErrorDescription, "AuthenticationResult.ErrorDescription");

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
                ValidateUserInfo(result.UserInfo.UniqueId, "user unique id", true);
                ValidateUserInfo(result.UserInfo.DisplayableId, "user displayable id", true);
                ValidateUserInfo(result.UserInfo.IdentityProvider, "identity provider", true);
                ValidateUserInfo(result.UserInfo.GivenName, "given name", false);
                ValidateUserInfo(result.UserInfo.FamilyName, "family name", false);
            }

            long expiresIn = (long)(result.ExpiresOn - DateTime.UtcNow).TotalSeconds;
            Log.Comment("Verifying token expiration...");
            Verify.IsGreaterThanOrEqual(expiresIn, (long)0, "Token ExpiresOn");
        }

        public static void VerifyErrorResult(AuthenticationResultProxy result, string error, string errorDescriptionKeyword, int statusCode = 0, string serviceErrorCode = null)
        {
            Log.Comment(string.Format(CultureInfo.CurrentCulture, " Verifying error result '{0}':'{1}'...", result.Error, result.ErrorDescription));
            Verify.AreNotEqual(AuthenticationStatusProxy.Success, result.Status);
            Verify.IsNullOrEmptyString(result.AccessToken);
            Verify.IsNotNullOrEmptyString(result.Error);
            Verify.IsNotNullOrEmptyString(result.ErrorDescription);
            Verify.IsFalse(result.ErrorDescription.Contains("+"), "Error description should not be in URL form encoding!");
            Verify.IsFalse(result.ErrorDescription.Contains("%2"), "Error description should not be in URL encoding!");

            if (!string.IsNullOrEmpty(error))
            {
                Verify.AreEqual(error, result.Error);
            }

            if (!string.IsNullOrEmpty(errorDescriptionKeyword))
            {
                VerifyErrorDescriptionContains(result.ErrorDescription, errorDescriptionKeyword);
            }

            if (statusCode != 0)
            {
                Verify.AreEqual(statusCode, result.ExceptionStatusCode);
            }

            if (serviceErrorCode != null)
            {
                Verify.IsTrue(result.ExceptionServiceErrorCodes.Contains(serviceErrorCode));
            }
        }

        private static async Task<List<AuthenticationResultProxy>> AcquireTokenPositiveWithCacheExpectingEqualResultsAsync(Sts sts, AuthenticationContextProxy context)
        {
            List<AuthenticationResultProxy> results = await AcquireTokenPositiveWithCacheAsync(sts, context);

            Verify.AreEqual(results[0].AccessToken, results[1].AccessToken, "AuthenticationResult.AccessToken");
            Log.Comment(string.Format(CultureInfo.CurrentCulture, " First ExpiresOn: {0}", results[0].ExpiresOn));
            Log.Comment(string.Format(CultureInfo.CurrentCulture, " Second ExpiresOn: {0}", results[1].ExpiresOn));
            return results;
        }

        private static void VerifyErrorDescriptionContains(string errorDescription, string keyword)
        {
            Log.Comment(string.Format(CultureInfo.CurrentCulture, " Verifying error description '{0}'...", errorDescription));
            Verify.IsGreaterThanOrEqual(errorDescription.IndexOf(keyword, StringComparison.OrdinalIgnoreCase), 0);
        }

        private static void ValidateAuthenticationResultsAreEqual(AuthenticationResultProxy result, AuthenticationResultProxy result2)
        {
            Verify.AreEqual(result.AccessToken, result2.AccessToken, "AuthenticationResult.AccessToken");
            Verify.AreEqual(result.UserInfo.UniqueId, result2.UserInfo.UniqueId);
            Verify.AreEqual(result.UserInfo.DisplayableId, result2.UserInfo.DisplayableId);
            Verify.AreEqual(result.UserInfo.GivenName, result2.UserInfo.GivenName);
            Verify.AreEqual(result.UserInfo.FamilyName, result2.UserInfo.FamilyName);
            Verify.AreEqual(result.TenantId, result2.TenantId);
        }
        
        private static void SetCredential(Sts sts)
        {
            AuthenticationContextProxy.SetCredentials(sts.Type == StsType.ADFS ? sts.ValidUserName : null, sts.ValidPassword);            
        }

        private static class NativeMethods
        {
            [DllImport("wininet.dll", SetLastError = true)]
            public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
        }
    }
}
