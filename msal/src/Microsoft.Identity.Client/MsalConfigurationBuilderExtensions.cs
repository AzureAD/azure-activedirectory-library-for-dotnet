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

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// Extension class to add specific configuration methods to the MsalConfigurationBuilder.
    /// </summary>
    public static class MsalConfigurationBuilderExtensions
    {
        /// <summary>
        /// Add an AAD authority to the configuration.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <param name="aadAuthorityAudience">The AAD Authority Audience value to use.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithAadAuthority(
            this MsalConfigurationBuilder builder,
            AadAuthorityAudience aadAuthorityAudience = AadAuthorityAudience.Default)
        {
            builder.Config.AddAuthorityInfo(new MsalAuthorityInfo(MsalAuthorityType.Aad, aadAuthorityAudience, string.Empty));
            return builder;
        }

        /// <summary>
        /// Add a B2C authority to the configuration.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <param name="authorityUri">The authority URI used in the B2C application.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithB2CAuthority(this MsalConfigurationBuilder builder, string authorityUri)
        {
            builder.Config.AddAuthorityInfo(
                new MsalAuthorityInfo(MsalAuthorityType.B2C, AadAuthorityAudience.None, authorityUri));
            return builder;
        }

        /// <summary>
        /// Sets the desired authorization user agent value.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <param name="authorizationUserAgent">Value of the Authorization User Agent.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithAuthorizationUserAgent(
            this MsalConfigurationBuilder builder,
            AuthorizationUserAgent authorizationUserAgent)
        {
            builder.Config.AuthorizationUserAgent = authorizationUserAgent;
            return builder;
        }

        /// <summary>
        /// Sets the Http Connection Timeout.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <param name="connectionTimeoutMilliseconds">HTTP Connection Timeout in milliseconds.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithHttpConnectionTimeoutMilliseconds(
            this MsalConfigurationBuilder builder,
            int connectionTimeoutMilliseconds)
        {
            builder.Config.HttpConnectionTimeoutMilliseconds = connectionTimeoutMilliseconds;
            return builder;
        }

        /// <summary>
        /// Sets the HTTP Read Timeout.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <param name="connectionReadTimeoutMilliseconds">HTTP Connection Read Timeout in milliseconds.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithHttpConnectionReadTimeoutMilliseconds(
            this MsalConfigurationBuilder builder,
            int connectionReadTimeoutMilliseconds)
        {
            builder.Config.HttpConnectionReadTimeoutMilliseconds = connectionReadTimeoutMilliseconds;
            return builder;
        }

        /// <summary>
        /// Configures an instance of IHttpClientFactory for MSAL to use to get to the appropriate HttpClient to use.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <param name="httpClientFactory">Implementation of IMsalHttpClientFactory.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithHttpClientFactory(
            this MsalConfigurationBuilder builder,
            IMsalHttpClientFactory httpClientFactory)
        {
            builder.Config.HttpClientFactory = httpClientFactory;
            return builder;
        }

        /// <summary>
        /// Sets whether to enable logging of PII data.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <param name="enablePii">True to enable PII, false to only allow scrubbed data logging.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithEnableLoggingPii(this MsalConfigurationBuilder builder, bool enablePii)
        {
            builder.Config.EnablePii = enablePii;
            return builder;
        }

        /// <summary>
        /// Sets the desired logging level.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithLoggingLevel(this MsalConfigurationBuilder builder, LogLevel loggingLevel)
        {
            builder.Config.LoggingLevel = loggingLevel;
            return builder;
        }

        /// <summary>
        /// Sets the token expiration buffer in milliseconds used during the token expiration status calculation.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <param name="tokenExpirationBufferMilliseconds">Token expiration buffer in milliseconds.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithTokenExpirationBufferMilliseconds(
            this MsalConfigurationBuilder builder,
            int tokenExpirationBufferMilliseconds)
        {
            builder.Config.TokenExpirationBufferMilliseconds = tokenExpirationBufferMilliseconds;
            return builder;
        }

        /// <summary>
        /// Sets the callback implementation to receive logging callback data.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithLoggingCallback(this MsalConfigurationBuilder builder)
        {
            return builder;
        }

        /// <summary>
        /// Sets the callback implementation to receive telemetry callback data.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <returns>MsalConfigurationBuilder</returns>
        public static MsalConfigurationBuilder WithTelemetryCallback(this MsalConfigurationBuilder builder)
        {
            return builder;
        }

        /// <summary>
        /// Finalize and validate the configuration and create the MsalConfiguration object.
        /// </summary>
        /// <param name="builder">The MsalConfigurationBuilder.</param>
        /// <returns>The MsalConfiguration object to send to a PublicClientApplication or ConfidentialClientApplication constructor.</returns>
        public static MsalConfiguration Build(this MsalConfigurationBuilder builder)
        {
            // todo: final validation/sanity checks here...
            return builder.Config;
        }
    }
}