using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using System.Threading;

namespace Test.ADAL.NET.UIAutomation
{
    [TestClass]
    public class AndroidTests
    {
        [TestMethod]
        public void AcquireTokenUITest()
        {
            AppiumDriver<RemoteWebElement> driver;
            DesiredCapabilities capabilities = new DesiredCapabilities();

            capabilities.SetCapability("deviceName", "Samsung SM-N950U");
            capabilities.SetCapability("platformVersion", "8.0");
            capabilities.SetCapability("platformName", "Android");

            capabilities.SetCapability("appPackage", "AdalAndroidTestApp.AdalAndroidTestApp");
            capabilities.SetCapability("appActivity", "md57140920655c706b14859e121da0dac29.MainActivity");

            driver = new AndroidDriver<RemoteWebElement>(new Uri("http://127.0.0.1:4725/wd/hub"), capabilities);

            //Clear cache
            //driver.FindElement(By.Id("AdalAndroidTestApp.AdalAndroidTestApp:id/clearCacheButton")).Click();

            //Start acquie token flow
            driver.FindElement(By.Id("AdalAndroidTestApp.AdalAndroidTestApp:id/acquireTokenInteractiveButton")).Click();
            Thread.Sleep(3000);

            //Enter UPN
            driver.FindElement(By.Id("i0116")).Click();
            driver.FindElement(By.Id("i0116")).SendKeys("temp@trwalke.onmicrosoft.com");
            driver.FindElement(By.Id("idSIButton9")).Click();
            Thread.Sleep(2000);

            //Enter Password
            driver.FindElement(By.Id("i0118")).Click();
            driver.FindElement(By.Id("i0118")).SendKeys("");
            driver.FindElement(By.Id("idSIButton9")).Click();
            Thread.Sleep(6000);

            //Verify access token
            var result = driver.FindElement(By.Id("AdalAndroidTestApp.AdalAndroidTestApp:id/accessTokenTextView")).Text;
            Assert.IsTrue(result.Contains("success! access token ="));
        }
    }
}
