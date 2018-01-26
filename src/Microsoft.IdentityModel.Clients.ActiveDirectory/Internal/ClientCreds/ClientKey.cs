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
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Instance;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.ClientCreds
{
    internal class ClientKey
    {

        public ClientKey(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            this.ClientId = clientId;
        }

        public ClientKey(ClientCredential clientCredential)
        {
            this.Credential = clientCredential ?? throw new ArgumentNullException(nameof(clientCredential));
            this.ClientId = clientCredential.ClientId;
            this.HasCredential = true;
        }

        public ClientKey(IClientAssertionCertificate clientCertificate, Authenticator authenticator)
        {
            this.Authenticator = authenticator;
            this.Certificate = clientCertificate ?? throw new ArgumentNullException(nameof(clientCertificate));
            this.ClientId = clientCertificate.ClientId;
            this.HasCredential = true;
        }

        public ClientKey(ClientAssertion clientAssertion)
        {
            this.Assertion = clientAssertion ?? throw new ArgumentNullException(nameof(clientAssertion));
            this.ClientId = clientAssertion.ClientId;
            this.HasCredential = true;
        }

        public ClientCredential Credential { get; private set; }

        public IClientAssertionCertificate Certificate { get; private set; }

        public ClientAssertion Assertion { get; private set; }

        public Authenticator Authenticator { get; private set; }

        public string ClientId { get; private set; }

        public bool HasCredential { get; private set; }

        public bool SendX5c { get; set; }

        public void AddToParameters(IDictionary<string, string> parameters)
        {
            if (this.ClientId != null)
            {
                parameters[OAuthParameter.ClientId] = this.ClientId;
            }

            if (this.Credential != null)
            {
                if (this.Credential.SecureClientSecret != null)
                {
                    this.Credential.SecureClientSecret.ApplyTo(parameters);
                }
                else
                {
                    parameters[OAuthParameter.ClientSecret] = this.Credential.ClientSecret;
                }
            }
            else if (this.Assertion != null)
            {
                parameters[OAuthParameter.ClientAssertionType] = this.Assertion.AssertionType;
                parameters[OAuthParameter.ClientAssertion] = this.Assertion.Assertion;
            }
            else if (this.Certificate != null)
            {
                JsonWebToken jwtToken = new JsonWebToken(this.Certificate, this.Authenticator.SelfSignedJwtAudience);
                ClientAssertion clientAssertion = jwtToken.Sign(this.Certificate, this.SendX5c);
                parameters[OAuthParameter.ClientAssertionType] = clientAssertion.AssertionType;
                parameters[OAuthParameter.ClientAssertion] = clientAssertion.Assertion;
            }
        }
    }
}
