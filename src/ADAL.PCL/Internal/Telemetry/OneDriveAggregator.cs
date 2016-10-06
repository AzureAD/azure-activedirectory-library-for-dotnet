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
    internal class OneDriveAggregator : DefaultAggregator
    {
        internal IDictionary<string, List<EventsBase>> ObjectsToBeDispatched =
            new ConcurrentDictionary<string, List<EventsBase>>();

        internal OneDriveAggregator(IDispatcher dispatcher) : base(dispatcher)
        {
            Dispatcher = dispatcher;
        }

        internal override void Flush(string requestID)
        {
            List<Tuple<string, string>> FlatList = new List<Tuple<string, string>>();
            Dictionary<string, string> DispatchMap = new Dictionary<string, string>();

            if (ObjectsToBeDispatched == null || (ObjectsToBeDispatched.Count == 0))
            {
                return;
            }

            int count = ObjectsToBeDispatched.Count;
            foreach (KeyValuePair<string, List<EventsBase>> pair in ObjectsToBeDispatched)
            {
                if (requestID.Equals(pair.Key))
                {
                    foreach (EventsBase Event in pair.Value)
                    {
                        Event.ProcessEvent(DispatchMap);
                    }
                }
            }

            foreach (KeyValuePair<string, string> value in DispatchMap)
            {
                FlatList.Add(new Tuple<string, string>(value.Key, value.Value));
            }

            if (Dispatcher != null)
            {
                Dispatcher.DispatchEvent(FlatList);
            }
        }

        internal override void Receive(string requestId, EventsBase eventsInterface)
        {
            List<EventsBase> eventValue;
            if (ObjectsToBeDispatched.TryGetValue(requestId, out eventValue))
            {
                eventValue.Add(eventsInterface);
                ObjectsToBeDispatched[requestId] = eventValue;
                return;
            }
            eventValue = new List<EventsBase>();
            eventValue.Add(eventsInterface);
            ObjectsToBeDispatched.Add(requestId, eventValue);
        }
    }
}