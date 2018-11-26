using Microsoft.Identity.AutomationTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

namespace Microsoft.Identity.AutomationTests
{
    [TestClass]
    public abstract class HeadlessTestsBase
    {
        /// <summary>
        /// A hook for tests to identify the platform they run on (Android / iOS etc.)
        /// </summary>
        protected abstract PlatformType PlatformType { get; }

        protected Logger Logger { get; private set; }

        protected DeviceSession DeviceSession { get; private set; }

        /// <summary>
        /// MSTest will create this object.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            Logger = new Logger(TestContext);
            Logger.LogInfo($"Starting test {TestContext.TestName}");
        }
    }
}
