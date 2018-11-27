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
        /// Gets an element from the main or a remote window.
        /// </summary>
        /// <param name="by">Method used to find the element</param>
        /// <param name="memberName">Name of desired element.</param>
        /// <param name="elementWindowName">Name of the window in which the element exists</param>
        /// <param name="timeout">How long to search for the element before quitting</param>
        /// <returns></returns>
        public IWebElement WaitForElement(
        By by,
        string elementWindowName = "",
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        int? timeout = null)
        {
            int timeoutValue = timeout * 1000 ?? DeviceSession.Configuration.FindElementsTimeout * 1000;
            Stopwatch sw = new Stopwatch();
            bool isRemoteWindow = elementWindowName != "";

            if (isRemoteWindow)
            {
                //Return focus to original app if needed
                if (DeviceSession.NeedsFocus)
                {
                    //fails if this happens too fast after interacting with remote windows. Not sure why, more investigation needed
                    Thread.Sleep(2000);
                    DeviceSession.AppiumDriver.SwitchTo().Window(DeviceSession.AppiumDriver.CurrentWindowHandle);
                    DeviceSession.NeedsFocus = false;
                }
            }
            else
            {
                //Remote Window selected. Original window now needs focus
                DeviceSession.NeedsFocus = true;
            }

            //Wait for elements to load. WebDriverWait could not be used because it does not support using WindowsDriver
            while (sw.ElapsedMilliseconds < timeoutValue)
            {
                if (!sw.IsRunning)
                    sw.Start();

                Thread.Sleep(100);

                try
                {
                    if (isRemoteWindow)
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
                    else
                    {
                        //try to get element on main window
                        var result = DeviceSession.AppiumDriver.FindElement(by);
                        if (result != null)
                        {
                            //return found element
                            sw.Stop();
                            return result;
                        }
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogInfo($"Still Searching For Elelement {memberName}");
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
    }
}
