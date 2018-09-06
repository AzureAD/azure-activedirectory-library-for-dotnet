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

using Microsoft.Identity.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.Core.Unit.Mocks;

namespace Test.Microsoft.Identity.Core.Unit
{
    [TestClass]
    public class UriParamsHelperTests
    {
        [TestMethod]
        public void CreateUriParamsFromPlatformInformation()
        {
            // Arrange
            CorePlatformInformationBase.Instance = new TestPlatformInformation();

            // Act
            var result = UriParamsHelper.GetUriParameters();

            // Assert
            Assert.IsTrue(result.ContainsKey(UriParamsHelper.CpuPlatform));
            Assert.AreEqual(TestPlatformInformation.TestCPU, result[UriParamsHelper.CpuPlatform]);

            Assert.IsTrue(result.ContainsKey(UriParamsHelper.DeviceModel));
            Assert.AreEqual(TestPlatformInformation.TestDevice, result[UriParamsHelper.DeviceModel]);

            Assert.IsTrue(result.ContainsKey(UriParamsHelper.OS));
            Assert.AreEqual(TestPlatformInformation.TestOS, result[UriParamsHelper.OS]);

            Assert.IsTrue(result.ContainsKey(UriParamsHelper.Product));
            Assert.AreEqual(TestPlatformInformation.TestProductName, result[UriParamsHelper.Product]);

            Assert.IsTrue(result.ContainsKey(UriParamsHelper.Version));
            Assert.AreEqual(TestPlatformInformation.TestAssemblyFileVersion, result[UriParamsHelper.Version]);
        }

        [TestMethod]
        [ExpectedException(typeof(TestClientException))]
        public void NullPlatformInformationTest()
        {
            // Arrange
            CorePlatformInformationBase.Instance = null;
            CoreExceptionFactory.Instance = new TestExceptionFactory();
            // Act
            var result = UriParamsHelper.GetUriParameters();
        }
    }
}
