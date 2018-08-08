using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core
{
    internal class CoreTelemetryService
    {
        public static void InitializeCoreTelemetryService(ITelemetry instance)
        {
            Instance = instance;
        }

        public static ITelemetry Instance { get; private set; }
    }
}
