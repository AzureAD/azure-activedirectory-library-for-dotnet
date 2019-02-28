//------------------------------------------------------------------------------
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

using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Test.ADAL.NET.Common.Mocks
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public MockHttpMessageHandler()
        {
        }

        public MockHttpMessageHandler(Action<HttpRequestMessage> AdditionalRequestValidation)
        {
            this.AdditionalRequestValidation = AdditionalRequestValidation;
        }
        public HttpResponseMessage ResponseMessage { get; set; }

        public string Url { get; set; }

        public IDictionary<string, string> QueryParamsToValidate { get; set; }

        public IDictionary<string, string> ActualQueryOrFormsParams { get; private set; }

        public IDictionary<string, string> PostData { get; set; }

        public HttpMethod Method { get; set; }

        public Exception ExceptionToThrow { get; set; }

        public Action<HttpRequestMessage> AdditionalRequestValidation { get; set; }

        public MockHttpMessageHandler(string Url)
        {
            this.Url = Url;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.AreEqual(Method, request.Method);

            Uri uri = request.RequestUri;
            if (!string.IsNullOrEmpty(Url))
            {
                Assert.AreEqual(Url, uri.AbsoluteUri.Split(new[] { '?' })[0]);
            }

            ActualQueryOrFormsParams = ComputeQueryOrFormParams(request);

            //match QP passed in for validation. 
            if (QueryParamsToValidate != null)
            {
                Assert.IsFalse(string.IsNullOrEmpty(uri.Query));

                foreach (var key in QueryParamsToValidate.Keys)
                {
                    Assert.IsTrue(ActualQueryOrFormsParams.ContainsKey(key));
                    Assert.AreEqual(QueryParamsToValidate[key], ActualQueryOrFormsParams[key]);
                }
            }

            if (PostData != null)
            {
                string text = request.Content.ReadAsStringAsync().Result;
                Dictionary<string, string> postDataInput = EncodingHelper.ParseKeyValueList(text, '&', true, null);

                foreach (string key in PostData.Keys)
                {
                    Assert.IsTrue(postDataInput.ContainsKey(key));
                    Assert.AreEqual(PostData[key], postDataInput[key]);
                }
            }

            AdditionalRequestValidation?.Invoke(request);

            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return new TaskFactory().StartNew(() => ResponseMessage, cancellationToken);
        }

        private static IDictionary<string, string> ComputeQueryOrFormParams(HttpRequestMessage request)
        {
            if (request.Method == HttpMethod.Get && !string.IsNullOrWhiteSpace(request.RequestUri.Query))
            {
                return EncodingHelper.ParseKeyValueList(request.RequestUri.Query.Substring(1), '&', true, null);
            }

            if (request.Method == HttpMethod.Post)
            {
                string content = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return EncodingHelper.ParseKeyValueList(content, '&', true, null);

            }

            return new Dictionary<string, string>();

        }
    }
}
