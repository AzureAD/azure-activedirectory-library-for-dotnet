using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Timers;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace Microsoft.Identity.AutomationTests.Pages
{
    public abstract class PageBase
    {
        /// <summary>
        /// A logger that is bound to the test execution.
        /// </summary>
        protected Logger Logger { get; }

        /// <summary>
        /// The <see cref="DeviceSession"/> for the current test.
        /// </summary>
        protected DeviceSession DeviceSession { get; }

        protected PageBase(Logger logger, DeviceSession deviceSession)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (deviceSession == null)
            {
                throw new ArgumentNullException(nameof(deviceSession));
            }

            Logger = logger;
            DeviceSession = deviceSession;
        }

        /// <summary>
        /// Gets an element from a remote window. The standard WaitForElement() is not able to search in the context of windows other 
        /// than the capabilities app because it uses AppiumDriver to get elements. AppiumDriver is unable to switch its context to
        /// other windows on Desktop platforms. This method uses WindowsDriver scoped to the entire desktop instead, which allows us 
        /// to get any element on any window.
        /// </summary>
        /// <param name="elementWindowName">Name of the window in which the element exists</param>
        /// <param name="by">Method used to find the element</param>
        /// <param name="timeout">How long to search for the element before quitting</param>
        /// <param name="memberName">Name of desired element.</param>
        /// <returns></returns>
        public IWebElement WaitForRemoteElement(
            string elementWindowName,
            By by,
            int? timeout = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            DeviceSession.NeedsFocus = true;
            int timeoutValue = timeout*1000 ?? DeviceSession.Configuration.FindElementsTimeout*1000;
            Stopwatch sw = new Stopwatch();

            //Wait for elements to load. WebDriverWait could not be used because it does not support using WindowsDriver
            while (sw.ElapsedMilliseconds < timeoutValue)
            {
                if (!sw.IsRunning)
                    sw.Start();

                Thread.Sleep(100);

                try
                {
                    //try to get window
                    var window = DeviceSession.DesktopWindowDriver.FindElement(By.Name(elementWindowName));
                    if (window != null)
                    {
                        //try to get element on window
                        var result = window.FindElement(by);
                        if (result != null)
                        {
                            //return found element
                            sw.Stop();
                            return result;
                        }
                        continue;
                    }
                    else
                        continue;
                }
                catch(Exception ex)
                {
                    //expected exceptions while looking for element. ignore and continue
                    if (ex is ElementNotVisibleException || ex is TargetInvocationException)
                        continue;
                    else
                    {
                        Logger.LogError($"Fatal - Failed to find the element. Tried for {sw.ElapsedMilliseconds} ms. " +
                                        $"See element_missing_{memberName} screenshot for details.", ex);
                        DeviceSession.TakeScreenshot($"element_missing_{memberName}");
                        sw.Stop();
                        throw;
                    }
                }
            }

            sw.Stop();

            //Could not find element in time
            Exception exception = new Exception("Failed to find the element.");
            Logger.LogError($"Fatal - Failed to find the element. Tried for {sw.ElapsedMilliseconds} ms. " +
                $"See element_missing_{memberName} screenshot for details.", exception);
            DeviceSession.TakeScreenshot($"element_missing_{memberName}");
            throw exception;
        }

        public IWebElement WaitForElement(
            By by,
            int? timeout = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            //Return focus to original app if needed
            if (DeviceSession.NeedsFocus)
            {
                //fails if this happens too fast after interacting with remote windows. Not sure why, more investigation needed
                Thread.Sleep(2000);
                DeviceSession.AppiumDriver.SwitchTo().Window(DeviceSession.AppiumDriver.CurrentWindowHandle);
                DeviceSession.NeedsFocus = false;
            }

            int timeoutValue = timeout ?? DeviceSession.Configuration.FindElementsTimeout;
            var wait = new WebDriverWait(DeviceSession.AppiumDriver, TimeSpan.FromSeconds(timeoutValue))
            {
                PollingInterval = TimeSpan.FromSeconds(DeviceSession.Configuration.FindElementPollInterval)
            };
            wait.IgnoreExceptionTypes(typeof(ElementNotVisibleException), typeof(TargetInvocationException));

            Stopwatch sw = new Stopwatch();
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(by));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Fatal - Failed to find the element. Tried for {sw.ElapsedMilliseconds} ms. " +
                                 $"See element_missing_{memberName} screenshot for details.", ex);
                DeviceSession.TakeScreenshot($"element_missing_{memberName}");
                throw;
            }

            return DeviceSession.AppiumDriver.FindElement(by);
        }
    }
}
