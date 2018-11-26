using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.PageObjects;

namespace Microsoft.Identity.AutomationTests.Pages
{
    public class AdfsV2SignInPage : SignInPage
    {
        public AdfsV2SignInPage(Logger logger, DeviceSession deviceSession)
            : base(logger, deviceSession)
        {
            if (deviceSession.PlatformType == Model.PlatformType.Desktop)
            {
                UsernameTextbox = new ByAccessibilityId("ContentPlaceHolder1_UsernameTextBox");
                PasswordTextbox = new ByAccessibilityId("ContentPlaceHolder1_PasswordTextBox");
                SignInButton = new ByAccessibilityId("ContentPlaceHolder1_SubmitButton");
            }
            else
            {
                UsernameTextbox = new ByIdOrName("ContentPlaceHolder1_UsernameTextBox");
                PasswordTextbox = new ByIdOrName("ContentPlaceHolder1_PasswordTextBox");
                SignInButton = new ByIdOrName("ContentPlaceHolder1_SubmitButton");
            }
        }
    }
}
