﻿// ------------------------------------------------------------------------------
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
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Instance;
using Microsoft.Identity.Core.Telemetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.Core.Unit.Mocks;

namespace Test.Microsoft.Identity.Core.Unit.InstanceTests
{
    [TestClass]
    [DeploymentItem("Resources\\OpenidConfiguration-B2C.json")]
    public class B2CAuthorityTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Authority.ValidatedAuthorities.Clear();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        [TestCategory("B2CAuthorityTests")]
        public void NotEnoughPathSegmentsTest()
        {
            try
            {
                var instance = Authority.CreateAuthority("https://login.microsoftonline.in/tfp/", false);
                Assert.IsNotNull(instance);
                Assert.AreEqual(instance.AuthorityType, AuthorityType.B2C);
                Task.Run(
                    async () =>
                    {
                        await instance.ResolveEndpointsAsync(
                            null,
                            null,
                            null,
                            new RequestContext(null, new TestLogger(Guid.NewGuid(), null))).ConfigureAwait(false);
                    }).GetAwaiter().GetResult();
                Assert.Fail("test should have failed");
            }
            catch (Exception exc)
            {
                Assert.IsInstanceOfType(exc, typeof(ArgumentException));
                Assert.AreEqual(CoreErrorMessages.B2cAuthorityUriInvalidPath, exc.Message);
            }
        }

        [TestMethod]
        [TestCategory("B2CAuthorityTests")]
        public void ValidationEnabledNotSupportedTest()
        {
            using (var httpManager = new MockHttpManager())
            {
                //add mock response for instance validation
                httpManager.AddMockHandler(
                    new MockHttpMessageHandler
                    {
                        Method = HttpMethod.Get,
                        Url = "https://login.microsoftonline.com/tfp/tenant/policy/v2.0/.well-known/openid-configuration",
                        ResponseMessage =
                            MockHelpers.CreateSuccessResponseMessage(
                                File.ReadAllText(ResourceHelper.GetTestResourceRelativePath("OpenidConfiguration-B2C.json")))
                    });

                var instance = Authority.CreateAuthority(CoreTestConstants.B2CAuthority, true);
                Assert.IsNotNull(instance);
                Assert.AreEqual(instance.AuthorityType, AuthorityType.B2C);
                try
                {
                    Task.Run(
                        async () =>
                        {
                            await instance.ResolveEndpointsAsync(
                                httpManager,
                                new TelemetryManager(),
                                null,
                                new RequestContext(null, new TestLogger(Guid.NewGuid(), null))).ConfigureAwait(false);
                        }).GetAwaiter().GetResult();
                }
                catch (Exception exc)
                {
                    Assert.AreEqual(CoreErrorMessages.UnsupportedAuthorityValidation, exc.Message);
                }
            }
        }
        [TestMethod]
        [TestCategory("B2CAuthorityTests")]
        public void CanonicalAuthorityInitTest()
        {
            const string uriNoPort = CoreTestConstants.B2CAuthority;
            const string uriNoPortTailSlash = CoreTestConstants.B2CAuthority;

            const string uriDefaultPort = "https://login.microsoftonline.in:443/tfp/tenant/policy";

            const string uriCustomPort = "https://login.microsoftonline.in:444/tfp/tenant/policy";
            const string uriCustomPortTailSlash = "https://login.microsoftonline.in:444/tfp/tenant/policy/";
            const string uriVanityPort = CoreTestConstants.B2CLoginAuthority;

            var authority = new B2CAuthority(uriNoPort, false);
            Assert.AreEqual(uriNoPortTailSlash, authority.CanonicalAuthority);

            authority = new B2CAuthority(uriDefaultPort, false);
            Assert.AreEqual(uriNoPortTailSlash, authority.CanonicalAuthority);

            authority = new B2CAuthority(uriCustomPort, false);
            Assert.AreEqual(uriCustomPortTailSlash, authority.CanonicalAuthority);

            authority = new B2CAuthority(uriVanityPort, false);
            Assert.AreEqual(uriVanityPort, authority.CanonicalAuthority);
        }
    }
}