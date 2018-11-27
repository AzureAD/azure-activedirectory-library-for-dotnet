using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Microsoft.Identity.AutomationTests
{
    public class LabUserComparer : IEqualityComparer<LabUser>
    {
        public bool Equals(LabUser x, LabUser y) => x.Upn == y.Upn;

        public int GetHashCode(LabUser obj) => obj.Upn.GetHashCode() * 31;
    }
}
