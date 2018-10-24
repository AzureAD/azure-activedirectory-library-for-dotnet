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
using System.Net;
using System.Net.Http;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.UI;
using NSubstitute;

namespace Test.MSAL.NET.Unit.Mocks
{
    internal static class MsalMockHelpers
    {
        public static void ConfigureMockWebUI(AuthorizationResult authorizationResult)
        {
            ConfigureMockWebUI(authorizationResult, new Dictionary<string, string>());
        }

        public static void ConfigureMockWebUI(AuthorizationResult authorizationResult, Dictionary<string, string> queryParamsToValidate)
        {
            MockWebUI webUi = new MockWebUI();
            webUi.QueryParamsToValidate = queryParamsToValidate;
            webUi.MockResult = authorizationResult;

            ConfigureMockWebUI(webUi);
        }


        public static void ConfigureMockWebUI(MockWebUI webUi)
        {
            IWebUIFactory mockFactory = Substitute.For<IWebUIFactory>();
            mockFactory.CreateAuthenticationDialog(Arg.Any<CoreUIParent>(), Arg.Any<RequestContext>()).Returns(webUi);
            PlatformPlugin.WebUIFactory = mockFactory;
        }
    }
}
