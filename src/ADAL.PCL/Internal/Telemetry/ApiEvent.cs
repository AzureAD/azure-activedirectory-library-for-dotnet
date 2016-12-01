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

        private string HashTenantIdFromAuthority(string authority)
        {
            string[] authoritySplit = authority.Split('/');

            string tenant = authoritySplit[authoritySplit.Length - 2];

            Regex regex = new Regex(tenant);

            string result = regex.Replace(authority, PlatformPlugin.CryptographyHelper.CreateSha256Hash(tenant), 1);

            return result;
        }

        internal override void SetEvent(string eventName, string eventParameter)
        {
            if (eventParameter != null)
            {
                EventList.Add(new Tuple<string, string>(eventName, eventParameter));
            }
        }

        internal void SetExtraQueryParameters(string extraQueryParameter)
        {
            string[] result = extraQueryParameter.Split('&');
            StringBuilder stringbuilder = new StringBuilder();
            foreach (string s in result)
            {
                if (s.Contains("="))
                {
                    stringbuilder.Append(s.Split('=')[0]).Append("&");
                }
            }

            SetEvent(EventConstants.ExtraQueryParameters,
                stringbuilder.ToString().Substring(0, stringbuilder.Length - 1));
        }

        internal override void ProcessEvent(Dictionary<string, string> dispatchMap)
        {
            foreach (Tuple<string, string> Event in EventList)
            {
                if (Event.Item1.Equals(EventConstants.ApplicationName) ||
                    Event.Item1.Equals(EventConstants.ApplicationVersion)
                    || Event.Item1.Equals(EventConstants.AuthorityType)
                    || Event.Item1.Equals(EventConstants.SdkVersion)
                    || Event.Item1.Equals(EventConstants.SdkPlatform)
                    || Event.Item1.Equals(EventConstants.AuthorityValidation)
                    || Event.Item1.Equals(EventConstants.ExtendedExpires)
                    || Event.Item1.Equals(EventConstants.PromptBehavior)
                    || Event.Item1.Equals(EventConstants.Idp)
                    || Event.Item1.Equals(EventConstants.Tenant)
                    || Event.Item1.Equals(EventConstants.LoginHint)
                    || Event.Item1.Equals(EventConstants.ResponseTime)
                    || Event.Item1.Equals(EventConstants.CorrelationId)
                    || Event.Item1.Equals(EventConstants.RequestId)
                    || Event.Item1.Equals(EventConstants.ApiId))
                {
                    dispatchMap.Add(Event.Item1, Event.Item2);
                }
            }
        }
    }
}