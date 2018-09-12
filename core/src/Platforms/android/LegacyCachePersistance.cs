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

using Android.App;
using Android.Content;
using System;

namespace Microsoft.Identity.Core.Cache
{
    [Android.Runtime.Preserve(AllMembers = true)]
    internal class LegacyCachePersistance : ILegacyCachePersistance
    {
        private const string SharedPreferencesName = "ActiveDirectoryAuthenticationLibrary";
        private const string SharedPreferencesKey = "cache";
        
        byte[] ILegacyCachePersistance.LoadCache()
        {
            try
            {
                ISharedPreferences preferences = Application.Context.GetSharedPreferences(SharedPreferencesName, FileCreationMode.Private);
                string stateString = preferences.GetString(SharedPreferencesKey, null);
                if (stateString != null)
                {
                    return Convert.FromBase64String(stateString);
                }
            }
            catch (Exception ex)
            {
                string msg = "An error occurred while reading the adal cache: ";
                string noPiiMsg = CoreExceptionFactory.Instance.GetPiiScrubbedDetails(ex);
                CoreLoggerBase.Default.Error(msg + noPiiMsg);
                CoreLoggerBase.Default.ErrorPii(msg + ex);
                // Ignore as the cache seems to be corrupt
            }

            return null;
        }

        void ILegacyCachePersistance.WriteCache(byte[] serializedCache)
        {
                try
                {
                    ISharedPreferences preferences = Application.Context.GetSharedPreferences(SharedPreferencesName, FileCreationMode.Private);
                    ISharedPreferencesEditor editor = preferences.Edit();
                    editor.Remove(SharedPreferencesKey);
                    string stateString = Convert.ToBase64String(serializedCache);
                    editor.PutString(SharedPreferencesKey, stateString);
                    editor.Apply();
                }
                catch (Exception ex)
            {
                const string msg = "Failed to save adal cache: ";
                string noPiiMsg = CoreExceptionFactory.Instance.GetPiiScrubbedDetails(ex);
                CoreLoggerBase.Default.Error(msg + noPiiMsg);
                CoreLoggerBase.Default.ErrorPii(msg + ex);
            }
        }
    }
}
