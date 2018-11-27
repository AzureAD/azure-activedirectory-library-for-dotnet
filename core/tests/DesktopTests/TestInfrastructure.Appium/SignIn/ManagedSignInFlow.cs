using System;
using System.Threading;
using Microsoft.Identity.AutomationTests.Pages;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Microsoft.Identity.AutomationTests.SignIn
{
    public class ManagedSignInFlow : ISignInFlow
    {
        private readonly DeviceSession _deviceSession;
        private readonly Logger _logger;

        private readonly bool _usernamePrefilled;

        public string Name => "Managed";

        public ManagedSignInFlow(Logger logger, DeviceSession deviceSession, bool usernamePrefilled = false)
        {
            if (deviceSession == null)
            {
                throw new ArgumentNullException(nameof(deviceSession));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _deviceSession = deviceSession;
            _logger = logger;

            _usernamePrefilled = usernamePrefilled;
        }

        public void SignIn(LabUser user)
        {
            var page = new AadSignInPage(_logger, _deviceSession);

            if (!_usernamePrefilled)
            {
                //Need to wait for window to load. If appium tries to search for a window that does not exist, it has a very 
                //long time out before trying again. Attempts to change this timeout have failed. Needs investigation or test hook
                Thread.Sleep(3000);
                page.EnterUsername(user.Upn);
                page.ClickNext();
            }

            page.EnterPassword(LabUserHelper.GetUserPassword(user));
            page.ClickSignIn();
        }
    }
}
