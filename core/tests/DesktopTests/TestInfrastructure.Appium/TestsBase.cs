using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

namespace Microsoft.Identity.AutomationTests
{
    [TestClass]
    public abstract class TestsBase
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

            InitSession();
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            try
            {
                Logger.LogInfo($"Entering Test Cleanup. Outcome: {TestContext.CurrentTestOutcome}");
                if (DeviceSession != null)
                {
                    if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
                    {
                        Logger.LogInfo("Taking failure screenshot");
                        DeviceSession.TakeScreenshot("Failure");
                    }
                }
            }
            finally
            {
                if (DeviceSession != null)
                {
                    DeviceSession.Dispose();
                    TrySaveOlympusLogs(DeviceSession.OlympusLogUri);
                }
            }

            Logger.LogInfo("Test cleanup finished.");
        }

        private void InitSession()
        {
            Assert.IsNotNull(Logger, "Initialize the logger before calling this method");
            try
            {
                DeviceSession = new DeviceSession(Logger, PlatformType);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to initialize the driver.", ex);
                throw;
            }
        }

        private void TrySaveOlympusLogs(Uri uri)
        {
            // Give Olympus a chance!
            Thread.Sleep(1000);
            try
            {
                using (var client = new HttpClient())
                {
                    Logger.LogInfo($"Attempting to fetch Olympus logs from {uri}");

                    var downloadTask = client.GetAsync(uri);
                    downloadTask.Wait();
                    HttpResponseMessage response = downloadTask.Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var contentTask = response.Content.ReadAsStringAsync();
                        string content = contentTask.Result;
                        Logger.AddContentAsLogFile(content, "Olympus.log");
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // This might be the case when not pointing at Olympus or if the session init failed.
                        Logger.LogInfo("Olympus logs do not exist for this session.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to fetch the Olympus logs from {uri}. ${ex.Message}");
            }
        }
    }
}
