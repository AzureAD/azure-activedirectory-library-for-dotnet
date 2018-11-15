using System.Threading.Tasks;
using Microsoft.Identity.Core.UI;

namespace Microsoft.Identity.Client.Internal.UI
{
    internal interface IHttpListener
    {
        Task<AuthorizationResult> WaitForCallbackAsync(int timeoutInSeconds = 300);
    }
}