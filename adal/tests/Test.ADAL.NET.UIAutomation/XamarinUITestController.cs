﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.UITest;

namespace Test.ADAL.NET.UIAutomation
{
    public class XamarinUITestController : ITestController
    {
        IApp app;
        TimeSpan defaultSearchTimeout;
        TimeSpan defaultRetryFrequency;
        TimeSpan defaultPostTimeout;
        const int defaultSearchTimeoutSec = 10;
        const int defaultRetryFrequencySec = 1;
        const int defaultPostTimeoutSec = 1;
        const string CSSIDSelector = "[id|={0}]";

        public XamarinUITestController(IApp app)
        {
            this.app = app;
            this.defaultSearchTimeout = new TimeSpan(0, 0, defaultSearchTimeoutSec);
            this.defaultRetryFrequency = new TimeSpan(0, 0, defaultRetryFrequencySec);
            this.defaultPostTimeout = new TimeSpan(0, 0, defaultPostTimeoutSec);
        }

        public void Tap(string elementID, bool isWebElement)
        {
            Tap(elementID, isWebElement, defaultSearchTimeout);
        }

        public void Tap(string elementID, int waitTime, bool isWebElement)
        {
            Tap(elementID, isWebElement, new TimeSpan(0, 0, waitTime));
        }

        public void EnterText(string elementID, string text, bool isWebElement)
        {
            EnterText(elementID, text, isWebElement, defaultSearchTimeout);
        }

        public void EnterText(string elementID, int waitTime, string text, bool isWebElement)
        {
            EnterText(elementID, text, isWebElement, new TimeSpan(0, 0, waitTime));
        }

        public object[] WaitForElement(string elementID, bool isWebElement)
        {
            if (isWebElement)
            {
                return app.WaitForElement(c => c.Css(String.Format(CSSIDSelector, elementID)), "Could not find element", defaultSearchTimeout, defaultRetryFrequency, defaultPostTimeout);
            }
            else
            {
                return app.WaitForElement(elementID, "Could not find element", defaultSearchTimeout, defaultRetryFrequency, defaultPostTimeout);
            }
        }

        private void Tap(string elementID, bool isWebElement, TimeSpan timeout)
        {
            if (isWebElement)
            {
                app.WaitForElement(c => c.Css(String.Format(CSSIDSelector, elementID)), "Could not find element", timeout, defaultRetryFrequency, defaultPostTimeout);
                app.Tap(c => c.Css(String.Format(CSSIDSelector, elementID)));
            }
            else
            {
                app.WaitForElement(elementID, "Could not find element", timeout, defaultRetryFrequency, defaultPostTimeout);
                app.Tap(x => x.Marked(elementID));
            }
        }

        private void EnterText(string elementID, string text, bool isWebElement, TimeSpan timeout)
        {
            if (isWebElement)
            {
                app.WaitForElement(c => c.Css(String.Format(CSSIDSelector, elementID)), "Could not find element", timeout, defaultRetryFrequency, defaultPostTimeout);
                app.EnterText(c => c.Css(String.Format(CSSIDSelector, elementID)), text);
            }
            else
            {
                app.WaitForElement(elementID, "Could not find element", timeout, defaultRetryFrequency, defaultPostTimeout);
                app.Tap(x => x.Marked(elementID));
            }
        }

        public string GetResultText(string elementID)
        {
            app.WaitForElement(elementID, "Could not find element", defaultSearchTimeout, defaultRetryFrequency, defaultPostTimeout);
            return app.Query(x => x.Marked(elementID)).FirstOrDefault().Text;
        }
    }
}
