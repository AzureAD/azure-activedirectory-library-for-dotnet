using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Labs;

namespace Microsoft.Identity.AutomationTests
{
    public class LabUserComparer : IEqualityComparer<IUser>
    {
        public bool Equals(IUser x, IUser y) => x.Upn == y.Upn;

        public int GetHashCode(IUser obj) => obj.Upn.GetHashCode() * 31;
    }
}
