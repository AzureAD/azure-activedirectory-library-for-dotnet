using System;
using Microsoft.Identity.AutomationTests.Configuration;
using Microsoft.Identity.Labs;

namespace Microsoft.Identity.AutomationTests
{
    public static class UserExtensions
    {
        public static string GetPassword(this IUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return KeyVaultSecretsProvider.GetSecret(user.CredentialUrl).Value;
        }
    }
}
