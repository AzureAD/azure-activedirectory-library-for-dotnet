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
using System.Security;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class ApiEvent : DefaultEvent
    {
        internal ApiEvent(Authenticator authenticator, UserInfo userinfo, string tenantId) : base(EventConstants.GrantEvent)
        {
            Tenant = PlatformPlugin.CryptographyHelper.CreateSha256Hash(tenantId);
            SetEvent(EventConstants.Tenant, Tenant);

            Idp = userinfo.IdentityProvider;
            SetEvent(EventConstants.Idp, Idp);

            if (userinfo.PasswordExpiresOn != null)
            {
                SetEvent(EventConstants.PasswordExpiration, PasswordExpiration.ToString());
            }

            if (userinfo.PasswordChangeUrl != null)
            {
                PasswordChangeUrl = userinfo.PasswordChangeUrl;
                SetEvent(EventConstants.PasswordChangeUrl, PasswordChangeUrl.ToString());
            }

            FamilyName = userinfo.FamilyName;
            SetEvent(EventConstants.FamilyName, FamilyName);

            GivenName = userinfo.GivenName;
            SetEvent(EventConstants.GivenName,GivenName);

            DisplayableId = PlatformPlugin.CryptographyHelper.CreateSha256Hash(userinfo.DisplayableId);
            SetEvent(EventConstants.DisplayableId, DisplayableId);

            UniqueId = PlatformPlugin.CryptographyHelper.CreateSha256Hash(userinfo.UniqueId);
            SetEvent(EventConstants.UniqueId, UniqueId);

            Authority = authenticator.Authority;
            SetEvent(EventConstants.Authority,Authority);

            AuthorityType = authenticator.AuthorityType.ToString();
            SetEvent(EventConstants.AuthorityType,AuthorityType);

            AuthorizationUri = authenticator.AuthorizationUri;
            SetEvent(EventConstants.AuthorizationUri, AuthorizationUri);

            CorrelationId = authenticator.CorrelationId.ToString();
            SetEvent(EventConstants.CorrelationId,CorrelationId);

            DeviceCodeUri = PlatformPlugin.CryptographyHelper.CreateSha256Hash(authenticator.DeviceCodeUri);
            SetEvent(EventConstants.DeviceCodeUri,DeviceCodeUri);

            IsTenantless = authenticator.IsTenantless.ToString();
            SetEvent(EventConstants.IsTenantless,IsTenantless);

            SelfSignedJwtAudience = authenticator.SelfSignedJwtAudience;
            SetEvent(EventConstants.SelfSignedJwtAudience,SelfSignedJwtAudience);

            TokenUri = PlatformPlugin.CryptographyHelper.CreateSha256Hash(authenticator.TokenUri);
            SetEvent(EventConstants.TokenUri,TokenUri);

            UserRealmUri = authenticator.UserRealmUri;
            SetEvent(EventConstants.UserRealmUri,UserRealmUri);

            ValidateAuthority = authenticator.ValidateAuthority.ToString();
            SetEvent(EventConstants.ValidateAuthority,ValidateAuthority);
        }

        internal override void SetEvent(string eventName, string eventParameter)
        {
            DefaultEvents.Add(new Tuple<string, string>(eventName, eventParameter));
        }

        internal string Tenant { get; set; }

        internal string RequestId { get; set; }

        internal string Issuer { get; set; }

        internal string Idp { get; set; }

        internal string Upn { get; set; }

        internal string Email { get; set; }

        internal DateTimeOffset? PasswordExpiration { get; set; }

        internal Uri PasswordChangeUrl { get; set; }

        internal string FamilyName { get; set; }

        internal string Authority { get; set; }

        internal string AuthorityType { get; set; }

        internal string AuthorizationUri { get; set; }

        internal string CorrelationId { get; set; }

        internal string DeviceCodeUri { get; set; }

        internal string IsTenantless { get; set; }

        internal string SelfSignedJwtAudience { get; set; }

        internal string TokenUri { get; set; }

        internal string UserRealmUri { get; set; }

        internal string ValidateAuthority { get; set; }

        internal string UpdateFromTemplateAsync { get; set; }

        internal string UpdateTenantId { get; set; }

        internal string DisplayableId { get; set; }

        internal string GivenName { get; set; }

        internal string IdentityProvider { get; set; }

        internal string UniqueId { get; set; }
    }
}

