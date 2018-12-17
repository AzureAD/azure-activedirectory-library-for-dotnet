using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Core.Cache;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Broker
{
    internal class NullBroker : IBroker
    {
        public bool CanInvokeBroker => false;

        public IPlatformParameters PlatformParameters
        {
            get => throw new NotImplementedException();
            set
            {
                // NO-OP
            }
        }

        public Task<AdalResultWrapper> AcquireTokenUsingBrokerAsync(IDictionary<string, string> brokerPayload)
        {
            throw new NotImplementedException();
        }
    }
}
