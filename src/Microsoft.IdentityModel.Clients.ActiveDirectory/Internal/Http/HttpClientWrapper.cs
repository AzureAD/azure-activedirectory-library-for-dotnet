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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Http
{
    internal class HttpClientWrapper : IHttpClient
    {
        private readonly string uri;
        private int _timeoutInMilliSeconds = 30000;
        private readonly long _maxResponseSizeInBytes = 1048576;

        public HttpClientWrapper(string uri, RequestContext requestContext)
        {
            this.uri = uri;
            this.Headers = new Dictionary<string, string>();
            this.RequestContext = requestContext;
        }

        protected RequestContext RequestContext { get; set; }

        public IRequestParameters BodyParameters { get; set; }

        public string Accept { get; set; }

        public string ContentType { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public Dictionary<string, string> Headers { get; private set; }

        public int TimeoutInMilliSeconds
        {
            set { this._timeoutInMilliSeconds = value; }

            get { return this._timeoutInMilliSeconds; }
        }

        public async Task<IHttpWebResponse> GetResponseAsync()
        {
            using (HttpClient client =
                new HttpClient(HttpMessageHandlerFactory.GetMessageHandler(this.UseDefaultCredentials)))
            {
                client.MaxResponseContentBufferSize = _maxResponseSizeInBytes;
                client.DefaultRequestHeaders.Accept.Clear();
                HttpRequestMessage requestMessage = new HttpRequestMessage();
                requestMessage.RequestUri = new Uri(uri);
                requestMessage.Headers.Accept.Clear();

                requestMessage.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(this.Accept ?? "application/json"));
                foreach (KeyValuePair<string, string> kvp in this.Headers)
                {
                    requestMessage.Headers.Add(kvp.Key, kvp.Value);
                }

                bool addCorrelationId = (this.RequestContext != null && this.RequestContext.CorrelationId != Guid.Empty);
                if (addCorrelationId)
                {
                    requestMessage.Headers.Add(OAuthHeader.CorrelationId, this.RequestContext.CorrelationId.ToString());
                    requestMessage.Headers.Add(OAuthHeader.RequestCorrelationIdInResponse, "true");
                }

                client.Timeout = TimeSpan.FromMilliseconds(this._timeoutInMilliSeconds);

                HttpResponseMessage responseMessage;

                try
                {
                    if (this.BodyParameters != null)
                    {
                        HttpContent content;
                        if (this.BodyParameters is StringRequestParameters)
                        {
                            content = new StringContent(this.BodyParameters.ToString(), Encoding.UTF8,
                                this.ContentType);
                        }
                        else
                        {
                            content = new FormUrlEncodedContent(((DictionaryRequestParameters) this.BodyParameters)
                                .ToList());
                        }

                        requestMessage.Method = HttpMethod.Post;
                        requestMessage.Content = content;
                    }
                    else
                    {
                        requestMessage.Method = HttpMethod.Get;
                    }

                    responseMessage = await client.SendAsync(requestMessage).ConfigureAwait(false);
                }
                catch (TaskCanceledException ex)
                {
                    throw new HttpRequestWrapperException(null, ex);
                }

                IHttpWebResponse webResponse = await CreateResponseAsync(responseMessage).ConfigureAwait(false);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    throw new HttpRequestWrapperException(webResponse, new HttpRequestException(
                        string.Format(CultureInfo.CurrentCulture,
                            "Response status code does not indicate success: {0} ({1}).",
                            (int) webResponse.StatusCode, webResponse.StatusCode),
                        new AdalException(webResponse.ResponseString)));
                }

                if (addCorrelationId)
                {
                    VerifyCorrelationIdHeaderInReponse(webResponse.Headers);
                }

                return webResponse;
            }
        }

        public async static Task<IHttpWebResponse> CreateResponseAsync(HttpResponseMessage response)
        {
            return new HttpWebResponseWrapper(await response.Content.ReadAsStringAsync().ConfigureAwait(false), response.Headers,
                response.StatusCode);
        }

        private void VerifyCorrelationIdHeaderInReponse(HttpResponseHeaders headers)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                string reponseHeaderKey = header.Key;
                string trimmedKey = reponseHeaderKey.Trim();
                if (string.Compare(trimmedKey, OAuthHeader.CorrelationId, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    string correlationIdHeader = headers.GetValues(trimmedKey).FirstOrDefault().Trim();
                    Guid correlationIdInResponse;
                    if (!Guid.TryParse(correlationIdHeader, out correlationIdInResponse))
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture,
                            "Returned correlation id '{0}' is not in GUID format.", correlationIdHeader);
                        RequestContext.Logger.Warning(msg);
                        RequestContext.Logger.WarningPii(msg);
                    }
                    else if (correlationIdInResponse != this.RequestContext.CorrelationId)
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture,
                            "Returned correlation id '{0}' does not match the sent correlation id '{1}'",
                            correlationIdHeader, RequestContext.CorrelationId);
                        RequestContext.Logger.Warning(msg);
                        RequestContext.Logger.WarningPii(msg);
                    }

                    break;
                }
            }
        }
    }
}