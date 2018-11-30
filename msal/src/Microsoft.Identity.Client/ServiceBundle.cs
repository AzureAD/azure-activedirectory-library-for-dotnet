// ------------------------------------------------------------------------------
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

using System.Net.Http;
using Microsoft.Identity.Client;
using Microsoft.Identity.Core.Http;

namespace Microsoft.Identity.Core
{
    // TODO: move to another file...
    internal class MsalHttpClientFactoryImpl : IHttpClientFactory
    {
        private readonly IMsalHttpClientFactory _msalHttpClientFactory;
        public MsalHttpClientFactoryImpl(IMsalHttpClientFactory msalHttpClientFactory)
        {
            _msalHttpClientFactory = msalHttpClientFactory;
        }

        /// <inheritdoc />
        public HttpClient HttpClient => _msalHttpClientFactory.GetHttpClient();
    }

    internal partial class ServiceBundle : IServiceBundle
    {
        public MsalConfiguration Config { get; }

        private ServiceBundle(MsalConfiguration msalConfiguration)
        {
            Config = msalConfiguration;

            if (Config.HttpClientFactory != null)
            {
                HttpManager = new HttpManager(ExceptionFactory, new MsalHttpClientFactoryImpl(Config.HttpClientFactory));
            }
            else if (Config.HttpManager != null)
            {
                HttpManager = Config.HttpManager;
            }

            // TODO: need a public callback for telemetry, and wire it through here.
            //if (Config.TelemetryReceiver != null)
            //{
            //    TelemetryManager = new TelemetryManager(Config.TelemetryReceiver);
            //}
        }

        public static ServiceBundle CreateWithConfiguration(MsalConfiguration msalConfiguration)
        {
            return new ServiceBundle(msalConfiguration);
        }
    }
}