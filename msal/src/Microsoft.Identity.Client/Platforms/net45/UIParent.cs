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

using Microsoft.Identity.Client.Internal;
using Microsoft.Identity.Core.UI;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// Contains UI properties for interactive flows, such as the parent window (on Windows), or the parent activity (on Xamarin.Android), and 
    /// which browser to use (on Xamarin.Android and Xamarin.iOS)
    /// </summary> 
    public sealed partial class UIParent
    {
        static UIParent()
        {
            ModuleInitializer.EnsureModuleInitialized();
        }

        internal CoreUIParent CoreUIParent { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UIParent()
        {
            CoreUIParent = new CoreUIParent();
        }

        /// <summary>
        /// Initializes an instance for a provided parent window.
        /// </summary>
        /// <param name="ownerWindow">Parent window object reference. OPTIONAL. The expected parent window
        /// are either of type <see cref="System.Windows.Forms.IWin32Window"/> or <see cref="System.IntPtr"/> (for window handle)</param>
        public UIParent(object ownerWindow)
        {
            CoreUIParent = new CoreUIParent(ownerWindow);
        }

        /// <summary>
        /// Platform agnostic constructor that allows building an UIParent from a NetStandard assembly.
        /// </summary>
        /// <param name="ownerWindow">Parent window object reference. OPTIONAL. The expected parent window
        /// are either of type <see cref="System.Windows.Forms.IWin32Window"/> or <see cref="System.IntPtr"/> (for window handle)</param>
        /// <param name="useEmbeddedWebview">Ignored, on .net desktop an embedded webview is always used</param>
        public UIParent(object ownerWindow, bool useEmbeddedWebview) :
            this(ownerWindow)
        {
        }

        //hidden webview can be used in both WinRT and desktop applications.
        internal bool UseHiddenBrowser
        {
            get => CoreUIParent.UseHiddenBrowser;
            set => CoreUIParent.UseHiddenBrowser = value;
        }

    }
}