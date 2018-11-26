using System.Collections.Generic;
using Microsoft.Identity.Labs;

namespace Microsoft.Identity.AutomationTests.Model
{
    public interface IAuthenticationRequest
    {
        BrokerType BrokerType { get; }

        IUser User { get; }

        IDictionary<string, string> AdditionalInfo { get; }
    }
}
