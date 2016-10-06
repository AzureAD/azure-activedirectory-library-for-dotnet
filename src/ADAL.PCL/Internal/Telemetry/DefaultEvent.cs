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

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class DefaultEvent : EventsBase
    {
        internal List<Tuple<string, string>> EventDictitionary = new List<Tuple<string, string>>();

        static DefaultEvent()
        {
            ApplicationName = PlatformPlugin.PlatformInformation.GetApplicationName();

            ApplicationVersion = PlatformPlugin.PlatformInformation.GetApplicationVersion();

            SdkVersion = AdalIdHelper.GetAdalVersion();

            SdkPlatform = AdalIdHelper.GetAssemblyFileVersion();

            DeviceId =
                PlatformPlugin.CryptographyHelper.CreateSha256Hash(PlatformPlugin.PlatformInformation.GetDeviceId());
        }

        internal DefaultEvent()
        {
            SetEvent(EventConstants.ApplicationName, ApplicationName);

            SetEvent(EventConstants.ApplicationVersion, ApplicationVersion);

            SetEvent(EventConstants.SdkVersion, SdkVersion);

            SetEvent(EventConstants.SdkPlatform, SdkPlatform);

            SetEvent(EventConstants.DeviceId, DeviceId);
        }

        internal string ClientId { get; set; }

        internal string ClientIp { get; set; }

        internal static string ApplicationName { get; set; }

        internal static string ApplicationVersion { get; set; }

        internal static string SdkPlatform { get; set; }

        internal static string SdkVersion { get; set; }

        internal string UserId { get; set; }

        internal static string DeviceId { get; set; }

        internal Guid CorrelationId { get; set; }

        internal override void SetEvent(string eventName, string eventParameter)
        {
            if (eventParameter != null && eventParameter.Length != 0)
            {
                EventDictitionary.Add(new Tuple<string, string>(eventName, eventParameter));
            }
        }

        internal void SetEvent(string eventName, bool eventParameter)
        {
            EventDictitionary.Add(new Tuple<string, string>(eventName, eventParameter.ToString()));
        }

        internal override List<Tuple<string, string>> GetEvents()
        {
            return EventDictitionary;
        }

        internal override void ProcessEvent(Dictionary<string, string> dispatchMap)
        {
            if (!dispatchMap.ContainsKey(EventConstants.ApplicationName))
            {
                dispatchMap.Add(EventConstants.ApplicationName, ApplicationName);
            }

            if (!dispatchMap.ContainsKey(EventConstants.ApplicationVersion))
            {
                dispatchMap.Add(EventConstants.ApplicationVersion, ApplicationVersion);
            }

            if (!dispatchMap.ContainsKey(EventConstants.ClientId))
            {
                dispatchMap.Add(EventConstants.ClientId, ClientId);
            }

            if (!dispatchMap.ContainsKey(EventConstants.DeviceId))
            {
                dispatchMap.Add(EventConstants.DeviceId, DeviceId);
            }
        }
    }
}