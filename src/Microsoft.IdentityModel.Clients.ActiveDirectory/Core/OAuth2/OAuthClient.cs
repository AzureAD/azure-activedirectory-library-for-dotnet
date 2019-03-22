// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License.

using Microsoft.Identity.Core.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core.OAuth2
{
    internal class OAuthClient
    {
        private const string DeviceAuthHeaderName = "x-ms-PKeyAuth";
        private const string DeviceAuthHeaderValue = "1.0";
        private const string WwwAuthenticateHeader = "WWW-Authenticate";
        private const string PKeyAuthName = "PKeyAuth";
        internal /* internal for test only */ const string ExtraQueryParamEnvVariable = "ExtraQueryParameter";
        private readonly RequestContext _requestContext;
        private readonly IHttpManager _httpManager;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();

        internal OAuthClient(IHttpManager httpManager, string uri, RequestContext requestContext)
        {
            _httpManager = httpManager ?? throw new ArgumentNullException(nameof(httpManager));
            RequestUri = CheckForExtraQueryParameter(uri);
            _requestContext = requestContext;
        }

        internal IRequestParameters BodyParameters { get; set; }
        internal string RequestUri { get; private set; }

        internal async Task<T> GetResponseAsync<T>()
        {
            return await GetResponseAsync<T>(true).ConfigureAwait(false);
        }

        private async Task<T> GetResponseAsync<T>(bool respondToDeviceAuthChallenge)
        {
            T typedResponse = default(T);

            try
            {
                IDictionary<string, string> adalIdHeaders = AdalIdHelper.GetAdalIdParameters();
                foreach (KeyValuePair<string, string> kvp in adalIdHeaders)
                {
                    _headers[kvp.Key] = kvp.Value;
                }

                //add pkeyauth header
                _headers[DeviceAuthHeaderName] = DeviceAuthHeaderValue;
                var response = await ExecuteRequestAsync().ConfigureAwait(false);
                typedResponse = EncodingHelper.DeserializeResponse<T>(response.Body);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw AdalExceptionFactory.GetServiceException(
                         response.StatusCode.ToString(),
                         string.Format(
                                    CultureInfo.CurrentCulture,
                                    "Response status code does not indicate success: {0} ({1}).",
                                    (int)response.StatusCode,
                                    response.StatusCode),
                         response);
                }
            }

            catch (AdalServiceException ex)
            {
                if (!IsDeviceAuthChallenge(ex.Response, respondToDeviceAuthChallenge))
                {
                    TokenResponse tokenResponse = TokenResponse.CreateFromErrorResponse(ex.Response);
                    string[] errorCodes = tokenResponse.ErrorCodes ?? new[] { ex.StatusCode.ToString(CultureInfo.InvariantCulture) };
                    AdalServiceException serviceEx = new AdalServiceException(
                        tokenResponse.Error,
                        tokenResponse.ErrorDescription,
                        errorCodes,
                        ex.Response,
                        ex);

                    if (ex.StatusCode == 400 &&
                        tokenResponse.Error == AdalError.InteractionRequired)
                    {
                        throw new AdalClaimChallengeException(tokenResponse.Error, tokenResponse.ErrorDescription, ex, tokenResponse.Claims);
                    }

                    throw serviceEx;
                }

                //attempt device auth
                return await HandleDeviceAuthChallengeAsync<T>(ex.Response).ConfigureAwait(false);
            }

            return typedResponse;
        }

        private bool IsDeviceAuthChallenge(IHttpWebResponse response, bool respondToDeviceAuthChallenge)
        {
            return DeviceAuthHelper.CanHandleDeviceAuthChallenge
                   && response != null
                   && respondToDeviceAuthChallenge
                   && response?.Headers != null
                   && response.StatusCode == HttpStatusCode.Unauthorized
                   && response.Headers.Contains(WwwAuthenticateHeader)
                   && response.Headers.GetValues(WwwAuthenticateHeader).FirstOrDefault()
                       .StartsWith(PKeyAuthName, StringComparison.OrdinalIgnoreCase);
        }

        private IDictionary<string, string> ParseChallengeData(IHttpWebResponse response)
        {
            IDictionary<string, string> data = new Dictionary<string, string>();
            string wwwAuthenticate = response.Headers.GetValues(WwwAuthenticateHeader).FirstOrDefault();
            wwwAuthenticate = wwwAuthenticate.Substring(PKeyAuthName.Length + 1);
            List<string> headerPairs = EncodingHelper.SplitWithQuotes(wwwAuthenticate, ',');
            foreach (string pair in headerPairs)
            {
                List<string> keyValue = EncodingHelper.SplitWithQuotes(pair, '=');
                data.Add(keyValue[0].Trim(), keyValue[1].Trim().Replace("\"", ""));
            }

            return data;
        }

        private async Task<T> HandleDeviceAuthChallengeAsync<T>(IHttpWebResponse response)
        {
            IDictionary<string, string> responseDictionary = ParseChallengeData(response);

            if (!responseDictionary.ContainsKey("SubmitUrl"))
            {
                responseDictionary["SubmitUrl"] = RequestUri;
            }

            string responseHeader = await DeviceAuthHelper.CreateDeviceAuthChallengeResponseAsync(responseDictionary)
                .ConfigureAwait(false);
            IRequestParameters rp = BodyParameters;
            CheckForExtraQueryParameter(responseDictionary["SubmitUrl"]);
            BodyParameters = rp;
            _headers["Authorization"] = responseHeader;
            return await GetResponseAsync<T>(false).ConfigureAwait(false);
        }

        private static string CheckForExtraQueryParameter(string url)
        {
            string extraQueryParameter = PlatformProxyFactory.GetPlatformProxy().GetEnvironmentVariable(ExtraQueryParamEnvVariable);
            string delimiter = (url.IndexOf('?') > 0) ? "&" : "?";
            if (!string.IsNullOrWhiteSpace(extraQueryParameter))
            {
                url += string.Concat(delimiter, extraQueryParameter);
            }

            return url;
        }

        internal async Task<IHttpWebResponse> ExecuteRequestAsync()
        {
            bool addCorrelationId = _requestContext != null && _requestContext.Logger.CorrelationId != Guid.Empty;
            if (addCorrelationId)
            {
                _headers[OAuthHeader.CorrelationId] = _requestContext.Logger.CorrelationId.ToString();
                _headers[OAuthHeader.RequestCorrelationIdInResponse] = "true";
            }

            HttpResponse responseMessage;

            if (BodyParameters != null)
            {
                responseMessage = await _httpManager.SendPostAsync(
                    new Uri(RequestUri),
                    _headers,
                    (Dictionary<string, string>)BodyParameters,
                    _requestContext)
                    .ConfigureAwait(false);
            }
            else
            {
                responseMessage = await _httpManager.SendGetAsync(
                     new Uri(RequestUri),
                     _headers,
                     _requestContext)
                     .ConfigureAwait(false);
            }

            var webResponse = CreateResponse(responseMessage);

            if (addCorrelationId)
            {
                VerifyCorrelationIdHeaderInResponse(webResponse.Headers, _requestContext);
            }

            return webResponse;
        }

        private static IHttpWebResponse CreateResponse(HttpResponse response)
        {
            return new HttpWebResponseWrapper(
                response.Body,
                response.Headers,
                response.StatusCode);
        }

        internal static async Task<IHttpWebResponse> CreateResponseAsync(HttpResponseMessage response)
        {
            return new HttpWebResponseWrapper(
                await response.Content.ReadAsStringAsync().ConfigureAwait(false),
                response.Headers,
                response.StatusCode);
        }

        private static void VerifyCorrelationIdHeaderInResponse(HttpResponseHeaders headers, RequestContext requestContext)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                string responseHeaderKey = header.Key;
                string trimmedKey = responseHeaderKey.Trim();
                if (string.Compare(trimmedKey, OAuthHeader.CorrelationId, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    string correlationIdHeader = headers.GetValues(trimmedKey).FirstOrDefault().Trim();
                    if (!Guid.TryParse(correlationIdHeader, out var correlationIdInResponse))
                    {
                        requestContext.Logger.Warning(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "Returned correlation id '{0}' is not in GUID format.",
                                correlationIdHeader));
                    }
                    else if (correlationIdInResponse != requestContext.Logger.CorrelationId)
                    {
                        requestContext.Logger.Warning(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "Returned correlation id '{0}' does not match the sent correlation id '{1}'",
                                correlationIdHeader,
                                requestContext.Logger.CorrelationId));
                    }

                    break;
                }
            }
        }
    }
}
