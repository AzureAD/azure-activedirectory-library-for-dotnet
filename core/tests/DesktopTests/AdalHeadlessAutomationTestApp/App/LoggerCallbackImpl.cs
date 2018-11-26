using System.Text;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

// COPIED FROM ADAL.NET repo test automation app
// https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/blob/dev/automation/WinFormsAutomationApp/LoggerCallbackImpl.cs

namespace WinFormsAutomationApp
{
    class LoggerCallbackImpl
    {
        private StringBuilder logCollector = new StringBuilder();
        public void Log(LogLevel level, string message, bool containsPii)
        {
            logCollector.AppendLine(message);
        }

        public string GetAdalLogs()
        {
            return logCollector.ToString();
        }
    }
}
