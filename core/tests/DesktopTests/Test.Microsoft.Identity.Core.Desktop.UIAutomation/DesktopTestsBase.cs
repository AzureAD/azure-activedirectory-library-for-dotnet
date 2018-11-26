using Microsoft.Identity.AutomationTests;
using Microsoft.Identity.AutomationTests.Model;

namespace DesktopTests
{
    public class DesktopTestsBase : TestsBase
    {
        protected override PlatformType PlatformType => PlatformType.Desktop;

        protected CommonTests GetAppiumCommonTests()
        {
            return new CommonTests(Logger, new AutomationTestAppController(Logger, DeviceSession),
                seedCacheInteractively: false /* the WinForms test app supports signing in non-interactively with user name and password */);
        }

        protected DesktopSpecificTests GetAppiumDesktopSpecificTests()
        {
            return new DesktopSpecificTests(Logger, new AutomationTestAppController(Logger, DeviceSession));
        }
    }
}
