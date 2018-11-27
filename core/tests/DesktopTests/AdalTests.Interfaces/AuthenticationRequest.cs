using System;
using System.Collections.Generic;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Microsoft.Identity.AutomationTests
{
    public class AuthenticationRequest : IAuthenticationRequest
    {
        public const string AuthorityBaseUrl = "https://login.microsoftonline.com/";
        public const string CommonAuthority = "https://login.microsoftonline.com/common/";

        public ApplicationType ApplicationType { get; set; }

        public string Authority { get; set; } = "https://login.microsoftonline.com/common";

        public ResourceType ResourceType { get; set; }

        public BrokerType BrokerType { get; set; }

        public LabUser User { get; set; }

        public PromptBehavior? PromptBehavior { get; set; }

        public IDictionary<string, string> AdditionalInfo { get; set; } = new Dictionary<string, string>();

        public bool UseRedirectUri { get; set; } = true;

        public static string CreateSpecificAuthorityUriFromGuid(string tenantGuid)
        {
            return $"https://login.microsoftonline.com/{tenantGuid}/";
        }
    }
}
