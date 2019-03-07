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

using Microsoft.Identity.Core.UI;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Extensibility;
using System;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    /// <summary>
    /// Additional parameters used in acquiring user's authorization
    /// </summary>
    public class PlatformParameters : IPlatformParameters
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="promptBehavior"></param>
        public PlatformParameters(PromptBehavior promptBehavior)
        {
            this.PromptBehavior = promptBehavior;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="promptBehavior"></param>
        /// <param name="ownerWindow"></param>
        public PlatformParameters(PromptBehavior promptBehavior, object ownerWindow)
        {
            this.PromptBehavior = promptBehavior;
            this.OwnerWindow = ownerWindow ?? throw new ArgumentNullException(nameof(ownerWindow));
        }

        /// <summary>
        /// Constructor that allows extends to configure their own web ui. 
        /// </summary>
        /// <param name="promptBehavior">Controls the prompt that is displayed on web ui. Default is <see cref="PromptBehavior.SelectAccount"/>.</param>
        /// <param name="customWebUi">Custom implementation of the web ui</param>
        public PlatformParameters(PromptBehavior promptBehavior, ICustomWebUi customWebUi)
        {
            this.PromptBehavior = promptBehavior;
            this.CustomWebUi = customWebUi ?? throw new ArgumentNullException(nameof(customWebUi));
        }

        /// <summary>
        /// Gets the owner of the browser dialog which pops up for receiving user credentials. It can be null.
        /// </summary>
        public object OwnerWindow { get; private set; }

        /// <summary>
        /// Gets prompt behavior. If <see cref="PromptBehavior.Always"/>, asks service to show user the authentication page which gives them chance to authenticate as a different user.
        /// </summary>
        public PromptBehavior PromptBehavior { get; internal set; }

        /// <summary>
        ///  Extension method enabling ADAK.NET extenders for public client applications to set a custom web ui
        ///  that will let the user sign-in with Azure AD, present consent if needed, and get back the authorization
        ///  code.
        /// </summary>
        public ICustomWebUi CustomWebUi { get; private set; }

        internal CoreUIParent GetCoreUIParent()
        {
            return new CoreUIParent()
            {
                OwnerWindow = this.OwnerWindow,
                UseHiddenBrowser = (this.PromptBehavior == PromptBehavior.Never),
                CustomWebUi = this.CustomWebUi
            };
        }
    }
}
