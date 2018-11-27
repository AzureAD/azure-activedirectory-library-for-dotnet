using System.Collections.Generic;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Microsoft.Identity.AutomationTests.Model
{
    public interface IAuthenticationRequest
    {
        BrokerType BrokerType { get; }

        LabUser User { get; }

        IDictionary<string, string> AdditionalInfo { get; }
    }
}
