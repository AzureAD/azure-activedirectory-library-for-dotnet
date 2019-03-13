// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License.

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AdalDesktopTestApp
{
    class MyHttpClientFactory : IHttpClientFactory
    {
        private HttpClient _httpClient;

        public HttpClient GetHttpClient()
        {
            _httpClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true })
            {
                MaxResponseContentBufferSize = 1 * 1024 * 1024 // 1 MB
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return _httpClient;
        }
    }
}
