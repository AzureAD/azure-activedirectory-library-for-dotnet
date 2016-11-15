using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace WinFormsAutomationApp
{
    internal static class AuthenticationHelper
    {
        public static async Task<string> AcquireToken(Dictionary<string, string> input)
        {
            AuthenticationContext ctx = new AuthenticationContext(input["authority"]);
            string output = string.Empty;
            try
            {
                AuthenticationResult result = await ctx.AcquireTokenAsync(input["resource"], input["client_id"], new Uri(input["redirect_uri"]), GetPlatformParametersInstance());
                output = result.ToJson();
            }
            catch (Exception exc)
            {
                output = exc.Message;
            }

            return output;
        }

        public static async Task<string> AcquireTokenSilent(Dictionary<string, string> input)
        {
            AuthenticationContext ctx = new AuthenticationContext(input["authority"]);
            string output = string.Empty;
            try
            {
                AuthenticationResult result = await ctx.AcquireTokenSilentAsync(input["resource"], input["client_id"]);
                output = result.ToJson();
            }
            catch (Exception exc)
            {
                output = exc.Message;
            }

            return output;
        }
        
        public static Task<string> ExpireAccessToken(Dictionary<string, string> input)
        {
            return null;
            //TokenCache.DefaultShared.
        }

        public static string ToJson(this object obj)
        {
            using (MemoryStream mstream = new MemoryStream())
            {
                DataContractJsonSerializer serializer =
                    new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(mstream, obj);
                mstream.Position = 0;

                using (StreamReader reader = new StreamReader(mstream))
                {
                    return JsonOutputFormat(reader.ReadToEnd());
                }
            }
        }

        public static Dictionary<string, string> CreateDictionaryFromJson(string json)
        {
            var jss = new JavaScriptSerializer();
            return jss.Deserialize<Dictionary<string, string>>(json); 
        }

        private static string JsonOutputFormat(string result)
        {
            Dictionary<string, string> jsonDictitionary = new Dictionary<string, string>();
            jsonDictitionary.Add("AccessTokenType", "access_token_type");
            jsonDictitionary.Add("AccessToken", "access_token");
            jsonDictitionary.Add("ExpiresOn", "expires_on");
            jsonDictitionary.Add("ExtendedExpiresOn", "extended_expires_on");
            jsonDictitionary.Add("ExtendedLifeTimeToken", "extended_lifetime_token");
            jsonDictitionary.Add("IdToken", "id_token");
            jsonDictitionary.Add("TenantId", "tenant_id");
            jsonDictitionary.Add("UserInfo", "user_info");
            jsonDictitionary.Add("DisplayableId", "displayable_id");
            jsonDictitionary.Add("FamilyName", "family_name");
            jsonDictitionary.Add("GivenName", "given_name");
            jsonDictitionary.Add("IdentityProvider", "identity_provider");
            jsonDictitionary.Add("PasswordChangeUrl", "password_change_url");
            jsonDictitionary.Add("PasswordExpiresOn", "password_expires_on");
            jsonDictitionary.Add("UniqueId", "unique_id");

            foreach (KeyValuePair<string, string> entry in jsonDictitionary)
            {
                if (result.Contains(entry.Key))
                {
                    result = result.Replace(entry.Key, entry.Value);
                }
            }

            return result;
        }

        private static IPlatformParameters GetPlatformParametersInstance()
        {
            IPlatformParameters platformParameters = null;

#if __ANDROID__
        platformParameters = new PlatformParameters(this);
#else
#if __IOS__
        platformParameters = new PlatformParameters(this);
#else
#if (WINDOWS_UWP || WINDOWS_APP)
            platformParameters = new PlatformParameters(PromptBehavior.Always, false);
#else
            //desktop
            platformParameters = new PlatformParameters(PromptBehavior.Auto, null);
#endif
#endif
#endif
            return platformParameters;
        }
    }
}
