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

using Microsoft.Identity.Core.Exceptions;
using Microsoft.Identity.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test.Microsoft.Identity.Core.Unit
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void GetCorePiiScrubbedDetails_CoreClient()
        {
            // Arrange
            string exMessage = "exMessage";
            string exCode = "exCode";
            string piiMessage = "";

            try
            {
                // throw it to have a stack trace
                throw new CoreClientException(exCode, exMessage);
            }
            catch (Exception e)
            {
                // Act
                piiMessage = e.GetCorePiiScrubbedDetails();
            }

            // Assert
            Assert.IsFalse(String.IsNullOrEmpty(piiMessage));
            Assert.IsTrue(
                piiMessage.Contains(typeof(CoreClientException).Name),
                "The pii message should contain the exception type");
            Assert.IsTrue(piiMessage.Contains(exCode));
            Assert.IsFalse(piiMessage.Contains(exMessage));
        }

        [TestMethod]
        public void GetCorePiiScrubbedDetails_CoreServices()
        {
            // Arrange
            string exMessage = "exMessage";
            string exCode = "exCode";
            int exStatus = 500;
            string innerMessage = "innerMessage";
            string piiMessage = "";

            var exception = new CoreServiceException(
                exCode,
                exMessage,
                new NotImplementedException(innerMessage))
            {
                StatusCode = exStatus
            };

            // Act
            piiMessage = exception.GetCorePiiScrubbedDetails();

            // Assert
            Assert.IsFalse(String.IsNullOrEmpty(piiMessage));
            Assert.IsTrue(
                piiMessage.Contains(typeof(CoreServiceException).Name),
                "The pii message should contain the exception type");
            Assert.IsTrue(piiMessage.Contains(exCode));
            Assert.IsTrue(piiMessage.Contains(exStatus.ToString()));
            Assert.IsFalse(piiMessage.Contains(exMessage));

            Assert.IsTrue(
                piiMessage.Contains(typeof(NotImplementedException).Name),
                "The pii message should contain the inner exception type");
            Assert.IsFalse(piiMessage.Contains(innerMessage));

        }
    }
}
