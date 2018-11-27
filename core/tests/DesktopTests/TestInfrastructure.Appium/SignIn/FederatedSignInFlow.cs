using System;
using System.Threading;
using Microsoft.Identity.AutomationTests.Pages;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Microsoft.Identity.AutomationTests.SignIn
{
    public class FederatedSignInFlow : ISignInFlow
    {
        private readonly Logger _logger;
        private readonly DeviceSession _deviceSession;
        private readonly SignInPage _federatedPage;
        private readonly bool _upnEntryRequired;
        private readonly bool _fullUpnSupported;

        public string Name => $"Federated ({_federatedPage.GetType().Name})";

        public FederatedSignInFlow(Logger logger, DeviceSession deviceSession, SignInPage federatedPage, bool upnEntryRequired, bool fullUpnSupported)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (deviceSession == null)
            {
                throw new ArgumentNullException(nameof(deviceSession));
            }

            if (federatedPage == null)
            {
                throw new ArgumentNullException(nameof(federatedPage));
            }

            _logger = logger;
            _deviceSession = deviceSession;
            _federatedPage = federatedPage;
            _federatedPage.RemoteSignInWindow = "Sign In";
            _upnEntryRequired = upnEntryRequired;
            _fullUpnSupported = fullUpnSupported;
        }

        public void SignIn(LabUser user)
        {
            var aadPage = new AadSignInPage(_logger, _deviceSession);

            aadPage.EnterUsername(user.Upn);
            aadPage.ClickNext();

            //Federated sign in page needs time to load
            Thread.Sleep(3000);

            // Redirection happens after clicking next
            _logger.LogInfo("FederatedSignInFlow: Expecting to be redirected");

            if (_upnEntryRequired)
            {
                var username = _fullUpnSupported ? user.Upn : user.Upn.Split('@')[0];

                _federatedPage.EnterUsername(username);
            }

            _federatedPage.EnterPassword(LabUserHelper.GetUserPassword(user));
            _federatedPage.ClickSignIn();
        }
    }
}
