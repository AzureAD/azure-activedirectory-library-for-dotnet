using Microsoft.Identity.AutomationTests.Model;

namespace Microsoft.Identity.AutomationTests
{
    public interface IAutomationTestAppController
    {
        #region Cache Management

        ReadCacheResponse ReadCache();

        int ExpireUserTokens(AuthenticationRequest authRequest, TokenType tokenToExpire);

        int ClearCache();

        #endregion

        #region Device Code

        DeviceCodeResponse GetDeviceCode(AuthenticationRequest authRequest);

        #endregion

        #region Flows

        AuthenticationResponse ExecuteAcquireTokenInteractiveFlow(AuthenticationRequest authRequest);

        AuthenticationResponse ExecuteAcquireTokenNonInteractiveFlow(AuthenticationRequest authRequest);

        AuthenticationResponse ExecuteAcquireTokenSilentFlow(AuthenticationRequest authRequest);

        AuthenticationResponse ExecuteAcquireTokenDeviceProfileFlow(AuthenticationRequest authRequest, DeviceCodeResponse deviceCode);

        #endregion
    }
}
