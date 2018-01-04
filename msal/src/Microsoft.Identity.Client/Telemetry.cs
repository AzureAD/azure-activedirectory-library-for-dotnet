﻿//----------------------------------------------------------------------
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
using System.Linq;
using Microsoft.Identity.Client.Internal.Telemetry;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// 
    /// </summary>
    public class Telemetry
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        public delegate void Receiver(List<Dictionary<string, string>> events);

        private Receiver _receiver = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        public void RegisterReceiver(Receiver r)
        {
            _receiver = r;
        }

        private static readonly Telemetry Singleton = new Telemetry();

        internal Telemetry(){}  // This is an internal constructor to build isolated unit test instance

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Telemetry GetInstance()
        {
            return Singleton;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool TelemetryOnFailureOnly { get; set; }

        internal ConcurrentDictionary<Tuple<string, string>, EventBase> EventsInProgress = new ConcurrentDictionary<Tuple<string, string>, EventBase>();

        internal ConcurrentDictionary<string, List<EventBase>> CompletedEvents = new ConcurrentDictionary<string, List<EventBase>>();

        internal string GenerateNewRequestId()
        {
            return Guid.NewGuid().ToString();
        }

        internal void StartEvent(string requestId, EventBase eventToStart)
        {
            if (_receiver != null && requestId != null)
            {
                EventsInProgress[new Tuple<string, string>(requestId, eventToStart[EventBase.EventNameKey])] = eventToStart;
            }
        }

        internal void StopEvent(string requestId, EventBase eventToStop)
        {
            if (_receiver == null || requestId == null)
            {
                return;
            }
            Tuple<string, string> eventKey = new Tuple<string, string>(requestId, eventToStop[EventBase.EventNameKey]);

            // Locate the same name event in the EventsInProgress map
            EventBase eventStarted = null;
            if (EventsInProgress.ContainsKey(eventKey))
            {
                eventStarted = EventsInProgress[eventKey];
            }

            // If we did not get anything back from the dictionary, most likely its a bug that StopEvent
            // was called without a corresponding StartEvent
            if (null == eventStarted)
            {
                // Stop Event called without a corresponding start_event.
                return;
            }

            // Set execution time properties on the event
            eventToStop.Stop();

            if (!CompletedEvents.ContainsKey(requestId))
            {
                // if this is the first event associated to this
                // RequestId we need to initialize a new List to hold
                // all of sibling events
                List<EventBase> events = new List<EventBase>();
                events.Add(eventToStop);
                CompletedEvents[requestId] = events;
            }
            else
            {
                // if this event shares a RequestId with other events
                // just add it to the List
                CompletedEvents[requestId].Add(eventToStop);
            }

            // Mark this event as no longer in progress
            EventBase dummy = null; // The TryRemove(...) next line requires an out parameter, even though we don't actually use it
            EventsInProgress.TryRemove(eventKey, out dummy);
            // We could use the following one-liner instead, but we believe it is less readable:
            // ((IDictionary<Tuple<string, string>, EventBase>)EventsInProgress).Remove(eventKey);
        }

        internal void Flush(string requestId)
        {
            if (_receiver == null)
            {
                return;
            }

            // check for orphaned events...
            List<EventBase> orphanedEvents = CollateOrphanedEvents(requestId);
            // Add the OrphanedEvents to the completed EventList
            if (!CompletedEvents.ContainsKey(requestId))
            {
                // No completed Events returned for RequestId
                return;
            }

            CompletedEvents[requestId].AddRange(orphanedEvents);

            List<EventBase> eventsToFlush;
            CompletedEvents.TryRemove(requestId, out eventsToFlush);

            if (TelemetryOnFailureOnly)
            {
                // iterate over Events, if the ApiEvent was successful, don't dispatch
                bool shouldRemoveEvents = false;

                foreach (var anEvent in eventsToFlush)
                {
                    var apiEvent = anEvent as ApiEvent;
                    if (apiEvent != null)
                    {
                        shouldRemoveEvents = apiEvent.WasSuccessful;
                        break;
                    }
                }

                if (shouldRemoveEvents)
                {
                    eventsToFlush.Clear();
                }
            }

            if (eventsToFlush.Count > 0)
            {
                eventsToFlush.Insert(0, new DefaultEvent(ClientId));
                _receiver(eventsToFlush.Cast<Dictionary<string, string>>().ToList());
            }
        }

        private List<EventBase> CollateOrphanedEvents(String requestId)
        {
            var orphanedEvents = new List<EventBase>();
            foreach (var key in EventsInProgress.Keys)
            {
                if (key.Item1 == requestId)
                {
                    // The orphaned event already contains its own start time, we simply collect it
                    EventBase orphan;
                    EventsInProgress.TryRemove(key, out orphan);
                    orphanedEvents.Add(orphan);
                }
            }
            return orphanedEvents;
        }

        internal string ClientId { get; set; }
    }
}
