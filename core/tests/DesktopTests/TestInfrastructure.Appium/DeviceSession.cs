using Microsoft.Identity.AutomationTests.Configuration;
using Microsoft.Identity.AutomationTests.Model;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Support.UI;
using PlatformType = Microsoft.Identity.AutomationTests.Model.PlatformType;

namespace Microsoft.Identity.AutomationTests
{
    /// <summary>
    /// Represents a connection to- and interaction with a device (or simulator/emulator).
    /// </summary>
    public class DeviceSession : IDisposable
    {
        private const string OlympusSessionIdKeyName = "olympusSessionId";
        private const int InitializationRetryCount = 3;

        public const string NativeAppContext = "NATIVE_APP";

        private readonly Logger _logger;
        private bool _isDisposed;

        /// <summary>
        /// Type of automation device deviceSession.
        /// </summary>
        public PlatformType PlatformType { get; }

        /// <summary>
        /// The unique identifier of the current deviceSession.
        /// </summary>
        private Guid _sessionId;

        /// <summary>
        /// The <see cref="DeviceConfiguration"/> used to create the current deviceSession.
        /// </summary>
        public DeviceConfiguration Configuration { get; private set; }

        /// <summary>
        /// The underlying Appium driver for the current deviceSession.
        /// </summary>
        public AppiumDriver<IWebElement> AppiumDriver { get; private set; }

        /// <summary>
        /// The underlying Appium driver for the current Desktop. This driver has the context for the entire desktop allowing it to
        /// search for elements on any window. 
        /// </summary>
        public WindowsDriver<WindowsElement> DesktopWindowDriver { get; private set; }

        /// <summary>
        /// The <see cref="Uri"/> of the Olympus logs associated with this deviceSession.
        /// Call <see cref="Dispose"/> to ensure the deviceSession is ended and the logs are created.
        /// </summary>
        /// <remarks>
        /// The resource at this location might not exist for a few reasons:
        /// <list type="number">
        /// <item>
        /// <description>The endpoint specified in the <see cref="Configuration"/> is not an Olympus server.</description>
        /// </item>
        /// <item>
        /// <description>The <see cref="DeviceSession"/> has not been disposed. See <seealso cref="Dispose"/>.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public Uri OlympusLogUri { get; private set; }

        /// <summary>
        /// Indicates that the current capabilities needs focus
        /// </summary>
        public bool NeedsFocus { get; set; }

        /// <summary>
        /// Create a new driver deviceSession as specified in the configuration file for the given <see cref="PlatformType"/>.
        /// See <seealso cref="ConfigurationProvider"/> for more information about the configuration files.
        /// </summary>
        /// <param name="logger"><see cref="Logger"/> instance with which to write driver information and error messages</param>
        /// <param name="platformType">The type of automation platform this deviceSession should target</param>
        public DeviceSession(Logger logger, PlatformType platformType)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
            PlatformType = platformType;

            _logger.LogInfo("DeviceSession: Initializing...");

            InitSessionId();
            InitDeviceConfig();

            int retryCounter = 0;

            while (retryCounter++ < InitializationRetryCount)
            {
                try
                {
                    InitAppiumDriver();
                    InitDesktopDriver();
                    break;
                }
                catch (WebDriverException e)
                {
                    _logger.LogInfo(string.Format("DeviceSession: failed WebDriver initialization with {0}. Retrying {1}", e, retryCounter));

                    // rethrow if we're done retrying.
                    if (retryCounter == InitializationRetryCount)
                    {
                        throw;
                    }
                }
            }

            AppiumDriver.SwitchTo().Window(AppiumDriver.CurrentWindowHandle);

            _logger.LogInfo("DeviceSession: Initialization completed.");
            _logger.LogInfo("DeviceSession: DeviceSession details:");
            _logger.LogInfo(JsonConvert.SerializeObject(AppiumDriver.SessionDetails, Formatting.Indented));
        }

