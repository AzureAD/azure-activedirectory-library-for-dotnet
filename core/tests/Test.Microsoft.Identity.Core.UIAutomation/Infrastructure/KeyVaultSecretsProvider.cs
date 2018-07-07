﻿
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace Test.Microsoft.Identity.Core.UIAutomation.infrastructure
{
    public class KeyVaultSecretsProvider
    {
        /// <summary>
        /// Token cache used by the test infrastructure when authenticating against KeyVault
        /// </summary>
        /// <remarks>We aren't using the default cache to make sure the tokens used by this
        /// test infrastructure can't end up in the cache being used by the tests (the UI-less
        /// Desktop test app runs in the same AppDomain as the infrastructure and uses the
        /// default cache).</remarks>
        private readonly static TokenCache keyVaultTokenCache = new TokenCache();

        private KeyVaultClient _keyVaultClient;

        private ClientAssertionCertificate _assertionCert;

        private KeyVaultConfiguration _config;

        private readonly string keyVaultClientID = "ebe49c8f-61de-4357-9194-7a786f6402b4";

        private readonly string keyVaultThumbPrint = "440A5BE6C4BE2FF02A0ADBED1AAA43D6CF12E269";

        /// <summary>Initialize the secrets provider with the "keyVault" configuration section.</summary>
        /// <remarks>
        /// <para>
        /// Authentication using <see cref="KeyVaultAuthenticationType.ClientCertificate"/>
        ///     1. Register Azure AD application of "Web app / API" type.
        ///        To set up certificate based access to the application PowerShell should be used.
        ///     2. Add an access policy entry to target Key Vault instance for this application.
        ///
        ///     The "keyVault" configuration section should define:
        ///         "authType": "ClientCertificate"
        ///         "clientId": [client ID]
        ///         "certThumbprint": [certificate thumbprint]
        /// </para>
        /// <para>
        /// Authentication using <see cref="KeyVaultAuthenticationType.UserCredential"/>
        ///     1. Register Azure AD application of "Native" type.
        ///     2. Add to 'Required permissions' access to 'Azure Key Vault (AzureKeyVault)' API.
        ///     3. When you run your native client application, it will automatically prompt user to enter Azure AD credentials.
        ///     4. To successfully access keys/secrets in the Key Vault, the user must have specific permissions to perform those operations.
        ///        This could be achieved by directly adding an access policy entry to target Key Vault instance for this user
        ///        or an access policy entry for an Azure AD security group of which this user is a member of.
        ///
        ///     The "keyVault" configuration section should define:
        ///         "authType": "UserCredential"
        ///         "clientId": [client ID]
        /// </para>
        /// </remarks>
        public KeyVaultSecretsProvider()
        {
            _config = new KeyVaultConfiguration();
            _config.AuthType = KeyVaultAuthenticationType.ClientCertificate;
            _config.ClientId = keyVaultClientID;
            _config.CertThumbprint = keyVaultThumbPrint;
            _keyVaultClient = new KeyVaultClient(AuthenticationCallbackAsync);
        }

        public SecretBundle GetSecret(string secretUrl)
        {
            return _keyVaultClient.GetSecretAsync(secretUrl).GetAwaiter().GetResult();
        }

        private async Task<string> AuthenticationCallbackAsync(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority, keyVaultTokenCache);

            AuthenticationResult authResult;
            switch (_config.AuthType)
            {
                case KeyVaultAuthenticationType.ClientCertificate:
                    if (_assertionCert == null)
                    {
                        var cert = CertificateHelper.FindCertificateByThumbprint(_config.CertThumbprint);
                        _assertionCert = new ClientAssertionCertificate(_config.ClientId, cert);
                    }
                    authResult = await authContext.AcquireTokenAsync(resource, _assertionCert);
                    break;
                case KeyVaultAuthenticationType.UserCredential:
                    authResult = await authContext.AcquireTokenAsync(resource, _config.ClientId, new UserCredential());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return authResult?.AccessToken;
        }
    }
}
