using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Identity.Core.Telemetry
{
    internal class XMsTelemInfo
    {
        public string Version { get; set; }
        public string ServerErrorCode { get; set; }
        public string ServerSubErrorCode { get; set; }
        public string TokenAge { get; set; }
        public string SpeInfo { get; set; }
    }

    internal static class XMsTelemInfoHelper
    {
        public static XMsTelemInfo parseXMsTelemHeader(string headerValue, RequestContext requestContext)
        {
            if (String.IsNullOrEmpty(headerValue))
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

            XMsTelemInfo xMsTelemInfo = new XMsTelemInfo();
            xMsTelemInfo.Version = headerVersion;

            if (headerVersion.Equals("1"))
            {
                // Verify the expected format "<version>, <error_code>, <sub_error_code>, <token_age>, <ring>"
                Regex headerFormat = new Regex(@"^[1-9]+\.?[0-9|\\.]*,[0-9|\\.]*,[0-9|\\.]*,[^,]*[0-9\\.]*,[^,]*$");
                MatchCollection formatMatcher = headerFormat.Matches(headerValue);

                if (formatMatcher.Count < 1)
                {
                    requestContext.Logger.WarningPii(
                            string.Format(CultureInfo.InvariantCulture,
                            "x-ms-clitelem header '{0}' does not match the expected format", headerValue),
                            "Malformed x-ms-clitelem header");
                    return null;
                }

                int ErrorCodeIndex = 1;
                int SubErrorCodeIndex = 2;
                int TokenAgeIndex = 3;
                int SpeInfoIndex = 4;

                xMsTelemInfo.ServerErrorCode = headerSegments[ErrorCodeIndex];
                xMsTelemInfo.ServerSubErrorCode = headerSegments[SubErrorCodeIndex];
                xMsTelemInfo.TokenAge = headerSegments[TokenAgeIndex];
                xMsTelemInfo.SpeInfo = headerSegments[SpeInfoIndex];
            }
            else
            {
                requestContext.Logger.Warning(
                    String.Format(CultureInfo.InvariantCulture,
                    "Header version '{0}' unrecognized", headerVersion));
                return null;
            }
            return xMsTelemInfo;
        }
    }
}
