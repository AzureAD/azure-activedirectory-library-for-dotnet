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
        public const string ApplicationName = "application_name";
        public const string ApplicationVersion = "application_version";
        public const string SdkVersion = "x-client-sku";
        public const string SdkId = "x-client-version";
        public const string DeviceId = "device_id";
        public const string Tenant = "tenant_id";
        public const string Issuer = "issuer";
        public const string Idp = "idp";
        public const string Upn = "upn";
        public const string Email = "email";
        public const string PasswordExpiration = "pwd_exp";
        public const string PasswordChangeUrl = "pwd_url";
        public const string FamilyName = "family_name";
        public const string ResponseTime = "response_time";
        public const string ClientIp = "client_ip";
        public const string ClientId = "client_id";
        public const string BrokerEvent = "broker_event";
        public const string HttpEvent = "http_event";
        public const string GrantEvent = "grant_event";
        public const string CryptographyEvent = "cryptography_event";
        public const string UIEvent = "ui_event";
        public const string CacheEvent = "cache_event";
        public const string Crypto = "crypto_event";
        public const string RequestId = "request_id";
        public const string StartTime = "start_time";
        public const string StopTime = "end_time";
        public const string Authority = "authority";
        public const string AuthorityType = "authority_type";
        public const string AuthorizationUri = "authorization_uri";
        public const string CorrelationId = "correlation_id";
        public const string DeviceCodeUri = "device_code_uri";
        public const string IsTenantless = "is_tenantless";
        public const string SelfSignedJwtAudience = "self_signed_jwt_audience";
        public const string TokenUri = "token_uri";
        public const string UserRealmUri = "user_realm_uri";
        public const string ValidateAuthority = "validate_authority";
        public const string GivenName = "given_name";
        public const string DisplayableId = "displayable_id";
        public const string UniqueId = "unique_id";
        public const string UserAgent = "user_agent";
        public const string RequestApiVersion = "request_api_version";
        public const string HttpBodyParameters = "http_body_paramters";
        public const string HttpResponseMethod = "http_response_method";
        public const string IsCrossTenantRt = "is_cross_tenant_rt";
        public const string IsMultipleResourceRt = "is_multiple_resource_rt";
        public const string ExtendedLifeTimeEnabled = "extended_lifetime_enabled";
        public const string ExpiredAt = "expired_at";
        public const string TokenFound = "token_found";
        public const string CacheLookUp = "cache_lookup";
        public const string LoginHint = "login_hint";
        public const string HttpQueryParameters = "query_parameters";
        public const string HttpStatusCode = "response_code";
        public const string IsDeprecated = "is_deprecated";
        public const string ExtendedExpires = "extended_expires_on_setting";
        public const string UiTime = "ui_time";
    }
}
