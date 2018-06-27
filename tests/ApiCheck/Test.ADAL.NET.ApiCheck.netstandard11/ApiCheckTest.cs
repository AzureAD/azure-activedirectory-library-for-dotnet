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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.ADAL.ApiCheck
{
    [TestClass]
    public class ApiCheckTest
    {
        public TestContext TestContext
        {
            get; set;
        }

        /// <summary>
        /// This test checks backwards compatibility of the public API between: 
        /// the latest 3.x.x version of ADAL as found on public NuGet
        /// the developer version which is just built
        /// 
        /// In case discrepancies are found, have a look at the reports attached in the test output. You can also
        /// manually inspect the 2 assemblies with the excellent ApiReviewer tool from the .net team found at \\fxcore\tools
        /// </summary>
        [TestMethod]
        public void PublicApi_ADALv3_netstandard11_ShouldRemain_BackwardsCompatible()
        {
            ApiCheckTestWrapper apiCheckTestWrapper = new ApiCheckTestWrapper(
                TestContext,
                "netstandard1.1",
                "Microsoft.IdentityModel.Clients.ActiveDirectory",
                "3.0.0",
                "4.0.0");

            apiCheckTestWrapper.RunTest();
        }
    }
}
