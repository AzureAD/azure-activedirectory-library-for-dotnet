using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Microsoft.Identity.Core.UIAutomation.infrastructure
{
    public enum FederationProvider
    {
        Unknown = 0,
        AdfsV2 = 1,
        AdfsV3 = 2,
        AdfsV4 = 3,
        PingFederateV83 = 4,
        Shibboleth = 5
    }
}
