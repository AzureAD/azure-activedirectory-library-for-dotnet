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
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Identity.Core.Realm;

namespace Microsoft.Identity.Core.WsTrust
{
    internal class CommonNonInteractiveHandler
    {
        private readonly RequestContext requestContext;
        private readonly IUsernameInput usernameInput;
        private readonly IPlatformProxy platformProxy;

        public CommonNonInteractiveHandler(RequestContext requestContext, IUsernameInput usernameInput)
        {
            this.requestContext = requestContext;
            this.usernameInput = usernameInput;
            this.platformProxy = PlatformProxyFactory.GetPlatformProxy();
        }

        /// <summary>
        /// Gets the currently logged in user. Works for Windows when user is AD or AAD joined. Throws otherwise if cannot be found.
        /// </summary>
        public async Task<string> GetPlatformUserAsync()
        {
            var logger = this.requestContext.Logger;
            string platformUsername = await this.platformProxy.GetUserPrincipalNameAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(platformUsername))
            {
                _requestContext.Logger.Error("Could not find UPN for logged in user.");

                throw CoreExceptionFactory.Instance.GetClientException(
                    CoreErrorCodes.UnknownUser,
                    CoreErrorMessages.UnknownUser);
            }

            _requestContext.Logger.InfoPii($"Logged in user detected with user name '{platformUsername}'", "Logged in user detected");
            return platformUsername;
        }

        public async Task<WsTrustResponse> QueryWsTrustAsync(
            MexParser mexParser,
            UserRealmDiscoveryResponse userRealmResponse,
            Func<string, WsTrustAddress, IUsernameInput, string> wsTrustMessageBuilder)
        {
            WsTrustAddress wsTrustAddress = await QueryForWsTrustAddressAsync(
                userRealmResponse,
                mexParser).ConfigureAwait(false);

            return await QueryWsTrustAsync(
                wsTrustMessageBuilder,
                userRealmResponse.CloudAudienceUrn,
                wsTrustAddress).ConfigureAwait(false);
        }

        public async Task<UserRealmDiscoveryResponse> QueryUserRealmDataAsync(string userRealmUriPrefix)
        {
            var userRealmResponse = await UserRealmDiscoveryResponse.CreateByDiscoveryAsync(
                userRealmUriPrefix,
                _usernameInput.UserName,
                _requestContext).ConfigureAwait(false);

            if (userRealmResponse == null)
            {
                throw CoreExceptionFactory.Instance.GetClientException(
                    CoreErrorCodes.UserRealmDiscoveryFailed,
                    CoreErrorMessages.UserRealmDiscoveryFailed);
            }

            _requestContext.Logger.InfoPii(
                string.Format(
                    CultureInfo.CurrentCulture,
                    " User with user name '{0}' detected as '{1}'",
                    _usernameInput.UserName,
                    userRealmResponse.AccountType),
                string.Empty);

            return userRealmResponse;
        }

        private async Task<WsTrustResponse> QueryWsTrustAsync(
            Func<string, WsTrustAddress, IUsernameInput, string> wsTrustMessageBuilder,
            string cloudAudience,
            WsTrustAddress wsTrustAddress)
        {
            try
            {
                string wsTrustRequest = wsTrustMessageBuilder(
                    cloudAudience,
                    wsTrustAddress,
                    _usernameInput);

                WsTrustResponse wsTrustResponse = await WsTrustRequest.SendRequestAsync(
                    wsTrustAddress,
                    wsTrustRequest.ToString(),
                    _requestContext).ConfigureAwait(false);

                _requestContext.Logger.Info(string.Format(CultureInfo.CurrentCulture,
                    " Token of type '{0}' acquired from WS-Trust endpoint", wsTrustResponse.TokenType));

                return wsTrustResponse;
            }
            catch (Exception ex)
            {
                throw CoreExceptionFactory.Instance.GetClientException(
                    CoreErrorCodes.ParsingWsTrustResponseFailed,
                    ex.Message,
                    ex);
            }
        }

        private async Task<WsTrustAddress> QueryForWsTrustAddressAsync(
            UserRealmDiscoveryResponse userRealmResponse,
            MexParser mexParser)
        {
            if (string.IsNullOrWhiteSpace(userRealmResponse.FederationMetadataUrl))
            {
                throw CoreExceptionFactory.Instance.GetClientException(
                    CoreErrorCodes.MissingFederationMetadataUrl,
                    CoreErrorMessages.MissingFederationMetadataUrl);
            }

            try
            {
                WsTrustAddress wsTrustAddress = await mexParser.FetchWsTrustAddressFromMexAsync(
                    userRealmResponse.FederationMetadataUrl).ConfigureAwait(false);

                if (wsTrustAddress == null)
                {
                    CoreExceptionFactory.Instance.GetClientException(
                      CoreErrorCodes.WsTrustEndpointNotFoundInMetadataDocument,
                      CoreErrorMessages.WsTrustEndpointNotFoundInMetadataDocument);
                }

                _requestContext.Logger.InfoPii(
                    string.Format(CultureInfo.CurrentCulture, " WS-Trust endpoint '{0}' fetched from MEX at '{1}'",
                        wsTrustAddress.Uri, userRealmResponse.FederationMetadataUrl),
                    "Fetched and parsed MEX");

                return wsTrustAddress;
            }
            catch (XmlException ex)
            {
                throw CoreExceptionFactory.Instance.GetClientException(
                    CoreErrorCodes.ParsingWsMetadataExchangeFailed,
                    CoreErrorMessages.ParsingMetadataDocumentFailed,
                    ex);
            }
        }
    }
}