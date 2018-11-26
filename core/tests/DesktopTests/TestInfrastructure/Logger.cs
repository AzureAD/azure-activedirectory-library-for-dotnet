using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Identity.AutomationTests
{
    /// <summary>
    /// A logging class that should be initialized / disposed of for each test (e.g. via TestInitialize / TestCleanup).
    /// It uses the TestContext to log messages and also to add result files.
    /// </summary>
    public class Logger
    {
        private readonly TestContext _testContext;

        public Logger(TestContext testContext)
        {
            if (testContext == null)
            {
                throw new ArgumentNullException(nameof(testContext));
            }

            _testContext = testContext;
        }

        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public void LogError(string message)
        {
            Log("ERROR", message);
        }

        public void LogError(string message, Exception ex)
        {
            LogError(message);
            LogError($"Exception details: {ex.Message}");
            LogError(ex.StackTrace);
        }

        /// <summary>
        /// Adds a file to the logs. The file name will be changed.
        /// </summary>
        public void AddFile(string path, string newFileName)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"Could not find the file: {path}");
            }

            // add the name of the test to the filename to avoid collisions
            string newPath = GetUniqueTestResultsPath(newFileName);

            File.Move(path, newPath);

            LogInfo($"Logger: Adding result file '{newFileName}'");
            _testContext.AddResultFile(newPath);
        }

        public void AddContentAsLogFile(string content, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                this.LogError($"Could not create log file {fileName} because there is no content for it");
                return;
            }

            // Write content to temp file
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, content);

            // Add file to results
            AddFile(tempFilePath, fileName);
        }

        private string GetUniqueTestResultsPath(string fileName)
        {
            string dir = _testContext.ResultsDirectory;
            string fileNoExt = Path.GetFileNameWithoutExtension(fileName);
            string fileExt = Path.GetExtension(fileName);

            string path = Path.Combine(dir, fileNoExt + fileExt);

            for (int i = 1; i < 10000; ++i)
            {
                if (!File.Exists(path))
                {
                    return path;
                }

                path = Path.Combine(dir, $"{fileNoExt}-{i}{fileExt}");
            }

            throw new InvalidOperationException("Too many files in the test results");
        }

        private void Log(string messageLevel, string message)
        {
            string fullMessage = $"{DateTime.Now:HH:mm:ss.fff} {messageLevel}: {message}";
            _testContext.WriteLine(EscapeTestContextMessage(fullMessage));
        }

        private static string EscapeTestContextMessage(string message)
        {
            return message.Replace("{", "{{").Replace("}", "}}");
        }
    }
}