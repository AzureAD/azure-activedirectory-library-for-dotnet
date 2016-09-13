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
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class HttpEvent : DefaultEvent
    {
        internal HttpEvent() : base(EventConstants.HttpEvent)
        {
            SetEvent(EventConstants.EventName,"http_event");
        }

        internal string UserAgent { get; set; }

        internal string HttpMethod { get; set; }

        internal HttpStatusCode HttpResponseCode { get; set; }

        internal string HttpQueryParameters { get; set; }

        internal string HttpResponseMethod { get; set; }

        internal string ResponseApiVersion { get; set; }

        internal void ParseQuery(string Query)
        {
            if (Query.Length != 0)
            {
                String QueryShifted = Query.Substring(1, Query.Length - 1);
                String[] result = QueryShifted.Split('&');
                StringBuilder sb = new StringBuilder();
                foreach (string s in result)
                {
                    if (!(s.Contains("password") || s.Contains("upn")))
                    {
                        sb.Append(s).Append(" ");
                    }
                }
                HttpQueryParameters = sb.ToString();
                SetEvent(EventConstants.HttpQueryParameters, HttpQueryParameters);
            }
        }

        internal void SetEvent(string eventName, HttpStatusCode eventParameter)
        {
                DefaultEvents.Add(new Tuple<string, string>(eventName, eventParameter.ToString()));
        }
    }
}
