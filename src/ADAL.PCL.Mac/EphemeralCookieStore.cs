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

using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using WebKit;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    class EphemeralCookieStore
    {
        readonly List<NSHttpCookie> storedCookies = new List<NSHttpCookie>();

        /// <summary>
        /// Captures cookies that were set by JavaScript.
        /// </summary>
        public void TakeCookies(WebView webView)
        {
            var docCookiesNSString = webView.WindowScriptObject.EvaluateWebScript("document.cookie") as NSString;
            if (docCookiesNSString == null)
            {
                return;
            }

            var docUrlNSString = webView.WindowScriptObject.EvaluateWebScript("document.URL") as NSString;

            var docCookies = docCookiesNSString.ToString();
            var docUrl = new Uri(docUrlNSString.ToString());

            foreach (var cookieStr in docCookies.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var eqIdx = cookieStr.IndexOf('=');
                var name = cookieStr.Substring(0, eqIdx).Trim();
                var value = cookieStr.Substring(eqIdx+1).Trim();
                var isSecure = string.Equals("https", docUrl.Scheme, StringComparison.OrdinalIgnoreCase);

                // NOTE:
                // we don't have any way to get info such as domain, expiry etc from the document
                // so make a best guess. It shouldn't really matter since the cookies are only kept
                // alive as long as the sign-in window is open
                //
                var cookie = new NSHttpCookie(new NSMutableDictionary {
                    { NSHttpCookie.KeyName, new NSString (name) },
                    { NSHttpCookie.KeyValue, new NSString (value) },
                    { NSHttpCookie.KeyDomain, new NSString (docUrl.Host) },
                    { NSHttpCookie.KeyPath, new NSString ("/") },
                    { NSHttpCookie.KeySecure, NSNumber.FromBoolean (isSecure) },
                });

                AddCookieToStorage(cookie);
            }
        }
        public void TakeCookies(NSUrlResponse response)
        {
            var httpResponse = response as NSHttpUrlResponse;
            if (httpResponse == null)
                return;

            var cookies = NSHttpCookie.CookiesWithResponseHeaderFields(httpResponse.AllHeaderFields, httpResponse.Url);
            foreach (var cookie in cookies)
            {
                AddCookieToStorage(cookie);
            }
        }

        public void GiveCookies(NSMutableUrlRequest request)
        {
            request.ShouldHandleCookies = false;
            var cookies = GetStoredCookiesForUrl(request.Url).ToArray();
            var headers = NSHttpCookie.RequestHeaderFieldsWithCookies(cookies);
            var mutableHeaders = (NSMutableDictionary)request.Headers.MutableCopy();
            mutableHeaders.SetValuesForKeysWithDictionary(headers);
            request.Headers = mutableHeaders;
        }

        void AddCookieToStorage(NSHttpCookie cookie)
        {
            //check whether this replaces an existing cookie
            for (int i = 0; i < storedCookies.Count; i++)
            {
                var storedCookie = storedCookies[i];
                if (storedCookie.Domain == cookie.Domain & storedCookie.Path == cookie.Path && storedCookie.Name == cookie.Name)
                {
                    NSDate cookieExpires = cookie.ExpiresDate;
                    if (cookieExpires != null && ((DateTime)cookieExpires) < DateTime.UtcNow)
                    {
                        storedCookies.RemoveAt(i);
                    }
                    else
                    {
                        storedCookies[i] = cookie;
                    }
                    return;
                }
            }
            //else just add it
            storedCookies.Add(cookie);
        }

        IEnumerable<NSHttpCookie> GetStoredCookiesForUrl(NSUrl url)
        {
            var urlPath = url.Path;
            var urlDomain = url.Host;
            var urlDotDomain = "." + url.Host;
            var urlIsSecure = string.Equals("https", url.Scheme, StringComparison.OrdinalIgnoreCase);

            foreach (var cookie in storedCookies)
            {
                NSDate cookieExpires = cookie.ExpiresDate;
                if (cookieExpires != null && ((DateTime)cookieExpires) < DateTime.UtcNow)
                {
                    continue;
                }

                if (cookie.IsSecure && !urlIsSecure)
                {
                    continue;
                }

                bool domainMatch = (cookie.Domain[0] == '.')
                    ? urlDotDomain.StartsWith(cookie.Domain, StringComparison.OrdinalIgnoreCase)
                    : string.Equals(cookie.Domain, urlDomain, StringComparison.OrdinalIgnoreCase);

                if (!domainMatch)
                {
                    continue;
                }

                var cookiePath = cookie.Path;
                var pathMatch = cookiePath == "/"
                    || (urlPath.StartsWith(cookiePath, StringComparison.Ordinal)
                        && (cookiePath.Length == urlPath.Length || urlPath[cookiePath.Length] == '/'));

                if (!pathMatch)
                {
                    continue;
                }

                yield return cookie;
            }
            yield break;
        }
    }
}