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
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class CacheEvent : DefaultEvent
    {
        internal CacheEvent(string eventName) : base(EventConstants.CacheEvent)
        {
            //Fill in the default parameters
            this.EventName = eventName;
        }

        internal string EventName { get; set; }

        internal string IsMultipleResourceRt { get; set; }

        internal string TokenFound { get; set; }

        internal string TokenNearExpiry { get; set; }

        internal string TokenExtendedLifeTimeExpired { get; set; }

        internal string IsCrossTenantRt { get; set; }

        internal string TokenExpired { get; set; }

        internal string ExtendedLifeTimeEnabled { get; set; }

        internal string ExpiredAt { get; set; }

        internal string TokenSubjectType { get; set; }

        internal override void SetEvent(string eventName, string eventParameter)
        {
            DefaultEvents.Add(new Tuple<string, string>(eventName, eventParameter));
        }

        internal void SetEvent(string eventName, bool eventParameter)
        {
            DefaultEvents.Add(new Tuple<string, string>(eventName, eventParameter.ToString()));
        }
    }
}
