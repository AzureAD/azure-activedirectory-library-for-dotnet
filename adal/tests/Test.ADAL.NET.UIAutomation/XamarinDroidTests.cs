using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Test.ADAL.NET.UIAutomation
{
    [TestFixture(Platform.Android)]
    class XamarinDroidTests
    {
        IApp app;
        Platform platform;

        public XamarinDroidTests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void AcquireTokenTest()
        {
            CoreMobileTests.AcquireTokenTest(new XamarinUITestController(app));
        }
    }
}
