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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Identity.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.ClientCreds;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Instance;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Flows
{
    class AcquireDeviceCodeHandler
    {
        private readonly Authenticator authenticator;
        private readonly ClientKey clientKey;
        private readonly string resource;
        private readonly RequestContext requestContext;
        private readonly string extraQueryParameters;

        public AcquireDeviceCodeHandler(Authenticator authenticator, string resource, string clientId, string extraQueryParameters)
        {
            this.authenticator = authenticator;
            this.requestContext = AcquireTokenHandlerBase.CreateCallState(this.authenticator.CorrelationId);
            this.clientKey = new ClientKey(clientId);
            this.resource = resource;
            this.extraQueryParameters = extraQueryParameters;
        }
        
        private string CreateDeviceCodeRequestUriString()
        {
            var deviceCodeRequestParameters = new DictionaryRequestParameters(this.resource, this.clientKey);

            if (this.requestContext != null && this.requestContext.CorrelationId != Guid.Empty)
            {
                deviceCodeRequestParameters[OAuthParameter.CorrelationId] = this.requestContext.CorrelationId.ToString();
            }
            
                IDictionary<string, string> adalIdParameters = AdalIdHelper.GetAdalIdParameters();
                foreach (KeyValuePair<string, string> kvp in adalIdParameters)
                {
                    deviceCodeRequestParameters[kvp.Key] = kvp.Value;
                }
            
            if (!string.IsNullOrWhiteSpace(extraQueryParameters))
            {
                // Checks for extraQueryParameters duplicating standard parameters
                Dictionary<string, string> kvps = EncodingHelper.ParseKeyValueList(extraQueryParameters, '&', false, this.requestContext);
                foreach (KeyValuePair<string, string> kvp in kvps)
                {
                    if (deviceCodeRequestParameters.ContainsKey(kvp.Key))
                    {
                        throw new AdalException(AdalError.DuplicateQueryParameter, string.Format(CultureInfo.CurrentCulture, AdalErrorMessage.DuplicateQueryParameterTemplate, kvp.Key));
                    }
                }

                deviceCodeRequestParameters.ExtraQueryParameter = extraQueryParameters;
            }

            return new Uri(new Uri(this.authenticator.DeviceCodeUri), "?" + deviceCodeRequestParameters).AbsoluteUri;
        }

        internal async Task<DeviceCodeResult> RunHandlerAsync()
        {
            await this.authenticator.UpdateFromTemplateAsync(this.requestContext).ConfigureAwait(false);
            this.ValidateAuthorityType();
            AdalHttpClient client = new AdalHttpClient(CreateDeviceCodeRequestUriString(), this.requestContext);
            DeviceCodeResponse response = await client.GetResponseAsync<DeviceCodeResponse>().ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Error))
            {
                throw new AdalException(response.Error, response.ErrorDescription);
            }

            return response.GetResult(clientKey.ClientId, resource);
        }

        private void ValidateAuthorityType()
        {
            if (this.authenticator.AuthorityType == AuthorityType.ADFS)
            {
                throw new AdalException(AdalError.InvalidAuthorityType,
                    string.Format(CultureInfo.CurrentCulture, AdalErrorMessage.InvalidAuthorityTypeTemplate, this.authenticator.Authority));
            }
        }

    }
}
