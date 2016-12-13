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
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.NET.Unit.Mocks;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class UIEventUnitTest
    {
        private PlatformParameters platformParameters;

        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();
            platformParameters = new PlatformParameters(PromptBehavior.Auto);
        }

        [TestMethod]
        [Description("Test case for checking UIEvent")]
        public async Task TelemetryUIEvent()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            TestDispatcher dispatcher = new TestDispatcher();
            telemetry.RegisterDispatcher(dispatcher, false);

            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityHomeTenant, true);

            DateTime startTime = DateTime.UtcNow;

            MockWebUI.UiTimeoutEnabled = true;

            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, platformParameters);
            Assert.IsNotNull(result);
            Assert.AreEqual(TestConstants.DefaultAuthorityHomeTenant, context.Authenticator.Authority);
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);

            DateTime stopTime = DateTime.UtcNow;
            TimeSpan diff1 = stopTime.Subtract(startTime);
            //Check if the UI event wait time is more than the sleep time in MockUI
            //This check is required to see if the UI is invoked
            Assert.IsTrue(diff1.TotalMilliseconds > 2000);

            Assert.IsTrue(dispatcher.UIEventTelemetryValidator());
        }

        private class TestDispatcher : IDispatcher
        {
            private readonly List<List<Tuple<string, string>>> storeList = new List<List<Tuple<string, string>>>();

            public int Count
            {
                get
                {
                    return storeList.Count;
                }
            }

            void IDispatcher.DispatchEvent(List<Tuple<string, string>> Event)
            {
                storeList.Add(Event);
            }

            public void clear()
            {
                storeList.Clear();
            }

            public bool UIEventTelemetryValidator()
            {
                HashSet<string> UIitems = new HashSet<string>();
                UIitems.Add("event_name");
                UIitems.Add("application_name");
                UIitems.Add("application_version");
                UIitems.Add("sdk_version");
                UIitems.Add("sdk_platform");
                UIitems.Add("device_id");
                UIitems.Add("correlation_id");
                UIitems.Add("start_time");
                UIitems.Add("end_time");
                UIitems.Add("response_time");
                UIitems.Add("request_id");
                UIitems.Add("is_deprecated");
                UIitems.Add("idp");
                UIitems.Add("displayable_id");
                UIitems.Add("unique_id");
                UIitems.Add("authority");
                UIitems.Add("authority_type");
                UIitems.Add("validation_status");
                UIitems.Add("extended_expires_on_setting");
                UIitems.Add("is_successful");
                UIitems.Add("user_id");
                UIitems.Add("tenant_id");
                UIitems.Add("login_hint");
                UIitems.Add("api_id");
                UIitems.Add("token_found");
                UIitems.Add("request_api_version");
                UIitems.Add("response_code");


                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        if (!(UIitems.Contains(tuple.Item1) && tuple.Item2 != null && tuple.Item2.Length > 0))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
    }
}
