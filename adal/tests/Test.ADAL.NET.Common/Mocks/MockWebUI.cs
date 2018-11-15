﻿//------------------------------------------------------------------------------
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
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.UI;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.ADAL.NET.Common.Mocks
{
    internal class MockWebUI : IWebUI
    {
        internal AuthorizationResult MockResult { get; set; }

        internal IDictionary<string, string> QueryParams { get; set; }
        
        public async Task<AuthorizationResult> AcquireAuthorizationAsync(Uri authorizationUri, Uri redirectUri, RequestContext requestContext)
        {
            //match QP passed in for validation. 
            if (QueryParams != null)
            {
                Assert.IsNotNull(authorizationUri.Query);
                IDictionary<string, string> inputQp =
                    EncodingHelper.ParseKeyValueList(authorizationUri.Query.Substring(1), '&', true, null);
                foreach (var key in QueryParams.Keys)
                {
                    Assert.IsTrue(inputQp.ContainsKey(key));
                    Assert.AreEqual(QueryParams[key], inputQp[key]);
                }
            }

            return await Task.Factory.StartNew(() => this.MockResult).ConfigureAwait(false);
        }

        public void ValidateRedirectUri(Uri redirectUri)
        {
            RedirectUriHelper.Validate(redirectUri);
        }
    }
}
