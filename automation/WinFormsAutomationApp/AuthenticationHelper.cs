﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Diagnostics;

namespace WinFormsAutomationApp
{
    internal static class AuthenticationHelper
    {
        static AuthenticationHelper()
        {
            LoggerCallbackHandler.Callback = new AdalLoggerCallBack();
        }       

        public static async Task<string> AcquireToken(Dictionary<string, string> input)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            AuthenticationContext ctx = new AuthenticationContext(input["authority"]);
            try
            {
                string s = input.ContainsKey("prompt_behavior") ? input["prompt_behavior"] : null;
                AuthenticationResult result =
                    await
                        ctx.AcquireTokenAsync(input["resource"], input["client_id"], new Uri(input["redirect_uri"]),
                            GetPlatformParametersInstance(s)).ConfigureAwait(false);                
                res.Add("unique_id", result.UserInfo.UniqueId);
                res.Add("access_token", result.AccessToken);
                res.Add("tenant_id", result.TenantId);
                res.Add("refresh_token", TokenCache.DefaultShared.tokenCacheDictionary[new TokenCacheKey(input["authority"], input["resource"], input["client_id"], TokenSubjectType.User, result.UserInfo)].RefreshToken);
           
            }
            catch (Exception exc)
            {
                res.Add("error", exc.Message);
            }
            return FromDictionaryToJson(res);
        }

        public static async Task<string> AcquireTokenSilent(Dictionary<string, string> input)
        {
            AuthenticationContext ctx = new AuthenticationContext(input["authority"]);
            Dictionary<string, string> res = new Dictionary<string, string>();
            try
            {
                AuthenticationResult result = await ctx.AcquireTokenSilentAsync(input["resource"], input["client_id"]).ConfigureAwait(false);
                res.Add("unique_id", result.UserInfo.UniqueId);
                res.Add("access_token", result.AccessToken);
                res.Add("tenant_id", result.TenantId);
                res.Add("refresh_token", TokenCache.DefaultShared.tokenCacheDictionary[new TokenCacheKey(input["authority"], input["resource"], input["client_id"], TokenSubjectType.User, result.UserInfo)].RefreshToken);
            }
            catch (Exception exc)
            {
                res.Add("error", exc.Message);
            }
            return FromDictionaryToJson(res);
        }

        public static async Task<string> ExpireAccessToken(Dictionary<string, string> input)
        {
            bool notifiedBeforeAccessCache = false;
            Task<string> myTask = Task<string>.Factory.StartNew(() =>
            {
            
                List<KeyValuePair<TokenCacheKey, AuthenticationResultEx>> CacheItems = QueryCache(input["authority"],
                    input["client_id"], input["user_identifier"]);

                foreach (KeyValuePair<TokenCacheKey, AuthenticationResultEx> item in CacheItems)
                {
                    // if resource was passed to cache lookup, then only expire token for that resource.
                    // otherwise expire all matching access tokens.
                    if (input["resource"] == null || item.Key.ResourceEquals(input["resource"]))
                    {
                            NotifyBeforeAccessCache(item.Key.Resource, item.Key.ClientId,item.Value.Result.UserInfo.UniqueId, item.Value.Result.UserInfo.DisplayableId);
                            notifiedBeforeAccessCache = true;
                            item.Value.Result.ExpiresOn = DateTime.UtcNow;                        
                            TokenCache.DefaultShared.tokenCacheDictionary[item.Key] = item.Value;                        
                            TokenCache.DefaultShared.StoreToCache(item.Value, item.Key.Authority, item.Key.Resource, item.Key.ClientId, item.Key.TokenSubjectType, new CallState(new Guid()));
                            if (notifiedBeforeAccessCache)
                            {
                                NotifyAfterAccessCache(item.Key.Resource, item.Key.ClientId, item.Value.Result.UserInfo.UniqueId, item.Value.Result.UserInfo.DisplayableId);
                                notifiedBeforeAccessCache = false;
                            }
                        }
                }
                Dictionary<string, string> output = new Dictionary<string, string>();
                //Send back error if userId or displayableId is not sent back to the user
                output.Add("expired_access_token_count", CacheItems.Count.ToString());
                return output.FromDictionaryToJson();
                             
            });

            return await myTask.ConfigureAwait(false);
        }

