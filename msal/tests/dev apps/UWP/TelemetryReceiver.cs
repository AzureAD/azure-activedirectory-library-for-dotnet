using Microsoft.Applications.Telemetry.Windows;
using Microsoft.Identity.Core.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UWP
{
    class TelemetryReceiver
    {
        private ILogger logger;
        private readonly static string EventNameKey = "msal.event_name";

        public TelemetryReceiver()
        {
            logger = LogManager.Initialize(tenantToken: "356c5f7286974ece8d52964f7ad35643-6c8c6db0-888b-446e-a80c-e15e35b8cbcf-7507");
        }

        public void OnEvents(List<Dictionary<string, string>> events)
        {
            Debug.WriteLine("{0} event(s) received", events.Count);
            foreach (var e in events)
            {
                Debug.WriteLine("Event: {0}", e[EventNameKey]);
                var eventData = new EventProperties(e[EventNameKey]);
                foreach (var entry in e)
                {
                    eventData.SetProperty(entry.Key, entry.Value);
                    Debug.WriteLine("  {0}: {1}", entry.Key, entry.Value);
                }
                logger.LogEvent(eventData);
            }
            LogManager.FlushAndTeardown();
        }
    }
}
