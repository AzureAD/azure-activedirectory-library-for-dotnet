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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    public class Telemetry
    {
        private static readonly Telemetry Instance = new Telemetry();

        private readonly IDictionary<Tuple<string, string>, DateTime> EventTracking =
            new ConcurrentDictionary<Tuple<string, string>, DateTime>();

        private readonly string format = "yyyy-mm-dd hh:mm:ss:ffff";

        private DefaultAggregator Dispatcher;

        public static Telemetry GetInstance()
        {
            return Instance;
        }

        internal string CreateRequestId()
        {
            return Guid.NewGuid().ToString();
        }

        public void RegisterDispatcher(IDispatcher dispatcher, bool aggregationRequired)
        {
            if (dispatcher == null)
            {
                throw new AdalException(AdalError.DispatcherIsNull);
            }

            if (aggregationRequired)
            {
                Dispatcher = new OneDriveAggregator(dispatcher);
            }
            else
            {
                Dispatcher = new DefaultAggregator(dispatcher);
            }
        }

        internal void StartEvent(string requestId, string eventName)
        {
            if (Dispatcher == null)
            {
                return;
            }

            EventTracking.Add(new Tuple<string, string>(requestId, eventName), DateTime.UtcNow);
        }

        internal void StopEvent(string requestId, EventsBase Event, string eventName)
        {
            if (Dispatcher == null)
            {
                return;
            }

            DateTime startTime;
            List<Tuple<string, string>> listEvent = Event.GetEvents();

            if (EventTracking.
                TryGetValue(new Tuple<string, string>(requestId, eventName), out startTime))
            {
                DateTime stopTime = DateTime.UtcNow;

                listEvent.Add(new Tuple<string, string>(EventConstants.StartTime, startTime.ToString(format)));
                listEvent.Add(new Tuple<string, string>(EventConstants.StopTime, stopTime.ToString(format)));

                TimeSpan diff1 = stopTime.Subtract(startTime);
                //Add the response time to the list
                listEvent.Add(new Tuple<string, string>(EventConstants.ResponseTime, diff1.Milliseconds.ToString()));
                //Adding event name to the start of the list
                listEvent.Insert(0, new Tuple<string, string>(EventConstants.EventName, eventName));
                listEvent.Add(new Tuple<string, string>(EventConstants.RequestId, requestId));

                Dispatcher.Receive(requestId, Event);
            }
            else
            {
                PlatformPlugin.Logger.Information(null, "StopEvent has been invoked without StartEvent");
            }

            //Remove the event from the tracking Map
            EventTracking.Remove(new Tuple<string, string>(requestId, eventName));
        }

        internal void Flush(string requestId)
        {
            if (Dispatcher != null)
            {
                Dispatcher.Flush(requestId);
            }
        }
    }
}