using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace Microsoft.Identity.AutomationTests.Pages
{
    public class PingFederateSignInPage : SignInPage
    {
        public PingFederateSignInPage(Logger logger, DeviceSession deviceSession)
            : base(logger, deviceSession)
        {
            UsernameTextbox = new ByAccessibilityId("pf.username");
            PasswordTextbox = new ByAccessibilityId("pf.pass");
            SignInButton = By.ClassName("ping-button normal allow");
        }
    }
}
