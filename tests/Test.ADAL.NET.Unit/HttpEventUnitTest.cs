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
    public class HttpEventUnitTest
    {
        private PlatformParameters platformParameters;

        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();
            platformParameters = new PlatformParameters(PromptBehavior.Auto);
        }

        [TestMethod]
        [Description("Test case for checking HttpEvent")]
        public void TelemetryHttpEvent()
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
            telemetry.Flush(requestIDThree);
            Assert.AreEqual(dispatcher.Count, 1);

            bool result = dispatcher.HttpEventTelemetryValidator();

            Assert.IsTrue(result);
        }

        [TestMethod]
        [Description("Test for query parsing in HttpEvent")]
        public void HttpEventQueryParsing()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            TestDispatcher dispatcher = new TestDispatcher();
            telemetry.RegisterDispatcher(dispatcher, false);

            string requestID = telemetry.CreateRequestId();
            telemetry.StartEvent(requestID, EventConstants.HttpEvent);

            HttpEvent httpEvent = new HttpEvent();
            string query = "?sourceid=chrome-instant&ion=1&espv=2&ie=UTF-8";
            httpEvent.ParseQuery(query);
            telemetry.StopEvent(requestID, httpEvent, EventConstants.HttpEvent);

            Assert.IsTrue(dispatcher.Parse());

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

            public bool Parse()
            {
                HashSet<string> queryKeys = new HashSet<string>();
                queryKeys.Add("sourceid");
                queryKeys.Add("ion");
                queryKeys.Add("espv");
                queryKeys.Add("ie");

                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        if (tuple.Item1.Equals(EventConstants.HttpQueryParameters))
                        {
                            foreach (string query in queryKeys)
                            {
                                if (! tuple.Item2.Contains(query))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }

            public bool HttpEventTelemetryValidator()
            {
                HashSet<string> Httpitems = new HashSet<string>();
                Httpitems.Add("event_name");
                Httpitems.Add("application_name");
                Httpitems.Add("application_version");
                Httpitems.Add("x-client-version");
                Httpitems.Add("x-client-sku");
                Httpitems.Add("device_id");
                Httpitems.Add("correlation_id");
                Httpitems.Add("start_time");
                Httpitems.Add("end_time");
                Httpitems.Add("response_time");
                Httpitems.Add("request_id");
                Httpitems.Add("request_api_version");

                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        if (!(Httpitems.Contains(tuple.Item1) && tuple.Item2 != null && tuple.Item2.Length > 0))
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
