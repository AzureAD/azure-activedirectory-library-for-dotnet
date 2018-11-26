using System;
using System.IdentityModel.Tokens.Jwt;

namespace Microsoft.Identity.AutomationTests
{
    public class AuthenticationResponse : ResponseBase
    {
        public AuthenticationResponse(string response, string resultLogs)
            : base(response, resultLogs) { }

        public string GetUniqueId() => GetPropertyAsString("unique_id");

        public Guid? GetTenantId()
        {
            var guidStr = GetPropertyAsString("tenant_id");

            Guid guid;
            if (Guid.TryParse(guidStr, out guid))
            {
                return guid;
            }

            return null;
        }

        public JwtSecurityToken GetAccessToken()
        {
            var encodedToken = GetPropertyAsString("access_token");

            if (!string.IsNullOrWhiteSpace(encodedToken))
            {
                return new JwtSecurityToken(encodedToken);
            }

            return null;
        }
    }
}
