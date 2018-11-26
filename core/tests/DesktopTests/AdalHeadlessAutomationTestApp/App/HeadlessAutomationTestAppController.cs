using Microsoft.Identity.AutomationTests.Configuration;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.Labs;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;
using WinFormsAutomationApp;

namespace Microsoft.Identity.AutomationTests.Adal.Headless
{
    public class HeadlessAutomationTestAppController : IAutomationTestAppController
    {
        private readonly Logger _logger;
        private readonly LoggerCallbackImpl loggerCallback = new LoggerCallbackImpl();

        private readonly ISet<IUser> _usersWithCachedTokens = new HashSet<IUser>(new LabUserComparer());


        public HeadlessAutomationTestAppController(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Override the product web factory with our own one that will handle
            // handle the interactive client-side behaviours without showing a UI.
            WebUIFactoryProvider.WebUIFactory = new HeadlessWebUIFactory();

            //DeleteCache.CleanCookies();

            loggerCallback.GetAdalLogs();
        }

        #region IAutomationTestAppController methods

        public int ClearCache()
        {
            _logger.LogInfo("TestAppController: Clearing cache");

            AuthenticationHelper.ClearCache(null).Wait();

            _usersWithCachedTokens.Clear();
            _logger.LogInfo("TestAppController: Signed in user collection has been cleared");

            // TODO: get number of cleared tokens
            return 0;
        }

        public AuthenticationResponse ExecuteAcquireTokenDeviceProfileFlow(AuthenticationRequest authRequest, DeviceCodeResponse deviceCode)
        {
            _logger.LogInfo("TestAppController: Executing AcquireTokenDeviceProfile flow");

            var inputData = GenerateInputData(authRequest);
            inputData.Merge(deviceCode.GetResponseAsDictionary().ToStringDictionary());

            string result = AuthenticationHelper.AcquireTokenUsingDeviceProfile(inputData).Result;

            return CreateAuthenticationResponse(result);
        }

        public AuthenticationResponse ExecuteAcquireTokenInteractiveFlow(AuthenticationRequest authRequest)
        {
            _logger.LogInfo("TestAppController: Executing AcquireToken (interactive) flow");

            Assert.Inconclusive("Test setup: mocked Web UI not yet implemented");

            return null;
        }

        public AuthenticationResponse ExecuteAcquireTokenNonInteractiveFlow(AuthenticationRequest authRequest)
        {
            _logger.LogInfo("TestAppController: Executing AcquireToken (non-interactive) flow");

            var inputData = GenerateInputData(authRequest);

            string response = AuthenticationHelper.AcquireToken(inputData).Result;

            // user cache will update only if non-interactive sign-in was conducted using user creds given by parameter
            var result = CreateAuthenticationResponse(response);

            if (authRequest.AdditionalInfo != null && authRequest.AdditionalInfo.ContainsKey("user_identifier"))
            {
                UpdateUserCache(authRequest, result);
            }
            return result;
        }

        public AuthenticationResponse ExecuteAcquireTokenSilentFlow(AuthenticationRequest authRequest)
        {
            _logger.LogInfo("TestAppController: Executing AcquireTokenSilent flow");

            var inputData = GenerateInputData(authRequest);

            string response = AuthenticationHelper.AcquireTokenSilent(inputData).Result;

            return CreateAuthenticationResponse(response);
        }

        public int ExpireUserTokens(AuthenticationRequest authRequest, TokenType tokenToExpire)
        {
            _logger.LogInfo($"TestAppController: Expiring user tokens ({tokenToExpire})");

            var inputData = GenerateInputData(authRequest);
            if (!inputData.ContainsKey("user_identifier"))
            {
                inputData.Add("user_identifier", authRequest.User.Upn);
            }

            string response = null;
            switch (tokenToExpire)
            {
                case TokenType.AccessToken:
                    response = AuthenticationHelper.ExpireAccessToken(inputData).Result;
                    break;

                case TokenType.RefreshToken:
                    response = AuthenticationHelper.InvalidateRefreshToken(inputData).Result;
                    break;

                case TokenType.FamilyRefreshToken:
                    throw new NotImplementedException();

                    //response = AuthenticationHelper.TODO(inputData).Result;
                    //_testApp.ClickInvalidateFamilyRefreshTokenButton();
                    break;
            }

            // return number of expired tokens
            var invalidateResponse = CreateInvalidateTokenResponse(response, tokenToExpire);
            int invalidatedTokens = invalidateResponse.GetTokensInvalidated();

            Assert.AreNotEqual(0, invalidatedTokens, "Expected to invalidate a token, but no tokens matched the input data.");
            return invalidatedTokens;
        }

