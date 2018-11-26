using OpenQA.Selenium;
using PlatformType = Microsoft.Identity.AutomationTests.Model.PlatformType;

namespace Microsoft.Identity.AutomationTests.Pages
{
    public abstract class SignInPage : PageBase
    {
        protected By UsernameTextbox;
        protected By PasswordTextbox;
        protected By SignInButton;
        protected string remoteSignInWindow = "Sign in to your account";

        public string RemoteSignInWindow
        {
            get { return remoteSignInWindow; }
            set { remoteSignInWindow = value; }
        }

        protected SignInPage(Logger logger, DeviceSession deviceSession)
            : base(logger, deviceSession) { }

        public virtual void EnterUsername(string username)
        {
            Logger.LogInfo($"{GetType().Name}: Entering username");

            var element = WaitForRemoteElement(remoteSignInWindow, UsernameTextbox);

            // This element is set nonClickable, try clicking on it will fail the test.
            // element.Click();
            // Logger.LogInfo($"{GetType().Name}: Username text field clicked");

            element.SendKeys(username);
            Logger.LogInfo($"{GetType().Name}: Username entered: {username}" );
        }

        public void ClickPasswordField()
        {
            Logger.LogInfo($"{GetType().Name}: Clicking on password field");

            WaitForElement(PasswordTextbox).Click();
            Logger.LogInfo($"{GetType().Name}: Password text field clicked");
        }

        public void EnterPassword(string password)
        {
            if (password.Contains("\\") && this.DeviceSession.PlatformType == PlatformType.Desktop) 
            {
                // WinAppDriver does not enter single backslashes correctly! https://github.com/Microsoft/WinAppDriver/issues/217
                Logger.LogError("Password contains a \\ character and will not be entered correctly by WinAppDriver.");
            }
            
            Logger.LogInfo($"{GetType().Name}: Entering password");

            WaitForRemoteElement(remoteSignInWindow, PasswordTextbox).SendKeys(password);
            Logger.LogInfo($"{GetType().Name}: Password entered");
        }

        public void ClickSignIn()
        {
            Logger.LogInfo($"{GetType().Name}: Clicking on the SignIn button");

            WaitForRemoteElement(remoteSignInWindow, SignInButton).Click();
        }
    }
}
