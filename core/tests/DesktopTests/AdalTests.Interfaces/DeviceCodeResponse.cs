namespace Microsoft.Identity.AutomationTests
{
    public class DeviceCodeResponse : ResponseBase
    {
        public DeviceCodeResponse(string response, string resultLogs)
            : base(response, resultLogs) { }

        public string GetVerificationUrl() => GetPropertyAsString("verification_url");

        public string GetUserCode() => GetPropertyAsString("user_code");

        public string GetDeviceCode() => GetPropertyAsString("device_code");
    }
}
