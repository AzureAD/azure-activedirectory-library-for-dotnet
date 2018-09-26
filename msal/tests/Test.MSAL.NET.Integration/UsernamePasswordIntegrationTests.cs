//----------------------------------------------------------------------
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

using LabInfrastructure;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Test.MSAL.NET.Integration.Infrastructure;

namespace Test.MSAL.NET.Integration
{
    [TestClass]
    public class UsernamePasswordIntegrationTests
    {
        public const string ClientId = "0615b6ca-88d4-4884-8729-b178178f7c27";
        public const string Authority = "https://login.microsoftonline.com/organizations/";
        public string[] Scopes = { "User.Read" };
        AuthHelper authHelper = new AuthHelper();

        [TestMethod]
        [TestCategory("UsernamePasswordIntegrationTests")]
        public async Task AcquireTokenWithManagedUsernamePasswordAsync()
        {
            var user = authHelper.GetUser(
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });

            SecureString securePassword = new NetworkCredential("", ((LabUser)user).GetPassword()).SecurePassword;

            PublicClientApplication msalPublicClient = new PublicClientApplication(ClientId, Authority);
            
            AuthenticationResult authResult = await msalPublicClient.AcquireTokenByUsernamePasswordAsync(Scopes, user.Upn, securePassword);
            Assert.IsNotNull(authResult);
            Assert.IsNotNull(authResult.AccessToken);
            Assert.IsNotNull(authResult.IdToken);
            Assert.AreEqual(user.Upn, authResult.Account.Username);
        }

        [TestMethod]
        [TestCategory("UsernamePasswordIntegrationTests")]
        public async Task AcquireTokenWithFederatedUsernamePasswordAsync()
        {
            var user = authHelper.GetUser(
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });

            SecureString securePassword = new NetworkCredential("", ((LabUser)user).GetPassword()).SecurePassword;

            PublicClientApplication msalPublicClient = new PublicClientApplication(ClientId, Authority);
            AuthenticationResult authResult = await msalPublicClient.AcquireTokenByUsernamePasswordAsync(Scopes, user.Upn, securePassword);
            Assert.IsNotNull(authResult);
            Assert.IsNotNull(authResult.AccessToken);
            Assert.IsNotNull(authResult.IdToken);
            Assert.AreEqual(user.Upn, authResult.Account.Username);
        }

        [TestMethod]
        [TestCategory("UsernamePasswordIntegrationTests")]
        public void AcquireTokenWithManagedUsernameIncorrectPassword()
        {
            var user = authHelper.GetUser(
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });

            SecureString incorrectSecurePassword = new SecureString();
            incorrectSecurePassword.AppendChar('x');
            incorrectSecurePassword.MakeReadOnly();

            PublicClientApplication msalPublicClient = new PublicClientApplication(ClientId, Authority);
            
            var result = Assert.ThrowsExceptionAsync<MsalException>(async () =>
                 await msalPublicClient.AcquireTokenByUsernamePasswordAsync(Scopes, user.Upn, incorrectSecurePassword));
        }

        [TestMethod]
        [TestCategory("UsernamePasswordIntegrationTests")]
        public void AcquireTokenWithFederatedUsernameIncorrectPassword()
        {
            var user = authHelper.GetUser(
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });

            SecureString incorrectSecurePassword = new SecureString();
            incorrectSecurePassword.AppendChar('x');
            incorrectSecurePassword.MakeReadOnly();

            PublicClientApplication msalPublicClient = new PublicClientApplication(ClientId, Authority);
            
            var result = Assert.ThrowsExceptionAsync<MsalException>(async () =>
                 await msalPublicClient.AcquireTokenByUsernamePasswordAsync(Scopes, user.Upn, incorrectSecurePassword));
        }
    }
}