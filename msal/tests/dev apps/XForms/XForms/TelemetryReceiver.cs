using Microsoft.Applications.Events;
using Microsoft.Identity.Core.Telemetry;
using System;
using System.Collections.Generic;

namespace XForms
{
    public class TelemetryReceiver
    {
        private readonly static string AriaTenantId = "9805a6187b0e4f74a4e93b8ac8ecbe35-ec52c907-a4ac-4a49-82e5-d99b9daa2a96-6973";
        private readonly static string EventNameKey = "msal.event_name";
        private ILogger logger;
        public static string DataBasePath { get; set; }

        public TelemetryReceiver()
        {
            var logConfig = new LogConfiguration
            {
                Memory = new MemoryConfiguration
                {
                    DatabasePath = DataBasePath
                }
            };
            // Aria configuration
            EVTStatus status = 0;
            LogManager.Start(logConfig);
            LogManager.SetNetCost(NetCost.Low);
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
            LogManager.UploadNow();
            LogManager.Teardown();
        }

        public static List<TransmitPolicy> REAL_TIME_FOR_ALL = new List<TransmitPolicy>
        {
            new TransmitPolicy
            {
                ProfileName = "RealTimeForALL",
                Rules = new List<Rules>
                {
                        new Rules
                    {
                        NetCost = NetCost.Unknown, PowerState = PowerState.Unknown,
                        Timers = new Timers { Normal = 1, RealTime = 1 }
                    },
                    new Rules
                    {
                        NetCost = NetCost.Low, PowerState = PowerState.Unknown,
                        Timers = new Timers { Normal = 1, RealTime = 1 }
                    },
                    new Rules
                    {
                        NetCost = NetCost.Low, PowerState = PowerState.Charging,
                        Timers = new Timers { Normal = 1, RealTime = 1 }
                    },
                    new Rules
                    {
                        NetCost = NetCost.Low, PowerState = PowerState.Battery,
                        Timers = new Timers { Normal = 1, RealTime = 1 }
                    },
                    new Rules
                    {
                        NetCost = NetCost.High, PowerState = PowerState.Charging,
                        Timers = new Timers { Normal = 1,RealTime = 1 }
                    },
                    new Rules
                    {
                        NetCost = NetCost.High, PowerState = PowerState.Battery,
                        Timers = new Timers { Normal = 1, RealTime = 1 }
                    },
                    new Rules
                    {
                        NetCost = NetCost.High, PowerState = PowerState.Unknown,
                        Timers = new Timers { Normal = 1, RealTime = 1 }
                    }
                }
            }
        };
    }
}
