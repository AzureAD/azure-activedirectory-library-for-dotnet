using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.PageObjects;

namespace Microsoft.Identity.AutomationTests.Pages
{
    public class BrokerSignInPage : SignInPage
    {
        private readonly By _registerButton;

        public BrokerSignInPage(Logger logger, DeviceSession deviceSession)
            : base(logger, deviceSession)
        {
            if (deviceSession.PlatformType == Model.PlatformType.Desktop)
            {
                // This method avoids Appium's problem with context switching between applications.
                UsernameTextbox = new ByAndroidUIAutomator("new UiSelector().resourceId(\"cred_userid_inputtext\")");
                PasswordTextbox = new ByAndroidUIAutomator("new UiSelector().resourceId(\"cred_password_inputtext\")");
                SignInButton = new ByAndroidUIAutomator("new UiSelector().resourceId(\"cred_sign_in_button\")");
                _registerButton = new ByAndroidUIAutomator("new UiSelector().resourceId(\"ca-confirm-button\")");
            }
            else
            {
                // Completely untested! It should be a regular AAD sign-in page though.
                UsernameTextbox = new ByIdOrName("cred_userid_inputtext");
                PasswordTextbox = new ByIdOrName("cred_password_inputtext");
                SignInButton = new ByIdOrName("cred_sign_in_button");
                _registerButton = new ByIdOrName("ca-confirm-button");
            }
        }

        public void ClickRegisterButton()
        {
            WaitForElement(_registerButton).Click();
            Logger.LogInfo("BrokerSignInPage: Register button clicked");
        }
    }
}