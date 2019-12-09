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

using Microsoft.Identity.Test.LabInfrastructure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Test.ADAL.NET.Common;

namespace Test.ADAL.Integration.net45.HeadlessTetss
{
#if DESKTOP
    [TestClass]
    public class UsernamePasswordTests
    {
        #region MSTest Hooks
        /// <summary>
        /// Initialized by MSTest (do not make private or readonly)
        /// </summary>
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        #endregion

        [TestMethod]
        public async Task ROPC_DefaultUserAsync()
        {
            // Arrange
            LabResponse labResponse = await LabUserHelper.GetDefaultUserAsync().ConfigureAwait(false);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV3_NotFederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV4,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = false
            };

            LabResponse labResponse = await LabUserHelper.GetLabUserDataAsync(query).ConfigureAwait(false);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV3_FederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV4,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = true
            };

            LabResponse labResponse = await LabUserHelper.GetLabUserDataAsync(query).ConfigureAwait(false);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV2_FederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV2,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = true
            };

            LabResponse labResponse = await LabUserHelper.GetLabUserDataAsync(query).ConfigureAwait(false);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV4_NotFederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV4,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = false
            };

            LabResponse labResponse = await LabUserHelper.GetLabUserDataAsync(query).ConfigureAwait(false);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ROPC_AdfsV4_FederatedAsync()
        {
            // Arrange
            var query = new UserQuery
            {
                FederationProvider = FederationProvider.AdfsV4,
                IsMamUser = false,
                IsMfaUser = false,
                IsFederatedUser = true
            };

            LabResponse labResponse = await LabUserHelper.GetLabUserDataAsync(query).ConfigureAwait(false);
            await RunTestForUserAsync(labResponse).ConfigureAwait(false);
        }


        private static async Task RunTestForUserAsync(LabResponse labResponse)
        {
            var context = new AuthenticationContext(AdalTestConstants.DefaultAuthorityCommonTenant);
            var authResult = await context.AcquireTokenAsync(
                AdalTestConstants.MSGraph,
                labResponse.User.AppId,
                new UserPasswordCredential(
                    labResponse.User.Upn, 
                    labResponse.User.GetOrFetchPassword()))
                .ConfigureAwait(false);

            Assert.IsNotNull(authResult.AccessToken);
        }
    }
#endif
}
