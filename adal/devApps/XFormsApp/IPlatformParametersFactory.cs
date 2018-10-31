using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Text;

namespace XFormsApp
{
    public interface IPlatformParametersFactory
    {
        IPlatformParameters GetPlatformParameters(string promptBehavior);
    }
}
