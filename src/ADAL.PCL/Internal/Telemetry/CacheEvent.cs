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

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class CacheEvent : DefaultEvent
    {
        internal string IsMultipleResourceRt { get; set; }

        internal string TokenSubjectType { get; set; }

        internal override void ProcessEvent(Dictionary<string, string> dispatchMap)
        {
            if (dispatchMap.ContainsKey(EventConstants.IsMRRT))
            {
                dispatchMap[EventConstants.IsMRRT] = string.Empty;
            }

            if (dispatchMap.ContainsKey(EventConstants.ExtendedExpires))
            {
                dispatchMap[EventConstants.ExtendedExpires] = string.Empty;
            }

            if (dispatchMap.ContainsKey(EventConstants.IsRT))
            {
                dispatchMap[EventConstants.IsRT] = string.Empty;
            }

            foreach (KeyValuePair<string, string> Event in EventDictitionary)
            {
                if (Event.Key.Equals(EventConstants.IsMRRT) ||
                    Event.Key.Equals(EventConstants.ExtendedExpires) ||
                    Event.Key.Equals(EventConstants.IsRT))
                {
                    dispatchMap[Event.Key] = Event.Value;
                }
            }
        }
    }
}