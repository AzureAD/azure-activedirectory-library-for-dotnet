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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.NET.Common;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class AuthenticationParametersTests
    {
        private const string AuthenticateHeader = "WWW-Authenticate";

        [TestInitialize]
        public void Initialize()
        {
            ModuleInitializer.ForceModuleInitializationTestOnly();
            InstanceDiscovery.InstanceCache.Clear();
        }

        [TestMethod]
        [Description("Tests for discovery via 401 challenge response using CreateFromResponseAuthenticateHeader()")]
        public void CreateFromResponseAuthenticateHeaderTest()
        {
            string authority = AdalTestConstants.DefaultAuthorityCommonTenant + "/oauth2/authorize";
            const string Resource = "test_resource";

            AuthenticationParameters authParams = AuthenticationParameters.CreateFromResponseAuthenticateHeader(string.Format(CultureInfo.InvariantCulture, @"Bearer authorization_uri=""{0}"",resource_id=""{1}""", authority, Resource));
            Assert.AreEqual(authority, authParams.Authority);
            Assert.AreEqual(Resource, authParams.Resource);

            authParams = AuthenticationParameters.CreateFromResponseAuthenticateHeader(string.Format(CultureInfo.InvariantCulture, @"bearer Authorization_uri=""{0}"",Resource_ID=""{1}""", authority, Resource));
            Assert.AreEqual(authority, authParams.Authority);
            Assert.AreEqual(Resource, authParams.Resource);

            authParams = AuthenticationParameters.CreateFromResponseAuthenticateHeader(string.Format(CultureInfo.InvariantCulture, @"Bearer authorization_uri=""{0}""", authority));
            Assert.AreEqual(authority, authParams.Authority);
            Assert.IsNull(authParams.Resource);

            authParams = AuthenticationParameters.CreateFromResponseAuthenticateHeader(string.Format(CultureInfo.InvariantCulture, @"Bearer resource_id=""{0}""", Resource));
            Assert.AreEqual(Resource, authParams.Resource);
            Assert.IsNull(authParams.Authority);

            // Null parameter -> error
            var ex = AssertException.Throws<ArgumentNullException>(() =>
                AuthenticationParameters.CreateFromResponseAuthenticateHeader(null));
            Assert.AreEqual(ex.ParamName, "authenticateHeader");

            // Invalid format -> error
            var argEx = AssertException.Throws<ArgumentException>(() =>
                AuthenticationParameters.CreateFromResponseAuthenticateHeader(string.Format(CultureInfo.InvariantCulture, @"authorization_uri=""{0}"",Resource_id=""{1}""", authority, Resource)));
            Assert.AreEqual(argEx.ParamName, "authenticateHeader");
            Assert.AreEqual(argEx.Message, AdalErrorMessage.InvalidAuthenticateHeaderFormat);
        }

        [TestMethod]
        [Description("Tests for discovery via 401 challenge response using CreateFromUnauthorizedResponseAsync()")]
        public async Task CreateFromUnauthorizedResponseAsyncTestAsync()
        {
            string authority = AdalTestConstants.DefaultAuthorityCommonTenant + "/oauth2/authorize";
            const string Resource = "test_resource";

            AuthenticationParameters authParams = await AuthenticationParameters.CreateFromUnauthorizedResponseAsync(CreateResponseMessage(string.Format(CultureInfo.InvariantCulture, @"Bearer authorization_uri=""{0}"",resource_id=""{1}""", authority, Resource))).ConfigureAwait(false);
            Assert.AreEqual(authority, authParams.Authority);
            Assert.AreEqual(Resource, authParams.Resource);

            authParams = await AuthenticationParameters.CreateFromUnauthorizedResponseAsync(CreateResponseMessage(string.Format(CultureInfo.InvariantCulture, @"bearer Authorization_uri=""{0}"",Resource_ID=""{1}""", authority, Resource))).ConfigureAwait(false);
            Assert.AreEqual(authority, authParams.Authority);
            Assert.AreEqual(Resource, authParams.Resource);

            authParams = await AuthenticationParameters.CreateFromUnauthorizedResponseAsync(CreateResponseMessage(string.Format(CultureInfo.InvariantCulture, @"Bearer authorization_uri=""{0}""", authority))).ConfigureAwait(false);
            Assert.AreEqual(authority, authParams.Authority);
            Assert.IsNull(authParams.Resource);

            authParams = await AuthenticationParameters.CreateFromUnauthorizedResponseAsync(CreateResponseMessage(string.Format(CultureInfo.InvariantCulture, @"Bearer resource_id=""{0}""", Resource))).ConfigureAwait(false);
            Assert.AreEqual(Resource, authParams.Resource);
            Assert.IsNull(authParams.Authority);

            // Null parameter -> error
            var ex = AssertException.TaskThrows<ArgumentNullException>(() =>
                AuthenticationParameters.CreateFromUnauthorizedResponseAsync(null));
            Assert.AreEqual(ex.ParamName, "responseMessage");

            // Invalid format -> error
            var argEx = AssertException.TaskThrows<ArgumentException>(async () =>
                await AuthenticationParameters.CreateFromUnauthorizedResponseAsync(CreateResponseMessage(string.Format(CultureInfo.InvariantCulture, @"authorization_uri=""{0}"",Resource_id=""{1}""", authority, Resource))).ConfigureAwait(false));
            Assert.AreEqual(argEx.ParamName, "authenticateHeader");
            Assert.AreEqual(argEx.Message, AdalErrorMessage.InvalidAuthenticateHeaderFormat);

            // Invalid status code -> error
            argEx = AssertException.TaskThrows<ArgumentException>(async () =>
                await AuthenticationParameters.CreateFromUnauthorizedResponseAsync(CreateResponseMessage(string.Format(CultureInfo.InvariantCulture, @"authorization_uri=""{0}"",Resource_id=""{1}""", authority, Resource), HttpStatusCode.Forbidden)).ConfigureAwait(false));
            Assert.AreEqual(argEx.ParamName, "response");
            Assert.AreEqual(argEx.Message, AdalErrorMessage.UnauthorizedHttpStatusCodeExpected);
        }

        private static HttpResponseMessage CreateResponseMessage(string authenticateHeader, HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
        {
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Headers =
                {
                    { AuthenticateHeader, authenticateHeader }
                },
                Content = new StringContent(string.Empty)
            };
        }
    }
}