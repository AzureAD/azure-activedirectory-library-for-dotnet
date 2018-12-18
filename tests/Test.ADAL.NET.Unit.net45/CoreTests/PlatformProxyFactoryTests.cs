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
using Microsoft.Identity.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.NET.Common;

namespace Microsoft.Identity.Test.Unit.CoreTests
{
    [TestClass]
    public class PlatformProxyFactoryTests
    {
        [TestMethod]
        public void PlatformProxyFactoryCachesTheProxy()
        {
            // Act 
            var proxy1 = PlatformProxyFactory.GetPlatformProxy();
            var proxy2 = PlatformProxyFactory.GetPlatformProxy();

            // Assert
            Assert.IsTrue(proxy1 == proxy2);
        }

        [TestMethod]
        public void PlatformProxyFactoryReturnsInstances()
        {
            // Arrange
            var proxy = PlatformProxyFactory.GetPlatformProxy();

            // Act and Assert
            Assert.AreNotSame(
                proxy.CreateLegacyCachePersistence(),
                proxy.CreateLegacyCachePersistence());

            Assert.AreNotSame(
                proxy.CreateTokenCacheAccessor(),
                proxy.CreateTokenCacheAccessor());

            Assert.AreSame(
                proxy.CryptographyManager,
                proxy.CryptographyManager);

        }

        [TestMethod]
        public void GetEnvRetrievesAValue()
        {
            try
            {
                // Arrange
                Environment.SetEnvironmentVariable("proxy_foo", "bar");

                var proxy = PlatformProxyFactory.GetPlatformProxy();

                // Act
                string actualValue = proxy.GetEnvironmentVariable("proxy_foo");
                string actualEmptyValue = proxy.GetEnvironmentVariable("no_such_env_var_exists");


                // Assert
                Assert.AreEqual("bar", actualValue);
                Assert.IsNull(actualEmptyValue);
            }
            finally
            {
                Environment.SetEnvironmentVariable("proxy_foo", "");
            }
        }

        [TestMethod]
        public void GetEnvThrowsArgNullEx()
        {

            AssertException.Throws<ArgumentNullException>(
                () =>
                PlatformProxyFactory.GetPlatformProxy().GetEnvironmentVariable(""));
        }

      
    }
}
