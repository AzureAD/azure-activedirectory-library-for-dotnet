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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Test.ADAL.WinPhone.UnitTest
{
    class ReplayerHttpWebRequestFactory : IHttpWebRequestFactory
    {

        public ReplayerHttpWebRequest UserProvidedHttpWebRequest { get; set; }

        public IHttpWebRequest Create(string uri)
        {
            if (UserProvidedHttpWebRequest != null)
            {
                return UserProvidedHttpWebRequest;
            }

            return new ReplayerHttpWebRequest(uri);
        }

        public IHttpWebResponse CreateResponse(WebResponse response)
        {
            return new ReplayerHttpWebResponse(response);
        }
    }
}
