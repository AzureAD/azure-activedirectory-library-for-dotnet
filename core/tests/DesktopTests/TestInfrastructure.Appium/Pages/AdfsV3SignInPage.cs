using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.PageObjects;

namespace Microsoft.Identity.AutomationTests.Pages
{
    public class AdfsV3SignInPage : SignInPage
    {
        public AdfsV3SignInPage(Logger logger, DeviceSession deviceSession)
            : base(logger, deviceSession)
        {
            if (deviceSession.PlatformType == Model.PlatformType.Desktop)
            {
                UsernameTextbox = new ByAccessibilityId("userNameInput");
                PasswordTextbox = new ByAccessibilityId("passwordInput");
                SignInButton = new ByAccessibilityId("submitButton");
            }
            else
            {
                UsernameTextbox = new ByIdOrName("userNameInput");
                PasswordTextbox = new ByIdOrName("passwordInput");
                SignInButton = new ByIdOrName("submitButton");
            }
        }
    }
}
