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
using Microsoft.Identity.Client.CacheV2;
using Microsoft.Identity.Client.CacheV2.Impl;
using Microsoft.Identity.Client.CacheV2.Impl.Utils;
using Microsoft.Identity.Client.CacheV2.Schema;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.Instance;
using Microsoft.Identity.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AuthorityType = Microsoft.Identity.Client.CacheV2.Schema.AuthorityType;

namespace Test.MSAL.NET.Unit.net45.CacheV2Tests
{
    [TestClass]
    public class CacheManagerTests
    {
        private IdToken GetIdToken(
            string oid,
            string subject,
            string upn,
            string email)
        {
            var idTokenJson = JObject.Parse($"{{'oid': '{oid}', 'sub': '{subject}', 'upn': '{upn}', 'email': '{email}'}}");
            return new IdToken($".{Base64UrlHelpers.Encode(idTokenJson.ToString())}.");
        }

        private TokenResponse GetTokenResponse(
            string uid,
            string utid,
            string subject,
            string upn,
            string email)
        {
            var idToken = GetIdToken("", subject, upn, email);
            var clientInfo = JObject.Parse($"{{'uid': '{uid}', 'utid': '{utid}'}}");
            var tokenResponse = new TokenResponse(null, null, null)
            {
                IdToken = idToken,
                ClientInfo = clientInfo
            };
            return tokenResponse;
        }

        private Credential GetAccessToken(long cachedAt, long expiresOn)
        {
            var accessToken = Credential.CreateEmpty();
            accessToken.CachedAt = cachedAt;
            accessToken.ExpiresOn = expiresOn;
            return accessToken;
        }

        private AuthorityType TestGetAuthorityType(string authority)
        {
            var authParameters = new AuthenticationRequestParameters
            {
                // todo(mzuber): this is going to be wonky probably...
                Authority = Authority.CreateAuthority(null, authority, false)
            };
            var cacheManager = new CacheManager(null, authParameters);
            return cacheManager.GetAuthorityType();
        }

        [TestMethod]
        public void GetLocalAccountId()
        {
            Assert.AreEqual("test_oid", CacheManager.GetLocalAccountId(GetIdToken("test_oid", "test_subject", "", "")));
            Assert.AreEqual("test_oid", CacheManager.GetLocalAccountId(GetIdToken("test_oid", "", "", "")));
            Assert.AreEqual("test_subject", CacheManager.GetLocalAccountId(GetIdToken("", "test_subject", "", "")));
            Assert.AreEqual("", CacheManager.GetLocalAccountId(GetIdToken("", "", "", "")));
        }

