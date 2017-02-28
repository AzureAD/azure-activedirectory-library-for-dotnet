using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace WinFormsAutomationApp
{
    public class AdalLoggerCallBack : IAdalLogCallback
    {
        public static string Message { get; private set; }

        public void Log(LogLevel level, string message)
        {
            Message += string.Format("[{0}] - {1} \r\n",level.ToString(),message);
        }
    }
}
