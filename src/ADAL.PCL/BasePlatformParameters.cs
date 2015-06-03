using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    public abstract class BasePlatformParameters : IPlatformParameters
    {
        protected BasePlatformParameters(bool disableOfflineAccess)
        {
            this.DisableOfflineAccess = disableOfflineAccess;
        }

        /// <summary>
        /// Gets or Sets flag to enable logged in user authentication. Note that enabling this flag requires some extra application capabilites.
        /// This flag only works in SSO mode and is ignored otherwise. To enable SSO mode, call AcquireTokenAsync with null or application's callback URI as redirectUri.
        /// </summary>
        public bool DisableOfflineAccess { get; private set; }
    }
}
