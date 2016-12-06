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
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class CacheEventUnitTests
    {
        private PlatformParameters platformParameters;

        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();
            platformParameters = new PlatformParameters(PromptBehavior.Auto);
        }

        [TestMethod]
        [Description("Test case for checking CacheEvent")]
        public void TelemetryCacheEvent()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            TestDispatcher dispatcher = new TestDispatcher();
            telemetry.RegisterDispatcher(dispatcher, true);
            dispatcher.clear();
            string requestIDThree = telemetry.CreateRequestId();
            telemetry.StartEvent(requestIDThree, "cache_lookup");
            CacheEvent testDefaultEvent = new CacheEvent();
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent, "cache_lookup");

            bool result = dispatcher.CacheTelemetryValidator();

            Assert.IsTrue(result);
        }

        private class TestDispatcher : IDispatcher
        {
            private readonly List<List<Tuple<string, string>>> storeList = new List<List<Tuple<string, string>>>();

            void IDispatcher.DispatchEvent(List<Tuple<string, string>> Event)
            {
                storeList.Add(Event);
            }

            public void clear()
            {
                storeList.Clear();
            }

            public bool CacheTelemetryValidator()
            {
                HashSet<string> Cacheitems = new HashSet<string>();
                Cacheitems.Add("event_name");
                Cacheitems.Add("application_name");
                Cacheitems.Add("application_version");
                Cacheitems.Add("x-client-version");
                Cacheitems.Add("x-client-sku");
                Cacheitems.Add("device_id");
                Cacheitems.Add("correlation_id");
                Cacheitems.Add("start_time");
                Cacheitems.Add("end_time");
                Cacheitems.Add("response_time");
                Cacheitems.Add("request_id");

                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        if (!(Cacheitems.Contains(tuple.Item1) && tuple.Item2 != null && tuple.Item2.Length > 0))
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
