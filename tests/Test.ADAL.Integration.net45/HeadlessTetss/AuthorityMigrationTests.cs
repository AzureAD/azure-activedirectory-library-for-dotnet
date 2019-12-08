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
using System.Diagnostics;
using System.Threading.Tasks;
using Test.ADAL.NET.Common;

namespace Test.ADAL.Integration.SeleniumTests
{
    [TestClass]
    public class AuthorityMigrationTests
    {

#if DESKTOP
        [TestMethod]
        public async Task AuthorityMigrationAsync()
        {
            var labResponse = await LabUserHelper.GetDefaultUserAsync().ConfigureAwait(false);

            Trace.WriteLine("Acquire a token using a not so common authority alias");

            var context = new AuthenticationContext("https://sts.windows.net/" + labResponse.User.TenantId);
            var authResult = await context.AcquireTokenAsync(
                AdalTestConstants.MSGraph,
                labResponse.User.AppId,
                new UserPasswordCredential(labResponse.User.Upn, labResponse.User.GetOrFetchPassword()))
                .ConfigureAwait(false);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(authResult?.AccessToken));

            Trace.WriteLine("Acquire a token silently using the common authority alias");

            context = new AuthenticationContext(AdalTestConstants.DefaultAuthorityCommonTenant);
            authResult = await context.AcquireTokenSilentAsync(
                AdalTestConstants.MSGraph,
                labResponse.User.AppId)
                .ConfigureAwait(false);
            Assert.IsNotNull(authResult.AccessToken);
        }
#endif
    }
}
