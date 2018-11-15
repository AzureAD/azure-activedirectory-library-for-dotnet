using System;
using System.Collections.Generic;
using Microsoft.Applications.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace TelemetryReceivers
{
    public class TelemetryClientReceiver
    {
        private Microsoft.Applications.Events.ILogger logger;
        private readonly static string EventNameKey = "msal.event_name";
        private readonly static string AriaTenantId = "356c5f7286974ece8d52964f7ad35643-6c8c6db0-888b-446e-a80c-e15e35b8cbcf-7507";
        public TelemetryClientReceiver()
        {
            // Aria configuration
            EVTStatus status = 0;

            var configuration = new LogConfiguration();
            var logLevel = LogLevel.Debug;
            configuration.TraceConfig = new TraceConfiguration(logLevel);
            configuration.TraceConfig.AddProvider(new ConsoleLoggerProvider((text, logLevelProvider) => logLevelProvider >= logLevel, true));

            LogManager.Start(configuration);
            // LogManager.Start(new LogConfiguration());
            LogManager.SetNetCost(NetCost.Low);
            LogManager.LoadTransmitProfiles(REAL_TIME_FOR_ALL);
            LogManager.SetTransmitProfile(REAL_TIME_FOR_ALL[0].ProfileName);
            LogManager.SetPowerState(PowerState.Charging);
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

        private List<TransmitPolicy> REAL_TIME_FOR_ALL = new List<TransmitPolicy>
        {
             new TransmitPolicy
             {
                 ProfileName = "RealTimeForALL",
                 Rules = new List<Rules>
                 {
                         new Rules
                     {
                         NetCost = NetCost.Unknown, PowerState = PowerState.Unknown,
                         Timers = new Timers { Normal = -1, RealTime = -1 }
                     },
                     new Rules
                     {
                         NetCost = NetCost.Low, PowerState = PowerState.Unknown,
                         Timers = new Timers { Normal = -1, RealTime = 10 }
                     },
                     new Rules
                     {
                         NetCost = NetCost.Low, PowerState = PowerState.Charging,
                         Timers = new Timers { Normal = 10, RealTime = 1 }
                     },
                     new Rules
                     {
                         NetCost = NetCost.Low, PowerState = PowerState.Battery,
                         Timers = new Timers { Normal = 30, RealTime = 10 }
                     },
                 }
             }
        };
    }
}