        /// <summary>
        /// Hide the keyboard on the current device.
        /// </summary>
        /// <remarks>This action is ignored if the preference <see cref="DeviceConfiguration.CanHideKeyboard"/> has been set.</remarks>
        public void TryHideKeyboard()
        {
            Debug.Assert(!_isDisposed, "DeviceSession has been disposed.");

            if (Configuration.CanHideKeyboard)
            {
                _logger.LogInfo("DeviceSession: Hiding keyboard");

                try
                {
                    AppiumDriver.HideKeyboard();
                    _logger.LogInfo("DeviceSession: Keyboard hidden");
                }
                catch (Exception ex)
                {
                    _logger.LogInfo("DeviceSession: Non-Fatal Error – cannot hide keyboard. " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Take a screenshot and add it as a file with the <see cref="Logger"/> for this <see cref="DeviceSession"/> instance.
        /// </summary>
        /// <remarks>
        /// This can be a slow operation, especially over a slow network connection ~8 seconds
        /// There are bugs in the web drivers that prevent taking screenshots from webviews. This helper will switch to the native view and back.
        /// </remarks>
        public void TakeScreenshot(string picName)
        {
            _logger.LogInfo("DeviceSession: Capturing a screenshot");
            Stopwatch sw = Stopwatch.StartNew();
            string oldContext = PlatformType == PlatformType.Desktop ? "" : this.AppiumDriver.Context;

            try
            {
                SwitchContext(NativeAppContext);

                Debug.Assert(!_isDisposed, "DeviceSession has been disposed.");

                if (AppiumDriver == null)
                {
                    _logger.LogError(
                        $"DeviceSession: Cannot take a screenshot {picName} because the driver has not been initialized or initialization failed.");
                    return;
                }

                string tempPath = Path.GetTempFileName();
                var screenshot = AppiumDriver.GetScreenshot();
                screenshot.SaveAsFile(tempPath, ScreenshotImageFormat.Jpeg);

                string tempPathDesktop = Path.GetTempFileName();
                var desktopScreenShot = DesktopWindowDriver.GetScreenshot();
                desktopScreenShot.SaveAsFile(tempPathDesktop, ScreenshotImageFormat.Jpeg);

                _logger.AddFile(tempPath, picName + ".jpg");
                _logger.AddFile(tempPathDesktop, picName + "2.jpg");
            }
            finally
            {
                SwitchContext(oldContext);
            }

            _logger.LogInfo($"DeviceSession: Screenshot capture succesful in {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Install the broker app.
        /// </summary>
        /// <remarks>
        /// This can be a slow operation (especially over a slow network connection).
        /// </remarks>
        public void InstallBrokerApp(BrokerType type)
        {
            if (type == BrokerType.None)
            {
                throw new ArgumentException("None is not a valid broker app installation option");
            }

            var appPath = Configuration.BrokerAppPaths[type.ToString()];
            _logger.LogInfo($"DeviceSession: Installing broker app '{appPath}'");
            AppiumDriver.InstallApp(appPath);
        }

        public string GetCurrentContext()
        {
            return AppiumDriver.Context;
        }

        /// <summary>
        /// Switches the context
        /// </summary>
        public void SwitchContext(string contextName)
        {
            Debug.Assert(AppiumDriver != null);

            if (PlatformType == PlatformType.Desktop)
            {
                // Desktop does not support different contexts
                return;
            }

            _logger.LogInfo($"DeviceSession: Switching driver context to {contextName}");

            try
            {
                AppiumDriver.Context = contextName;
            }
            catch (Exception ex)
            {
                _logger.LogError("DeviceSession: Context switch failed", ex);
                _logger.LogInfo("DeviceSession: Contexts available: " + string.Join(",", AppiumDriver.Contexts));
                throw;
            }

            _logger.LogInfo("DeviceSession: Context switch successful");
        }

        public IEnumerable<string> GetWebContexts()
        {
            _logger.LogInfo("DeviceSession: Searching for web contexts...");

            // Either due to poor device/host performance, or a slow network, it can sometimes take
            // some time for the webview context to be fully available.
            var wait = new WebDriverWait(AppiumDriver, TimeSpan.FromSeconds(Configuration.WebViewContextSearchTimeout));

            var webContexts = wait.Until(d =>
            {
                var ctxs = AppiumDriver.Contexts.Where(c => c.StartsWith("WEBVIEW_")).ToArray();
                return ctxs.Any() ? ctxs : null;
            });

            _logger.LogInfo($"DeviceSession: Available web contexts: {string.Join(", ", webContexts)}");

            return webContexts;
        }

        #region IDisposable

        /// <summary>
        /// Ends the current deviceSession and disposes of the <see cref="AppiumDriver"/>.
        /// </summary>
        /// <remarks>
        /// If the current deviceSession endpoint is an Olympus server then you must call this method to 
        /// end the deviceSession and cause Olympus to save the logs at <see cref="OlympusLogUri"/>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Throws if this <see cref="DeviceSession"/> has already been disposed.</exception>
        public void Dispose()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(DeviceSession));
            }

            _isDisposed = true;

            _logger?.LogInfo("DeviceSession: Disposing");
            AppiumDriver?.Quit();
            AppiumDriver = null;
        }

        #endregion

        #region Private

        private void InitSessionId()
        {
            _sessionId = Guid.NewGuid();
            OlympusLogUri = new Uri($"https://deviceal.blob.core.windows.net/log/appium-{_sessionId:D}.log");
            _logger.LogInfo($"DeviceSession: DeviceSession ID is {_sessionId:D}");
        }

        private void InitDeviceConfig()
        {
            _logger.LogInfo("DeviceSession: Loading device configuration...");

            Configuration = ConfigurationProvider.Instance.GetDeviceConfiguration(PlatformType);

            string pathToApp = (string) Configuration.DeviceCapabilities["app"];
            
            bool appExists;
            if (PlatformType == PlatformType.Ios && pathToApp.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
            {
                // iOS ".app" packages are actually directories
                appExists = Directory.Exists(pathToApp) || !ConfigurationProvider.IsVstsBuild;
            }
            else
            {
                appExists = File.Exists(pathToApp);
            }
            Assert.IsTrue(appExists, $"App does not exist at path: {pathToApp}");

            Configuration.DeviceCapabilities.Add(OlympusSessionIdKeyName, _sessionId);

            _logger.LogInfo("DeviceSession: Device configuration:");
            _logger.LogInfo(JsonConvert.SerializeObject(Configuration, Formatting.Indented));
        }

        private void InitAppiumDriver()
        {
            _logger.LogInfo($"DeviceSession: Connecting to {Configuration.ServerUrl}...");

            var serverUrl = Configuration.ServerUrl;
            var desiredCapabilities = new DesiredCapabilities(Configuration.DeviceCapabilities);
            var commandTimeout = TimeSpan.FromSeconds(Configuration.CommandTimeout);

            switch (PlatformType)
            {
                case PlatformType.Android:
                    AppiumDriver = new AndroidDriver<IWebElement>(serverUrl, desiredCapabilities, commandTimeout);
                    break;
                case PlatformType.Ios:
                    AppiumDriver = new IOSDriver<IWebElement>(serverUrl, desiredCapabilities, commandTimeout);
                    break;
                case PlatformType.Desktop:
                    AppiumDriver = new WindowsDriver<IWebElement>(serverUrl, desiredCapabilities, commandTimeout);
                    break;
                default:
                    throw new NotImplementedException("Only Android, iOS and Desktop are supported.");
            }
        }

        private void InitDesktopDriver()
        {
            DesiredCapabilities desktopCapabilities = new DesiredCapabilities();
            desktopCapabilities.SetCapability("app", "Root");
            DesktopWindowDriver = new WindowsDriver<WindowsElement>(Configuration.ServerUrl, desktopCapabilities,
                TimeSpan.FromSeconds(Configuration.CommandTimeout*2));
        }

        #endregion
    }
}