        public DeviceCodeResponse GetDeviceCode(AuthenticationRequest authRequest)
        {

            IDictionary<string, string> inputData = GenerateInputData(authRequest);
            string codeResponse = AuthenticationHelper.AcquireDeviceCode(inputData).Result;

            return CreateDeviceCodeResponse(codeResponse);
        }

        public ReadCacheResponse ReadCache()
        {
            _logger.LogInfo("TestAppController: Reading cache");

            var result = AuthenticationHelper.ReadCache().Result;

            return CreateReadCacheResponse(result);
        }

        #endregion

        #region Private


        private Dictionary<string, string> GenerateInputData(AuthenticationRequest authRequest)
        {
            // Copied from AutomationTestAppController.cs

            var application = ConfigurationProvider.Instance.GetApplication(authRequest.ApplicationType);
            var resource = ConfigurationProvider.Instance.GetResource(authRequest.ResourceType);

            // Determine user's authority
            // If the user already has token in the cache for ADAL .NET, we need to use that user’s resolved authority
            if (_usersWithCachedTokens.Contains(authRequest.User))
            {
                authRequest.Authority =
                    AuthenticationRequest.CreateSpecificAuthorityUriFromGuid(authRequest.User.CurrentTenantId);
            }

            // Get redirect URI
            var redirectUri = application.RedirectUris[RedirectUriType.Default.ToString()].ToString();

            // Generate the core input data
            var result = new Dictionary<string, string>
            {
                {"authority", authRequest.Authority},
                {"client_id", application.ClientId},
                {"redirect_uri", redirectUri},
                {"resource", resource.RequestUrl}
            };

            // Remove redirect URI if requested
            if (!authRequest.UseRedirectUri)
            {
                result.Remove("redirect_uri");
            }

            // Switch to broker if one is requested
            if (authRequest.BrokerType != BrokerType.None)
            {
                result.Add("use_broker", true.ToString());
            }

            // Add prompt behavior
            result.Add("prompt_behavior", authRequest.PromptBehavior.GetValueOrDefault(PromptBehavior.Auto).ToString());

            // Merge any additional information if set
            if (authRequest.AdditionalInfo?.Any() ?? false)
            {
                result.Merge(authRequest.AdditionalInfo);
            }

            return result;
        }

        private AuthenticationResponse CreateAuthenticationResponse(string response)
        {
            return new AuthenticationResponse(response, loggerCallback.GetAdalLogs());
        }

        private DeviceCodeResponse CreateDeviceCodeResponse(string response)
        {
            // Capture auth response and library logs
            return new DeviceCodeResponse(response, loggerCallback.GetAdalLogs());
        }

        private InvalidateTokenResponse CreateInvalidateTokenResponse(string response, TokenType tokenType)
        {
            // Capture auth response and library logs
            return new InvalidateTokenResponse(response, loggerCallback.GetAdalLogs(), tokenType);
        }

        private ReadCacheResponse CreateReadCacheResponse(string response)
        {
            // Capture auth response and library logs
            return new ReadCacheResponse(response, loggerCallback.GetAdalLogs());
        }

        private void UpdateUserCache(AuthenticationRequest request, AuthenticationResponse authResponse)
        {
            if (authResponse.IsSuccess)
            {
                _logger.LogInfo($"TestAppController: User '{request.User.Upn}' has been added to collection of signed in users");
                _usersWithCachedTokens.Add(request.User);
            }
        }

        #endregion
    }
}
