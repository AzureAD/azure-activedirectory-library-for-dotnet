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

using Microsoft.Identity.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if ANDROID || iOS || WINDOWS_APP
using Microsoft.Identity.Core.Cache;
#endif

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal
{
    /// <summary>
    /// This class marked with ifdefs because only iOS/Android/WinRT provide platform default storage. 
    /// Delegates have no implementation for netstandard1.1, netstandard1.3 and net45.
    /// Platform specific persistance logic is implemented in core.
    /// </summary>

#if ANDROID
    [Android.Runtime.Preserve(AllMembers = true)]
#endif
    internal static class StorageDelegates
    {
        internal static readonly ILegacyCachePersistance legacyCachePersistance = new LegacyCachePersistance();

        public static void BeforeAccess(TokenCacheNotificationArgs args)
        {
#if ANDROID || iOS || WINDOWS_APP
            if (args != null && args.TokenCache != null)
            {
                args.TokenCache.Deserialize(legacyCachePersistance.LoadCache());
            }
#endif
        }

        public static void AfterAccess(TokenCacheNotificationArgs args)
        {
#if ANDROID || iOS || WINDOWS_APP
            if (args != null && args.TokenCache != null && args.TokenCache.HasStateChanged)
            {
                legacyCachePersistance.WriteCache(args.TokenCache.Serialize());
                args.TokenCache.HasStateChanged = false;
            }
#endif
        }

    }
}
