// ------------------------------------------------------------------------------
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
// ------------------------------------------------------------------------------

using System;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// Authority Information for use with the MsalConfigurationBuilder to configure a PublicClientApplication or ConfidentialClientApplication.
    /// </summary>
    public sealed class MsalAuthorityInfo
    {
        internal MsalAuthorityInfo(
            MsalAuthorityType authorityType,
            AadAuthorityAudience aadAuthorityAudience,
            string authorityUri)
        {
            AuthorityType = authorityType;
            AadAuthorityAudience = aadAuthorityAudience;
            AuthorityUri = authorityUri;

            switch (authorityType)
            {
            case MsalAuthorityType.Aad:
                switch (aadAuthorityAudience)
                {
                case AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount:
                    AuthorityUri = "https://login.microsoftonline.com/common/";
                    break;
                case AadAuthorityAudience.MicrosoftAccountOnly:
                    AuthorityUri = "https://login.microsoftonline.com/consumers/";
                    break;
                case AadAuthorityAudience.Default:
                case AadAuthorityAudience.AzureAdOnly:
                    AuthorityUri = "https://login.microsoftonline.com/organizations/";
                    break;
                case AadAuthorityAudience.None:
                default:
                    throw new ArgumentException(nameof(aadAuthorityAudience));
                }

                break;
            case MsalAuthorityType.B2C when AadAuthorityAudience != AadAuthorityAudience.None:
                throw new ArgumentException(nameof(aadAuthorityAudience));
            }
        }

        /// <summary>
        /// The type of the authority
        /// </summary>
        public MsalAuthorityType AuthorityType { get; }

        /// <summary>
        /// The Authority Audience if the AuthorityType is AAD
        /// </summary>
        public AadAuthorityAudience AadAuthorityAudience { get; }

        /// <summary>
        /// The Authority URI as applicable.
        /// </summary>
        public string AuthorityUri { get; }
    }
}