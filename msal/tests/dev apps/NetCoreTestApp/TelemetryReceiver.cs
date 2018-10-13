using System;
using System.Collections.Generic;
using Microsoft.Identity.Core.Telemetry;

namespace NetCoreTestApp
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
            Console.WriteLine("{0} event(s) received", events.Count);
            foreach (var e in events)
            {
                Console.WriteLine("Event: {0}", e[EventBase.EventNameKey]);
                foreach (var entry in e)
                {
                    Console.WriteLine("  {0}: {1}", entry.Key, entry.Value);
                }
            }
        }
    }
}
