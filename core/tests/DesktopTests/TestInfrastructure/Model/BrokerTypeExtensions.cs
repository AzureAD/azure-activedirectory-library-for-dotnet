using System.Collections.Generic;

namespace Microsoft.Identity.AutomationTests.Model
{
    public static class BrokerTypeExtensions
    {
        private static readonly IDictionary<PlatformType, IDictionary<BrokerType, string>>
            PlatformTypeToBundleIdMapping = new Dictionary<PlatformType, IDictionary<BrokerType, string>>
            {
                {
                    PlatformType.Android,
                    new Dictionary<BrokerType, string>
                    {
                        {
                            BrokerType.None, "NONE"
                        },
                        {
                            BrokerType.AzureAuthenticator, "com.azure.authenticator"
                        },
                        {
                            BrokerType.CompanyPortal, "com.microsoft.windowsintune.companyportal"
                        }
                    }
                }
            };

        public static string BundleId(this BrokerType brokerType, PlatformType platformType)
        {
            return PlatformTypeToBundleIdMapping[platformType][brokerType];
        }
    }
}