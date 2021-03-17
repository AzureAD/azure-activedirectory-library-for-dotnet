//----------------------------------------------------------------------
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
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.OAuth2;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    // This is a helper class and its functionality is not tied to ADAL. It uses a vanilla HttpClient

    /// <summary>
    /// Contains authentication parameters based on unauthorized response from resource server.
    /// </summary>
    public sealed class AuthenticationParameters
    {
        private const string AuthenticateHeader = "WWW-Authenticate";
        private const string Bearer = "bearer";
        private const string AuthorityKey = "authorization_uri";
        private const string ResourceKey = "resource_id";

        /// <summary>
        /// Gets or sets the address of the authority to issue token.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the target resource that is the recipient of the requested token.
        /// </summary>
        public string Resource { get; set; }

        static AuthenticationParameters()
        {
            ModuleInitializer.EnsureModuleInitialized();
        }

        /// <summary>
        /// Ctor
        /// </summary>
        [Obsolete("Please use the static builders of this class, not the constructor", false)]
        public AuthenticationParameters(string authority, string resource)
        {
            Authority = authority;
            Resource = resource;
        }

        /// <summary>
        /// Sends a GET request to the url provided with no Authenticate header. If a 401 Unauthorized is received, this helper will parse the WWW-Authenticate header to 
        /// retrieve the authority and resource.
        /// </summary>
        /// <param name="resourceUrl">Address of the resource</param>
        /// <returns>AuthenticationParameters object containing authentication parameters</returns>
        /// <remarks>Most protected APIs, including those owned by Microsoft, no longer advertise a resource. Authentication should be done using MSAL, which uses scopes. See https://aka.ms/msal-net-migration-adal-msal </remarks>
        [Obsolete("Please use the static version of this method - CreateFromUrlAsync", false)]
        public async Task<AuthenticationParameters> CreateFromResourceUrlAsync(Uri resourceUrl)
        {
            var result = await CreateFromUrlAsync(resourceUrl).ConfigureAwait(false);
            this.Authority = result.Authority;
            this.Resource = result.Resource;

            return this;
        }

        /// <summary>
        /// Sends a GET request to the url provided with no Authenticate header. If a 401 Unauthorized is received, this helper will parse the WWW-Authenticate header to 
        /// retrieve the authority and resource.
        /// </summary>
        /// <param name="resourceUrl">Address of the resource</param>
        /// <returns>AuthenticationParameters object containing authentication parameters</returns>
        /// <remarks>Most protected APIs, including those owned by Microsoft, no longer advertise a resource. Authentication should be done using MSAL, which uses scopes. See https://aka.ms/msal-net-migration-adal-msal </remarks>
        public static async Task<AuthenticationParameters> CreateFromUrlAsync(Uri resourceUrl)
        {
            if (resourceUrl == null)
            {
                throw new ArgumentNullException("resourceUrl");
            }

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, resourceUrl))
                .ConfigureAwait(false);


            return await CreateFromUnauthorizedResponseAsync(response).ConfigureAwait(false);
        }

        /// <summary>
        /// Looks at the Http response for an WWW-Authenticate header and parses it to retrieve the authority and resource</summary>
        /// <param name="responseMessage">Response received from the resource (e.g. via an http call using HttpClient).</param>
        /// <returns>AuthenticationParameters object containing authentication parameters</returns>
        /// <remarks>Most protected APIs, including those owned by Microsoft, no longer advertise a resource. Authentication should be done using MSAL, which uses scopes. See https://aka.ms/msal-net-migration-adal-msal </remarks>
        public static async Task<AuthenticationParameters> CreateFromUnauthorizedResponseAsync(
            HttpResponseMessage responseMessage)
        {
            return CreateFromUnauthorizedResponseCommon(await OAuthClient.CreateResponseAsync(responseMessage)
                .ConfigureAwait(false));
        }

        /// <summary>
        /// Creates authentication parameters from the WWW-Authenticate header in response received from resource. This method expects the header to contain authentication parameters.
        /// </summary>
        /// <param name="authenticateHeader">Content of header WWW-Authenticate header</param>
        /// <returns>AuthenticationParameters object containing authentication parameters</returns>
        /// <remarks>Most protected APIs, including those owned by Microsoft, no longer advertise a resource. Authentication should be done using MSAL, which uses scopes. See https://aka.ms/msal-net-migration-adal-msal </remarks>        
        public static AuthenticationParameters CreateFromResponseAuthenticateHeader(string authenticateHeader)
        {
            if (string.IsNullOrWhiteSpace(authenticateHeader))
            {
                throw new ArgumentNullException("authenticateHeader");
            }

            authenticateHeader = authenticateHeader.Trim();

            // This also checks for cases like "BearerXXXX authorization_uri=...." and "Bearer" and "Bearer "
            if (!authenticateHeader.StartsWith(Bearer, StringComparison.OrdinalIgnoreCase)
                || authenticateHeader.Length < Bearer.Length + 2
                || !char.IsWhiteSpace(authenticateHeader[Bearer.Length]))
            {
                var ex = new ArgumentException(AdalErrorMessage.InvalidAuthenticateHeaderFormat,
                    nameof(authenticateHeader));
                CoreLoggerBase.Default.Error(AdalErrorMessage.InvalidAuthenticateHeaderFormat);
                CoreLoggerBase.Default.ErrorPii(ex);
                throw ex;
            }

            authenticateHeader = authenticateHeader.Substring(Bearer.Length).Trim();

            IDictionary<string, string> authenticateHeaderItems;
            try
            {
                authenticateHeaderItems =
                    EncodingHelper.ParseKeyValueListStrict(authenticateHeader, ',', false, true, null);
            }
            catch (ArgumentException ex)
            {
                var newEx = new ArgumentException(AdalErrorMessage.InvalidAuthenticateHeaderFormat,
                    nameof(authenticateHeader), ex);
                CoreLoggerBase.Default.Error(AdalErrorMessage.InvalidAuthenticateHeaderFormat);
                CoreLoggerBase.Default.ErrorPii(newEx);
                throw newEx;
            }

            string param;
            authenticateHeaderItems.TryGetValue(AuthorityKey, out param);
            string authority = param;
            authenticateHeaderItems.TryGetValue(ResourceKey, out param);
            string resource = param;

#pragma warning disable CS0618 // Type or member is obsolete
            return new AuthenticationParameters(authority, resource);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static AuthenticationParameters CreateFromUnauthorizedResponseCommon(IHttpWebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            AuthenticationParameters authParams;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (response.Headers.Contains(AuthenticateHeader))
                {
                    authParams = CreateFromResponseAuthenticateHeader(response.Headers.GetValues(AuthenticateHeader).FirstOrDefault());
                }
                else
                {
                    var ex = new ArgumentException(AdalErrorMessage.MissingAuthenticateHeader, "response");
                    CoreLoggerBase.Default.ErrorPii(ex);
                    throw ex;
                }
            }
            else
            {
                var ex = new ArgumentException(AdalErrorMessage.UnauthorizedHttpStatusCodeExpected, "response");
                CoreLoggerBase.Default.ErrorPii(ex);
                throw ex;
            }

            return authParams;
        }
    }
}