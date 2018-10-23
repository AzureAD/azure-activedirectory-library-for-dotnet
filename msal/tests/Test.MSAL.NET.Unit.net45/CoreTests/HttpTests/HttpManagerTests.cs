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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.Instance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.Core.Unit;
using Test.Microsoft.Identity.Core.Unit.Mocks;

namespace Test.Microsoft.Identity.Unit.HttpTests
{
    [TestClass]
    public class HttpManagerTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Authority.ValidatedAuthorities.Clear();
            CoreExceptionFactory.Instance = new TestExceptionFactory();
        }

        [TestMethod]
        [TestCategory("HttpManagerTests")]
        public void TestSendPostNullHeaderNullBody()
        {
            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddSuccessTokenResponseMockHandlerForPost();

                var response = httpManager.SendPostAsync(
                    new Uri(CoreTestConstants.AuthorityHomeTenant + "oauth2/token"),
                    null,
                    (IDictionary<string, string>)null,
                    null).Result;

                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(MockHelpers.DefaultTokenResponse, response.Body);
            }
        }

        [TestMethod]
        [TestCategory("HttpManagerTests")]
        public void TestSendPostNoFailure()
        {
            var bodyParameters = new Dictionary<string, string>
            {
                ["key1"] = "some value1",
                ["key2"] = "some value2"
            };
            var queryParams = new Dictionary<string, string>
            {
                ["key1"] = "qp1",
                ["key2"] = "qp2"
            };

            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddSuccessTokenResponseMockHandlerForPost(bodyParameters, queryParams);

                var response = httpManager.SendPostAsync(
                    new Uri(CoreTestConstants.AuthorityHomeTenant + "oauth2/token?key1=qp1&key2=qp2"),
                    queryParams,
                    bodyParameters,
                    null).Result;

                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(MockHelpers.DefaultTokenResponse, response.Body);
            }
        }

        [TestMethod]
        [TestCategory("HttpManagerTests")]
        public void TestSendGetNoFailure()
        {
            var queryParams = new Dictionary<string, string>
            {
                ["key1"] = "qp1",
                ["key2"] = "qp2"
            };

            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddSuccessTokenResponseMockHandlerForGet(queryParameters: queryParams);

                var response = httpManager.SendGetAsync(
                    new Uri(CoreTestConstants.AuthorityHomeTenant + "oauth2/token?key1=qp1&key2=qp2"),
                    queryParams,
                    null).Result;

                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(MockHelpers.DefaultTokenResponse, response.Body);
            }
        }

        [TestMethod]
        [TestCategory("HttpManagerTests")]
        public async Task TestSendGetWithHttp500TypeFailureAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddResiliencyMessageMockHandler(HttpMethod.Get, HttpStatusCode.GatewayTimeout);
                httpManager.AddResiliencyMessageMockHandler(HttpMethod.Get, HttpStatusCode.InternalServerError);

                try
                {
                    var msalHttpResponse = await httpManager.SendGetAsync(
                                                                new Uri(CoreTestConstants.AuthorityHomeTenant + "oauth2/token"),
                                                                new Dictionary<string, string>(),
                                                                new RequestContext(new TestLogger(Guid.NewGuid(), null)))
                                                            .ConfigureAwait(false);
                    Assert.Fail("request should have failed");
                }
                catch (TestServiceException exc)
                {
                    Assert.IsNotNull(exc);
                    Assert.AreEqual(CoreErrorCodes.ServiceNotAvailable, exc.ErrorCode);
                }
            }
        }

        [TestMethod]
        [TestCategory("HttpManagerTests")]
        public async Task TestSendGetWithHttp500TypeFailure2Async()
        {
            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddResiliencyMessageMockHandler(HttpMethod.Post, HttpStatusCode.BadGateway);
                httpManager.AddResiliencyMessageMockHandler(HttpMethod.Post, HttpStatusCode.BadGateway);

                var msalHttpResponse = await httpManager.SendPostForceResponseAsync(
                                                            new Uri(CoreTestConstants.AuthorityHomeTenant + "oauth2/token"),
                                                            new Dictionary<string, string>(),
                                                            new StringContent("body"),
                                                            new RequestContext(new TestLogger(Guid.NewGuid(), null)))
                                                        .ConfigureAwait(false);

                Assert.AreEqual(HttpStatusCode.BadGateway, msalHttpResponse.StatusCode);
            }
        }

        [TestMethod]
        [TestCategory("HttpManagerTests")]
        public async Task TestSendPostWithHttp500TypeFailureAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddResiliencyMessageMockHandler(HttpMethod.Post, HttpStatusCode.GatewayTimeout);
                httpManager.AddResiliencyMessageMockHandler(HttpMethod.Post, HttpStatusCode.ServiceUnavailable);

                try
                {
                    var msalHttpResponse = await httpManager.SendPostAsync(
                                                                new Uri(CoreTestConstants.AuthorityHomeTenant + "oauth2/token"),
                                                                new Dictionary<string, string>(),
                                                                (IDictionary<string, string>)null,
                                                                new RequestContext(new TestLogger(Guid.NewGuid(), null)))
                                                            .ConfigureAwait(false);
                    Assert.Fail("request should have failed");
                }
                catch (TestServiceException exc)
                {
                    Assert.IsNotNull(exc);
                    Assert.AreEqual(CoreErrorCodes.ServiceNotAvailable, exc.ErrorCode);
                }
            }
        }

        [TestMethod]
        [TestCategory("HttpManagerTests")]
        public async Task TestSendGetWithRetryOnTimeoutFailureAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddRequestTimeoutResponseMessageMockHandler(HttpMethod.Get);
                httpManager.AddRequestTimeoutResponseMessageMockHandler(HttpMethod.Get);

                try
                {
                    var msalHttpResponse = await httpManager.SendGetAsync(
                                                                new Uri(CoreTestConstants.AuthorityHomeTenant + "oauth2/token"),
                                                                new Dictionary<string, string>(),
                                                                new RequestContext(new TestLogger(Guid.NewGuid(), null)))
                                                            .ConfigureAwait(false);
                    Assert.Fail("request should have failed");
                }
                catch (TestServiceException exc)
                {
                    Assert.IsNotNull(exc);
                    Assert.AreEqual(CoreErrorCodes.RequestTimeout, exc.ErrorCode);
                    Assert.IsTrue(exc.InnerException is TaskCanceledException);
                }
            }
        }

        [TestMethod]
        [TestCategory("HttpManagerTests")]
        public async Task TestSendPostWithRetryOnTimeoutFailureAsync()
        {
            using (var httpManager = new MockHttpManager())
            {
                httpManager.AddRequestTimeoutResponseMessageMockHandler(HttpMethod.Post);
                httpManager.AddRequestTimeoutResponseMessageMockHandler(HttpMethod.Post);

                try
                {
                    var msalHttpResponse = await httpManager.SendPostAsync(
                                                                new Uri(CoreTestConstants.AuthorityHomeTenant + "oauth2/token"),
                                                                new Dictionary<string, string>(),
                                                                new Dictionary<string, string>(),
                                                                new RequestContext(new TestLogger(Guid.NewGuid(), null)))
                                                            .ConfigureAwait(false);
                    Assert.Fail("request should have failed");
                }
                catch (TestServiceException exc)
                {
                    Assert.IsNotNull(exc);
                    Assert.AreEqual(CoreErrorCodes.RequestTimeout, exc.ErrorCode);
                    Assert.IsTrue(exc.InnerException is TaskCanceledException);
                }
            }
        }
    }
}