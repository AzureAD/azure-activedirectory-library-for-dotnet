// ------------------------------------------------------------------------------
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
// ------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Identity.Core.Telemetry
{
    internal class TelemetryManager : ITelemetryManager, ITelemetry
    {
        private readonly object _lockObj = new object();
        private ITelemetryReceiver _telemetryReceiver;

        public TelemetryManager(ITelemetryReceiver telemetryReceiver = null)
        {
            _telemetryReceiver = telemetryReceiver;
        }

        public ITelemetryReceiver TelemetryReceiver
        {
            get
            {
                lock (_lockObj)
                {
                    return _telemetryReceiver;
                }
            }
            set
            {
                lock (_lockObj)
                {
                    _telemetryReceiver = value;
                }
            }
        }

        private bool HasReceiver()
        {
            lock (_lockObj)
            {
                return _telemetryReceiver != null;
            }
        }

        /// <inheritdoc />
        public string GenerateNewRequestId()
        {
            return Guid.NewGuid().ToString();
        }

        public TelemetryHelper CreateTelemetryHelper(
            string requestId,
            string clientId,
            EventBase eventToStart,
            EventBase eventToEnd = null,
            bool shouldFlush = false)
        {
            return new TelemetryHelper(
                this,
                requestId,
                clientId,
                eventToStart,
                eventToEnd ?? eventToStart,
                shouldFlush);
        }

        internal readonly ConcurrentDictionary<Tuple<string, string>, EventBase> EventsInProgress = new ConcurrentDictionary<Tuple<string, string>, EventBase>();

        internal readonly ConcurrentDictionary<string, List<EventBase>> CompletedEvents = new ConcurrentDictionary<string, List<EventBase>>();

        internal void StartEvent(string requestId, EventBase eventToStart)
        {
            if (!HasReceiver() || string.IsNullOrWhiteSpace(requestId))
            {
                return;
            }

            EventsInProgress[new Tuple<string, string>(requestId, eventToStart[EventBase.EventNameKey])] = eventToStart;
        }

        internal void StopEvent(string requestId, EventBase eventToStop)
        {
            if (!HasReceiver() || string.IsNullOrWhiteSpace(requestId))
            {
                return;
            }

                var eventKey = new Tuple<string, string>(requestId, eventToStop[EventBase.EventNameKey]);

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
                    var events = new List<EventBase>
                    {
                        eventToStop
                    };
                    CompletedEvents[requestId] = events;
                }
                else
                {
                    // if this event shares a RequestId with other events
                    // just add it to the List
                    CompletedEvents[requestId].Add(eventToStop);
                }

                // Mark this event as no longer in progress
                EventsInProgress.TryRemove(eventKey, out var dummy);
        }

        internal void Flush(string requestId, string clientId)
        {
            if (!HasReceiver())
            {
                return;
            }

            if (!CompletedEvents.ContainsKey(requestId))
            {
                // No completed Events returned for RequestId
                return;
            }

            CompletedEvents[requestId].AddRange(CollateOrphanedEvents(requestId));
            CompletedEvents.TryRemove(requestId, out List<EventBase> eventsToFlush);

            bool onlySendFailureTelemetry;
            lock (_lockObj)
            {
                onlySendFailureTelemetry = _telemetryReceiver?.OnlySendFailureTelemetry ?? false;
            }

            if (onlySendFailureTelemetry)
            {
                // iterate over Events, if the ApiEvent was successful, don't dispatch
                bool shouldRemoveEvents = false;

                foreach (var anEvent in eventsToFlush)
                {
                    if (anEvent is ApiEvent apiEvent)
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

            if (eventsToFlush.Count <= 0)
            {
                return;
            }

            eventsToFlush.Insert(0, new DefaultEvent(clientId));

            lock (_lockObj)
            {
                _telemetryReceiver?.HandleTelemetryEvents(eventsToFlush.Cast<Dictionary<string, string>>().ToList());
            }
        }

        private IEnumerable<EventBase> CollateOrphanedEvents(string requestId)
        {
            var orphanedEvents = new List<EventBase>();
            foreach (var key in EventsInProgress.Keys)
            {
                if (key.Item1 == requestId)
                {
                    // The orphaned event already contains its own start time, we simply collect it
                    if (EventsInProgress.TryRemove(key, out var orphan))
                    {
                        orphanedEvents.Add(orphan);
                    }
                }
            }
            return orphanedEvents;
        }

        void ITelemetry.StartEvent(string requestId, EventBase eventToStart)
        {
            StartEvent(requestId, eventToStart);
        }

        void ITelemetry.StopEvent(string requestId, EventBase eventToStop)
        {
            StopEvent(requestId, eventToStop);
        }

        void ITelemetry.Flush(string requestId, string clientId)
        {
            Flush(requestId, clientId);
        }
    }
}