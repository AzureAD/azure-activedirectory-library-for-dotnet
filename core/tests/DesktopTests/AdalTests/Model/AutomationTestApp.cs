using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenQA.Selenium;
using PlatformType = Microsoft.Identity.AutomationTests.Model.PlatformType;

namespace Microsoft.Identity.AutomationTests.Pages
{
    public class AutomationTestApp : PageBase
    {
        protected By AcquireTokenButton;
        protected By AcquireTokenSilentButton;
        protected By DataInputTextBox;
        protected By GoButton;
        protected By ResultTextBox;
        protected By LogsTextBox;
        protected By DoneButton;
        protected By ReadCacheButton;
        protected By ClearCacheButton;
        protected By InvalidateRefreshTokenButton;
        protected By ExpireAccessTokenButton;
        protected By InvalidateFamilyRefreshTokenButton;
        protected By AcquireTokenDeviceProfileButton;
        protected By AcquireTokenDeviceCodeButton;

        public AutomationTestApp(Logger logger, DeviceSession deviceSession)
            : base(logger, deviceSession)
        {
            var byFunc = deviceSession.PlatformType == PlatformType.Desktop ? By.Name : (Func<string, By>)By.Id;

            AcquireTokenButton = byFunc("acquireToken");
            AcquireTokenSilentButton = byFunc("acquireTokenSilent");
            DataInputTextBox = byFunc("requestInfo");
            GoButton = byFunc("requestGo");
            ResultTextBox = byFunc("resultInfo");

            // Temporary: the Android automation app has not been updated with the new logs element name
            LogsTextBox = byFunc(deviceSession.PlatformType == PlatformType.Android ? "adalLogs" : "resultLogs");

            DoneButton = byFunc("resultDone");
            ReadCacheButton = byFunc("readCache");
            ClearCacheButton = byFunc("clearCache");
            InvalidateRefreshTokenButton = byFunc("invalidateRefreshToken");
            ExpireAccessTokenButton = byFunc("expireAccessToken");
            InvalidateFamilyRefreshTokenButton = byFunc("invalidateFamilyRefreshToken");
            AcquireTokenDeviceProfileButton = byFunc("acquireTokenDeviceProfile");
            AcquireTokenDeviceCodeButton = byFunc("acquireDeviceCode");
        }

        public virtual void ClickAcquireTokenButton()
        {
            Logger.LogInfo("TestApp: clicking the AcquireToken button");
            WaitForElement(AcquireTokenButton).Click();
        }

        public virtual void ClickAcquireTokenSilentButton()
        {
            Logger.LogInfo("TestApp: clicking the AcquireTokenSilentButton button");
            WaitForElement(AcquireTokenSilentButton).Click();
        }

        public virtual void ClickGoButton()
        {
            Logger.LogInfo("TestApp: clicking the Go button ");
            WaitForElement(GoButton).Click();
        }

        public virtual void EnterInputData(IDictionary<string, string> inputData)
        {
            Logger.LogInfo("TestApp: entering input data");
            Logger.LogInfo(JsonConvert.SerializeObject(inputData, Formatting.Indented));

            var json = JsonConvert.SerializeObject(inputData);
            WaitForElement(DataInputTextBox).SendKeys(json);
            DeviceSession.TryHideKeyboard();
        }

        public virtual void ClickDoneButton()
        {
            Logger.LogInfo("TestApp: clicking the Done button");
            WaitForElement(DoneButton).Click();
        }

        public virtual void ClickReadCacheButton()
        {
            Logger.LogInfo("TestApp: clicking the ReadCache button");
            WaitForElement(ReadCacheButton).Click();
        }

        public virtual void ClickAcquireTokenDeviceProfileButton()
        {
            Logger.LogInfo("TestApp: clicking the Acquire Token using Device Profile button");
            WaitForElement(AcquireTokenDeviceProfileButton).Click();
        }

        public virtual void ClickAcquireTokenDeviceCodeButton()
        {
            Logger.LogInfo("TestApp: clicking the Acquire Device Code button");
            WaitForElement(AcquireTokenDeviceCodeButton).Click();
        }

        public virtual void ClickClearCacheButton()
        {
            Logger.LogInfo("TestApp: clicking the ClearCache button");
            WaitForElement(ClearCacheButton).Click();
        }

        public virtual void ClickInvalidateRefreshTokenButton()
        {
            Logger.LogInfo("TestApp: clicking the InvalidateRefreshToken button");
            WaitForElement(InvalidateRefreshTokenButton).Click();
        }

        public virtual void ClickExpireAccessTokenButton()
        {
            Logger.LogInfo("TestApp: clicking the ExpireAccessToken button");
            WaitForElement(ExpireAccessTokenButton).Click();
        }

        public virtual void ClickInvalidateFamilyRefreshTokenButton()
        {
            Logger.LogInfo("TestApp: clicking the InvalidateFamilyRefreshToken button");
            WaitForElement(InvalidateFamilyRefreshTokenButton).Click();
        }

        public virtual string GetResult()
        {
            Logger.LogInfo("TestApp: getting the result");

            // do NOT print this!
            return WaitForElement(ResultTextBox).Text;
        }

        public virtual string GetResultLogs()
        {
            Logger.LogInfo("TestApp: getting result logs");
            return WaitForElement(LogsTextBox).Text;
        }
    }
}
