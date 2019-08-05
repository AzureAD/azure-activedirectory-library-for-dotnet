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
using System.Globalization;

namespace Microsoft.Identity.Core.Helpers
{
    internal static class UriBuilderExtensions
    {
        private const int DefaultHttpsPort = 443;

        public static void AppendQueryParameters(this UriBuilder builder, string queryParams)
        {
            if (builder == null || String.IsNullOrEmpty(queryParams))
            {
                return;
            }

            if (builder.Query.Length > 1)
            {
                builder.Query = builder.Query.Substring(1) + "&" + queryParams;
            }
            else
            {
                builder.Query = queryParams;
            }
        }

        public static string GetHttpsUriWithOptionalPort(string host, string tenant, string path, int port)
        {
            if (string.IsNullOrEmpty(tenant))
            {
                return GetHttpsUriWithOptionalPort(string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}", host, path), port);
            }
            else
            {
                return GetHttpsUriWithOptionalPort(string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}/{2}", host, tenant, path), port);
            }
        }

        public static string GetHttpsUriWithOptionalPort(string uri, int port)
        {
            //No need to set port if it equals 443 as it is the default https port
            if (port != DefaultHttpsPort)
            {
                var builder = new UriBuilder(uri);
                builder.Port = port;
                return builder.Uri.AbsoluteUri;
            }

            return uri;
        }
    }
}
