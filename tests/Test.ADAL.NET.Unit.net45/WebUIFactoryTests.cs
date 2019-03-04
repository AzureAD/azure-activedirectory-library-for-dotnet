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

using Microsoft.Identity.Core;
using Microsoft.Identity.Core.UI;
using Microsoft.Identity.Test.Common.Core.Mocks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Extensibility;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.ADAL.NET.Common;

namespace Test.ADAL.NET.Unit.net45
{
    [TestClass]
    public class WebUIFactoryTests
    {
        private RequestContext _requestContext = new RequestContext(
            AdalTestConstants.DefaultClientId,
            new TestLogger());
        private WebUIFactory _webUIFactory = new WebUIFactory();


#if DESKTOP
        [TestMethod]
        public void WebUIFactory_Net45()
        {

            var webUI = _webUIFactory.CreateAuthenticationDialog(
                new CoreUIParent() { UseHiddenBrowser = true }, _requestContext);
            Assert.IsInstanceOfType(webUI, typeof(SilentWebUI));

            webUI = _webUIFactory.CreateAuthenticationDialog(
                new CoreUIParent() , _requestContext);
            Assert.IsInstanceOfType(webUI, typeof(InteractiveWebUI));

            webUI = _webUIFactory.CreateAuthenticationDialog(
                new CoreUIParent() { CustomWebUi = new TestWebUI()} , _requestContext);
            Assert.IsInstanceOfType(webUI, typeof(CustomWebUiHandler));

        }

#endif

#if NET_CORE

        [TestMethod]
        public void WebUIFactory_NetCore()
        {
            var webUI = _webUIFactory.CreateAuthenticationDialog(
                new CoreUIParent() { CustomWebUi = Substitute.For<ICustomWebUi>() }, _requestContext);
            Assert.IsInstanceOfType(webUI, typeof(CustomWebUiHandler));


            var exception = AssertException.Throws<AdalException>(
                () => _webUIFactory.CreateAuthenticationDialog(new CoreUIParent(), _requestContext));
            Assert.AreEqual(AdalError.NetStandardCustomWebUi, exception.ErrorCode);
        }
#endif

    }
}
