using System;
using System.Collections.Generic;

namespace Microsoft.Identity.AutomationTests.Model
{
    public class Application
    {
        public string ClientId { get; set; }

        public Dictionary<string, Uri> RedirectUris { get; set; }
    }
}
