using Microsoft.Identity.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core.UI
{
    internal abstract class WebviewBase : IWebUI
    {
        protected static SemaphoreSlim returnedUriReady;
        protected static AuthorizationResult authorizationResult;

        public static void SetAuthorizationResult(AuthorizationResult authorizationResultInput, RequestContext requestContext)
        {
            if (returnedUriReady != null)
            {
                authorizationResult = authorizationResultInput;
                returnedUriReady.Release();
            }
            else
            {
                const string msg = "No pending request for response from web ui.";
                requestContext.Logger.Info(msg);
                requestContext.Logger.InfoPii(msg);
            }
        }

        public abstract Task<AuthorizationResult> AcquireAuthorizationAsync(Uri authorizationUri, Uri redirectUri, RequestContext requestContext);
    }
}
