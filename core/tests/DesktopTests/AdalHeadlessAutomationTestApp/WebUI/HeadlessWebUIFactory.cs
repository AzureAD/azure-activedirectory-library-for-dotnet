using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;

namespace Microsoft.Identity.AutomationTests.Adal.Headless
{
    internal class HeadlessWebUIFactory : IWebUIFactory
    {
        private PlatformParameters parameters;

        public IWebUI CreateAuthenticationDialog(IPlatformParameters inputParameters)
        {
            this.parameters = inputParameters as PlatformParameters;
            if (this.parameters == null)
            {
                throw new ArgumentException("parameters should be of type PlatformParameters", "inputParamters");
            }

            switch (this.parameters.PromptBehavior)
            {
                case PromptBehavior.Auto:
                    // Some of the non-interactive flow tests pass in "Auto". 
                    // We should only be called for flows that don't require user interaction, so
                    // we'll return the product SilentWebUI here.
                    // If the flow doesn't actually require any interaction then it will work.
                    // If the flow does require interaction then the SilentWebUI will throw and the test will fail.
                    return new SilentWebUI { OwnerWindow = this.parameters.OwnerWindow };

                case PromptBehavior.Always:
                case PromptBehavior.RefreshSession:
                    throw new NotSupportedException("The ADAL UI-less desktop test application only supports non-interactive logins");

                case PromptBehavior.Never:
                    // Return the real (product) SilentWebUI window
                    return new SilentWebUI { OwnerWindow = this.parameters.OwnerWindow };
                default:
                    throw new InvalidOperationException("Unexpected PromptBehavior value");
            }
        }
    }
}
