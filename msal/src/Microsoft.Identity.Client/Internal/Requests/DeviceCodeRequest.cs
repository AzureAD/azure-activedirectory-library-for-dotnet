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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Core.OAuth2;

namespace Microsoft.Identity.Client.Internal.Requests
{
    // todo: 
    // * unit tests
    // * integration/end2end tests in internal vsts repo
    // * sample app / test dev app as a console application for validation
    // * do we have lab accounts/environments setup for device auth?
    // * do we need to set SaveToCache in the constructor?
    internal class DeviceCodeRequest : RequestBase
    {
        private readonly Action<DeviceCodeResult> _deviceCodeResultCallback;
        private DeviceCodeResult _deviceCodeResult;

        public DeviceCodeRequest(AuthenticationRequestParameters authenticationRequestParameters, Action<DeviceCodeResult> deviceCodeResultCallback) 
            : base(authenticationRequestParameters)
        {
            _deviceCodeResultCallback = deviceCodeResultCallback;
            
            LoadFromCache = false;  // no cache lookup for token
            SupportADFS = false;
        }

        internal override async Task PreTokenRequestAsync()
        {
            await base.PreTokenRequestAsync().ConfigureAwait(false);

            OAuth2Client client = new OAuth2Client();

            var deviceCodeScopes = new SortedSet<string>();
            deviceCodeScopes.UnionWith(AuthenticationRequestParameters.Scope);
            deviceCodeScopes.Add("offline_access");
            deviceCodeScopes.Add("profile");
            deviceCodeScopes.Add("openid");

            client.AddBodyParameter(OAuth2Parameter.ClientId, AuthenticationRequestParameters.ClientId);
            client.AddBodyParameter(OAuth2Parameter.Scope, deviceCodeScopes.AsSingleString());

            // todo: THIS IS A MAJOR HACK.  Work with Shiung/Henrik on proper way to determine the device code endpoint
            string deviceCodeEndpoint = AuthenticationRequestParameters.Authority.TokenEndpoint
                .Replace("token", "devicecode")
                .Replace("common", "organizations");

            AuthenticationRequestParameters.Authority.TokenEndpoint = AuthenticationRequestParameters.Authority.TokenEndpoint.Replace("common", "organizations");

            DeviceCodeResponse response = await client.ExecuteRequestAsync<DeviceCodeResponse>(
                new Uri(deviceCodeEndpoint), 
                HttpMethod.Post,
                AuthenticationRequestParameters.RequestContext).ConfigureAwait(false);

            // todo: fix resource/scopes here for how we want to invoke this...
            _deviceCodeResult = response.GetResult(AuthenticationRequestParameters.ClientId, "todo resource");
            _deviceCodeResultCallback(_deviceCodeResult);
        }

        protected override async Task SendTokenRequestAsync()
        {
            TimeSpan timeRemaining = _deviceCodeResult.ExpiresOn - DateTimeOffset.UtcNow;

            while (timeRemaining.TotalSeconds > 0.0)
            {
                try
                {
                    await base.SendTokenRequestAsync().ConfigureAwait(false);
                    return;
                }
                catch (MsalServiceException ex)
                {
                    if (ex.ErrorCode.Equals("authorization_pending", StringComparison.OrdinalIgnoreCase))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(_deviceCodeResult.Interval)).ConfigureAwait(false);
                        timeRemaining = _deviceCodeResult.ExpiresOn - DateTimeOffset.UtcNow;
                    }
                    else
                    { 
                        throw;
                    }
                }
            }

            throw new MsalServiceException("code_expired", "Verification code expired before contacting the server");
        }

        protected override void SetAdditionalRequestParameters(OAuth2Client client)
        {
            client.AddBodyParameter(OAuth2Parameter.GrantType, OAuth2GrantType.DeviceCode);
            client.AddBodyParameter(OAuth2Parameter.Code, _deviceCodeResult.DeviceCode);
        }
    }
}
