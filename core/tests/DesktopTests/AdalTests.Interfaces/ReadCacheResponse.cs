using System;
using System.Collections.Generic;

namespace Microsoft.Identity.AutomationTests
{
    public class ReadCacheResponse : ResponseBase
    {
        public ReadCacheResponse(string response, string resultLogs)
        : base(response, resultLogs) { }

        // #TODO: Implement GetAccessToken

        public string GetRefreshToken()
        {
            return GetPropertyAsString("refresh_token");
        }

        public DateTime GetExpireTime()
        {
            //return DateTime.Parse((string) GetResponseAsDictionary()["expires_on"]);
            return (DateTime) GetResponseAsDictionary()["expires_on"];
        }

        public bool HasExpireTime()
        {
            return GetResponseAsDictionary().ContainsKey("expires_on");
        }

    }
}