using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core
{
    internal class CoreTestTelemetry : iTelemetry
    {
        public iTelemetry GetInstance()
        {
            return this;
        }

        public void StartEvent(string requestId, EventBase eventToStart)
        {
            
        }

        public void StopEvent(string requestId, EventBase eventToStop)
        {
            
        }
    }
}
