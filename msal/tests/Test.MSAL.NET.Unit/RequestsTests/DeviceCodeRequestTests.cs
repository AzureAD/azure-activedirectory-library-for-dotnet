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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.Instance;
using Microsoft.Identity.Core.OAuth2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.Core.Unit.Mocks;

namespace Test.MSAL.NET.Unit.RequestsTests
{
    [TestClass]
    public class DeviceCodeRequestTests
    {
        private TokenCache _cache;
        private readonly MyReceiver _myReceiver = new MyReceiver();

        [TestInitialize]
        public void TestInitialize()
        {
            RequestTestsCommon.InitializeRequestTests();
            Telemetry.GetInstance().RegisterReceiver(_myReceiver.OnEvents);

            _cache = new TokenCache();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _cache.tokenCacheAccessor.AccessTokenCacheDictionary.Clear();
            _cache.tokenCacheAccessor.RefreshTokenCacheDictionary.Clear();
        }

        private HttpResponseMessage CreateDeviceCodeResponseSuccessMessage()
        {
            return MockHelpers.CreateSuccessResponseMessage(
                "{\"user_code\":\"B6SUYU5PL\",\"device_code\":\"BAQABAAEAAADXzZ3ifr-GRbDT45zNSEFEfU4P-bZYS1vkvv8xiXdb1_zX2xAcdcfEoei1o-t9-zTB9sWyTcddFEWahP1FJJJ_YVA1zvPM2sV56d_8O5G23ti5uu0nRbIsniczabYYEr-2ZsbgRO62oZjKlB1zF3EkuORg2QhMOjtsk-KP0aw8_iAA\",\"verification_url\":\"https://microsoft.com/devicelogin\",\"expires_in\":\"900\",\"interval\":\"5\",\"message\":\"To sign in, use a web browser to open the page https://microsoft.com/devicelogin and enter the code B6SUYU5PL to authenticate.\"}");
        }

        [TestMethod]
        [TestCategory("DeviceCodeRequestTests")]
        public void TestDeviceCodeAuthSuccess()
        {
            Authority authority = Authority.CreateAuthority(TestConstants.AuthorityHomeTenant, false);
            _cache = new TokenCache()
            {
                ClientId = TestConstants.ClientId
            };

            AuthenticationRequestParameters parameters = new AuthenticationRequestParameters()
            {
                Authority = authority,
                // todo: what is this? SliceParameters = "key1=value1%20with%20encoded%20space&key2=value2",
                ClientId = TestConstants.ClientId,
                Scope = TestConstants.Scope,
                TokenCache = _cache,
                RequestContext = new RequestContext(new MsalLogger(Guid.NewGuid(), null))
            };

            RequestTestsCommon.MockInstanceDiscoveryAndOpenIdRequest();

            var expectedScopes = new SortedSet<string>();
            expectedScopes.UnionWith(TestConstants.Scope);
            expectedScopes.Add("openid");
            expectedScopes.Add("offline_access");
            expectedScopes.Add("profile");

            // Mock Handler for device code request
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                PostData = new Dictionary<string, string>()
                {
                    {OAuth2Parameter.ClientId, TestConstants.ClientId},
                    {OAuth2Parameter.Scope, expectedScopes.AsSingleString()}
                },
                ResponseMessage = CreateDeviceCodeResponseSuccessMessage()
            });

            // Mock Handler for devicecode->token exchange request
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                PostData = new Dictionary<string, string>()
                {
                    {OAuth2Parameter.ClientId, TestConstants.ClientId},
                    {OAuth2Parameter.Scope, expectedScopes.AsSingleString()}
                },
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });


            DeviceCodeResult actualDeviceCodeResult = null;
            DeviceCodeRequest request = new DeviceCodeRequest(parameters, result => { actualDeviceCodeResult = result; });
            var task = request.RunAsync();
            task.Wait();
            var authenticationResult = task.Result;
            Assert.IsNotNull(authenticationResult);
            Assert.IsNotNull(actualDeviceCodeResult);

            Assert.AreEqual("client_id", actualDeviceCodeResult.ClientId);
            Assert.AreEqual("BAQABAAEAAADXzZ3ifr-GRbDT45zNSEFEfU4P-bZYS1vkvv8xiXdb1_zX2xAcdcfEoei1o-t9-zTB9sWyTcddFEWahP1FJJJ_YVA1zvPM2sV56d_8O5G23ti5uu0nRbIsniczabYYEr-2ZsbgRO62oZjKlB1zF3EkuORg2QhMOjtsk-KP0aw8_iAA", actualDeviceCodeResult.DeviceCode);
            Assert.AreEqual("client_id", actualDeviceCodeResult.ClientId);

            // todo: figure out how to unit test this one since it's calculated from current time based on the datetime offset retrieved in the response
            // Assert.AreEqual(5, actualDeviceCodeResult.ExpiresOn); 

            Assert.AreEqual("To sign in, use a web browser to open the page https://microsoft.com/devicelogin and enter the code B6SUYU5PL to authenticate.", actualDeviceCodeResult.Message);
            Assert.AreEqual("B6SUYU5PL", actualDeviceCodeResult.UserCode);
            Assert.AreEqual("https://microsoft.com/devicelogin", actualDeviceCodeResult.VerificationUrl);

            Assert.IsTrue(HttpMessageHandlerFactory.IsMocksQueueEmpty, "All mocks should have been consumed");
        }
    }
}
