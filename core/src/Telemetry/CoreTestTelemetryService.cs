using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core
{
    internal class CoreTestTelemetryService : ITelemetry
    {

        public void StartEvent(string requestId, EventBase eventToStart) {}

        public void StopEvent(string requestId, EventBase eventToStop) {}
    }
}
