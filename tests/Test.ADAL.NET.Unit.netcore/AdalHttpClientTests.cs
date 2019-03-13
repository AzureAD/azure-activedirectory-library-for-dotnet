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
using Microsoft.Identity.Test.Common.Core.Mocks;
using Test.ADAL.NET.Common.Mocks;
using Microsoft.Identity.Core.OAuth2;
using Microsoft.Identity.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class OAuthClientTests
    {
        IServiceBundle _serviceBundle = TestCommon.CreateDefaultServiceBundle();

        [TestMethod]
        public void QueryParamsFromEnvVariable()
        {
            try
            {
                // Arrange
                string uriWithVars = "http://contoso.com?existingVar=var&foo=bar";
                string uriWithoutVars = "http://contoso.com";

                RequestContext requestContext = new RequestContext(
                    "id",
                    new TestLogger(Guid.NewGuid(), null));

                const string extraQueryParams = "n1=v1&n2=v2";

                Environment.SetEnvironmentVariable(
                    OAuthClient.ExtraQueryParamEnvVariable, 
                    extraQueryParams);

                // Act
                OAuthClient client = new OAuthClient(
                    _serviceBundle.HttpManager,
                    uriWithVars,
                    requestContext);
                string actualUriWithVars = client.RequestUri;

                OAuthClient client2 = new OAuthClient(
                    _serviceBundle.HttpManager,
                    uriWithoutVars,
                    requestContext);

                string actualUriWithoutVars = client2.RequestUri;

                // Assert
                Assert.AreEqual(uriWithoutVars + "?" + extraQueryParams, actualUriWithoutVars);
                Assert.AreEqual(uriWithVars + "&" + extraQueryParams, actualUriWithVars);
            }
            finally
            {
                Environment.SetEnvironmentVariable(OAuthClient.ExtraQueryParamEnvVariable, null);
            }
        }

        [TestMethod]
        public void QueryParamsNoEnvVariable()
        {
            // Arrange
            Environment.SetEnvironmentVariable(OAuthClient.ExtraQueryParamEnvVariable, "");
            string initialUri = "http://contoso.com?existingVar=var&foo=bar";

            RequestContext requestContext = new RequestContext(
                "id",
                new TestLogger(Guid.NewGuid(), null));

            // Act
            OAuthClient client = new OAuthClient(
                    _serviceBundle.HttpManager,
                    initialUri,
                    requestContext);

            string requestUri = client.RequestUri;

            // Assert
            Assert.AreEqual(initialUri, requestUri);
        }
    }
}
