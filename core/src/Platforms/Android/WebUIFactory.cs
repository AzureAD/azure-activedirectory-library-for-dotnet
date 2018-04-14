using Microsoft.Identity.Core.UI.SystemWebview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core.UI
{
    [Android.Runtime.Preserve(AllMembers = true)]
    internal class WebUIFactory : IWebUIFactory
    {
        public IWebUI CreateAuthenticationDialog(CoreUIParent coreUIParent, RequestContext requestContext)
        {
            if (coreUIParent.UseEmbeddedWebview)
            {
                //return new EmbeddedWebUI(coreUIParent)
                //{
                //    RequestContext = requestContext
                //};
            }

            return new SystemWebUI(coreUIParent)
            {
                RequestContext = requestContext
            };
        }
    }
}
