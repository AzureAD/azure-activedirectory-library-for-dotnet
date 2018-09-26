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

using Microsoft.Identity.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Test.Microsoft.Identity.Core.UIAutomation.infrastructure;
using Test.MSAL.NET.Integration.Infrastructure;

namespace Test.MSAL.NET.Integration
{
    [TestClass]
    public class UsernamePasswordTests
    {
        public const string ClientId = "0615b6ca-88d4-4884-8729-b178178f7c27";
        public string[] Scopes = { "user.read" };
        AuthHelper authHelper = new AuthHelper();

        [TestMethod]
        public async Task AcquireTokenWithManagedUsernamePasswordAsync()
        {
            //Get User from Lab
            var user = authHelper.GetUser(
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = false
                });

            SecureString securePassword = new NetworkCredential("", ((LabUser)user).GetPassword()).SecurePassword;

            try
            {
                PublicClientApplication msalPublicClient = new PublicClientApplication(ClientId);
                AuthenticationResult authResult = await msalPublicClient.AcquireTokenByUsernamePasswordAsync(Scopes, user.ToString(), securePassword);
            }
            catch(Exception ex)
            {

            }
        }

        [TestMethod]
        public async Task AcquireTokenWithFederatedUsernamePasswordAsync()
        {
            //Get User from Lab
            var user = authHelper.GetUser(
                new UserQueryParameters
                {
                    IsMamUser = false,
                    IsMfaUser = false,
                    IsFederatedUser = true
                });

            SecureString securePassword = new NetworkCredential("", ((LabUser)user).GetPassword()).SecurePassword;

            try
            {
                PublicClientApplication msalPublicClient = new PublicClientApplication(ClientId);
                AuthenticationResult authResult = await msalPublicClient.AcquireTokenByUsernamePasswordAsync(Scopes, user.ToString(), securePassword);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
