using System;
using System.Linq;
using Microsoft.Identity.AutomationTests.Configuration;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Microsoft.Identity.AutomationTests
{
    public class MamTests
    {
        private readonly Logger _logger;
        private readonly DeviceSession _deviceSession;
        private readonly IAutomationTestAppController _testAppController;
        private readonly ResponseValidator _responseValidator;
        private readonly PlatformType _currentAutomationType;

        public MamTests(Logger logger, DeviceSession deviceSession)
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
            _testAppController = new AutomationTestAppController(logger, deviceSession);
            _responseValidator = new ResponseValidator(logger);

            _currentAutomationType = deviceSession.PlatformType;

            _testAppController.ClearCache();
        }

        //TestCase 6367 : MAM Enrollment Flow (Android)
        public void MamRegisterTest(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userQuery)
        {
            var user = LabUserHelper.GetLabUserData(userQuery).User;

            CleanBrokerInstalls();

            InstallBroker(BrokerType.AzureAuthenticator);

            var request = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.AzureAuthenticator,
                User = user
            };
            var response = _testAppController.ExecuteAcquireTokenInteractiveFlow(request);
            _responseValidator.AssertIsValid(request, response);
        }

        public void AcquireTokenInteractiveAndSilentTest(
            ResourceType resource1,
            ResourceType resource2,
            ApplicationType application,
            UserQueryParameters userQuery)
        {
            var user = LabUserHelper.GetLabUserData(userQuery).User;

            var resource1Request = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resource1,
                BrokerType = BrokerType.AzureAuthenticator,
                User = user
            };
            var resource1Response = _testAppController.ExecuteAcquireTokenInteractiveFlow(resource1Request);
            _responseValidator.AssertIsValid(resource1Request, resource1Response);

            var resource2Request = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resource2,
                BrokerType = BrokerType.AzureAuthenticator,
                User = user
            };
            var resource2Response = _testAppController.ExecuteAcquireTokenSilentFlow(resource2Request);
            _responseValidator.AssertIsValid(resource2Request, resource2Response);
        }

        private void CleanBrokerInstalls()
        {
            //make sure no broker apps intalled on the device
            if (IsBrokerAppInstalled(BrokerType.AzureAuthenticator))
            {
                RemoveBrokerApp(BrokerType.AzureAuthenticator);
                _logger.LogInfo($"Uninstalled broker: {BrokerType.AzureAuthenticator}");
            }

            if (IsBrokerAppInstalled(BrokerType.CompanyPortal))
            {
                RemoveBrokerApp(BrokerType.CompanyPortal);
                _logger.LogInfo($"Uninstalled broker: {BrokerType.CompanyPortal}");
            }

            foreach (var brokerType in Enum.GetValues(typeof(BrokerType)).Cast<BrokerType>())
            {
                if (brokerType != BrokerType.None)
                {
                    Assert.IsFalse(IsBrokerAppInstalled(brokerType), "There should be no broker installations remaining.");
                }
            }

            _logger.LogInfo("Cleaned all broker installations.");
        }

        private bool IsBrokerAppInstalled(BrokerType type)
        {
            return _deviceSession.AppiumDriver.IsAppInstalled(type.BundleId(_currentAutomationType));
        }

        private void InstallBroker(BrokerType brokerType)
        {
            _deviceSession.InstallBrokerApp(brokerType);
            Assert.IsTrue(IsBrokerAppInstalled(brokerType));
            _logger.LogInfo($"Installed broker: {brokerType}");
        }

        private void RemoveBrokerApp(BrokerType type)
        {
            switch (type)
            {
                case BrokerType.AzureAuthenticator:
                    _deviceSession.AppiumDriver.RemoveApp(BrokerType.AzureAuthenticator.BundleId(_currentAutomationType));
                    break;
                case BrokerType.CompanyPortal:
                    _deviceSession.AppiumDriver.RemoveApp(BrokerType.CompanyPortal.BundleId(_currentAutomationType));
                    break;
                default:
                    throw new InvalidOperationException("Can only remove Azure Authenticator or Company Portal brokers.");
            }
        }
    }
}