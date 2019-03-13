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
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Cache;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.OAuth2;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Broker;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Cache;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.ClientCreds;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Instance;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Flows
{
    internal abstract class AcquireTokenHandlerBase
    {
        private readonly TokenCache _tokenCache;
        private OAuthClient _client = null;

        protected const string NullResource = "null_resource_as_optional";
        protected CacheQueryData CacheQueryData = new CacheQueryData();

        internal /* internal for test, otherwise protected */ RequestContext RequestContext { get; }
        protected IBroker BrokerHelper { get; }
        internal /* internal for test, otherwise protected */ IDictionary<string, string> BrokerParameters { get; }

        protected AcquireTokenHandlerBase(IServiceBundle serviceBundle, RequestData requestData)
        {
            ServiceBundle = serviceBundle;
            Authenticator = requestData.Authenticator;
            _tokenCache = requestData.TokenCache;
            RequestContext = CreateCallState(null, this.Authenticator.CorrelationId);
            BrokerHelper = BrokerFactory.CreateBrokerFacade(RequestContext.Logger);

            RequestContext.Logger.Info(string.Format(CultureInfo.CurrentCulture,
                "ADAL {0} with assembly version '{1}', file version '{2}' and informational version '{3}' is running...",
                PlatformProxyFactory.GetPlatformProxy().GetProductName(), AdalIdHelper.GetAdalVersion(),
                AssemblyUtils.GetAssemblyFileVersionAttribute(), AssemblyUtils.GetAssemblyInformationalVersion()));

            {
                string msg = string.Format(CultureInfo.CurrentCulture,
                    "=== Token Acquisition started: \n\tCacheType: {0}\n\tAuthentication Target: {1}\n\t",
                    _tokenCache != null
                        ? _tokenCache.GetType().FullName +
                          string.Format(CultureInfo.CurrentCulture, " ({0} items)", _tokenCache._tokenCacheDictionary.Count)
                        : "null",
                    requestData.SubjectType);
                if (InstanceDiscovery.IsWhitelisted(requestData.Authenticator.GetAuthorityHost()))
                {
                    msg += string.Format(CultureInfo.CurrentCulture,
                        ", Authority Host: {0}",
                        requestData.Authenticator.GetAuthorityHost());
                }

                var piiMsg = string.Format(CultureInfo.CurrentCulture,
                    "=== Token Acquisition started:\n\tAuthority: {0}\n\tResource: {1}\n\tClientId: {2}\n\tCacheType: {3}\n\tAuthentication Target: {4}\n\t",
                    requestData.Authenticator.Authority, requestData.Resource, requestData.ClientKey.ClientId,
                    (_tokenCache != null)
                        ? _tokenCache.GetType().FullName +
                          string.Format(CultureInfo.CurrentCulture, " ({0} items)", _tokenCache._tokenCacheDictionary.Count)
                        : "null",
                    requestData.SubjectType);
                RequestContext.Logger.InfoPii(piiMsg, msg);
            }

            _tokenCache = requestData.TokenCache;

            if (string.IsNullOrWhiteSpace(requestData.Resource))
            {
                throw new ArgumentNullException("resource");
            }

            Resource = (requestData.Resource != NullResource) ? requestData.Resource : null;
            ClientKey = requestData.ClientKey;
            TokenSubjectType = requestData.SubjectType;

            LoadFromCache = (_tokenCache != null);
            StoreToCache = (_tokenCache != null);
            SupportADFS = false;

            BrokerParameters = new Dictionary<string, string>
            {
                [BrokerParameter.Authority] = requestData.Authenticator.Authority,
                [BrokerParameter.Resource] = requestData.Resource,
                [BrokerParameter.ClientId] = requestData.ClientKey.ClientId,
                [BrokerParameter.CorrelationId] = RequestContext.Logger.CorrelationId.ToString(),
                [BrokerParameter.ClientVersion] = AdalIdHelper.GetAdalVersion()
            };
            ResultEx = null;

            CacheQueryData.ExtendedLifeTimeEnabled = requestData.ExtendedLifeTimeEnabled;
        }

        protected bool SupportADFS { get; set; }

        protected Authenticator Authenticator { get; set; }

        protected string Resource { get; set; }

        protected ClientKey ClientKey { get; private set; }

        protected AdalResultWrapper ResultEx { get; set; }

        protected TokenSubjectType TokenSubjectType { get; private set; }

        protected string UniqueId { get; set; }

        protected string DisplayableId { get; set; }

        protected UserIdentifierType UserIdentifierType { get; set; }

        protected bool LoadFromCache { get; set; }

        protected bool StoreToCache { get; set; }

        protected IServiceBundle ServiceBundle { get; }

        public async Task<AuthenticationResult> RunAsync()
        {
            bool notifiedBeforeAccessCache = false;
            AdalResultWrapper extendedLifetimeResultEx = null;

            try
            {
                await PreRunAsync().ConfigureAwait(false);

                if (LoadFromCache)
                {
                    RequestContext.Logger.Verbose("Loading from cache.");

                    CacheQueryData.Authority = Authenticator.Authority;
                    CacheQueryData.Resource = this.Resource;
                    CacheQueryData.ClientId = this.ClientKey.ClientId;
                    CacheQueryData.SubjectType = this.TokenSubjectType;
                    CacheQueryData.UniqueId = this.UniqueId;
                    CacheQueryData.DisplayableId = this.DisplayableId;

                    this.NotifyBeforeAccessCache();
                    notifiedBeforeAccessCache = true;
                    ResultEx = await _tokenCache.LoadFromCacheAsync(CacheQueryData, RequestContext).ConfigureAwait(false);
                    extendedLifetimeResultEx = ResultEx;

                    // Check if we need to get an AT from the RT
                    if (ResultEx?.Result != null &&
                        ((ResultEx.Result.AccessToken == null && ResultEx.RefreshToken != null) ||
                         (ResultEx.Result.ExtendedLifeTimeToken && ResultEx.RefreshToken != null)))
                    {
                        RequestContext.Logger.Verbose("Refreshing the AT based on the RT.");

                        ResultEx = await RefreshAccessTokenAsync(ResultEx).ConfigureAwait(false);
                        if (ResultEx != null && ResultEx.Exception == null)
                        {
                            notifiedBeforeAccessCache = await StoreResultExToCacheAsync(notifiedBeforeAccessCache).ConfigureAwait(false);
                        }
                    }
                }

                if (ResultEx == null || ResultEx.Exception != null)
                {
                    RequestContext.Logger.Verbose("Either a token was not found or an exception was thrown.");

                    if (BrokerHelper.CanInvokeBroker)
                    {
                        RequestContext.Logger.Verbose("Trying to acquire a token using the broker...");
                        ResultEx = await BrokerHelper.AcquireTokenUsingBrokerAsync(BrokerParameters).ConfigureAwait(false);
                    }
                    else
                    {
                        RequestContext.Logger.Verbose("Cannot invoke the broker directly, may require install ...");

                        await PreTokenRequestAsync().ConfigureAwait(false);
                        // check if broker app installation is required for authentication.
                        await CheckAndAcquireTokenUsingBrokerAsync().ConfigureAwait(false);
                    }

                    //broker token acquisition failed
                    if (ResultEx != null && ResultEx.Exception != null)
                    {
                        RequestContext.Logger.Verbose("Broker token acquisition failed, throwing...");
                        throw ResultEx.Exception;
                    }

                    await this.PostTokenRequestAsync(ResultEx).ConfigureAwait(false);
                    notifiedBeforeAccessCache = await StoreResultExToCacheAsync(notifiedBeforeAccessCache).ConfigureAwait(false);
                }

                // At this point we have an Acess Token - return it
                await this.PostRunAsync(ResultEx.Result).ConfigureAwait(false);
                return new AuthenticationResult(ResultEx.Result);
            }
            catch (Exception ex)
            {
                RequestContext.Logger.ErrorPii(ex);

                if (_client == null)
                {
                    _client = new OAuthClient();
                }

                if (ex.InnerException is AdalServiceException serviceException)
                {
                    if (serviceException.StatusCode >= 500 && serviceException.StatusCode < 600 ||
                    serviceException.StatusCode == 408)
                    {
                        _client.Resiliency = true;
                    }

                    if (_client.Resiliency && extendedLifetimeResultEx != null)
                    {
                        RequestContext.Logger.Info("Refreshing access token failed due to one of these reasons:- Internal Server Error, Gateway Timeout and Service Unavailable. " +
                                           "Hence returning back stale access token");

                        return new AuthenticationResult(extendedLifetimeResultEx.Result);
                    }
                }
                throw;
            }
            finally
            {
                if (notifiedBeforeAccessCache)
                {
                    this.NotifyAfterAccessCache();
                }
            }
        }

        private async Task<bool> StoreResultExToCacheAsync(bool notifiedBeforeAccessCache)
        {
            if (StoreToCache)
            {
                if (!notifiedBeforeAccessCache)
                {
                    NotifyBeforeAccessCache();
                    notifiedBeforeAccessCache = true;
                }

                await _tokenCache.StoreToCacheAsync(ResultEx, Authenticator.Authority, Resource,
                    ClientKey.ClientId, TokenSubjectType, RequestContext).ConfigureAwait(false);
            }
            return notifiedBeforeAccessCache;
        }

        private async Task CheckAndAcquireTokenUsingBrokerAsync()
        {
            RequestContext.Logger.Verbose("Check and AcquireToken using broker ");

            if (BrokerInvocationRequired())
            {
                RequestContext.Logger.Verbose("Broker invocation is required");
                ResultEx = await BrokerHelper.AcquireTokenUsingBrokerAsync(BrokerParameters).ConfigureAwait(false);
            }
            else
            {
                RequestContext.Logger.Verbose("Broker invocation is NOT required");
                ResultEx = await this.SendTokenRequestAsync().ConfigureAwait(false);
            }
        }

        protected virtual void UpdateBrokerParameters(IDictionary<string, string> parameters)
        {
        }

        protected virtual bool BrokerInvocationRequired()
        {
            return false;
        }

        public static RequestContext CreateCallState(string clientId, Guid correlationId)
        {
            correlationId = (correlationId != Guid.Empty) ? correlationId : Guid.NewGuid();
            return new RequestContext(clientId, new AdalLogger(correlationId));
        }

        protected virtual Task PostRunAsync(AdalResult result)
        {
            LogReturnedToken(result);
            return Task.FromResult(false);
        }

        protected virtual async Task PreRunAsync()
        {
            await Authenticator.UpdateFromTemplateAsync(RequestContext).ConfigureAwait(false);
            ValidateAuthorityType();
        }

        protected internal /* internal for test only */ virtual Task PreTokenRequestAsync()
        {
            return Task.FromResult(false);
        }

        protected async Task UpdateAuthorityAsync(string updatedAuthority)
        {
            if (!Authenticator.Authority.Equals(updatedAuthority, StringComparison.OrdinalIgnoreCase))
            {
                await Authenticator.UpdateAuthorityAsync(updatedAuthority, RequestContext, ServiceBundle).ConfigureAwait(false);
                ValidateAuthorityType();
            }
        }

        protected virtual async Task PostTokenRequestAsync(AdalResultWrapper resultEx)
        {
            // if broker returned Authority update Authentiator
            if (!string.IsNullOrEmpty(resultEx.Result.Authority))
            {
                await UpdateAuthorityAsync(resultEx.Result.Authority).ConfigureAwait(false);
            }

            Authenticator.UpdateTenantId(resultEx.Result.TenantId);

            resultEx.Result.Authority = Authenticator.Authority;
        }

        protected abstract void AddAdditionalRequestParameters(DictionaryRequestParameters requestParameters);

        protected internal /* internal for test only */ virtual async Task<AdalResultWrapper> SendTokenRequestAsync()
        {
            var requestParameters = new DictionaryRequestParameters(this.Resource, this.ClientKey)
            {
                { OAuthParameter.ClientInfo, "1" }
            };

            AddAdditionalRequestParameters(requestParameters);
            return await SendHttpMessageAsync(requestParameters).ConfigureAwait(false);
        }

        protected async Task<AdalResultWrapper> SendTokenRequestByRefreshTokenAsync(string refreshToken)
        {
            var requestParameters = new DictionaryRequestParameters(Resource, ClientKey);
            requestParameters[OAuthParameter.GrantType] = OAuthGrantType.RefreshToken;
            requestParameters[OAuthParameter.RefreshToken] = refreshToken;
            requestParameters[OAuthParameter.Scope] = OAuthValue.ScopeOpenId;
            requestParameters[OAuthParameter.ClientInfo] = "1";

            AdalResultWrapper result = await SendHttpMessageAsync(requestParameters).ConfigureAwait(false);

            if (result.RefreshToken == null)
            {
                result.RefreshToken = refreshToken;
                RequestContext.Logger.Verbose("Refresh token was missing from the token refresh response, so the refresh token in the request is returned instead");
            }

            return result;
        }

        private async Task<AdalResultWrapper> RefreshAccessTokenAsync(AdalResultWrapper result)
        {
            AdalResultWrapper newResultEx = null;

            if (Resource != null)
            {
                RequestContext.Logger.Verbose("Refreshing access token...");

                try
                {
                    newResultEx = await SendTokenRequestByRefreshTokenAsync(result.RefreshToken)
                        .ConfigureAwait(false);
                    this.Authenticator.UpdateTenantId(result.Result.TenantId);

                    newResultEx.Result.Authority = Authenticator.Authority;

                    if (newResultEx.Result.IdToken == null)
                    {
                        // If Id token is not returned by token endpoint when refresh token is redeemed, we should copy tenant and user information from the cached token.
                        newResultEx.Result.UpdateTenantAndUserInfo(result.Result.TenantId, result.Result.IdToken,
                            result.Result.UserInfo);
                    }
                }
                catch (AdalException ex)
                {
                    AdalServiceException serviceException = ex as AdalServiceException;
                    if (serviceException != null && serviceException.ErrorCode == "invalid_request")
                    {
                        throw new AdalServiceException(
                            AdalError.FailedToRefreshToken,
                            AdalErrorMessage.FailedToRefreshToken + ". " + serviceException.Message,
                            serviceException.ServiceErrorCodes,
                            serviceException.Response,
                            serviceException);
                    }
                    newResultEx = new AdalResultWrapper { Exception = ex };
                }
            }

            return newResultEx;
        }

        private async Task<AdalResultWrapper> SendHttpMessageAsync(IRequestParameters requestParameters)
        {
            _client = new OAuthClient(ServiceBundle.HttpManager, Authenticator.TokenUri, RequestContext)
            {
                BodyParameters = requestParameters
            };

            TokenResponse tokenResponse = await _client.GetResponseAsync<TokenResponse>().ConfigureAwait(false);
            return tokenResponse.GetResult();
        }

        private void NotifyBeforeAccessCache()
        {
            _tokenCache.OnBeforeAccess(new TokenCacheNotificationArgs
            {
                TokenCache = _tokenCache,
                Resource = Resource,
                ClientId = ClientKey.ClientId,
                UniqueId = UniqueId,
                DisplayableId = DisplayableId
            });
        }

        private void NotifyAfterAccessCache()
        {
            _tokenCache.OnAfterAccess(new TokenCacheNotificationArgs
            {
                TokenCache = _tokenCache,
                Resource = Resource,
                ClientId = ClientKey.ClientId,
                UniqueId = UniqueId,
                DisplayableId = DisplayableId
            });
        }

        private void LogReturnedToken(AdalResult result)
        {
            if (result.AccessToken != null)
            {
                var accessTokenHash = PlatformProxyFactory
                                      .GetPlatformProxy()
                                      .CryptographyManager
                                      .CreateSha256Hash(result.AccessToken);

                {
                    var msg = string.Format(CultureInfo.CurrentCulture,
                        "=== Token Acquisition finished successfully. An access token was returned: Expiration Time: {0}",
                        result.ExpiresOn);

                    var piiMsg = msg + string.Format(CultureInfo.CurrentCulture, "Access Token Hash: {0}\n\t User id: {1}",
                                     accessTokenHash,
                                     result.UserInfo != null
                                         ? result.UserInfo.UniqueId
                                         : "null");
                    RequestContext.Logger.InfoPii(piiMsg, msg);
                }
            }
        }

        protected void ValidateAuthorityType()
        {
            if (!SupportADFS && Authenticator.AuthorityType == Instance.AuthorityType.ADFS)
            {
                throw new AdalException(AdalError.InvalidAuthorityType,
                    string.Format(CultureInfo.InvariantCulture, AdalErrorMessage.InvalidAuthorityTypeTemplate,
                        Authenticator.Authority));
            }
        }
    }
}
