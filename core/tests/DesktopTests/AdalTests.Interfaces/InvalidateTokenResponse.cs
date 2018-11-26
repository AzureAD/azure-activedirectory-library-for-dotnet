using System;
using Microsoft.Identity.AutomationTests.Model;

namespace Microsoft.Identity.AutomationTests
{
    public class InvalidateTokenResponse : ResponseBase
    {
        private readonly string _tokensExpiredPropertyName;

        public InvalidateTokenResponse(string response, string resultLogs, TokenType tokenType)
            : base(response, resultLogs)
        {
            switch (tokenType)
            {
                case TokenType.AccessToken:
                    _tokensExpiredPropertyName = "expired_access_token_count";
                    break;
                case TokenType.RefreshToken:
                    _tokensExpiredPropertyName = "invalidated_refresh_token_count";
                    break;
                case TokenType.FamilyRefreshToken:
                    _tokensExpiredPropertyName = "invalidated_family_refresh_token_count";
                    break;
                default:
                    throw new NotImplementedException("Unrecognised token type.");
            }
        }

        public int GetTokensInvalidated()
        {
            return Int32.Parse(GetPropertyAsString(_tokensExpiredPropertyName));
        }
    }
}