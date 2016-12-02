//------------------------------------------------------------------------------
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

using AppKit;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    /// <summary>
    /// Additional parameters used in acquiring user's authorization
    /// </summary>
    public class PlatformParameters : IPlatformParameters
    {
        private PlatformParameters()
        {
        }


        /// <summary>
        /// Additional parameters used in acquiring user's authorization
        /// </summary>
        /// <param name="callerWindow">NSWindow instance</param>
        public PlatformParameters(NSWindow callerWindow, PromptBehavior promptBehavior) : this(callerWindow)
        {
            this.PromptBehavior = promptBehavior;
        }

        /// <summary>
        /// Additional parameters used in acquiring user's authorization
        /// </summary>
        /// <param name="callerWindow">NSWindow instance</param>
        public PlatformParameters(NSWindow callerWindow):this()
        {
            this.CallerWindow = callerWindow;
        }

        /// <summary>
        /// Caller NSWindow
        /// </summary>
        public NSWindow CallerWindow { get; private set; }


        /// <summary>
        /// Gets prompt behavior. If <see cref="PromptBehavior.Always"/>, asks service to show user the authentication page which gives them chance to authenticate as a different user. PromptBehavior.Never is not supported.
        /// </summary>
        public PromptBehavior PromptBehavior { get; private set; }
    }
}
