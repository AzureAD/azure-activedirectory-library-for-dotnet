﻿//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

using Test.ADAL.Common;

namespace Test.ADAL.WinPhone.UnitTest
{
    internal class ReplayerHttpWebRequest : ReplayerBase, IHttpWebRequest
    {
        private readonly IHttpWebRequest internalHttpWebRequest;
        private readonly Dictionary<string, string> keyElements;

        public string ResponseValue { get; set; }
        public bool Success { get; set; }

        public ReplayerHttpWebRequest(string uri)
        {
            this.internalHttpWebRequest = (new HttpWebRequestFactory()).Create(uri);
            this.keyElements = new Dictionary<string, string>();
            this.keyElements["Uri"] = uri;
        }



        public RequestParameters BodyParameters
        {
            set
            {
                this.internalHttpWebRequest.BodyParameters = value;
            }

            get
            {
                return this.internalHttpWebRequest.BodyParameters;
            }
        }

        public string Accept
        {
            set
            {
                this.keyElements["Accept"] = value;
            }
        }

        public string ContentType
        {
            set
            {
                this.keyElements["ContentType"] = value;
                this.internalHttpWebRequest.ContentType = value;
            }
        }

        public string Method
        {
            set
            {
                this.keyElements["Method"] = value;
            }
        }

        public bool UseDefaultCredentials
        {
            set
            {
                this.keyElements["UseDefaultCredentials"] = value.ToString();
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return this.internalHttpWebRequest.Headers;
            }
        }

        public async Task<IHttpWebResponse> GetResponseSyncOrAsync(CallState callState)
        {
            foreach (var headerKey in this.internalHttpWebRequest.Headers.AllKeys)
            {
                this.keyElements["Header-" + headerKey] = this.internalHttpWebRequest.Headers[headerKey];
            }

            if (this.internalHttpWebRequest.BodyParameters != null)
            {
                foreach (var kvp in this.internalHttpWebRequest.BodyParameters)
                {
                    this.keyElements["Body-" + kvp.Key] = kvp.Value;
                }
            }

            if (this.Success)
            {
                return new ReplayerHttpWebResponse(this.ResponseValue, HttpStatusCode.OK);
            }
            else
            {
                throw SerializationHelper.DeserializeWebException(this.ResponseValue);
            }
        }
    }
}
