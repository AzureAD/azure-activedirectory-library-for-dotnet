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
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Test.ADAL.NET.Friend
{
    public static class RecorderJwtId
    {
        public static int JwtIdIndex { get; set; }
    }

    class RecorderHttpClientFactory :  RecorderBase, IHttpClientFactory
    {
        public RecorderHttpClientFactory()
        {
            Initialize();
        }

        public IHttpClient Create(string uri, CallState callState)
        {
            return new RecorderHttpClient(uri, callState);
        }

        public bool AddAdditionalHeaders
        {
            get { return false; }
        }

        public DateTime GetJsonWebTokenValidFrom()
        {
            const string JsonWebTokenValidFrom = "JsonWebTokenValidFrom";
            if (IOMap.ContainsKey(JsonWebTokenValidFrom))
            {
                return new DateTime(long.Parse(IOMap[JsonWebTokenValidFrom]));
            }

            DateTime result = DateTime.UtcNow;

            IOMap[JsonWebTokenValidFrom] = result.Ticks.ToString();

            return result;
        }

        public string GetJsonWebTokenId()
        {
            const string JsonWebTokenIdPrefix = "JsonWebTokenId";
            string jsonWebTokenId = JsonWebTokenIdPrefix + RecorderJwtId.JwtIdIndex;
            if (IOMap.ContainsKey(jsonWebTokenId))
            {
                return IOMap[jsonWebTokenId];
            }

            string id = Guid.NewGuid().ToString();

            IOMap[jsonWebTokenId] = id;

            return id;
        }
    }
}
