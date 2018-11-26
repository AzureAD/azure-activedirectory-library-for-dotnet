using Microsoft.Identity.AutomationTests;
using Microsoft.Identity.Labs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.PageObjects;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DesktopTests
{
    internal static class DeviceCodeSignin
    {
        public static void SignIn(IUser user, DeviceCodeResponse deviceCodeResponse, Logger logger)
        {
            ChromeSignIn.SignIn(user, deviceCodeResponse, logger);
        }

        private static class ChromeSignIn
        {
            public static void SignIn(IUser user, DeviceCodeResponse deviceCodeResponse, Logger logger)
            {
                var federationProvider = FederationProvider.None;
                if (user.IsFederated)
                {
                    federationProvider = user.FederationProvider;
                }

                //Open Chrome Driver in incognito mode
                ChromeOptions options = new ChromeOptions();
                options.AddArgument(@"--incognito");
                options.AddArgument(@"--start-maximized");

                using (ChromeDriver chromeDriver = new ChromeDriver(options))
                // TODO: investigate using the eventing driver to check for scripts running // pages loading
                //using(EventFiringWebDriver driver = new EventFiringWebDriver(chromeDriver))
                {

                    var verificationUrl = deviceCodeResponse.GetVerificationUrl();
                    var userCode = deviceCodeResponse.GetUserCode();

                    string currentUrl = verificationUrl;

                    // Do device auth on chrome
                    chromeDriver.Navigate().GoToUrl(verificationUrl);
                    chromeDriver.FindElement(By.Id("code"), 15).SendKeys(userCode); 
                    Thread.Sleep(3000); // wait for the continue button to appear
                    chromeDriver.FindElement(By.Id("continueBtn"), 15).Click();
                    WaitForPageToChange(chromeDriver, currentUrl, 4, logger);

                    string userId = user.Upn;
                    chromeDriver.FindElement(By.Id("cred_userid_inputtext"), 15).SendKeys(userId);
                    chromeDriver.FindElement(By.Id("cred_password_inputtext"), 15).Click();

                    WaitForPageToChange(chromeDriver, currentUrl, 4, logger); // Page may not change in the case of login failure. Could perhaps shorten the wait by checking when the script finished running

                    By[] controls = GetLoginControlNames(federationProvider);
                    if (federationProvider == FederationProvider.AdfsV2 || federationProvider == FederationProvider.Shibboleth)
                    {
                        Thread.Sleep(6000);
                        string upn = user.Upn;
                        if (federationProvider == FederationProvider.Shibboleth)
                        {
                            upn = upn.Split('@')[0];
                        }
                        chromeDriver.FindElement(controls[0], 15).SendKeys(upn);
                    }

                    string pwd = user.GetPassword();
                    chromeDriver.FindElement(controls[1], 15).SendKeys(pwd);
                    Thread.Sleep(3000);
                    currentUrl = chromeDriver.Url;
                    chromeDriver.FindElement(controls[2], 15).Click();

                    WaitForPageToChange(chromeDriver, currentUrl, 3, logger); // Page may not change in the case of login failure. Could perhaps shorten the wait by checking when the script finished running

                    string finalPageContent = chromeDriver.PageSource;
                    if (finalPageContent.Contains("Incorrect") || !finalPageContent.Contains("You have signed in"))
                    {
                        TakeScreenshot("SignInByDeviceCode_login_failure", chromeDriver, logger);
                        Assert.Fail($"Test error: failed to sign in to redeem the token. User {user.Upn}.   See the screenshot in the test output for more information.");
                    }

                    chromeDriver.Quit();
                }
            }

            private static void TakeScreenshot(string pictureName, ChromeDriver chromeDriver, Logger logger)
            {
                string tempPath = Path.GetTempFileName();
                var screenshot = chromeDriver.GetScreenshot();
                screenshot.SaveAsFile(tempPath, ScreenshotImageFormat.Jpeg);
                logger.AddFile(tempPath, pictureName + ".jpg");
            }

            private static bool WaitForPageToChange(IWebDriver webDriver, string currentUrl, int timeoutInSeconds, Logger logger)
            {
                logger.LogInfo($"Waiting for page to change from {currentUrl}...");

                Stopwatch timer = Stopwatch.StartNew();
                while (webDriver.Url == currentUrl && timer.ElapsedMilliseconds < timeoutInSeconds * 1000)
                {
                    Thread.Sleep(500);
                }

                bool changed = webDriver.Url != currentUrl;

                if (changed)
                {
                    logger.LogInfo($"Browser page changed to {webDriver.Url}");
                }
                else
                {
                    logger.LogError($"Browser page did not change in {timeoutInSeconds} seconds");
                }
                return changed;
            }

            private static By[] GetLoginControlNames(FederationProvider federationProvider)
            {
                By[] controls = new By[3];

                if (federationProvider == FederationProvider.AdfsV3 || federationProvider == FederationProvider.AdfsV4)
                {
                    controls[0] = new ByIdOrName("userNameInput");
                    controls[1] = new ByIdOrName("passwordInput");
                    controls[2] = new ByIdOrName("submitButton");
                }
                else if (federationProvider == FederationProvider.AdfsV2)
                {
                    controls[0] = new ByIdOrName("ContentPlaceHolder1_UsernameTextBox");
                    controls[1] = new ByIdOrName("ContentPlaceHolder1_PasswordTextBox");
                    controls[2] = new ByIdOrName("ContentPlaceHolder1_SubmitButton");
                }
                else if (federationProvider == FederationProvider.Shibboleth)
                {
                    controls[0] = new ByIdOrName("username");
                    controls[1] = new ByIdOrName("password");
                    controls[2] = By.XPath("//button[@type='submit']");
                }
                else
                {
                    controls[0] = new ByIdOrName("cred_userid_inputtext");
                    controls[1] = new ByIdOrName("cred_password_inputtext");
                    controls[2] = new ByIdOrName("cred_sign_in_button");
                }
                return controls;
            }

        }

        private static class UilessSignIn
        {
            public static void SignIn(IUser user, DeviceCodeResponse deviceCodeResponse, Logger logger)
            {

            }
        }
    }
}
