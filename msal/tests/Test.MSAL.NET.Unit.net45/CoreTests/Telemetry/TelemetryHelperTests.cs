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
using Microsoft.Identity.Core.Telemetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Microsoft.Identity.Core.Unit.Telemetry
{
    [TestClass]
    public class TelemetryHelperTests
    {
        private const string RequestId = "therequestid";
        private const string ClientId = "theclientid";
        private _TestEvent _startEvent;
        private _TestEvent _stopEvent;
        private TelemetryManager _telemetryManager;
        private _TestReceiver _testReceiver;

        [TestInitialize]
        public void Setup()
        {
            _testReceiver = new _TestReceiver();
            _telemetryManager = new TelemetryManager(_testReceiver);

            _startEvent = new _TestEvent("start event");
            _stopEvent = new _TestEvent("stop event");
        }

        private class _TestReceiver : ITelemetryReceiver
        {
            public List<Dictionary<string, string>> ReceivedEvents = new List<Dictionary<string, string>>();

            /// <inheritdoc />
            public void HandleTelemetryEvents(List<Dictionary<string, string>> events)
            {
                ReceivedEvents.AddRange(events);
            }

            /// <inheritdoc />
            public bool OnlySendFailureTelemetry { get; set; }
        }

        private class _TestEvent : EventBase
        {
            public _TestEvent(string eventName) : base(eventName)
            {
            }
        }

        [TestMethod]
        [TestCategory("TelemetryHelperTests")]
        public void TestTelemetryHelper()
        {
            using (_telemetryManager.CreateTelemetryHelper(RequestId, ClientId, _startEvent))
            {
            }

            ValidateResults(_testReceiver, RequestId, ClientId, _startEvent, _startEvent, false);
        }

        [TestMethod]
        [TestCategory("TelemetryHelperTests")]
        public void TestTelemetryHelperWithFlush()
        {
            using (_telemetryManager.CreateTelemetryHelper(RequestId, ClientId, _startEvent, shouldFlush: true))
            {
            }

            ValidateResults(_testReceiver, RequestId, ClientId, _startEvent, _startEvent, true);
        }

        [TestMethod]
        [TestCategory("TelemetryHelperTests")]
        public void TestTelemetryHelperWithDifferentStopStartEvents()
        {
            using (_telemetryManager.CreateTelemetryHelper(RequestId, ClientId, _startEvent, eventToEnd: _stopEvent))
            {
            }

            ValidateResults(_testReceiver, RequestId, ClientId, _startEvent, _stopEvent, false);
        }

        [TestMethod]
        [TestCategory("TelemetryHelperTests")]
        public void TestTelemetryHelperWithDifferentStopStartEventsWithFlush()
        {
            using (_telemetryManager.CreateTelemetryHelper(RequestId, ClientId, _startEvent, eventToEnd: _stopEvent, shouldFlush: true))
            {
            }

            ValidateResults(_testReceiver, RequestId, ClientId, _startEvent, _stopEvent, true);
        }

        private void ValidateResults(
            _TestReceiver _testReceiver,
            string expectedRequestId,
            string expectedClientId,
            _TestEvent expectedStartEvent,
            _TestEvent expectedStopEvent,
            bool shouldFlush)
        {
            // TODO: implement this to check for proper events...
            //if (shouldFlush)
            //{
            //    Assert.AreEqual(1, telem.NumFlushCalls);
            //    Assert.AreEqual(expectedRequestId, telem.LastFlushEventRequestId);
            //    Assert.AreEqual(expectedClientId, telem.LastFlushClientId);
            //}
            //else
            //{
            //    Assert.AreEqual(0, telem.NumFlushCalls);
            //    Assert.AreEqual(string.Empty, telem.LastFlushEventRequestId);
            //}

            //Assert.AreEqual(1, telem.NumStartEventCalls);
            //Assert.AreEqual(1, telem.NumStopEventCalls);

            //Assert.AreEqual(expectedRequestId, telem.LastStartEventRequestId);
            //Assert.AreEqual(expectedRequestId, telem.LastStopEventRequestId);

            //Assert.AreEqual(expectedStartEvent, telem.LastEventToStart);
            //Assert.AreEqual(expectedStopEvent, telem.LastEventToStop);
        }
    }
}
