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
using System;
using Microsoft.Identity.Client;
using Microsoft.Identity.Core.Exceptions;

namespace Test.Microsoft.Identity.Core.Unit
{
    [TestClass]
    public class MsalExceptionAdapterTests
    {
        private const string exCode = "exCode";
        private const string exMessage = "exMessage";

        [TestMethod]
        public void MsalClientException_FromCoreException()
        {
            // Arrange
            CoreClientException coreClientException =
                new CoreClientException(exCode, exMessage);

            // Act
            MsalException msalException =
                MsalExceptionAdapter.FromCoreException(coreClientException);

            // Assert
            var msalClientException = msalException as MsalClientException;
            Assert.AreEqual(coreClientException, msalClientException.CoreException);
            Assert.AreEqual(exCode, msalClientException.ErrorCode);
            Assert.AreEqual(exMessage, msalClientException.Message);
            Assert.IsNull(msalClientException.InnerException);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DoNotConvert_CoreExeception()
        {
            // Arrange
            CoreException coreEx =
                new CoreException(exCode, exMessage);

            // Act
            MsalExceptionAdapter.FromCoreException(coreEx);
        }

        [TestMethod]
        public void MsalServiceException_FromCoreException()
        {
            string claims = "claims";
            string reponseBody = "responseBody";

            // Arrange
            NotImplementedException innerException = new NotImplementedException("inner");
            CoreServiceException coreException =
                new CoreServiceException(exCode, exMessage, innerException)
                {
                    Claims = claims,
                    ResponseBody = reponseBody
                };

            // Act
            MsalException msalException =
                MsalExceptionAdapter.FromCoreException(coreException);

            // Assert
            var msalServiceException = msalException as MsalServiceException;
            Assert.AreEqual(coreException, msalServiceException.CoreException);
            Assert.IsFalse(coreException.IsUiRequired);
            Assert.AreEqual(innerException, msalServiceException.InnerException);
            Assert.AreEqual(exCode, msalServiceException.ErrorCode);
            Assert.AreEqual(claims, msalServiceException.Claims);
            Assert.AreEqual(reponseBody, msalServiceException.ResponseBody);
            Assert.AreEqual(exMessage, msalServiceException.Message);
        }

        [TestMethod]
        public void MsalServiceException_UiRequired_FromCoreException()
        {
            // Arrange
            NotImplementedException innerException = new NotImplementedException("inner");
            CoreServiceException coreException =
                new CoreServiceException(exCode, exMessage, innerException)
                {
                    IsUiRequired = true
                };

            // Act
            MsalException msalException =
                MsalExceptionAdapter.FromCoreException(coreException);

            // Assert
            var msalServiceException = msalException as MsalUiRequiredException;
            Assert.AreEqual(coreException, msalServiceException.CoreException);
            Assert.AreEqual(innerException, msalServiceException.InnerException);
            Assert.AreEqual(exCode, msalServiceException.ErrorCode);
            Assert.IsNull(msalServiceException.Claims);
            Assert.IsNull(msalServiceException.ResponseBody);
            Assert.AreEqual(exMessage, msalServiceException.Message);
        }
    }
}
