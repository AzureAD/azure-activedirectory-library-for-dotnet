using Microsoft.Applications.Telemetry.Windows;
using Microsoft.Identity.Core.Telemetry;
using System;
using System.Collections.Generic;


namespace AutomationApp
{
    class TelemetryReceiver
    {
        private ILogger logger;

        public TelemetryReceiver()
        {

            logger = LogManager.Initialize(tenantToken: "356c5f7286974ece8d52964f7ad35643-6c8c6db0-888b-446e-a80c-e15e35b8cbcf-7507");
        }

        public void OnEvents(List<Dictionary<string, string>> events)
        {
            foreach (var e in events)
            {
                var eventData = new EventProperties(e[EventBase.EventNameKey]);
                foreach (var entry in e)
                {
                    eventData.SetProperty(entry.Key, entry.Value);
                }
                logger.LogEvent(eventData);
            }
            LogManager.FlushAndTeardown();
        }
    }
}
