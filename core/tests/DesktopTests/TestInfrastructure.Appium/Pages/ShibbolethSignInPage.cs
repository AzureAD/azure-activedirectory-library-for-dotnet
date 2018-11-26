using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.PageObjects;

namespace Microsoft.Identity.AutomationTests.Pages
{
    public class ShibbolethSignInPage : SignInPage
    {
        public ShibbolethSignInPage(Logger logger, DeviceSession deviceSession)
            : base(logger, deviceSession)
        {
            UsernameTextbox = new ByAccessibilityId("username");
            PasswordTextbox = new ByAccessibilityId("password");
            SignInButton = new ByIdOrName("Continue");
        }
    }
}