        [TestMethod]
        public void GetHomeAccountId()
        {
            Assert.AreEqual(
                "test_uid.test_utid",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "test_uid",
                        "test_utid",
                        "",
                        "",
                        "")));
            Assert.AreEqual(
                "test_uid.test_utid",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "test_uid",
                        "test_utid",
                        "test_subject",
                        "",
                        "")));
            Assert.AreEqual(
                "test_uid.test_utid",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "test_uid",
                        "test_utid",
                        "test_subject",
                        "test_upn",
                        "")));
            Assert.AreEqual(
                "test_uid.test_utid",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "test_uid",
                        "test_utid",
                        "test_subject",
                        "test_upn",
                        "test_email")));

            Assert.AreEqual(
                "test_upn",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "test_utid",
                        "test_subject",
                        "test_upn",
                        "test_email")));
            Assert.AreEqual(
                "test_upn",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "test_uid",
                        "",
                        "test_subject",
                        "test_upn",
                        "test_email")));
            Assert.AreEqual(
                "test_upn",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "",
                        "test_subject",
                        "test_upn",
                        "test_email")));
            Assert.AreEqual(
                "test_upn",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "",
                        "test_subject",
                        "test_upn",
                        "")));
            Assert.AreEqual(
                "test_upn",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "",
                        "",
                        "test_upn",
                        "test_email")));
            Assert.AreEqual(
                "test_upn",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "",
                        "",
                        "test_upn",
                        "")));

            Assert.AreEqual(
                "test_email",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "test_utid",
                        "test_subject",
                        "",
                        "test_email")));
            Assert.AreEqual(
                "test_email",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "test_uid",
                        "",
                        "test_subject",
                        "",
                        "test_email")));
            Assert.AreEqual(
                "test_email",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "",
                        "test_subject",
                        "",
                        "test_email")));
            Assert.AreEqual(
                "test_email",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "",
                        "",
                        "",
                        "test_email")));

            Assert.AreEqual(
                "test_subject",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "test_uid",
                        "",
                        "test_subject",
                        "",
                        "")));
            Assert.AreEqual(
                "test_subject",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "test_utid",
                        "test_subject",
                        "",
                        "")));
            Assert.AreEqual(
                "test_subject",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "",
                        "test_subject",
                        "",
                        "")));

            Assert.AreEqual(
                "",
                CacheManager.GetHomeAccountId(
                    GetTokenResponse(
                        "",
                        "",
                        "",
                        "",
                        "")));
        }

        [TestMethod]
        public void IsAccessTokenValid()
        {
            long now = TimeUtils.GetSecondsFromEpochNow();
            Assert.IsTrue(CacheManager.IsAccessTokenValid(GetAccessToken(now, now + 3600)));
            Assert.IsTrue(CacheManager.IsAccessTokenValid(GetAccessToken(now, now + 310)));
            Assert.IsTrue(CacheManager.IsAccessTokenValid(GetAccessToken(now - 600, now + 3000)));
            Assert.IsTrue(CacheManager.IsAccessTokenValid(GetAccessToken(now - 600, now + 1000)));
            Assert.IsTrue(CacheManager.IsAccessTokenValid(GetAccessToken(now - 600, now + 100000)));

            Assert.IsFalse(CacheManager.IsAccessTokenValid(GetAccessToken(now + 5, now + 3600)));
            Assert.IsFalse(CacheManager.IsAccessTokenValid(GetAccessToken(now - 10, now - 10)));
            Assert.IsFalse(CacheManager.IsAccessTokenValid(GetAccessToken(now - 3600, now)));
            Assert.IsFalse(CacheManager.IsAccessTokenValid(GetAccessToken(now, now + 10))); // Time window is too short
            Assert.IsFalse(CacheManager.IsAccessTokenValid(GetAccessToken(now, now + 290)));
        }

        [TestMethod]
        public void GetAuthorityType()
        {
            // TODO: need to reconcile our authority uri validation with msal c++
            //Assert.AreEqual(AuthorityType.MsSts, TestGetAuthorityType("https://login.microsoftonline.com"));
            //Assert.AreEqual(AuthorityType.MsSts, TestGetAuthorityType("https://login.microsoftonline.com/"));
            //Assert.AreEqual(AuthorityType.MsSts, TestGetAuthorityType("https://login.microsoftonline.com/stuff"));
            //Assert.AreEqual(AuthorityType.MsSts, TestGetAuthorityType("https://login.microsoftonline.com/stuff/adfs"));
            //Assert.AreEqual(AuthorityType.MsSts, TestGetAuthorityType("https://login.microsoftonline.com/stuff/adfs#row=4"));
            //Assert.AreEqual(AuthorityType.MsSts, TestGetAuthorityType("https://adfs.com"));

            //Assert.AreEqual(AuthorityType.Adfs, TestGetAuthorityType("https://login.microsoftonline.com/adfs"));
            //Assert.AreEqual(AuthorityType.Adfs, TestGetAuthorityType("https://login.microsoftonline.com/adfs/"));
            //Assert.AreEqual(AuthorityType.Adfs, TestGetAuthorityType("https://login.microsoftonline.com/adfs/stuff"));
            //Assert.AreEqual(AuthorityType.Adfs, TestGetAuthorityType("https://login.microsoftonline.com/adfs?life=42"));
            //Assert.AreEqual(AuthorityType.Adfs, TestGetAuthorityType("https://login.microsoftonline.com/adfs?life=42#row=4"));
            //Assert.AreEqual(AuthorityType.Adfs, TestGetAuthorityType("https://login.microsoftonline.com/ADFS"));
            //Assert.AreEqual(AuthorityType.Adfs, TestGetAuthorityType("https://login.microsoftonline.com/AdFs"));

            //Assert.AreEqual(AuthorityType.MsSts, TestGetAuthorityType("http://login.microsoftonline.com"));
            //Assert.AreEqual(AuthorityType.Adfs, TestGetAuthorityType("http://login.microsoftonline.com/adfs"));
            //Assert.AreEqual(AuthorityType.MsSts, TestGetAuthorityType("ftp://login.microsoftonline.com"));
            //Assert.AreEqual(AuthorityType.Adfs, TestGetAuthorityType("ftp://login.microsoftonline.com/adfs"));
        }
    }
}