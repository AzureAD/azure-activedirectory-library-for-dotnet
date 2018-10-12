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

using Microsoft.Identity.Core;
using Microsoft.Identity.Core.UI;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform
{
    internal abstract class WebUI : IWebUI
    {
        protected Uri RequestUri { get; private set; }

        protected RequestContext context;

        protected Uri CallbackUri { get; private set; }

        public Object OwnerWindow { get; set; }

        public async Task<AuthorizationResult> AcquireAuthorizationAsync(Uri authorizationUri, Uri redirectUri, RequestContext requestContext)
        {
            AuthorizationResult authorizationResult = null;

            var other = PlatformParameters._syncContext;
            other.Send(state =>
                {
                    var tup = (Tuple<Uri, Uri>)state;
                    authorizationResult = this.Authenticate(tup.Item1, tup.Item2);
                },
            Tuple.Create(authorizationUri, redirectUri));

            return await Task.Factory.StartNew(() => authorizationResult).ConfigureAwait(false);
        }

        internal AuthorizationResult Authenticate(Uri requestUri, Uri callbackUri)
        {
            this.RequestUri = requestUri;
            this.CallbackUri = callbackUri;

            ThrowOnNetworkDown();
            return this.OnAuthenticate();
        }

        private static void ThrowOnNetworkDown()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                throw new AdalException(AdalError.NetworkNotAvailable);
            }
        }

        protected abstract AuthorizationResult OnAuthenticate();
    }
}
