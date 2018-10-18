using System;
using System.Collections.Generic;
using Microsoft.Applications.Events;
using Microsoft.Identity.Core.Telemetry;

namespace WebApi.Utils
{
    public class TelemetryReceiver
    {
        private ILogger logger;
        private readonly static string EventNameKey = "msal.event_name";
        private readonly static string AriaTenantId = "356c5f7286974ece8d52964f7ad35643-6c8c6db0-888b-446e-a80c-e15e35b8cbcf-7507";

        public TelemetryReceiver()
        {

            // Aria configuration
            EVTStatus status = 0;
            ILogManager myLogManager = LogManagerProvider.CreateLogManager(AriaTenantId, out status, true, new LogConfiguration());
            LogManager.Start(new LogConfiguration());
            logger = LogManager.GetLogger(AriaTenantId, out status);
        }

        public void OnEvents(List<Dictionary<string, string>> events)
        {
            
            Console.WriteLine("{0} event(s) received", events.Count);
            foreach (var e in events)
            {
                Console.WriteLine("Event: {0}", e[EventNameKey]);
                var eventData = new EventProperties();
                eventData.Name = e[EventNameKey];
                foreach (var entry in e)
                {
                    eventData.SetProperty(entry.Key, entry.Value);
                    Console.WriteLine("  {0}: {1}", entry.Key, entry.Value);
                }
                EVTStatus result = logger.LogEvent(eventData);
            }
            LogManagerProvider.DestroyLogManager(AriaTenantId);
        }
    }
}
