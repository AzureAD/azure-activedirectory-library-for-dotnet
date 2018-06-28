using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Test.ADAL.NET.UIAutomation
{
	public class CoreMobileTests
	{
        public static void AcquireTokenTest(ITestController controller)
		{
            controller.Tap("secondPage", false);
            controller.Tap("acquireToken", false);
            controller.EnterText("i0116", "temp@trwalke.onmicrosoft.com", true);
            controller.Tap("idSIButton9", true);
            controller.EnterText("i0118", "", true);
            controller.Tap("idSIButton9", true);
            Assert.IsTrue(controller.GetResultText("testResult") == "Succsess: True");
        }
	}
}
