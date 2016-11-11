using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AutomationApp
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
                    return reader.ReadToEnd();
                }
            }
        }

        public static Dictionary<string, string> CreateDictionaryFromJson(string json)
        {
            var jss = new JavaScriptSerializer();
            return jss.Deserialize<Dictionary<string, string>>(json); 
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
