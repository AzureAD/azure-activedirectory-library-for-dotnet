using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Identity.AutomationTests.Configuration
{
    /// <summary>
    /// Holds the configuration of a device, including the settings needed to configure Appium and Olympus
    /// </summary>
    public class DeviceConfiguration
    {
        /// <summary>
        /// Creates a device config based on a config
        /// </summary>
        /// <remarks>Properties must have public getters and setters for biding to work. Dictionaries must have key values</remarks>
        public DeviceConfiguration(IConfigurationSection section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            section.Bind(this);

            if (this.ServerUrl == null)
            {
                throw new InvalidOperationException("Could not find a setting for 'serverUrl'");
            }

            string appLocation = this.DeviceCapabilities["app"].ToString();
            if (string.IsNullOrWhiteSpace(appLocation))
            {
                throw new InvalidOperationException("Could not find a setting for 'deviceCapabilities:app'");
            }
        }

        /// <summary>
        /// The Url to the Appium server.
        /// </summary>
        public Uri ServerUrl { get; set; }

        /// <summary>
        /// This dictionary is used to initialize the Appium Driver.
        /// </summary>
        public Dictionary<string, object> DeviceCapabilities { get; set; }

        /// <summary>
        /// The timeout, in seconds, for how long Appium will wait for any command.
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        /// Default timeout, in seconds, for finding a UI element.
        /// </summary>
        public int FindElementsTimeout { get; set; }

        /// <summary>
        /// Indicates how frequently, in seconds, an attempt is made to find an element.
        /// </summary>
        public int FindElementPollInterval { get; set; }

        /// <summary>
        /// The timeout, in seconds, for how long we should wait looking for an applicable WebView context.
        /// </summary>
        public int WebViewContextSearchTimeout { get; set; }

        /// <summary>
        /// In some emulator and SDK configurations keyboard hiding fails - setting this property to true will instruct the tests to not hide the keyboard
        /// </summary>
        public bool CanHideKeyboard { get; set; }

        /// <summary>
        /// The location to install various broker apps (i.e. CompanyPortal, AzureAuthenticator) from, if required for the test.
        /// </summary>
        public Dictionary<string, string> BrokerAppPaths { get; set; }
    }
}
