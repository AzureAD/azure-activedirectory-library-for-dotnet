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

using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.ADAL.NET.Unit.net45
{

   
    [TestClass]
    public class AdalExceptionSerializationTests
    {
        private const string SomeErrorCode = "some_error_code";
        private const string SomeErrorMessage = "Some error message.";

        [TestInitialize]
        public void Initialize()
        {
            ModuleInitializer.ForceModuleInitializationTestOnly();
            InstanceDiscovery.InstanceCache.Clear();
        }

        private AdalException SerializeAndDeserialize(AdalException ex)
        {
            string json = ex.ToJsonString();
            return AdalException.FromJsonString(json);
        }

        private void ValidateCommon(AdalException exOriginal, AdalException exDeserialized, Type expectedType)
        {
            Assert.AreEqual(expectedType, exDeserialized.GetType());
            Assert.AreEqual(exOriginal.ErrorCode, exDeserialized.ErrorCode);
            Assert.AreEqual(exOriginal.Message, exDeserialized.Message);
        }

        [TestMethod]
        public void AdalExceptionCanSerializeAndDeserializeRoundTrip()
        {
            var ex = new AdalException(SomeErrorCode, SomeErrorMessage);
            var exDeserialized = SerializeAndDeserialize(ex);
            ValidateCommon(ex, exDeserialized, typeof(AdalException));
        }

        [TestMethod]
        public void AdalServiceExceptionCanSerializeAndDeserializeRoundTrip()
        {
            const int SomeStatusCode = 12345;

            var ex = new AdalServiceException(SomeErrorCode, SomeErrorMessage)
            {
                StatusCode = SomeStatusCode
            };

            var exDeserialized = SerializeAndDeserialize(ex);
            ValidateCommon(ex, exDeserialized, typeof(AdalServiceException));

            var serviceEx = (AdalServiceException)exDeserialized;

            Assert.AreEqual(SomeStatusCode, serviceEx.StatusCode);
        }

        [TestMethod]
        public void AdalSilentTokenAcquisitionExceptionCanSerializeAndDeserializeRoundTrip()
        {
            var ex = new AdalSilentTokenAcquisitionException();
            var exDeserialized = SerializeAndDeserialize(ex);
            ValidateCommon(ex, exDeserialized, typeof(AdalSilentTokenAcquisitionException));
        }

        [TestMethod]
        public void AdalUserMismatchExceptionCanSerializeAndDeserializeRoundTrip()
        {
            const string RequestedUser = "the@requested.user";
            const string ReturnedUser = "the@returned.user";
            var ex = new AdalUserMismatchException(RequestedUser, ReturnedUser);
            var exDeserialized = SerializeAndDeserialize(ex);

            ValidateCommon(ex, exDeserialized, typeof(AdalUserMismatchException));

            var mismatchEx = (AdalUserMismatchException)exDeserialized;
            Assert.AreEqual(RequestedUser, mismatchEx.RequestedUser);
            Assert.AreEqual(ReturnedUser, mismatchEx.ReturnedUser);
        }
    }
}
