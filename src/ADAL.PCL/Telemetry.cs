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
        private readonly static Telemetry Instance = new Telemetry();
        private readonly String format = "yyyy-mm-dd hh:mm:ss:ffff";

        public static Telemetry GetInstance()
        {
            return Instance;
        }

        DefaultDispatcher Dispatcher = null ;

        private IDictionary<Tuple<string, string>,string> EventTracking = new ConcurrentDictionary<Tuple<string, string>,string>();

        internal string RegisterNewRequest()
        {
            return Guid.NewGuid().ToString();
        }

        public void RegisterDispatcher(IDispatcher dispatcher, bool aggregationRequired)
        {
            if (dispatcher != null)
            {
                if (aggregationRequired)
                {
                    Dispatcher = new DefaultDispatcher(dispatcher);
                }
                else
                {
                    Dispatcher = new AggregatedDispatcher(dispatcher);
                }
            }
            else
            {
                return;
            }
        }

        internal void StartEvent(string requestId, string eventName)
        {
            if (Dispatcher == null)
            {
                return;
            }
            if (! EventTracking.ContainsKey(new Tuple<string, string>(requestId, eventName)))
            {
                EventTracking.Add(new Tuple<string, string>(requestId, eventName), DateTime.Now.ToString());
            }
        }

        internal void StopEvent(string requestId, EventsBase Event,string eventName)
        {
            if (Dispatcher == null)
            {
                return;
            }
            string value;
            List<Tuple<string, string>> listEvent = Event.GetEvents();
            if (EventTracking.
                TryGetValue(new Tuple<string, string>(requestId, eventName), out value))
            {
                DateTime startTime = DateTime.Parse(value);
                DateTime stopTime = DateTime.Now;

                listEvent.Add(new Tuple<string, string>(EventConstants.StartTime, startTime.ToString(format)));
                listEvent.Add(new Tuple<string, string>(EventConstants.StopTime, stopTime.ToString(format)));

                System.TimeSpan diff1 = stopTime.Subtract(startTime);
                //Add the response time to the list
                listEvent.Add(new Tuple<string, string>(EventConstants.ResponseTime,diff1.ToString()));
                //Adding event name to the start of the list
                listEvent.Insert(0, new Tuple<string, string>(EventConstants.EventName,eventName));
                listEvent.Add(new Tuple<string, string>(EventConstants.RequestId, requestId));
                //Remove the event from the tracking Map
                EventTracking.Remove(new Tuple<string, string>(requestId, eventName));
            }
            Dispatcher.Receive(requestId,Event);
        }

        internal void DispatchEventNow(string requestId, EventsBase Event,string eventName)
        {
        }

        internal int EventsStored()
        {
            return EventTracking.Count;
        }

        internal void flush(string requestId)
        {
            if (Dispatcher != null)
            {
                Dispatcher.Flush(requestId);
            }
        }
    }
}
