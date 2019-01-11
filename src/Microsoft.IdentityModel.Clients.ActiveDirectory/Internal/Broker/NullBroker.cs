using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Core.Cache;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Broker
{
    /// <summary>
    /// For platforms that do not support a broker (net desktop, net core, UWP, netstandard)
    /// </summary>
    internal class NullBroker : IBroker
    {
        public bool CanInvokeBroker => false;

        public IPlatformParameters PlatformParameters
        {
           get; set;
        }

        public Task<AdalResultWrapper> AcquireTokenUsingBrokerAsync(IDictionary<string, string> brokerPayload)
        {
            throw new NotImplementedException();
        }
    }
}
