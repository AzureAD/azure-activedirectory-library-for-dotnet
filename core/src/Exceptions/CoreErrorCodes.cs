﻿//----------------------------------------------------------------------
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


namespace Microsoft.Identity.Core.Exceptions
{
    /// <summary>
    /// Error codes attached to each exception.
    /// These need to be duplicated and publicly exposed in the MSAL and ADAL because users refer to them
    /// </summary>
    internal class CoreErrorCodes //TODO: add a test that reflects over this class and the MSAL / ADAL ones
    {
        public const string JsonParseError = "json_parse_failed";
        public const string RequestTimeout = "request_timeout";
        public const string ServiceNotAvailable = "service_not_available";

        public const string InvalidJwtError = "invalid_jwt";
        public const string TenantDiscoveryFailedError = "tenant_discovery_failed";
        public const string InvalidAuthorityType = "invalid_authority_type";
        public const string AuthenticationUiFailedError = "authentication_ui_failed";
        public const string InvalidGrantError = "invalid_grant";
        public const string UnknownError = "unknown_error";
        public const string AuthenticationCanceledError = "authentication_canceled";
        public const string AuthenticationFailed = "authentication_failed";
        public const string AuthenticationUiFailed = "authentication_ui_failed";
        public const string NonHttpsRedirectNotSupported = "non_https_redirect_failed";

#if ANDROID
        public const string FailedToCreateSharedPreference = "shared_preference_creation_failed";
        public const string ChromeNotInstalledError = "chrome_not_installed";
        public const string ChromeDisabledError = "chrome_disabled";
        public const string InvalidRequest = "invalid_request";
#endif

    }
}
