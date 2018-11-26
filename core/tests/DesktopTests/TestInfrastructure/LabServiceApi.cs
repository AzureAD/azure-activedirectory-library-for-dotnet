using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Labs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Identity.AutomationTests
{
    /// <summary>
    /// Wrapper for new lab service API
    /// </summary>
    public class LabServiceApi : ILabService
    {
        public IEnumerable<IUser> GetUsers(UserQueryParameters query)
        {
            LabUser user = new LabUser();
            WebClient webClient = new WebClient();

            //Disabled for now until there are tests that use it.
            webClient.QueryString.Add("mamca", "false");
            webClient.QueryString.Add("mdmca", "false");

            //Building user query
            if (query.FederationProvider != null)
                webClient.QueryString.Add("federationProvider", query.FederationProvider.ToString());

                webClient.QueryString.Add("mam", query.IsMamUser != null && (bool)(query.IsMamUser) ? "true" : "false");

                webClient.QueryString.Add("mfa", query.IsMfaUser != null && (bool)(query.IsMfaUser) ? "true" : "false");

            if (query.Licenses != null && query.Licenses.Count > 0)
                webClient.QueryString.Add("license", query.Licenses.ToArray().ToString());

                webClient.QueryString.Add("isFederated", query.IsFederatedUser != null && (bool)(query.IsFederatedUser) ? "true" : "false");

            if (query.IsUserType != null)
                webClient.QueryString.Add("usertype", query.IsUserType.ToString());

                webClient.QueryString.Add("external", query.IsExternalUser != null && (bool)(query.IsExternalUser) ? "true" : "false");

            //Fetch user
            string result = webClient.DownloadString("http://api.msidlab.com/api/user");

            user = JsonConvert.DeserializeObject<LabResponse>(result).Users;

            if (user == null)            
                user =  JsonConvert.DeserializeObject<LabUser>(result);

            if (!String.IsNullOrEmpty(user.HomeTenantId) && !String.IsNullOrEmpty(user.HomeUPN))
                user.InitializeHomeUser();

            yield return user;
        }
    }
}