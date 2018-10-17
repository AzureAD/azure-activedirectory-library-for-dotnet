using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Identity.Core.Telemetry
{
    internal class XMsTelemetryInfo
    {
        public string Version { get; set; }
        public string ServerErrorCode { get; set; }
        public string ServerSubErrorCode { get; set; }
        public string TokenAge { get; set; }
        public string SpeInfo { get; set; }
    }

    internal static class XMsTelemInfoHelper
    {
        public static XMsTelemetryInfo parseXMsTelemHeader(string headerValue, RequestContext requestContext)
        {
            if (string.IsNullOrEmpty(headerValue))
            {
                return null;
            }

            string[] headerSegments = headerValue.Split(',');
            if (headerSegments.Length == 0)
            {
                requestContext.Logger.Warning("Malformed x-ms-clitelem header");
                return null;
            }
            string headerVersion = headerSegments[0];

            XMsTelemetryInfo xMsTelemetryInfo = new XMsTelemetryInfo();
            xMsTelemetryInfo.Version = headerVersion;

            if (headerVersion.Equals("1"))
            {
                // Verify the expected format "<version>, <error_code>, <sub_error_code>, <token_age>, <ring>"
                Regex headerFormat = new Regex(@"^[1-9]+\.?[0-9|\\.]*,[0-9|\\.]*,[0-9|\\.]*,[^,]*[0-9\\.]*,[^,]*$");
                MatchCollection formatMatcher = headerFormat.Matches(headerValue);

                if (formatMatcher.Count < 1)
                {
                    requestContext.Logger.Warning(
                            string.Format(CultureInfo.InvariantCulture,
                            "x-ms-clitelem header '{0}' does not match the expected format", headerValue));
                    return null;
                }

                int ErrorCodeIndex = 1;
                int SubErrorCodeIndex = 2;
                int TokenAgeIndex = 3;
                int SpeInfoIndex = 4;

                xMsTelemetryInfo.ServerErrorCode = headerSegments[ErrorCodeIndex];
                xMsTelemetryInfo.ServerSubErrorCode = headerSegments[SubErrorCodeIndex];
                xMsTelemetryInfo.TokenAge = headerSegments[TokenAgeIndex];
                xMsTelemetryInfo.SpeInfo = headerSegments[SpeInfoIndex];
            }
            else
            {
                requestContext.Logger.Warning(
                    string.Format(CultureInfo.InvariantCulture,
                    "Header version '{0}' unrecognized", headerVersion));
                return null;
            }
            return xMsTelemetryInfo;
        }
    }
}
