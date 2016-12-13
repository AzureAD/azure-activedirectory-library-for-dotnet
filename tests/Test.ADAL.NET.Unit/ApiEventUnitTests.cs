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
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class ApiEventUnitTests
    {
        private PlatformParameters platformParameters;

        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();
            platformParameters = new PlatformParameters(PromptBehavior.Auto);
        }

        [TestMethod]
        [Description("Test case for checking hashing of the tenantID")]
        public void HashTenantIdFromAuthority()
        {
            Authenticator authenticator = new Authenticator(TestConstants.DefaultAuthorityCommonTenant, true);
            UserInfo userinfo = new UserInfo();
            userinfo.UniqueId = TestConstants.DefaultUniqueId;
            userinfo.DisplayableId = TestConstants.DefaultDisplayableId;
            ApiEvent apiEvent = new ApiEvent(authenticator, userinfo, "tenant-id", "<some-api-id>");
            string hashedTenantId = apiEvent.HashTenantIdFromAuthority("https://login.microsoftonline.com/blabla.onmicrosoft.com/");
            Uri authority = new Uri("https://login.microsoftonline.com/blabla.onmicrosoft.com/");
            string hashedTenantIdMatch =
                "https://login.microsoftonline.com" + PlatformPlugin.CryptographyHelper.CreateSha256Hash(authority.AbsolutePath);
            Assert.AreEqual(hashedTenantId, hashedTenantIdMatch);
        }

        [TestMethod]
        [Description("Test case for checking ApiEvent")]
        public void TelemetryApiEvent()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            TestDispatcher dispatcher = new TestDispatcher();
            telemetry.RegisterDispatcher(dispatcher, true);
            dispatcher.clear();
            string requestIDThree = telemetry.CreateRequestId();
            telemetry.StartEvent(requestIDThree, EventConstants.ApiEvent);
            Authenticator authenticator = new Authenticator(TestConstants.DefaultAuthorityCommonTenant, true);
            UserInfo userinfo = new UserInfo();
            userinfo.UniqueId = TestConstants.DefaultUniqueId;
            userinfo.DisplayableId = TestConstants.DefaultDisplayableId;
            ApiEvent testDefaultEvent = new ApiEvent(authenticator, userinfo, "tenantId", "3");
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent, EventConstants.ApiEvent);

            bool result = dispatcher.ApiTelemetryValidator();

            Assert.IsTrue(result);
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

            public bool ApiTelemetryValidator()
            {
                HashSet<string> Apiitems = new HashSet<string>();
                Apiitems.Add("event_name");
                Apiitems.Add("application_name");
                Apiitems.Add("application_version");
                Apiitems.Add("sdk_version");
                Apiitems.Add("sdk_platform");
                Apiitems.Add("device_id");
                Apiitems.Add("correlation_id");
                Apiitems.Add("start_time");
                Apiitems.Add("end_time");
                Apiitems.Add("response_time");
                Apiitems.Add("request_id");
                Apiitems.Add("is_deprecated");
                Apiitems.Add("idp");
                Apiitems.Add("displayable_id");
                Apiitems.Add("unique_id");
                Apiitems.Add("authority");
                Apiitems.Add("authority_type");
                Apiitems.Add("validation_status");
                Apiitems.Add("extended_expires_on_setting");
                Apiitems.Add("is_successful");
                Apiitems.Add("user_id");
                Apiitems.Add("tenant_id");
                Apiitems.Add("login_hint");
                Apiitems.Add("api_id");

                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        if (!(Apiitems.Contains(tuple.Item1) && tuple.Item2 != null && tuple.Item2.Length > 0))
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
