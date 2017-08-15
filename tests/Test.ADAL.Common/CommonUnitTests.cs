﻿//----------------------------------------------------------------------
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.ADAL.Common
{
    public static class CommonUnitTests
    {
        public static void CreateSha256HashTest()
        {
            string hash = CryptographyHelper.CreateSha256Hash("abc");
            string hash2 = CryptographyHelper.CreateSha256Hash("abd");
            string hash3 = CryptographyHelper.CreateSha256Hash("abc");
            Assert.AreEqual(hash, hash3);
            Assert.AreNotEqual(hash, hash2);
            Assert.AreEqual(hash, "ungWv48Bz+pBQUDeXa4iI7ADYaOWF3qctBD/YfIAFa0=");
        }

        public static void AdalIdTest()
        {
            var adalParameters = AdalIdHelper.GetAdalIdParameters();

            Assert.AreEqual(4, adalParameters.Count);
            Assert.IsNotNull(adalParameters[AdalIdParameter.Product]);
            Assert.IsNotNull(adalParameters[AdalIdParameter.Version]);
            Assert.IsNotNull(adalParameters[AdalIdParameter.CpuPlatform]);
            Assert.IsNotNull(adalParameters[AdalIdParameter.OS]);
            Assert.IsFalse(adalParameters.ContainsKey(AdalIdParameter.DeviceModel));
            adalParameters = AdalIdHelper.GetAdalIdParameters();

            Assert.AreEqual(4, adalParameters.Count);
            Assert.IsNotNull(adalParameters[AdalIdParameter.Product]);
            Assert.IsNotNull(adalParameters[AdalIdParameter.Version]);
            Assert.IsNotNull(adalParameters[AdalIdParameter.CpuPlatform]);
            Assert.IsNotNull(adalParameters[AdalIdParameter.OS]);
            Assert.IsFalse(adalParameters.ContainsKey(AdalIdParameter.DeviceModel));
        }
    }
}
