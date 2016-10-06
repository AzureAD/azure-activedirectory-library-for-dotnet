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

                UniqueId = PlatformPlugin.CryptographyHelper.CreateSha256Hash(userinfo.UniqueId);
                SetEvent(EventConstants.UniqueId, UniqueId);
            }

            if (tenantId != null)
            {
                Authority = HashTenantIdFromAuthority(authenticator.Authority, tenantId);
            }
            else
            {
                Authority = authenticator.Authority;
            }
            SetEvent(EventConstants.Authority, Authority);

            AuthorityType = authenticator.AuthorityType.ToString();
            SetEvent(EventConstants.AuthorityType, AuthorityType);

            IsDeprecated = false;
            SetEvent(EventConstants.IsDeprecated, IsDeprecated.ToString());

            SetEvent(EventConstants.ApiId, apiId);
        }

        internal string Tenant { get; set; }

        internal string Idp { get; set; }

        internal string Upn { get; set; }

        internal string Authority { get; set; }

        internal string AuthorityType { get; set; }

        internal string ValidateAuthority { get; set; }

        internal string DisplayableId { get; set; }

        internal string IdentityProvider { get; set; }

        internal string UniqueId { get; set; }

        internal bool IsDeprecated { get; set; }

        internal bool ExtendedExpires { get; set; }

        private string HashTenantIdFromAuthority(string authority, string tenantId)
        {
            Regex regex = new Regex(tenantId);

            string result = regex.Replace(authority, PlatformPlugin.CryptographyHelper.CreateSha256Hash(tenantId), 5);

            return result;
        }

        internal override void SetEvent(string eventName, string eventParameter)
        {
            if (eventParameter != null)
            {
                EventDictitionary.Add(new Tuple<string, string>(eventName, eventParameter));
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
            SetEvent(EventConstants.ExtraQueryParameters, stringbuilder.ToString());
        }

        internal override void ProcessEvent(Dictionary<string, string> dispatchMap)
        {
            List<Tuple<string, string>> listEvent = EventDictitionary;
            int size = EventDictitionary.Count;

            for (int i = 0; i < size; i++)
            {
                if (listEvent[i].Item1.Equals(EventConstants.ApplicationName) ||
                    listEvent[i].Item1.Equals(EventConstants.ApplicationVersion)
                    || listEvent[i].Item1.Equals(EventConstants.AuthorityType)
                    || listEvent[i].Item1.Equals(EventConstants.SdkVersion)
                    || listEvent[i].Item1.Equals(EventConstants.SdkPlatform)
                    || listEvent[i].Item1.Equals(EventConstants.AuthorityValidation)
                    || listEvent[i].Item1.Equals(EventConstants.ExtendedExpires)
                    || listEvent[i].Item1.Equals(EventConstants.PromptBehavior)
                    || listEvent[i].Item1.Equals(EventConstants.Idp)
                    || listEvent[i].Item1.Equals(EventConstants.Tenant)
                    || listEvent[i].Item1.Equals(EventConstants.Upn)
                    || listEvent[i].Item1.Equals(EventConstants.LoginHint)
                    || listEvent[i].Item1.Equals(EventConstants.ResponseTime)
                    || listEvent[i].Item1.Equals(EventConstants.CorrelationId)
                    || listEvent[i].Item1.Equals(EventConstants.RequestId)
                    || listEvent[i].Item1.Equals(EventConstants.ApiId))
                {
                    dispatchMap.Add(listEvent[i].Item1, listEvent[i].Item2);
                }
            }
        }
    }
}