        public static async Task<string> InvalidateRefreshTokens(Dictionary<string, string> input)
        {
            bool notifiedBeforeAccessCache = false;
            Dictionary<string, string> output = new Dictionary<string, string>();
            Task<string> myTask = Task<string>.Factory.StartNew(() =>
            {
                try
                {
                    List<KeyValuePair<TokenCacheKey, AuthenticationResultEx>> CacheItems = QueryCache(input["authority"],
                    input["client_id"], input["user_identifier"]);

                    foreach (KeyValuePair<TokenCacheKey, AuthenticationResultEx> item in CacheItems)
                    {
                        NotifyBeforeAccessCache(item.Key.Resource, item.Key.ClientId, item.Value.Result.UserInfo.UniqueId, item.Value.Result.UserInfo.DisplayableId);
                        notifiedBeforeAccessCache = true;
                        item.Value.RefreshToken = "bad_refresh_token";
                        item.Value.Result.ExpiresOn = DateTime.UtcNow;
                        TokenCache.DefaultShared.tokenCacheDictionary[item.Key] = item.Value;

                        TokenCache.DefaultShared.StoreToCache(item.Value, item.Key.Authority, item.Key.Resource, item.Key.ClientId, item.Key.TokenSubjectType, new CallState(new Guid()));
                        if (notifiedBeforeAccessCache)
                        {
                            NotifyAfterAccessCache(item.Key.Resource, item.Key.ClientId, item.Value.Result.UserInfo.UniqueId, item.Value.Result.UserInfo.DisplayableId);
                            notifiedBeforeAccessCache = false;
                        }
                    }
                    //Send back error if userId or displayableId is not sent back to the user
                    output.Add("invalidated_refresh_token_count", CacheItems.Count.ToString());                   
                }
                catch (Exception exc)
                {
                    output.Add("error", exc.Message); 
                }
                return output.FromDictionaryToJson();
            });

            return await myTask.ConfigureAwait(false);
        }

        private static List<KeyValuePair<TokenCacheKey, AuthenticationResultEx>> QueryCache(string authority,
            string clientId, string displayableId)
        {
            return TokenCache.DefaultShared.tokenCacheDictionary.Where(
                p =>
                    (string.IsNullOrWhiteSpace(authority) || p.Key.Authority == authority)
                    && (string.IsNullOrWhiteSpace(clientId) || p.Key.ClientIdEquals(clientId))
                    && (string.IsNullOrWhiteSpace(displayableId) || p.Key.DisplayableIdEquals(displayableId))).ToList();
        }

        public static async Task<string> ReadCache(Dictionary<string, string> input)
        {
            Task<string> myTask = Task<string>.Factory.StartNew(() =>
            {                
                int count = TokenCache.DefaultShared.Count;
                Dictionary<string, string> output = new Dictionary<string, string>();
                 output.Add("item_count", count.ToString());
                var list = TokenCache.DefaultShared.ReadItems();
                if (list.Any())
                {
                    output.Add("AccessToken", ((list.ToList())[0]).AccessToken);
                }
                return FromDictionaryToJson(output);
            });
            return await myTask.ConfigureAwait(false);
        }

        public static async Task<string> ClearCache()
        {
            Task<string> myTask = Task<string>.Factory.StartNew(() =>
            {
                int count = TokenCache.DefaultShared.Count;
                Dictionary<string, string> output = new Dictionary<string, string>();
                output.Add("item_count", count.ToString());
                TokenCache.DefaultShared.Clear();
                output.Add("cache_clear_status", "Cleared the entire cache");
                return output.ToJson();
            });
            return await myTask.ConfigureAwait(false);
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

        public static string FromDictionaryToJson(this Dictionary<string, string> dictionary)
        {
            var kvs = dictionary.Select(kvp => string.Format("\"{0}\":\"{1}\"", kvp.Key, string.Concat(kvp.Value)));
            return string.Concat("{", string.Join(",", kvs), "}");
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

        private static IPlatformParameters GetPlatformParametersInstance(string prompt)
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
            if (PromptBehavior.Always.ToString() == prompt)
            {
                platformParameters = new PlatformParameters(PromptBehavior.Always, null);
            }
            else
            {
                platformParameters = new PlatformParameters(PromptBehavior.Auto, false);
            }
#endif
#endif
#endif
            return platformParameters;
        }

        private static void NotifyBeforeAccessCache(string resource,string clientid, string uniqueid,string displayableid)
        {
            TokenCache.DefaultShared.OnBeforeAccess(new TokenCacheNotificationArgs
            {
                TokenCache = TokenCache.DefaultShared,
                Resource = resource,
                ClientId = clientid,
                UniqueId = uniqueid,
                DisplayableId = displayableid
            });
        }

        private static void NotifyAfterAccessCache(string resource, string clientid, string uniqueid, string displayableid)
        {
            TokenCache.DefaultShared.OnAfterAccess(new TokenCacheNotificationArgs
            {
                TokenCache = TokenCache.DefaultShared,
                Resource = resource,
                ClientId = clientid,
                UniqueId = uniqueid,
                DisplayableId = displayableid
            });
        }
    }
}
