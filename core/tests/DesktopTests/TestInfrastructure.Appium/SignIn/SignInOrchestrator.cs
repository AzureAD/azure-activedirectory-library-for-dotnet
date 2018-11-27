using System;
using System.Linq;
using Microsoft.Identity.AutomationTests.Configuration;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.AutomationTests.Pages;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Microsoft.Identity.AutomationTests.SignIn
{
    public class SignInOrchestrator
    {
        private readonly Logger _logger;
        private readonly DeviceSession _deviceSession;

        public SignInOrchestrator(Logger logger, DeviceSession deviceSession)
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
        }

        public void SignIn(IAuthenticationRequest authRequest)
        {
            bool usernamePrefilled = authRequest.AdditionalInfo != null &&
                                     authRequest.AdditionalInfo.ContainsKey("user_identifier_type") &&
                                     authRequest.AdditionalInfo.ContainsKey("user_identifier");
            var flow = CreateSignInFlow(authRequest.User, authRequest.BrokerType, usernamePrefilled);
            _logger.LogInfo($"SignInOrchestrator: Created '{flow.Name}' sign-in flow");

            // Switch to web context
            _logger.LogInfo("SignInOrchestrator: Switching to web context");
            _deviceSession.SwitchContext(GetWebContext());

            // Execute sign in flow
            _logger.LogInfo("SignInOrchestrator: Executing sign-in flow");
            flow.SignIn(authRequest.User);

            // Switch back to native context
            _logger.LogInfo("SignInOrchestrator: Switching to native context");
            _deviceSession.SwitchContext(DeviceSession.NativeAppContext);
        }

        private ISignInFlow CreateSignInFlow(LabUser user, BrokerType brokerType, bool usernamePrefilled)
        {
            switch (brokerType)
            {
                case BrokerType.None:
                {
                    return user.IsFederated
                        ? CreateFederatedFlow(user.FederationProvider)
                        : CreateManagedFlow(usernamePrefilled);
                }
                case BrokerType.AzureAuthenticator:
                    return new BrokerSignInFlow(_logger, _deviceSession, brokerType);
                case BrokerType.CompanyPortal:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(brokerType), brokerType, null);
            }
        }

        private ISignInFlow CreateManagedFlow(bool usernamePrefilled)
        {
             return new ManagedSignInFlow(_logger, _deviceSession, usernamePrefilled);
        }

        private ISignInFlow CreateFederatedFlow(FederationProvider federationProvider)
        {
            var federatedPage = CreateFederatedPage(federationProvider);

            bool upnEntryRequired = federationProvider == FederationProvider.Shibboleth
                                    || federationProvider == FederationProvider.AdfsV2;
            bool fullUpnSupported = federationProvider != FederationProvider.Shibboleth;


            return new FederatedSignInFlow(_logger, _deviceSession, federatedPage, upnEntryRequired, fullUpnSupported);
        }

        private SignInPage CreateFederatedPage(FederationProvider federationProvider)
        {
            switch (federationProvider)
            {
                case FederationProvider.AdfsV2:
                    return new AdfsV2SignInPage(_logger, _deviceSession);
                case FederationProvider.AdfsV3:
                case FederationProvider.AdfsV4:
                    return new AdfsV3SignInPage(_logger, _deviceSession);
                case FederationProvider.PingFederateV83:
                    return new PingFederateSignInPage(_logger, _deviceSession);
                case FederationProvider.Shibboleth:
                    return new ShibbolethSignInPage(_logger, _deviceSession);
                default:
                    throw new ArgumentOutOfRangeException(nameof(federationProvider), federationProvider, null);
            }
        }

        private string GetWebContext()
        {
            if (_deviceSession.PlatformType == PlatformType.Desktop)
            {
                return null;
            }

            var matchingContexts = _deviceSession.GetWebContexts().ToArray();
            if (_deviceSession.PlatformType == PlatformType.Android)
            {
                matchingContexts = matchingContexts.Where(c => c.EndsWith("automation.testapp")
                                                          && !c.Contains(BrokerType.AzureAuthenticator.BundleId(_deviceSession.PlatformType))
                                                          && !c.Contains(BrokerType.CompanyPortal.BundleId(_deviceSession.PlatformType)))
                                                   .ToArray();
            }

            if (matchingContexts.Length > 1)
            {
                throw new Exception($"Found multiple possible webviews: {string.Join(", ", matchingContexts)}");
            }

            if (!matchingContexts.Any())
            {
                throw new Exception("Could not find a suitable webview");
            }

            return matchingContexts.Single();
        }
    }
}
