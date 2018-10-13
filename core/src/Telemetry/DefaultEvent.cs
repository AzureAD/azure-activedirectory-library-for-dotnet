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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Identity.Core.Telemetry
{
    internal class DefaultEvent : EventBase
    {
        public DefaultEvent(string clientId, ConcurrentDictionary<string, int> eventCount) : base((string) (EventNamePrefix + "default_event"))
        {
            this[EventNamePrefix + "client_id"] = clientId;
            this[EventNamePrefix + "sdk_platform"] = CorePlatformInformationBase.Instance.GetProductName()?.ToLowerInvariant();
            this[EventNamePrefix + "sdk_version"] = MsalIdHelper.GetMsalVersion();
            this[EventNamePrefix + "application_name"] = PlatformProxyFactory.GetPlatformProxy().GetApplicationName()?.ToLowerInvariant();
            this[EventNamePrefix + "application_version"] = PlatformProxyFactory.GetPlatformProxy().GetApplicationVersion()?.ToLowerInvariant();
            this[EventNamePrefix + "device_id"] = PlatformProxyFactory.GetPlatformProxy().GetDeviceId()?.ToLowerInvariant();
            this[EventNamePrefix + "ui_event_count"] = SetEventCount(EventNamePrefix + "ui_event", eventCount);
            this[EventNamePrefix + "http_event_count"] = SetEventCount(EventNamePrefix + "http_event", eventCount);
            this[EventNamePrefix + "cache_event_count"] = SetEventCount(EventNamePrefix + "cache_event", eventCount);
        }

        private string SetEventCount(string eventName, ConcurrentDictionary<string, int> eventCount)
        {
            if (!eventCount.ContainsKey(eventName))
            {
                return string.Empty;
            }
            return eventCount[eventName].ToString(CultureInfo.InvariantCulture);
        }

    }
}