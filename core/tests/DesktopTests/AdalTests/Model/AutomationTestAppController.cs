using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Identity.AutomationTests.Configuration;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.AutomationTests.Pages;
using Microsoft.Identity.AutomationTests.SignIn;
using Microsoft.Identity.Labs;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Identity.AutomationTests
{
    public class AutomationTestAppController : IAutomationTestAppController
    {
        private readonly Logger _logger;
        private readonly DeviceSession _deviceSession;
        private readonly SignInOrchestrator _signInOrchestrator;
        private readonly AutomationTestApp _testApp;
        private readonly ISet<IUser> _usersWithCachedTokens = new HashSet<IUser>(new LabUserComparer());
        //This delay is needed after hitting the go button to give the authentication flow time to complete, otherwise, the driver will fail to find the next element.
        private const int AuthenticationDelayTime = 6000;

        public AutomationTestAppController(Logger logger, DeviceSession deviceSession)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (deviceSession == null)
            {
                throw new ArgumentNullException(nameof(deviceSession));
            }

            _logger = logger;
            _deviceSession = deviceSession;
            _testApp = new AutomationTestApp(logger, deviceSession);
            _signInOrchestrator = new SignInOrchestrator(_logger, _deviceSession);
        }

        #region Cache Management

        public ReadCacheResponse ReadCache()
        {
            _logger.LogInfo("TestAppController: Reading cache");
            _testApp.ClickReadCacheButton();

            return CreateReadCacheResponse();
        }

        public int ExpireUserTokens(AuthenticationRequest authRequest, TokenType tokenToExpire)
        {
            _logger.LogInfo($"TestAppController: Expiring user tokens ({tokenToExpire})");

            var inputData = GenerateInputData(authRequest);
            if (!inputData.ContainsKey("user_identifier"))
            {
                inputData.Add("user_identifier", authRequest.User.Upn);
            }

            switch (tokenToExpire)
            {
                case TokenType.AccessToken:
                    _testApp.ClickExpireAccessTokenButton();
                    break;
                case TokenType.RefreshToken:
                    _testApp.ClickInvalidateRefreshTokenButton();
                    break;
                case TokenType.FamilyRefreshToken:
                    _testApp.ClickInvalidateFamilyRefreshTokenButton();
                    break;
            }

            _testApp.EnterInputData(inputData);
            _testApp.ClickGoButton();

            // return number of expired tokens
            var invalidateResponse = CreateInvalidateTokenResponse(tokenToExpire);
            int invalidatedTokens = invalidateResponse.GetTokensInvalidated();

            Assert.AreNotEqual(0, invalidatedTokens, "Expected to invalidate a token, but no tokens matched the input data.");
            return invalidatedTokens;
        }

        public int ClearCache()
        {
            _logger.LogInfo("TestAppController: Clearing cache");

            _testApp.ClickClearCacheButton();
            _testApp.ClickDoneButton();

            _usersWithCachedTokens.Clear();
            _logger.LogInfo("TestAppController: Signed in user collection has been cleared");

            // TODO: get number of cleared tokens
            return 0;
        }

        #endregion

        #region Device Code

        public DeviceCodeResponse GetDeviceCode(AuthenticationRequest authRequest)
        {
            _logger.LogInfo("TestAppController: Getting device code");

            var inputData = GenerateInputData(authRequest);

            _testApp.ClickAcquireTokenDeviceCodeButton();
            _testApp.EnterInputData(inputData);
            _testApp.ClickGoButton();

            return CreateDeviceCodeResponse();
        }

        #endregion

        #region Flows

        public AuthenticationResponse ExecuteAcquireTokenInteractiveFlow(AuthenticationRequest authRequest)
        {
            _logger.LogInfo("TestAppController: Executing AcquireToken (interactive) flow");

            var inputData = GenerateInputData(authRequest);

            _testApp.ClickAcquireTokenButton();
            _testApp.EnterInputData(inputData);
            _testApp.ClickGoButton();

            //This delay is needed after hitting the go button to give the authentication flow time to complete, otherwise, the driver will fail to find the next element.
            Thread.Sleep(AuthenticationDelayTime);

            _signInOrchestrator.SignIn(authRequest);

            var result = CreateAuthenticationResponse();
            UpdateUserCache(authRequest, result);
            return result;
        }

        public AuthenticationResponse ExecuteAcquireTokenNonInteractiveFlow(AuthenticationRequest authRequest)
        {
            _logger.LogInfo("TestAppController: Executing AcquireToken (non-interactive) flow");

            var inputData = GenerateInputData(authRequest);

            _testApp.ClickAcquireTokenButton();
            _testApp.EnterInputData(inputData);
            _testApp.ClickGoButton();

            //This delay is needed after hitting the go button to give the authentication flow time to complete, otherwise, the driver will fail to find the next element.
            Thread.Sleep(AuthenticationDelayTime);

            // user cache will update only if non-interactive sign-in was conducted using user creds given by parameter
            var result = CreateAuthenticationResponse();
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

            _testApp.ClickAcquireTokenSilentButton();
            _testApp.EnterInputData(inputData);
            _testApp.ClickGoButton();

            //This delay is needed after hitting the go button to give the authentication flow time to complete, otherwise, the driver will fail to find the next element.
            Thread.Sleep(AuthenticationDelayTime);

            return CreateAuthenticationResponse();
        }

        public AuthenticationResponse ExecuteAcquireTokenDeviceProfileFlow(AuthenticationRequest authRequest, DeviceCodeResponse deviceCode)
        {
            _logger.LogInfo("TestAppController: Executing AcquireTokenDeviceProfile flow");

            var inputData = GenerateInputData(authRequest);
            inputData.Merge(deviceCode.GetResponseAsDictionary().ToStringDictionary());

            _testApp.ClickAcquireTokenDeviceProfileButton();
            _testApp.EnterInputData(inputData);
            _testApp.ClickGoButton();

            //This delay is needed after hitting the go button to give the authentication flow time to complete, otherwise, the driver will fail to find the next element.
            Thread.Sleep(AuthenticationDelayTime);

            var result = CreateAuthenticationResponse();
            UpdateUserCache(authRequest, result);
            return result;
        }

        #endregion

        #region Private

        private IDictionary<string, string> GenerateInputData(AuthenticationRequest authRequest)
        {
            var application = ConfigurationProvider.Instance.GetApplication(authRequest.ApplicationType);
            var resource = ConfigurationProvider.Instance.GetResource(authRequest.ResourceType);

            // Determine user's authority
            // If the user already has token in the cache for ADAL .NET, we need to use that user’s resolved authority
            if (_deviceSession.PlatformType == PlatformType.Desktop &&
                _usersWithCachedTokens.Contains(authRequest.User))
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

        private AuthenticationResponse CreateAuthenticationResponse()
        {
            // Capture auth response and library logs
            var authResponse = _testApp.GetResult();
            var resultLogs = _testApp.GetResultLogs();

            // Return to main menu
            _testApp.ClickDoneButton();

            return new AuthenticationResponse(authResponse, resultLogs);
        }

        private DeviceCodeResponse CreateDeviceCodeResponse()
        {
            // Capture auth response and library logs
            var codeResponse = _testApp.GetResult();
            var resultLogs = _testApp.GetResultLogs();

            // Return to main menu
            _testApp.ClickDoneButton();

            return new DeviceCodeResponse(codeResponse, resultLogs);
        }

        private InvalidateTokenResponse CreateInvalidateTokenResponse(TokenType tokenType)
        {
            // Capture auth response and library logs
            var codeResponse = _testApp.GetResult();
            var resultLogs = _testApp.GetResultLogs();

            // Return to main menu
            _testApp.ClickDoneButton();

            return new InvalidateTokenResponse(codeResponse, resultLogs, tokenType);
        }

        private ReadCacheResponse CreateReadCacheResponse()
        {
            // Capture auth response and library logs
            var codeResponse = _testApp.GetResult();
            var resultLogs = _testApp.GetResultLogs();

            // Return to main menu
            _testApp.ClickDoneButton();

            return new ReadCacheResponse(codeResponse, resultLogs);
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
