using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;

namespace Microsoft.Identity.AutomationTests.Adal.Headless
{
    internal class MockedInteractiveWebUI : WebUI
    {
        private WindowsFormsWebAuthenticationDialogBase dialog;

        protected override AuthorizationResult OnAuthenticate()
        {
            AuthorizationResult result;

            using (this.dialog = new WindowsFormsWebAuthenticationDialog(this.OwnerWindow))
            {
                result = this.dialog.AuthenticateAAD(this.RequestUri, this.CallbackUri);
            }

            return result;
        }
    }
}
