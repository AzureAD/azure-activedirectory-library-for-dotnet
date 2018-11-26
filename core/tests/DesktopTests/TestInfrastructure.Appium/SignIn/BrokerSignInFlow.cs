using System;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.Identity.Labs;

namespace Microsoft.Identity.AutomationTests.SignIn
{
    public class BrokerSignInFlow : ISignInFlow
    {
        private readonly Logger _logger;
        private readonly DeviceSession _deviceSession;
        private readonly BrokerType _brokerType;

        public string Name => $"Broker ({_brokerType})";

        public BrokerSignInFlow(Logger logger, DeviceSession deviceSession, BrokerType brokerType)
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
            _brokerType = brokerType;
        }

        public void SignIn(IUser user)
        {
            // TODO: ensure context switching is done correctly.
            throw new NotImplementedException();
        }
    }
}