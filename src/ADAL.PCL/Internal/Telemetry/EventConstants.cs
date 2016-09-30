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
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class EventConstants
    {
        public const string EventName = "event_name";
        public const string ApiId = "api_id";
        public const string IsSuccessful = "is_successful";
        public const string AuthorityValidation = "authority_validation";
        public const string ApplicationName = "application_name";
        public const string ApplicationVersion = "application_version";
        public const string PromptBehavior = "prompt_behavior";
        public const string SdkVersion = "sdk_version";
        public const string SdkPlatform = "sdk_platform";
        public const string DeviceId = "device_id";
        public const string Tenant = "tenant_id";
        public const string Issuer = "issuer";
        public const string Idp = "idp";
        public const string Upn = "upn";
        public const string ResponseTime = "response_time";
        public const string ClientIp = "client_ip";
        public const string ClientId = "client_id";
        public const string BrokerEvent = "broker_event";
        public const string HttpEvent = "http_event";
        public const string ApiEvent = "api_event";
        public const string CryptographyEvent = "cryptography_event";
        public const string UIEvent = "ui_event";
        public const string CacheEvent = "cache_event";
        public const string RequestId = "request_id";
        public const string StartTime = "start_time";
        public const string StopTime = "end_time";
        public const string Authority = "authority";
        public const string AuthorityType = "authority_type";
        public const string CorrelationId = "correlation_id";
        public const string GivenName = "given_name";
        public const string DisplayableId = "displayable_id";
        public const string UniqueId = "unique_id";
        public const string UserAgent = "user_agent";
        public const string RequestApiVersion = "request_api_version";
        public const string HttpBodyParameters = "http_body_parameters";
        public const string HttpResponseMethod = "http_response_method";
        public const string IsMultipleResourceRt = "is_multiple_resource_rt";
        public const string ExpiredAt = "expired_at";
        public const string TokenFound = "token_found";
        public const string CacheLookUp = "cache_lookup";
        public const string LoginHint = "login_hint";
        public const string HttpQueryParameters = "query_parameters";
        public const string HttpStatusCode = "response_code";
        public const string IsDeprecated = "is_deprecated";
        public const string ExtendedExpires = "extended_expires_on_setting";
        public const string UiTime = "ui_time";
        public const string UserCancel = "user_cancel";
        public const string HttpEventCount = "http_event_count";
    }
}
