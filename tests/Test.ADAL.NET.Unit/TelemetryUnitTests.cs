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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    class TelemetryUnitTests
    {
        [TestMethod]
        [Description("Telemetry tests Default Dispatcher")]
        public void TelemetryDefaultDispatcher()
        {
            Microsoft.IdentityModel.Clients.ActiveDirectory.Telemetry telemetry =
            Microsoft.IdentityModel.Clients.ActiveDirectory.Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            string requestIDOne = telemetry.RegisterNewRequest();
            Assert.IsNotNull(requestIDOne);

            telemetry.StartEvent(requestIDOne, "acquire_token");
            string requestIDTwo = telemetry.RegisterNewRequest();
            Assert.AreEqual(telemetry.EventsStored(), 1);

            DefaultEvent testDefaultEvent = new DefaultEvent("random_event");
            Assert.IsNotNull(DefaultEvent.ApplicationName);
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);

            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, true);
            dispatcher.clear();

            string requestIDThree = telemetry.RegisterNewRequest();
            telemetry.StartEvent(requestIDThree, "event_3");
            telemetry.StopEvent(requestIDThree, testDefaultEvent, "event_3");
            Assert.AreEqual(dispatcher.Count, 1);
            //dispatcher.file();

            telemetry.StartEvent(requestIDTwo, "cache_lookup");
            DefaultEvent cacheDefaultEvent = new DefaultEvent("cache_lookup");
            telemetry.StopEvent(requestIDTwo, cacheDefaultEvent, "cache_lookup");
            Assert.AreEqual(dispatcher.Count, 2);

            string requestIDFour = telemetry.RegisterNewRequest();
            telemetry.StartEvent(requestIDFour, EventConstants.Crypto);
            DefaultEvent cryptoDefaultEvent = new DefaultEvent(EventConstants.Crypto);
            telemetry.StopEvent(requestIDFour, cacheDefaultEvent, EventConstants.Crypto);
            Assert.AreEqual(dispatcher.Count, 3);

            dispatcher.file();
        }

        [TestMethod]
        [Description("Telemetry tests Aggregate Dispatcher for a single event in requestID")]
        public void TelemetryAggregateDispatcherSingleEventRequestID()
        {
            Microsoft.IdentityModel.Clients.ActiveDirectory.Telemetry telemetry =
Microsoft.IdentityModel.Clients.ActiveDirectory.Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, false);
            dispatcher.clear();
            string requestIDThree = telemetry.RegisterNewRequest();
            telemetry.StartEvent(requestIDThree, "event_3");
            DefaultEvent testDefaultEvent = new DefaultEvent("event_3");
            Assert.IsNotNull(DefaultEvent.ApplicationName);
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent, "event_3");
            telemetry.flush(requestIDThree);
            Assert.AreEqual(dispatcher.Count, 1);

            dispatcher.file();
        }

        [TestMethod]
        [Description("Telemetry tests for Aggregate Dispatcher for multiple events in requestID")]
        public void TelemetryAggregateDispatcherMultipleEventsRequestId()
        {
            Microsoft.IdentityModel.Clients.ActiveDirectory.Telemetry telemetry =
Microsoft.IdentityModel.Clients.ActiveDirectory.Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, false);
            dispatcher.clear();
            string requestIDThree = telemetry.RegisterNewRequest();
            telemetry.StartEvent(requestIDThree, "event_3");
            DefaultEvent testDefaultEvent3 = new DefaultEvent("event_3");
            Assert.IsNotNull(DefaultEvent.ApplicationName);
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent3, "event_3");

            telemetry.StartEvent(requestIDThree, "event_4");
            DefaultEvent testDefaultEvent4 = new DefaultEvent("event_4");
            Assert.IsNotNull(DefaultEvent.ApplicationName);
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent4, "event_4");

            telemetry.StartEvent(requestIDThree, "event_5");
            DefaultEvent testDefaultEvent5 = new DefaultEvent("event_5");
            Assert.IsNotNull(DefaultEvent.ApplicationName);
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent5, "event_5");
            telemetry.flush(requestIDThree);
            Assert.AreEqual(dispatcher.Count, 1);

            dispatcher.file();
        }

        class DispatcherImplement : IDispatcher
        {
            private readonly List<List<Tuple<string, string>>> storeList = new List<List<Tuple<string, string>>>();

            void IDispatcher.Dispatch(List<Tuple<string, string>> Event)
            {
                storeList.Add(Event);
            }

            public int Count
            {
                get { return storeList.Count; }
            }

            public void clear()
            {
                storeList.Clear();
            }

            public void file()
            {
                using (TextWriter tw = new StreamWriter("test.txt"))
                {
                    foreach (List<Tuple<string, string>> list in storeList)
                    {
                        foreach (Tuple<string, string> tuple in list)
                        {
                            tw.WriteLine(tuple.Item1 + " " + tuple.Item2 + "\r\n");
                        }
                    }
                }
            }
        }
    }
}
