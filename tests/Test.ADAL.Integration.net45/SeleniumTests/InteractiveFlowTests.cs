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
using OpenQA.Selenium;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Test.ADAL.Integration.Infrastructure;
using Test.ADAL.NET.Common;

namespace Test.ADAL.Integration.SeleniumTests
{
    [TestClass]
    public class InteractiveFlowTests
    {
        private readonly TimeSpan _seleniumTimeout = TimeSpan.FromMinutes(2);

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

#if DESKTOP
        [TestMethod]
        public async Task InteractiveAuth_DefaultUserAsync()
        {
            // Arrange
            LabResponse labResponse = await LabUserHelper.GetDefaultUserAsync().ConfigureAwait(false);

            Action<IWebDriver> seleniumLogic = (driver) =>
            {
                Trace.WriteLine("Starting Selenium automation");
                driver.PerformLogin(labResponse.User);
            };

            var context = new AuthenticationContext(AdalTestConstants.DefaultAuthorityCommonTenant);

            // Act - acquire with empty cache => interactive auth
            AuthenticationResult result = await context.AcquireTokenAsync(
                AdalTestConstants.MSGraph,
                labResponse.App.AppId,
                new Uri(SeleniumWebUI.FindFreeLocalhostRedirectUri()),
                new PlatformParameters(
                    PromptBehavior.Auto,
                    new SeleniumWebUI(seleniumLogic, _seleniumTimeout)))
                    .ConfigureAwait(false);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.AccessToken));

            // Act - acquire with token in cache => silent
            result = await context.AcquireTokenAsync(
               AdalTestConstants.MSGraph,
               labResponse.App.AppId,
               new Uri(SeleniumWebUI.FindFreeLocalhostRedirectUri()),
               new PlatformParameters(PromptBehavior.Auto))
               .ConfigureAwait(false);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.AccessToken));
        }
#endif

        [TestMethod]
        public async Task Interactive_AdfsV3_NotFederatedAsync()
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
            await RunTestForUserPromptAutoAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task Interactive_AdfsV3_FederatedAsync()
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
            await RunTestForUserPromptAutoAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task Interactive_AdfsV2_FederatedAsync()
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
            await RunTestForUserPromptAutoAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task Interactive_AdfsV4_NotFederatedAsync()
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
            await RunTestForUserPromptAutoAsync(labResponse).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task Interactive_AdfsV4_FederatedAsync()
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
            await RunTestForUserPromptAutoAsync(labResponse).ConfigureAwait(false);
        }

        private async Task RunTestForUserPromptAutoAsync(LabResponse labResponse)
        {
            Action<IWebDriver> seleniumLogic = (driver) =>
            {
                Trace.WriteLine("Starting Selenium automation");
                driver.PerformLogin(labResponse.User);
            };

            var context = new AuthenticationContext(AdalTestConstants.DefaultAuthorityCommonTenant);

            // Act
            AuthenticationResult result = await context.AcquireTokenAsync(
                AdalTestConstants.MSGraph, 
                labResponse.App.AppId, 
                new Uri(SeleniumWebUI.FindFreeLocalhostRedirectUri()),
                new PlatformParameters(
                    PromptBehavior.Always, 
                    new SeleniumWebUI(seleniumLogic, _seleniumTimeout)));

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.AccessToken));
        }

       
    }
}
