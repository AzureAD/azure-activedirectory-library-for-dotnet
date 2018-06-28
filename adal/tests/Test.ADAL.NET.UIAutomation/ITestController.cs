using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.ADAL.NET.UIAutomation
{
    public interface ITestController
    {
        void Tap(string elementID, bool isWebElement);
        void Tap(string elementID, int waitTime, bool isWebElement);
        void EnterText(string elementID, string text, bool isWebElement);
        void EnterText(string elementID, int waitTime, string text, bool isWebElement);
        object[] WaitForElement(string automationID, bool isWebElement);
        string GetResultText(string elementID);
    }
}
