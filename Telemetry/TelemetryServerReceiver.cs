extern alias Server;

using System;
using System.Collections.Generic;
using System.Text;
using Server::Microsoft.Applications.Events;

namespace TelemetryReceivers
{
    public class TelemetryServerReceiver
    {
        private Server::Microsoft.Applications.Events.ILogger logger;
        private readonly static string EventNameKey = "msal.event_name";
        private readonly static string AriaTenantId = "356c5f7286974ece8d52964f7ad35643-6c8c6db0-888b-446e-a80c-e15e35b8cbcf-7507";
        public TelemetryServerReceiver()
        {
            EVTStatus status = 0;
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
            LogManager.UploadNow(); 
            LogManagerProvider.DestroyLogManager(AriaTenantId);
        }
    }
}
