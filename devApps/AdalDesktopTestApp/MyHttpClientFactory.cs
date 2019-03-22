// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License.

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AdalDesktopTestApp
{
    class MyHttpClientFactory : IHttpClientFactory
    {
        private HttpClient _httpClient;

        public MyHttpClientFactory()
        {
            _httpClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true })
            {
                MaxResponseContentBufferSize = 1 * 1024 * 1024 // 1 MB
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }
    }
}
