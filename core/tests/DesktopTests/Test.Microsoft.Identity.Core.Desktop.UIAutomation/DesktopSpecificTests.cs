using Microsoft.Identity.AutomationTests;
using Microsoft.Identity.AutomationTests.Configuration;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.Labs;
using System;
using System.Collections.Generic;
using System.Net;

namespace DesktopTests
{
    public class DesktopSpecificTests
    {
        private readonly IAutomationTestAppController _testAppController;
        private readonly ResponseValidator _testValidator;
        private readonly Logger logger;

        public DesktopSpecificTests(Logger logger, IAutomationTestAppController testAppController)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (testAppController == null)
            {
                throw new ArgumentNullException(nameof(testAppController));
            }

            this.logger = logger;
            _testAppController = testAppController;
            _testValidator = new ResponseValidator(logger);

            _testAppController.ClearCache();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public void AcquireTokenByDeviceAuth(ResourceType resource,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = ConfigurationProvider.Instance.GetUser(userAttributes);

            // Get device code
            var request = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resource,
                BrokerType = BrokerType.None,
                User = user
            };
            var deviceCode = _testAppController.GetDeviceCode(request);

            // Sign in via external device
            DeviceCodeSignin.SignIn(user, deviceCode, logger);
            
            // Complete auth process
            var finalAuthResponse = _testAppController.ExecuteAcquireTokenDeviceProfileFlow(request, deviceCode);

            _testValidator.AssertIsValid(request, finalAuthResponse);
        }
        
        public void AquireToken_Using_UserCredentials(ResourceType resource,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = ConfigurationProvider.Instance.GetUser(userAttributes);

            // There should not be any prompt and it should work as silent
            var requestWithPassword = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resource,
                BrokerType = BrokerType.None,
                User = user,
                AdditionalInfo = new Dictionary<string, string>
                {
                    {"user_identifier", user.Upn},
                    {"password", user.GetPassword()}
                }
            };
            var authResponse = _testAppController.ExecuteAcquireTokenNonInteractiveFlow(requestWithPassword);
            _testValidator.AssertIsValid(requestWithPassword, authResponse);

            var requestWithoutPassword = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resource,
                BrokerType = BrokerType.None,
                User = user,
                AdditionalInfo = new Dictionary<string, string>
                {
                    {"user_identifier", user.Upn}
                }
            };
            var silentAuthResponse = _testAppController.ExecuteAcquireTokenSilentFlow(requestWithoutPassword);
            _testValidator.AssertIsValid(requestWithoutPassword, silentAuthResponse);

            // Check both responses are same
            _testValidator.AssertUniqueIdsAreEqual(authResponse.GetResponseAsDictionary(), silentAuthResponse.GetResponseAsDictionary());
            _testValidator.AssertAccessTokensAreEqual(authResponse.GetResponseAsDictionary(), silentAuthResponse.GetResponseAsDictionary());
        }

        public void AquireToken_Using_UserCredentials_Cache(ResourceType resource,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = ConfigurationProvider.Instance.GetUser(userAttributes);

            //There should not be any prompt and it should work as silent
            var requestWithPassword = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resource,
                BrokerType = BrokerType.None,
                User = user,
                AdditionalInfo = new Dictionary<string, string>
                {
                    {"user_identifier", user.Upn},
                    {"password", user.GetPassword()}
                }
            };
            var authResponse = _testAppController.ExecuteAcquireTokenNonInteractiveFlow(requestWithPassword);
            _testValidator.AssertIsValid(requestWithPassword, authResponse);

            // Calling same acquireToken again without password
            requestWithPassword.AdditionalInfo.Remove("password");
            var authResponse2 = _testAppController.ExecuteAcquireTokenNonInteractiveFlow(requestWithPassword);
            _testValidator.AssertIsValid(requestWithPassword, authResponse2);

            _testValidator.AssertUniqueIdsAreEqual(authResponse.GetResponseAsDictionary(),
                authResponse2.GetResponseAsDictionary());
            _testValidator.AssertAccessTokensAreEqual(authResponse.GetResponseAsDictionary(),
                authResponse2.GetResponseAsDictionary());
        }

        public void AquireTokenDomainJoined(ResourceType resource,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = ConfigurationProvider.Instance.GetUser(userAttributes);

            var request = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resource,
                BrokerType = BrokerType.None,
                User = user,
                UseRedirectUri = false
            };
            var response = _testAppController.ExecuteAcquireTokenInteractiveFlow(request);
            _testValidator.AssertIsValid(request, response);
        }

        public void AquireTokenDomainJoinedCache(ResourceType resource,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = ConfigurationProvider.Instance.GetUser(userAttributes);

            var request = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resource,
                BrokerType = BrokerType.None,
                User = user,
                UseRedirectUri = false
            };
            var response1 = _testAppController.ExecuteAcquireTokenInteractiveFlow(request);
            _testValidator.AssertIsValid(request, response1);

            var response2 = _testAppController.ExecuteAcquireTokenInteractiveFlow(request);
            _testValidator.AssertIsValid(request, response1);

            // Save result logs and return to menu
            // TODO: _testAppController.TrySaveLogsAndClickDone();

            _testValidator.AssertUniqueIdsAreEqual(response1.GetResponseAsDictionary(),
                response2.GetResponseAsDictionary());
            _testValidator.AssertAccessTokensAreEqual(response1.GetResponseAsDictionary(),
                response2.GetResponseAsDictionary());
        }

        #region Private

        // TODO: investigate what the requirments for 'cross tenant' tests are
        //private Tenant GetCrossTenantForUser(string username, Tenant tenant)
        //{
        //    //TODO: we need to find the way to read this data from LabData.json

        //    return tenant;
        //}

        #endregion
    }
}