using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Identity.Core.Telemetry;

namespace XForms.UWP
{
    public class TelemetryReceiver
    {
        public List<Dictionary<string, string>> EventsReceived { get; set; }

        public TelemetryReceiver()
        {
            EventsReceived = new List<Dictionary<string, string>>();
        }

        public void OnEvents(List<Dictionary<string, string>> events)
        {
            EventsReceived = events;  // Only for testing purpose
            Debug.WriteLine("{0} event(s) received", events.Count);
            foreach (var e in events)
            {
                foreach (var entry in e)
                {
                    Debug.WriteLine("  {0}: {1}", entry.Key, entry.Value);
                }
            }
        }
    }
}
