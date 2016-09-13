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
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal enum AuthorityValidationType
    {
        True,
        False,
        NotProvided
    }

    /// <summary>
    /// The AuthenticationContext class retrieves authentication tokens from Azure Active Directory and ADFS services.
    /// </summary>
    public sealed class AuthenticationContext
    {
        internal Authenticator Authenticator;

        static AuthenticationContext()
        {
            PlatformPlugin.Logger.Information(null, string.Format(CultureInfo.CurrentCulture, "ADAL {0} with assembly version '{1}', file version '{2}' and informational version '{3}' is running...",
                PlatformPlugin.PlatformInformation.GetProductName(), AdalIdHelper.GetAdalVersion(), AdalIdHelper.GetAssemblyFileVersion(), AdalIdHelper.GetAssemblyInformationalVersion()));
        }

        /// <summary>
        /// Constructor to create the context with the address of the authority.
        /// Using this constructor will turn ON validation of the authority URL by default if validation is supported for the authority address.
        /// </summary>
        /// <param name="authority">Address of the authority to issue token.</param>
        public AuthenticationContext(string authority)
            : this(authority, AuthorityValidationType.NotProvided, TokenCache.DefaultShared)
        {
        }

        /// <summary>
        /// Constructor to create the context with the address of the authority and flag to turn address validation off.
        /// Using this constructor, address validation can be turned off. Make sure you are aware of the security implication of not validating the address.
        /// </summary>
        /// <param name="authority">Address of the authority to issue token.</param>
        /// <param name="validateAuthority">Flag to turn address validation ON or OFF.</param>
        public AuthenticationContext(string authority, bool validateAuthority)
            : this(authority, validateAuthority ? AuthorityValidationType.True : AuthorityValidationType.False, TokenCache.DefaultShared)
        {
        }

        /// <summary>
        /// Constructor to create the context with the address of the authority.
        /// Using this constructor will turn ON validation of the authority URL by default if validation is supported for the authority address.
        /// </summary>
        /// <param name="authority">Address of the authority to issue token.</param>
        /// <param name="tokenCache">Token cache used to lookup cached tokens on calls to AcquireToken</param>
        public AuthenticationContext(string authority, TokenCache tokenCache)
            : this(authority, AuthorityValidationType.NotProvided, tokenCache)
        {
        }

        /// <summary>
        /// Constructor to create the context with the address of the authority and flag to turn address validation off.
        /// Using this constructor, address validation can be turned off. Make sure you are aware of the security implication of not validating the address.
        /// </summary>
        /// <param name="authority">Address of the authority to issue token.</param>
        /// <param name="validateAuthority">Flag to turn address validation ON or OFF.</param>
        /// <param name="tokenCache">Token cache used to lookup cached tokens on calls to AcquireToken</param>
        public AuthenticationContext(string authority, bool validateAuthority, TokenCache tokenCache)
            : this(authority, validateAuthority ? AuthorityValidationType.True : AuthorityValidationType.False, tokenCache)
        {
        }

        private AuthenticationContext(string authority, AuthorityValidationType validateAuthority, TokenCache tokenCache)
        {
            // If authorityType is not provided (via first constructor), we validate by default (except for ASG and Office tenants).
            this.Authenticator = new Authenticator(authority, (validateAuthority != AuthorityValidationType.False));

            this.TokenCache = tokenCache;
        }
        /// <summary>
        /// Used to set the flag for AAD extended lifetime
        /// </summary>
        public bool ExtendedLifeTimeEnabled { get; set; }

        /// <summary>
        /// Gets address of the authority to issue token.
        /// </summary>
        public string Authority
        {
            get
            {
                return this.Authenticator.Authority;
            }
        }

        /// <summary>
        /// Gets a value indicating whether address validation is ON or OFF.
        /// </summary>
        public bool ValidateAuthority
        {
            get
            {
                return this.Authenticator.ValidateAuthority;
            }
        }

        /// <summary>
        /// Property to provide ADAL's token cache. Depending on the platform, TokenCache may have a default persistent cache or not. 
        /// Library will automatically save tokens in default TokenCache whenever you obtain them. Cached tokens will be available only to the application that saved them. 
        /// If the cache is persistent, the tokens stored in it will outlive the application's execution, and will be available in subsequent runs.
        /// To turn OFF token caching, set TokenCache to null. 
        /// </summary>
        public TokenCache TokenCache { get; private set; }

        /// <summary>
        /// Gets or sets correlation Id which would be sent to the service with the next request. 
        /// Correlation Id is to be used for diagnostics purposes. 
        /// </summary>
        public Guid CorrelationId
        {
            get
            {
                return this.Authenticator.CorrelationId;
            }

            set
            {
                this.Authenticator.CorrelationId = value;                
            }
        }
             
        /// <summary>
        /// Acquires device code from the authority.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<DeviceCodeResult> AcquireDeviceCodeAsync(string resource, string clientId)
        {
            return await this.AcquireDeviceCodeAsync(resource, clientId, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Acquires device code from the authority.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="extraQueryParameters">This parameter will be appended as is to the query string in the HTTP authentication request to the authority. The parameter can be null.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<DeviceCodeResult> AcquireDeviceCodeAsync(string resource, string clientId, string extraQueryParameters)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            var handler = new AcquireDeviceCodeHandler(this.Authenticator, resource, clientId, extraQueryParameters,requestId);
            DeviceCodeResult result = await handler.RunHandlerAsync().ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, null, null, "606");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority using an device code previously received.
        /// This method does not lookup token cache, but stores the result in it, so it can be looked up using other methods such as <see cref="AuthenticationContext.AcquireTokenSilentAsync(string, string, UserIdentifier)"/>.
        /// </summary>
        /// <param name="deviceCodeResult">The device code result received from calling AcquireDeviceCodeAsync.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenByDeviceCodeAsync(DeviceCodeResult deviceCodeResult)
        {
            RequestData requestData = new RequestData
            {
                Authenticator = this.Authenticator,
                TokenCache = this.TokenCache,
                ExtendedLifeTimeEnabled = this.ExtendedLifeTimeEnabled,
                Resource = deviceCodeResult.Resource,
                ClientKey = new ClientKey(deviceCodeResult.ClientId),
                RequestId = Telemetry.GetInstance().RegisterNewRequest()
            };

            Telemetry.GetInstance().StartEvent(requestData.RequestId, "api_event");
            if (deviceCodeResult == null)
            {
                throw new ArgumentNullException("deviceCodeResult");
            }

            var handler = new AcquireTokenByDeviceCodeHandler(requestData, deviceCodeResult);
            AuthenticationResult result = await handler.RunAsync().ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo ,result.TenantId, "611");
            apiEvent.CorrelationId = requestData.CorrelationId;
            Telemetry.GetInstance().StopEvent(requestData.RequestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="userAssertion">The assertion to use for token acquisition.</param>
        /// <returns>It contains Access Token and the Access Token's expiration time. Refresh Token property will be null for this overload.</returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, string clientId, UserAssertion userAssertion)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenCommonAsync(resource, clientId, userAssertion, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "700");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientCredential">The client credential to use for token acquisition.</param>
        /// <returns>It contains Access Token and the Access Token's expiration time. Refresh Token property will be null for this overload.</returns>        
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenForClientCommonAsync(resource, new ClientKey(clientCredential), requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "706");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientCertificate">The client certificate to use for token acquisition.</param>
        /// <returns>It contains Access Token and the Access Token's expiration time. Refresh Token property will be null for this overload.</returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, IClientAssertionCertificate clientCertificate)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenForClientCommonAsync(resource, new ClientKey(clientCertificate, this.Authenticator), requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "711");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientAssertion">The client assertion to use for token acquisition.</param>
        /// <returns>It contains Access Token and the Access Token's expiration time. Refresh Token property will be null for this overload.</returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientAssertion clientAssertion)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenForClientCommonAsync(resource, new ClientKey(clientAssertion), requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "716");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority using authorization code previously received.
        /// This method does not lookup token cache, but stores the result in it, so it can be looked up using other methods such as <see cref="AuthenticationContext.AcquireTokenSilentAsync(string, string, UserIdentifier)"/>.
        /// </summary>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="clientCredential">The credential to use for token acquisition.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeAsync(string authorizationCode, Uri redirectUri, ClientCredential clientCredential)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenByAuthorizationCodeCommonAsync(authorizationCode, redirectUri, new ClientKey(clientCredential), null, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "800");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority using an authorization code previously received.
        /// This method does not lookup token cache, but stores the result in it, so it can be looked up using other methods such as <see cref="AuthenticationContext.AcquireTokenSilentAsync(string, string, UserIdentifier)"/>.
        /// </summary>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="clientCredential">The credential to use for token acquisition.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token. It can be null if provided earlier to acquire authorizationCode.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeAsync(string authorizationCode, Uri redirectUri, ClientCredential clientCredential, string resource)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenByAuthorizationCodeCommonAsync(authorizationCode, redirectUri, new ClientKey(clientCredential), resource, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "806");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority using an authorization code previously received.
        /// This method does not lookup token cache, but stores the result in it, so it can be looked up using other methods such as <see cref="AuthenticationContext.AcquireTokenSilentAsync(string, string, UserIdentifier)"/>.
        /// </summary>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="redirectUri">The redirect address used for obtaining authorization code.</param>
        /// <param name="clientAssertion">The client assertion to use for token acquisition.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeAsync(string authorizationCode, Uri redirectUri, ClientAssertion clientAssertion)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenByAuthorizationCodeCommonAsync(authorizationCode, redirectUri, new ClientKey(clientAssertion), null, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "811");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority using an authorization code previously received.
        /// This method does not lookup token cache, but stores the result in it, so it can be looked up using other methods such as <see cref="AuthenticationContext.AcquireTokenSilentAsync(string, string, UserIdentifier)"/>.
        /// </summary>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="redirectUri">The redirect address used for obtaining authorization code.</param>
        /// <param name="clientAssertion">The client assertion to use for token acquisition.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token. It can be null if provided earlier to acquire authorizationCode.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeAsync(string authorizationCode, Uri redirectUri, ClientAssertion clientAssertion, string resource)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenByAuthorizationCodeCommonAsync(authorizationCode, redirectUri, new ClientKey(clientAssertion), resource, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "816");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority using an authorization code previously received.
        /// This method does not lookup token cache, but stores the result in it, so it can be looked up using other methods such as <see cref="AuthenticationContext.AcquireTokenSilentAsync(string, string, UserIdentifier)"/>.
        /// </summary>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="redirectUri">The redirect address used for obtaining authorization code.</param>
        /// <param name="clientCertificate">The client certificate to use for token acquisition.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeAsync(string authorizationCode, Uri redirectUri, IClientAssertionCertificate clientCertificate)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenByAuthorizationCodeCommonAsync(authorizationCode, redirectUri, new ClientKey(clientCertificate, this.Authenticator), null, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "821");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority using an authorization code previously received.
        /// This method does not lookup token cache, but stores the result in it, so it can be looked up using other methods such as <see cref="AuthenticationContext.AcquireTokenSilentAsync(string, string, UserIdentifier)"/>.
        /// </summary>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="redirectUri">The redirect address used for obtaining authorization code.</param>
        /// <param name="clientCertificate">The client certificate to use for token acquisition.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token. It can be null if provided earlier to acquire authorizationCode.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeAsync(string authorizationCode, Uri redirectUri, IClientAssertionCertificate clientCertificate, string resource)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenByAuthorizationCodeCommonAsync(authorizationCode, redirectUri, new ClientKey(clientCertificate, this.Authenticator), resource, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "826");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires an access token from the authority on behalf of a user. It requires using a user token previously received.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientCredential">The client credential to use for token acquisition.</param>
        /// <param name="userAssertion">The user assertion (token) to use for token acquisition.</param>
        /// <returns>It contains Access Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential, UserAssertion userAssertion)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenOnBehalfCommonAsync(resource, new ClientKey(clientCredential), userAssertion, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "500");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires an access token from the authority on behalf of a user. It requires using a user token previously received.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientCertificate">The client certificate to use for token acquisition.</param>
        /// <param name="userAssertion">The user assertion (token) to use for token acquisition.</param>
        /// <returns>It contains Access Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, IClientAssertionCertificate clientCertificate, UserAssertion userAssertion)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenOnBehalfCommonAsync(resource, new ClientKey(clientCertificate, this.Authenticator), userAssertion, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "506");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires an access token from the authority on behalf of a user. It requires using a user token previously received.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientAssertion">The client assertion to use for token acquisition.</param>
        /// <param name="userAssertion">The user assertion (token) to use for token acquisition.</param>
        /// <returns>It contains Access Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientAssertion clientAssertion, UserAssertion userAssertion)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenOnBehalfCommonAsync(resource, new ClientKey(clientAssertion), userAssertion, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "511");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token without asking for user credential.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time. If acquiring token without user credential is not possible, the method throws AdalException.</returns>
        public async Task<AuthenticationResult> AcquireTokenSilentAsync(string resource, string clientId)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenSilentAsync(resource, clientId, UserIdentifier.AnyUser).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "1");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token without asking for user credential.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="userId">Identifier of the user token is requested for. This parameter can be <see cref="UserIdentifier"/>.Any.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time. If acquiring token without user credential is not possible, the method throws AdalException.</returns>
        public async Task<AuthenticationResult> AcquireTokenSilentAsync(string resource, string clientId, UserIdentifier userId)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenSilentCommonAsync(resource, new ClientKey(clientId), userId, null, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "3");
            apiEvent.SetEvent(EventConstants.LoginHint, PlatformPlugin.CryptographyHelper.CreateSha256Hash(userId.Id));
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token without asking for user credential.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="userId">Identifier of the user token is requested for. This parameter can be <see cref="UserIdentifier"/>.Any.</param>
        /// <param name="parameters">Instance of PlatformParameters containing platform specific arguments and information.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time. If acquiring token without user credential is not possible, the method throws AdalException.</returns>
        public async Task<AuthenticationResult> AcquireTokenSilentAsync(string resource, string clientId, UserIdentifier userId, IPlatformParameters parameters)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenSilentCommonAsync(resource, new ClientKey(clientId), userId, parameters, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "9");
            apiEvent.SetEvent(EventConstants.LoginHint, PlatformPlugin.CryptographyHelper.CreateSha256Hash(userId.Id));
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token without asking for user credential.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientCredential">The client credential to use for token acquisition.</param>
        /// <param name="userId">Identifier of the user token is requested for. This parameter can be <see cref="UserIdentifier"/>.Any.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time. If acquiring token without user credential is not possible, the method throws AdalException.</returns>
        public async Task<AuthenticationResult> AcquireTokenSilentAsync(string resource, ClientCredential clientCredential, UserIdentifier userId)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenSilentCommonAsync(resource, new ClientKey(clientCredential), userId, null, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "10");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token without asking for user credential.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientCertificate">The client certificate to use for token acquisition.</param>
        /// <param name="userId">Identifier of the user token is requested for. This parameter can be <see cref="UserIdentifier"/>.Any.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time. If acquiring token without user credential is not possible, the method throws AdalException.</returns>
        public async Task<AuthenticationResult> AcquireTokenSilentAsync(string resource, IClientAssertionCertificate clientCertificate, UserIdentifier userId)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenSilentCommonAsync(resource, new ClientKey(clientCertificate, this.Authenticator), userId, null, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "11");
            apiEvent.SetEvent(EventConstants.LoginHint, PlatformPlugin.CryptographyHelper.CreateSha256Hash(userId.Id));
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token without asking for user credential.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientAssertion">The client assertion to use for token acquisition.</param>
        /// <param name="userId">Identifier of the user token is requested for. This parameter can be <see cref="UserIdentifier"/>.Any.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time. If acquiring token without user credential is not possible, the method throws AdalException.</returns>
        public async Task<AuthenticationResult> AcquireTokenSilentAsync(string resource, ClientAssertion clientAssertion, UserIdentifier userId)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenSilentCommonAsync(resource, new ClientKey(clientAssertion), userId, null, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "12");
            apiEvent.SetEvent(EventConstants.LoginHint, PlatformPlugin.CryptographyHelper.CreateSha256Hash(userId.Id));
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Gets URL of the authorize endpoint including the query parameters.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="userId">Identifier of the user token is requested for. This parameter can be <see cref="UserIdentifier"/>.Any.</param>
        /// <param name="extraQueryParameters">This parameter will be appended as is to the query string in the HTTP authentication request to the authority. The parameter can be null.</param>
        /// <returns>URL of the authorize endpoint including the query parameters.</returns>
        public async Task<Uri> GetAuthorizationRequestUrlAsync(string resource, string clientId, Uri redirectUri, UserIdentifier userId, string extraQueryParameters)
        {
            RequestData requestData = new RequestData
            {
                Authenticator = this.Authenticator,
                TokenCache = this.TokenCache,
                Resource = resource,
                ClientKey = new ClientKey(clientId),
                ExtendedLifeTimeEnabled = ExtendedLifeTimeEnabled
            };
            var handler = new AcquireTokenInteractiveHandler(requestData, redirectUri, null, userId, extraQueryParameters, null);
            return await handler.CreateAuthorizationUriAsync(this.CorrelationId).ConfigureAwait(false);
        }

        /// <summary>
        /// Acquires security token from the authority.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="parameters">An object of type PlatformParameters which may pass additional parameters used for authorization.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, string clientId, Uri redirectUri, IPlatformParameters parameters)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenCommonAsync(resource, clientId, redirectUri, parameters, UserIdentifier.AnyUser, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "140");
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="parameters">An object of type PlatformParameters which may pass additional parameters used for authorization.</param>
        /// <param name="userId">Identifier of the user token is requested for. If created from DisplayableId, this parameter will be used to pre-populate the username field in the authentication form. Please note that the end user can still edit the username field and authenticate as a different user. 
        /// If you want to be notified of such change with an exception, create UserIdentifier with type RequiredDisplayableId. This parameter can be <see cref="UserIdentifier"/>.Any.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, string clientId, Uri redirectUri, IPlatformParameters parameters, UserIdentifier userId)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenCommonAsync(resource, clientId, redirectUri, parameters, userId, requestId).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "146");
            apiEvent.SetEvent(EventConstants.LoginHint, PlatformPlugin.CryptographyHelper.CreateSha256Hash(userId.Id));
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        /// <summary>
        /// Acquires security token from the authority.
        /// </summary>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="userId">Identifier of the user token is requested for. If created from DisplayableId, this parameter will be used to pre-populate the username field in the authentication form. Please note that the end user can still edit the username field and authenticate as a different user. 
        /// If you want to be notified of such change with an exception, create UserIdentifier with type RequiredDisplayableId. This parameter can be <see cref="UserIdentifier"/>.Any.</param>
        /// <param name="parameters">Parameters needed for interactive flow requesting authorization code. Pass an instance of PlatformParameters.</param>
        /// <param name="extraQueryParameters">This parameter will be appended as is to the query string in the HTTP authentication request to the authority. The parameter can be null.</param>
        /// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource, string clientId, Uri redirectUri, IPlatformParameters parameters, UserIdentifier userId, string extraQueryParameters)
        {
            string requestId = Telemetry.GetInstance().RegisterNewRequest();
            Telemetry.GetInstance().StartEvent(requestId, "api_event");
            AuthenticationResult result = await this.AcquireTokenCommonAsync(resource, clientId, redirectUri, parameters, userId, requestId,extraQueryParameters).ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "151");
            apiEvent.SetEvent(EventConstants.LoginHint, PlatformPlugin.CryptographyHelper.CreateSha256Hash(userId.Id));
            Telemetry.GetInstance().StopEvent(requestId, apiEvent, "api_event");
            return result;
        }

        private async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeCommonAsync(string authorizationCode, Uri redirectUri, ClientKey clientKey, string resource, string requestId)
        {
            const string nullResource = "null_resource_as_optional";
            RequestData requestData = new RequestData
            {
                Authenticator = this.Authenticator,
                TokenCache = this.TokenCache,
                Resource = resource,
                ClientKey = clientKey,
                ExtendedLifeTimeEnabled = this.ExtendedLifeTimeEnabled,
                RequestId = requestId
            };
            if (requestData.Resource == null)
            {
                requestData.Resource = nullResource;
            }
            var handler = new AcquireTokenByAuthorizationCodeHandler(requestData, authorizationCode, redirectUri);
            return await handler.RunAsync().ConfigureAwait(false);
        }

        private async Task<AuthenticationResult> AcquireTokenForClientCommonAsync(string resource, ClientKey clientKey, string requestId)
        {
            RequestData requestData = new RequestData
            {
                Authenticator = this.Authenticator,
                TokenCache = this.TokenCache,
                Resource = resource,
                ClientKey = clientKey,
                ExtendedLifeTimeEnabled = this.ExtendedLifeTimeEnabled,
                SubjectType = TokenSubjectType.Client,
                RequestId = requestId
            };
            var handler = new AcquireTokenForClientHandler(requestData);
            return await handler.RunAsync().ConfigureAwait(false);
        }

        private async Task<AuthenticationResult> AcquireTokenOnBehalfCommonAsync(string resource, ClientKey clientKey, UserAssertion userAssertion, string requestId)
        {
            RequestData requestData = new RequestData
            {
                Authenticator = this.Authenticator,
                TokenCache = this.TokenCache,
                Resource = resource,
                ClientKey = clientKey,
                ExtendedLifeTimeEnabled = this.ExtendedLifeTimeEnabled,
                RequestId = requestId
            };

            var handler = new AcquireTokenOnBehalfHandler(requestData, userAssertion);
            return await handler.RunAsync().ConfigureAwait(false);
        }

        internal IWebUI CreateWebAuthenticationDialog(IPlatformParameters parameters)
        {
            return PlatformPlugin.WebUIFactory.CreateAuthenticationDialog(parameters);
        }

        internal async Task<AuthenticationResult> AcquireTokenCommonAsync(string resource, string clientId, UserCredential userCredential)
        {
            RequestData requestData = new RequestData
            {
                Authenticator = this.Authenticator,
                TokenCache = this.TokenCache,
                Resource = resource,
                ClientKey = new ClientKey(clientId),
                ExtendedLifeTimeEnabled = this.ExtendedLifeTimeEnabled,
                RequestId = Telemetry.GetInstance().RegisterNewRequest(),
            };
            Telemetry.GetInstance().StartEvent(requestData.RequestId, "api_event");
            var handler = new AcquireTokenNonInteractiveHandler(requestData, userCredential);
            AuthenticationResult result = await handler.RunAsync().ConfigureAwait(false);
            ApiEvent apiEvent = new ApiEvent(this.Authenticator, result.UserInfo, result.TenantId, "721");
            apiEvent.CorrelationId = this.Authenticator.CorrelationId;
            Telemetry.GetInstance().StopEvent(requestData.RequestId, apiEvent, "api_event");
            return result;
        }

        private async Task<AuthenticationResult> AcquireTokenCommonAsync(string resource, string clientId, UserAssertion userAssertion, string requestId)
        {
            RequestData requestData = new RequestData
            {
                Authenticator = this.Authenticator,
                TokenCache = this.TokenCache,
                Resource = resource,
                ClientKey = new ClientKey(clientId),
                ExtendedLifeTimeEnabled = this.ExtendedLifeTimeEnabled,
                RequestId = requestId
            };
            var handler = new AcquireTokenNonInteractiveHandler(requestData, userAssertion);
            return await handler.RunAsync().ConfigureAwait(false);
        }
        
        private async Task<AuthenticationResult> AcquireTokenCommonAsync(string resource, string clientId, Uri redirectUri, IPlatformParameters parameters, UserIdentifier userId, string requestId,string extraQueryParameters = null)
        {
            RequestData requestData = new RequestData
            {
                Authenticator = this.Authenticator,
                TokenCache = this.TokenCache,
                Resource = resource,
                ClientKey = new ClientKey(clientId),
                ExtendedLifeTimeEnabled = this.ExtendedLifeTimeEnabled,
                RequestId = requestId
            };
            var handler = new AcquireTokenInteractiveHandler(requestData, redirectUri, parameters, userId, extraQueryParameters, this.CreateWebAuthenticationDialog(parameters));
            return await handler.RunAsync().ConfigureAwait(false);
        }

        private async Task<AuthenticationResult> AcquireTokenSilentCommonAsync(string resource, ClientKey clientKey, UserIdentifier userId, IPlatformParameters parameters, string requestId)
        {
            RequestData requestData = new RequestData
            {
                Authenticator = Authenticator,
                TokenCache = this.TokenCache,
                Resource = resource,
                ExtendedLifeTimeEnabled = this.ExtendedLifeTimeEnabled,
                ClientKey = clientKey,
                RequestId = requestId
            };

            var handler = new AcquireTokenSilentHandler(requestData, userId, parameters);
            return await handler.RunAsync().ConfigureAwait(false);
        }
    }
}
