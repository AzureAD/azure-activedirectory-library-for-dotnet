// ------------------------------------------------------------------------------
// 
// Copyright (c) Microsoft Corporation.
// All rights reserved.
// 
// This code is licensed under the MIT License.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// ------------------------------------------------------------------------------

using Microsoft.Identity.Client.CacheV2;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConfidentialClientApplicationBuilder : MsalConfigurationBuilder<ConfidentialClientApplicationBuilder>
    {
        internal ConfidentialClientApplicationBuilder(MsalConfiguration msalConfiguration)
            : base(msalConfiguration)
        {
        }

        /// <summary>
        ///     Read the configuration from a JSON stream according to the schema.
        /// </summary>
        /// <param name="json">The JSON to parse in string format.</param>
        /// <returns>An MsalConfigurationBuilder object which can be further modified.</returns>
        public static ConfidentialClientApplicationBuilder CreateFromJson(string json)
        {
            var config = new MsalConfiguration();
            return new ConfidentialClientApplicationBuilder(config);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="redirectUri"></param>
        /// <param name="clientCredential"></param>
        /// <returns></returns>
        public static ConfidentialClientApplicationBuilder Create(
            string clientId,
            string redirectUri,
            object clientCredential)
        {
            var config = new MsalConfiguration
            {
                ClientId = clientId,
                RedirectUri = redirectUri
            };
            return new ConfidentialClientApplicationBuilder(config);
        }

        // TODO: need to figure out #if blocks for this since ClientCredential doesn't exist in mobile scenarios.
        ///// <summary>
        /////     Creates a new MsalConfigurationBuilder with the base required parameters of clientId and redirectUri.
        /////     Other values will be set to defaults and can be overridden by other methods of the MsalConfigurationBuilder.
        ///// </summary>
        ///// <param name="clientId">The ClientId of the application to reference.</param>
        ///// <param name="redirectUri">The RedirectUri of the application to reference.</param>
        ///// <param name="clientCredential"></param>
        ///// <returns></returns>
        //public static ConfidentialClientApplicationBuilder Create(string clientId, string redirectUri, ClientCredential clientCredential)
        //{
        //    var config = new MsalConfiguration
        //    {
        //        ClientId = clientId,
        //        RedirectUri = redirectUri,
        //        ClientCredential = clientCredential

        //    };
        //    return new ConfidentialClientApplicationBuilder(config);
        //}

        /// <summary>
        ///     Sets the desired authorization user agent value.
        /// </summary>
        /// <param name="authorizationUserAgent">Value of the Authorization User Agent.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public ConfidentialClientApplicationBuilder WithAuthorizationUserAgent(AuthorizationUserAgent authorizationUserAgent)
        {
            Config.AuthorizationUserAgent = authorizationUserAgent;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenCache"></param>
        /// <returns></returns>
        public ConfidentialClientApplicationBuilder WithAppTokenCache(TokenCache tokenCache)
        {
            Config.TokenCache = tokenCache;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenCache"></param>
        /// <returns></returns>
        public ConfidentialClientApplicationBuilder WithUserTokenCache(TokenCache tokenCache)
        {
            Config.UserTokenCache = tokenCache;
            return this;
        }

        // TODO: need to figure out #if blocks for this since ClientCredential doesn't exist in mobile scenarios.
        ///// <summary>
        ///// </summary>
        ///// <returns></returns>
        //public IConfidentialClientApplication Build()
        //{
        //    return new ConfidentialClientApplication(Validate());
        //}
    }
}