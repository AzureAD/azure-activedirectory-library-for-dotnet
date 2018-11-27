using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.PageObjects;

namespace Microsoft.Identity.AutomationTests.Pages
{
    /// <summary>
    /// Models the new converged, paginated AAD/MSA sign-in page.
    /// </summary>
    public class AadSignInPage : SignInPage
    {
        private readonly By _nextButton;
        private readonly By _backButton;

        public AadSignInPage(Logger logger, DeviceSession deviceSession)
            : base(logger, deviceSession)
        {
            var byFunc = deviceSession.PlatformType == Model.PlatformType.Desktop
                ? (Func<string, By>) (x => new ByAccessibilityId(x))
                : (x => new ByIdOrName(x));

            UsernameTextbox = byFunc("i0116");
            PasswordTextbox = byFunc("i0118");

            _nextButton = byFunc("idSIButton9");
            _backButton = byFunc("idBtn_Back");
            SignInButton = byFunc("idSIButton9");
        }

        public void ClickNext()
        {
            Logger.LogInfo($"{GetType().Name}: Clicking on the Next button");

            WaitForElement(_nextButton, remoteSignInWindow).Click();
        }

        public void ClickBack()
        {
            Logger.LogInfo($"{GetType().Name}: Clicking on the Back button");

            WaitForElement(_backButton).Click();
        }
    }
}
