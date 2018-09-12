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

using Foundation;
using Security;
using System;

namespace Microsoft.Identity.Core.Cache
{
    internal class LegacyCachePersistance : ILegacyCachePersistance
    {
        const string NAME = "ADAL.PCL.iOS";
        private const string LocalSettingsContainerName = "ActiveDirectoryAuthenticationLibrary";

        private string keychainGroup;

        private string GetBundleId()
        {
            return NSBundle.MainBundle.BundleIdentifier;
        }

        public void SetKeychainSecurityGroup(string keychainSecurityGroup)
        {
            if (keychainSecurityGroup == null)
            {
                keychainGroup = GetBundleId();
            }
            else
            {
                keychainGroup = keychainSecurityGroup;
            }
        }

        byte[] ILegacyCachePersistance.LoadCache()
        {
            try
            {
                SecStatusCode res;
                var rec = new SecRecord(SecKind.GenericPassword)
                {
                    Generic = NSData.FromString(LocalSettingsContainerName),
                    Accessible = SecAccessible.Always,
                    Service = NAME + " Service",
                    Account = NAME + " cache",
                    Label = NAME + " Label",
                    Comment = NAME + " Cache",
                    Description = "Storage for cache"
                };

                if (keychainGroup != null)
                {
                    rec.AccessGroup = keychainGroup;
                }

                var match = SecKeyChain.QueryAsRecord(rec, out res);
                if (res == SecStatusCode.Success && match != null && match.ValueData != null)
                {
                    return match.ValueData.ToArray();

                }
            }
            catch (Exception ex)
            {
                string msg = "Failed to load adal cache: ";
                string noPiiMsg = CoreExceptionFactory.Instance.GetPiiScrubbedDetails(ex);
                CoreLoggerBase.Default.Warning(msg + noPiiMsg);
                CoreLoggerBase.Default.WarningPii(msg + ex);
                // Ignore as the cache seems to be corrupt
            }
            return null;
        }

        void ILegacyCachePersistance.WriteCache(byte[] serializedCache)
        {
            try
            {
                var s = new SecRecord(SecKind.GenericPassword)
                {
                    Generic = NSData.FromString(LocalSettingsContainerName),
                    Accessible = SecAccessible.Always,
                    Service = NAME + " Service",
                    Account = NAME + " cache",
                    Label = NAME + " Label",
                    Comment = NAME + " Cache",
                    Description = "Storage for cache"
                };

                if (keychainGroup != null)
                {
                    s.AccessGroup = keychainGroup;
                }

                var err = SecKeyChain.Remove(s);
                if (err != SecStatusCode.Success)
                {
                    string msg = "Failed to remove adal cache record: ";
                    CoreLoggerBase.Default.Warning(msg);
                    CoreLoggerBase.Default.WarningPii(msg + err);
                }

                if (serializedCache != null && serializedCache.Length > 0)
                {
                    s.ValueData = NSData.FromArray(serializedCache);
                    err = SecKeyChain.Add(s);
                    if (err != SecStatusCode.Success)
                    {
                        string msg = "Failed to save adal cache record: ";
                        CoreLoggerBase.Default.Warning(msg);
                        CoreLoggerBase.Default.WarningPii(msg + err);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Failed to save adal cache: ";
                string noPiiMsg = CoreExceptionFactory.Instance.GetPiiScrubbedDetails(ex);
                CoreLoggerBase.Default.Warning(msg + noPiiMsg);
                CoreLoggerBase.Default.WarningPii(msg + ex);
            }
        }
    }
}
