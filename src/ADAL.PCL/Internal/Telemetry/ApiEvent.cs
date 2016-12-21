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
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class ApiEvent : DefaultEvent
    {
        internal ApiEvent(Authenticator authenticator, UserInfo userinfo, string tenantId, string apiId)
        {
            Tenant = PlatformPlugin.CryptographyHelper.CreateSha256Hash(tenantId);
            SetEvent(EventConstants.Tenant, Tenant);

            if (userinfo != null)
            {
                Idp = userinfo.IdentityProvider;
                SetEvent(EventConstants.Idp, Idp);

                DisplayableId = PlatformPlugin.CryptographyHelper.CreateSha256Hash(userinfo.DisplayableId);
                SetEvent(EventConstants.DisplayableId, DisplayableId);

                UserId = PlatformPlugin.CryptographyHelper.CreateSha256Hash(userinfo.UniqueId);
                SetEvent(EventConstants.UserId, UserId);
            }

            Authority = HashTenantIdFromAuthority(authenticator.Authority);

            SetEvent(EventConstants.Authority, Authority);

            AuthorityType = authenticator.AuthorityType.ToString();
            SetEvent(EventConstants.AuthorityType, AuthorityType);

            IsDeprecated = false;
            SetEvent(EventConstants.IsDeprecated, IsDeprecated.ToString());

            SetEvent(EventConstants.ApiId, apiId);
        }

        internal string Tenant { get; set; }

        internal string Idp { get; set; }

        internal string UserId { get; set; }

        internal string Authority { get; set; }

        internal string AuthorityType { get; set; }

        internal string ValidateAuthority { get; set; }

        internal string DisplayableId { get; set; }

        internal string IdentityProvider { get; set; }

        internal bool IsDeprecated { get; set; }

        internal bool ExtendedExpires { get; set; }

        internal string HashTenantIdFromAuthority(string authority)
        {
            Uri uri = new Uri(authority);
            Regex regex = new Regex(uri.AbsolutePath);
            string result = regex.Replace(authority, PlatformPlugin.CryptographyHelper.CreateSha256Hash(uri.AbsolutePath), 1);
            return result;
        }

        internal override void SetEvent(string eventName, string eventParameter)
        {
            if (!string.IsNullOrEmpty(eventParameter))
            {
                EventDictitionary[eventName] = eventParameter;
            }
        }

        internal void SetExtraQueryParameters(string extraQueryParameter)
        {
            string[] result = extraQueryParameter.Split('&');
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string s in result)
            {
                if (s.Contains("="))
                {
                    stringBuilder.Append(s.Split('=')[0]).Append("&");
                }
            }

            if (string.IsNullOrEmpty(stringBuilder.ToString()))
            {
                SetEvent(EventConstants.ExtraQueryParameters,
                    stringBuilder.ToString().Substring(0, stringBuilder.Length - 1));
            }
        }

        internal override void ProcessEvent(Dictionary<string, string> dispatchMap)
        {
            foreach (KeyValuePair<string, string> Event in EventDictitionary)
            {
                if (Event.Key.Equals(EventConstants.AuthorityType)
                    || Event.Key.Equals(EventConstants.SdkVersion)
                    || Event.Key.Equals(EventConstants.SdkPlatform)
                    || Event.Key.Equals(EventConstants.AuthorityValidation)
                    || Event.Key.Equals(EventConstants.ExtendedExpires)
                    || Event.Key.Equals(EventConstants.PromptBehavior)
                    || Event.Key.Equals(EventConstants.Idp)
                    || Event.Key.Equals(EventConstants.Tenant)
                    || Event.Key.Equals(EventConstants.LoginHint)
                    || Event.Key.Equals(EventConstants.ResponseTime)
                    || Event.Key.Equals(EventConstants.CorrelationId)
                    || Event.Key.Equals(EventConstants.RequestId)
                    || Event.Key.Equals(EventConstants.ApiId)
                    || Event.Key.Equals(EventConstants.IsSuccessful)
                    || Event.Key.Equals(EventConstants.ExtendedExpires))
                {
                    dispatchMap.Add(Event.Key, Event.Value);
                }
            }
        }
    }
}