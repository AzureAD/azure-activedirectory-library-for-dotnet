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

using System;
using System.Linq;
using Microsoft.Identity.Core.Http;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// The builder class to construct a MsalConfiguration object which can be used to initialize a PublicClientApplication or ConfidentialClientApplication.
    /// </summary>
    public abstract class MsalConfigurationBuilder<T> where T : MsalConfigurationBuilder<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msalConfiguration"></param>
        internal MsalConfigurationBuilder(MsalConfiguration msalConfiguration)
        {
            Config = msalConfiguration;
        }

        internal MsalConfiguration Config { get; }

        /// <summary>
        /// Add an AAD authority to the configuration.
        /// </summary>
        /// <param name="aadAuthorityAudience">The AAD Authority Audience value to use.</param>
        /// <param name="isDefaultAuthority"></param>
        /// <returns>MsalConfigurationBuilder</returns>
        public T WithAadAuthority(AadAuthorityAudience aadAuthorityAudience, bool isDefaultAuthority)
        {
            Config.AddAuthorityInfo(new MsalAuthorityInfo(MsalAuthorityType.Aad, aadAuthorityAudience, string.Empty, isDefaultAuthority));
            return (T)this;
        }

        /// <summary>
        /// Add a B2C authority to the configuration.
        /// </summary>
        /// <param name="authorityUri">The authority URI used in the B2C application.</param>
        /// <param name="isDefaultAuthority"></param>
        /// <returns>MsalConfigurationBuilder</returns>
        public T WithB2CAuthority(string authorityUri, bool isDefaultAuthority)
        {
            Config.AddAuthorityInfo(
                new MsalAuthorityInfo(MsalAuthorityType.B2C, AadAuthorityAudience.None, authorityUri, isDefaultAuthority));
            return (T)this;
        }

        /// <summary>
        /// This is only here for back compat with existing public constructors.  Ideally we deprecate this with the other public constructors so we can move to the builder pattern.
        /// </summary>
        /// <returns></returns>
        internal T WithAuthorityFromAuthorityString(string authority, bool isDefaultAuthority)
        {
            var canonical = new Uri(authority).ToString().ToLowerInvariant();
            if (string.Compare(canonical, "https://login.microsoftonline.com/common/", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return WithAadAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount, isDefaultAuthority);
            }
            if (string.Compare(canonical, "https://login.microsoftonline.com/organizations/", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return WithAadAuthority(AadAuthorityAudience.AzureAdOnly, isDefaultAuthority);
            }
            if (string.Compare(canonical, "https://login.microsoftonline.com/consumers/", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return WithAadAuthority(AadAuthorityAudience.MicrosoftAccountOnly, isDefaultAuthority);
            }

            // TODO: i'm sure we're missing some parsing/cases here.  need to validate and unit test...
            return WithB2CAuthority(authority, isDefaultAuthority);
        }

        /// <summary>
        /// Sets the Http Connection Timeout.
        /// </summary>
        /// <param name="connectionTimeoutMilliseconds">HTTP Connection Timeout in milliseconds.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public T WithHttpConnectionTimeoutMilliseconds(
            int connectionTimeoutMilliseconds)
        {
            Config.HttpConnectionTimeoutMilliseconds = connectionTimeoutMilliseconds;
            return (T)this;
        }

        /// <summary>
        /// Sets the HTTP Read Timeout.
        /// </summary>
        /// <param name="connectionReadTimeoutMilliseconds">HTTP Connection Read Timeout in milliseconds.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public MsalConfigurationBuilder<T> WithHttpConnectionReadTimeoutMilliseconds(
            int connectionReadTimeoutMilliseconds)
        {
            Config.HttpConnectionReadTimeoutMilliseconds = connectionReadTimeoutMilliseconds;
            return this;
        }

        /// <summary>
        /// Configures an instance of IHttpClientFactory for MSAL to use to get to the appropriate HttpClient to use.
        /// </summary>
        /// <param name="httpClientFactory">Implementation of IMsalHttpClientFactory.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public MsalConfigurationBuilder<T> WithHttpClientFactory(
            IMsalHttpClientFactory httpClientFactory)
        {
            Config.HttpClientFactory = httpClientFactory;
            return this;
        }

        /// <summary>
        /// Sets whether to enable logging of PII data.
        /// </summary>
        /// <param name="enablePii">True to enable PII, false to only allow scrubbed data logging.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public MsalConfigurationBuilder<T> WithEnableLoggingPii(bool enablePii)
        {
            Config.EnablePii = enablePii;
            return this;
        }

        /// <summary>
        /// Sets the desired logging level.
        /// </summary>
        /// <param name="loggingLevel">The logging level.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public MsalConfigurationBuilder<T> WithLoggingLevel(LogLevel loggingLevel)
        {
            Config.LoggingLevel = loggingLevel;
            return this;
        }

        /// <summary>
        /// Sets the token expiration buffer in milliseconds used during the token expiration status calculation.
        /// </summary>
        /// <param name="tokenExpirationBufferMilliseconds">Token expiration buffer in milliseconds.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public MsalConfigurationBuilder<T> WithTokenExpirationBufferMilliseconds(
            int tokenExpirationBufferMilliseconds)
        {
            Config.TokenExpirationBufferMilliseconds = tokenExpirationBufferMilliseconds;
            return this;
        }

        /// <summary>
        /// Sets the callback implementation to receive logging callback data.
        /// </summary>
        /// <returns>MsalConfigurationBuilder</returns>
        public MsalConfigurationBuilder<T> WithLoggingCallback()
        {
            return this;
        }

        /// <summary>
        /// Sets the callback implementation to receive telemetry callback data.
        /// </summary>
        /// <returns>MsalConfigurationBuilder</returns>
        public MsalConfigurationBuilder<T> WithTelemetryReceiver()
        {
            return this;
        }

        /// <summary>
        /// Used for tests that need to override the HttpManager
        /// </summary>
        /// <param name="httpManager"></param>
        /// <returns></returns>
        internal MsalConfigurationBuilder<T> WithHttpManager(IHttpManager httpManager)
        {
            Config.HttpManager = httpManager;
            return this;
        }

        internal MsalConfigurationBuilder<T> WithExtendedTokenLifetimeEnabled(bool enabled)
        {
            Config.IsExtendedTokenLifetimeEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Finalize and validate the configuration and create the MsalConfiguration object.
        /// </summary>
        /// <returns>The MsalConfiguration object to send to a PublicClientApplication or ConfidentialClientApplication constructor.</returns>
        internal MsalConfiguration Validate()
        {
            // todo: final validation/sanity checks here...

            // validate that we only have ONE default authority
            if (Config.Authorities.Where(x => x.IsDefault).ToList().Count > 1)
            {
                throw new InvalidOperationException("More than one default authority was configured.");
            }

            if (!Config.Authorities.Any())
            {
                throw new InvalidOperationException("No authorities were configured.");
            }

            return Config;
        }
 
    }
}