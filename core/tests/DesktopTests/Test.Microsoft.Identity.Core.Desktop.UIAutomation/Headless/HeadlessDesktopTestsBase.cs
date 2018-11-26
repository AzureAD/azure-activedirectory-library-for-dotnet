using Microsoft.Identity.AutomationTests;
using Microsoft.Identity.AutomationTests.Adal.Headless;
using Microsoft.Identity.AutomationTests.Model;

namespace DesktopTests
{
    public class HeadlessDesktopTestsBase : HeadlessTestsBase
    {
        protected override PlatformType PlatformType => PlatformType.Desktop;

        protected DesktopSpecificTests GetAppiumDesktopSpecificTests()
        {
            return new DesktopSpecificTests(Logger, new AutomationTestAppController(Logger, DeviceSession));
        }

        protected DesktopSpecificTests GetNonAppiumDesktopSpecificTests()
        {
            Logger logger = new Logger(this.TestContext);
            DesktopSpecificTests tests = new DesktopSpecificTests(logger, new HeadlessAutomationTestAppController(logger));
            return tests;
        }

        protected CommonTests GetNonAppiumCommonTests()
        {
            Logger logger = new Logger(this.TestContext);
            CommonTests tests = new CommonTests(logger, new HeadlessAutomationTestAppController(logger),
                seedCacheInteractively: false /* the headless test app supports signing in non-interactively with user name and password */);
            return tests;
        }

    }
}
