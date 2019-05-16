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
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Test.ADAL.NET.Common;

namespace Test.ADAL.Integration.SeleniumTests
{
    [TestClass]
    public class AuthenticationParamtersTest
    {

        [TestMethod]
        public async Task AuthenticationParametersCanBeDiscovered()
        {
            string protectedApi = "https://graph.microsoft.com/v1.0/me/";

            var ap = await AuthenticationParameters.CreateFromResourceUrlAsync(
                new Uri(protectedApi))
                .ConfigureAwait(false);

            // Authority might change, but should be a rare occurence
            Assert.AreEqual(ap.Authority, "https://login.microsoftonline.com/common/oauth2/authorize");

            // Graph does not provide a resource_id in the response header, probably because they want MSAL to access it
            // I couldn't find protected APIs that advertise resources (tried Grasph, AAD Graph, Dynamics...)
            Assert.IsNull(ap.Resource);
        }


    }
}
