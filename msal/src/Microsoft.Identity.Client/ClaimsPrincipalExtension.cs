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

using System;
using System.Security.Claims;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// Extension methods for ClaimsPrincipal
    /// </summary>
    public static class ClaimsPrincipalExtension
    {
        /// <summary>
        /// Get the Account identifier for an MSAL.NET account from a ClaimsPrincipal
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal</param>
        /// <returns>A string corresponding to an account identifier as defined in <see cref="Microsoft.Identity.Client.AccountId.Identifier"/></returns>
        public static string GetMsalAccountId(this ClaimsPrincipal claimsPrincipal)
        {
            string userObjectId = claimsPrincipal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            if (string.IsNullOrEmpty(userObjectId))
            {
                userObjectId = claimsPrincipal.FindFirstValue("oid");
            }
            string tenantId = claimsPrincipal.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
            if (string.IsNullOrEmpty(tenantId))
            {
                tenantId = claimsPrincipal.FindFirstValue("tid");
            }

            if (string.IsNullOrWhiteSpace(userObjectId)) // TODO: find a better typed exception
                throw new ArgumentOutOfRangeException("Missing claim 'http://schemas.microsoft.com/identity/claims/objectidentifier' or 'oid' ");

            if (string.IsNullOrWhiteSpace(tenantId))
                throw new ArgumentOutOfRangeException("Missing claim 'http://schemas.microsoft.com/identity/claims/tenantid' or 'tid' ");

            string accountId = userObjectId + "." + tenantId;
            return accountId;
        }

        /// <summary>
        /// Get the display name for the signed-in user, based on their claims principal
        /// </summary>
        /// <param name="claimsPrincipal">Claims about the user/account</param>
        /// <returns>A string containing the display name for the user, as brought by Azure AD v1.0 and v2.0 tokens,
        /// or <c>null</c> if the claims cannot be found</returns>
        /// <remarks>See https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens#payload-claims </remarks>
        public static string GetDisplayName(this ClaimsPrincipal claimsPrincipal)
        {
            // Attempting the claims brought by an Azure AD v2.0 token first
            string displayName = claimsPrincipal.FindFirstValue("preferred_username");

            // Otherwise falling back to the claims brought by an Azure AD v1.0 token
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = claimsPrincipal.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);
            }
            
            // Finally falling back to name
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = claimsPrincipal.FindFirstValue("name");
            }
            return displayName;
        }

         /// <summary>
        ///     Return the claim value for the first claim with the specified type if it exists, null otherwise
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        private static string FindFirstValue(this ClaimsPrincipal principal, string claimType)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("identity");
            }
            
            return principal.FindFirst(claimType)?.Value;
        }
    }
}