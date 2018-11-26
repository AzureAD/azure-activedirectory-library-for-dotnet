using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Identity.AutomationTests.Configuration
{
    public class KeyVaultConfiguration
    {
        public KeyVaultConfiguration(IConfigurationSection section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            section.Bind(this);
        }

        /// <summary>
        /// The URL of the Key Vault instance.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The authentication type to use to communicate with the Key Vault.
        /// </summary>
        public KeyVaultAuthenticationType AuthType { get; set; }

        /// <summary>
        /// The ID of the test harness client.
        /// </summary>
        /// <remarks>
        /// This should be configured as as having access to the Key Vault instance specified at <see cref="Url"/>.
        /// </remarks>
        public string ClientId { get; set; }

        /// <summary>
        /// The thumbprint of the <see cref="System.Security.Cryptography.X509Certificates.X509Certificate2"/> to use when
        /// <see cref="AuthType"/> is <see cref="KeyVaultAuthenticationType.ClientCertificate"/>.
        /// </summary>
        public string CertThumbprint { get; set; }
    }
}
