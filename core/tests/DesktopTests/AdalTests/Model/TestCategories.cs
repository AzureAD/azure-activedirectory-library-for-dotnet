namespace Microsoft.Identity.AutomationTests
{
    public class TestCategories
    {
        public class TestUse
        {
            /// <summary>
            /// A test marked with this category will run as part of the sanity checks for the test automation repo
            /// </summary>
            public const string AutomationSanity = "AutomationSanity";
            public const string Bvt = "Bvt";
            public const string Automation = "Automation";
        }

        public class Api
        {
            public const string AcquireToken = "AcquireToken";
            public const string AcquireTokenSilentWithMultipleUsers = "AcquireTokenSilentWithMultipleUsers";
            public const string AcquireTokenSilentWithValidAccessToken = "AcquireTokenSilentWithValidAccessToken";
            public const string AcquireTokenSilentByRefreshToken = "AcquireTokenSilentWithValidAccessToken";
            public const string AcquireTokenSilentWithRefreshTokenRejected = "AcquireTokenSilentWithRefreshTokenRejected";
            public const string AcquireTokenPromptAuto = "AcquireTokenPromptAuto";
            public const string AcquireTokenPromptAlwaysNoHint = "AcquireTokenInteractiveWithPromptAlwaysNoHint";
            public const string AcquireTokenPromptAlwaysWithHint = "AcquireTokenInteractiveWithPromptAlwaysWithHint";
            public const string AcquireTokenSilentWithFamilyRefreshToken = "AcquireTokenSilentWithFamilyRefreshToken";
            public const string AcquireTokenSilentWithMultiResourceRefreshToken = "AcquireTokenSilentWithMultiResourceRefreshToken";
            public const string AcquireTokenInteractiveUsingNtlm = "AcquireTokenInteractiveUsingNtlm";
            public const string AcquireTokenWithDeviceAuthCode = "AcquireTokenWithDeviceAuthCode";
            public const string AcquireTokenSilentWithCrossTenant = "AcquireTokenSilentWithCrossTenant";
            public const string AcquireTokenWithUserCredentials = "AcquireTokenWithUserCredentials";
            public const string AcquireTokenPromptNever = "AcquireTokenPromptNever";
        }

        public class Federation
        {
            public const string AdfsV3 = "ADFSv3";
            public const string AdfsV2 = "ADFSv2";
            public const string AdfsV4 = "ADFSv4";
            public const string Shibboleth = "Shibboleth";
            public const string Managed = "Managed";
        }

        public class UserAttributeCollection
        {
            public const string Federated = "Federated";
        }

        public class BehaveAs
        {
            public const string Office = "Office";
            public const string OneDrive = "OneDrive";
            public const string Ntlm = "Ntlm";
        }

        public class Platform
        {
            public const string Android = "Android";
            public const string Ios = "Ios";
            public const string Desktop = "Desktop";
        }
    }
}
