using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core
{
    internal interface ITelemetry
    {
        void StartEvent(string requestId, EventBase eventToStart);

        void StopEvent(string requestId, EventBase eventToStop);
    }
}
