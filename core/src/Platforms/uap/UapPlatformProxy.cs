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


using Microsoft.Identity.Core.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.System;
using Microsoft.Identity.Core.Cache;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace Microsoft.Identity.Core
{
    /// <summary>
    /// Platform / OS specific logic. No library (ADAL / MSAL) specific code should go in here. 
    /// </summary>
    internal class UapPlatformProxy : IPlatformProxy
    {
        private readonly bool _isMsal;

        public UapPlatformProxy(bool isMsal)
        {
            _isMsal = isMsal;
        }

        /// <summary>
        /// Get the user logged in to Windows or throws
        /// </summary>
        /// <remarks>
        /// Win10 allows several identities to be logged in at once; 
        /// select the first principal name that can be used
        /// </remarks>
        /// <returns>The username or throws</returns>
        public async Task<string> GetUserPrincipalNameAsync()
        {
            IReadOnlyList<User> users = await User.FindAllAsync();
            if (users == null || !users.Any())
            {
                throw CoreExceptionFactory.Instance.GetClientException(
                    CoreErrorCodes.CannotAccessUserInformationOrUserNotDomainJoined,
                    CoreErrorMessages.UapCannotFindDomainUser);
            }

            var getUserDetailTasks = users.Select(async u =>
            {
                object domainObj = await u.GetPropertyAsync(KnownUserProperties.DomainName);
                string domainString = domainObj?.ToString();

                object principalObject = await u.GetPropertyAsync(KnownUserProperties.PrincipalName);
                string principalNameString = principalObject?.ToString();

                return new { Domain = domainString, PrincipalName = principalNameString };
            }).ToList();

            var userDetails = await Task.WhenAll(getUserDetailTasks).ConfigureAwait(false);

            // try to get a user that has both domain name and upn
            var userDetailWithDomainAndPn = userDetails.FirstOrDefault(
                d => !String.IsNullOrWhiteSpace(d.Domain) &&
                !String.IsNullOrWhiteSpace(d.PrincipalName));

            if (userDetailWithDomainAndPn != null)
            {
                return userDetailWithDomainAndPn.PrincipalName;
            }

            // try to get a user that at least has upn
            var userDetailWithPn = userDetails.FirstOrDefault(
              d => !String.IsNullOrWhiteSpace(d.PrincipalName));

            if (userDetailWithPn != null)
            {
                return userDetailWithPn.PrincipalName;
            }

            // user has domain name, but no upn -> missing Enterprise Auth capability
            if (userDetails.Any(d => !String.IsNullOrWhiteSpace(d.Domain)))
            {
                throw CoreExceptionFactory.Instance.GetClientException(
                   CoreErrorCodes.CannotAccessUserInformationOrUserNotDomainJoined,
                   CoreErrorMessages.UapCannotFindUpn);
            }

            // no domain, no upn -> missing User Info capability
            throw CoreExceptionFactory.Instance.GetClientException(
                CoreErrorCodes.CannotAccessUserInformationOrUserNotDomainJoined,
                CoreErrorMessages.UapCannotFindDomainUser);

        }

        public async Task<bool> IsUserLocalAsync(RequestContext requestContext)
        {
            IReadOnlyList<User> users = await User.FindAllAsync();
            return users.Any(u => u.Type == UserType.LocalUser || u.Type == UserType.LocalGuest);
        }

        public bool IsDomainJoined()
        {
            return NetworkInformation.GetHostNames().Any(entry => entry.Type == HostNameType.DomainName);
        }


        public string GetEnvironmentVariable(string variable)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            return localSettings.Values.ContainsKey(variable) ? localSettings.Values[variable].ToString() : null;
        }

        public string GetProcessorArchitecture()
        {
            return WindowsNativeMethods.GetProcessorArchitecture();
        }

        public string GetOperatingSystem()
        {
            // In WinRT, there is no way to reliably get OS version. All can be done reliably is to check 
            // for existence of specific features which does not help in this case, so we do not emit OS in WinRT.
            return null;
        }

        public string GetDeviceModel()
        {
            var deviceInformation = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            return deviceInformation.SystemProductName;
        }

        /// <inheritdoc />
        public void ValidateRedirectUri(Uri redirectUri, RequestContext requestContext)
        {
            if (_isMsal)
            {
            }
            else
            {
                // FROM ADAL
                if (redirectUri == null)
                {
                    redirectUri = Constants.SsoPlaceHolderUri;
                    requestContext.Logger.Verbose("ms-app redirect Uri is used");
                }
            }
        }

        /// <inheritdoc />
        public string GetRedirectUriAsString(Uri redirectUri, RequestContext requestContext)
        {
            return ReferenceEquals(redirectUri, Constants.SsoPlaceHolderUri)
                       ? WebAuthenticationBroker.GetCurrentApplicationCallbackUri().OriginalString
                       : redirectUri.OriginalString;
        }

        /// <inheritdoc />
        public string GetDefaultRedirectUri(string correlationId)
        {
            return Constants.DefaultRedirectUri;
        }

        public string GetProductName()
        {
            return _isMsal ? "MSAL.UAP" : "PCL.UAP";
        }

        public string GetApplicationName()
        {
            return Package.Current.DisplayName;
        }

        public string GetApplicationVersion()
        {
            return Package.Current.Id.Version.ToString();
        }

        public string GetDeviceId()
        {
            return new EasClientDeviceInformation().Id.ToString(); 
        }

        /// <inheritdoc />
        public ILegacyCachePersistence LegacyCachePersistence { get; } = new UapLegacyCachePersistence(new UapCryptographyManager());

        /// <inheritdoc />
        public ITokenCacheAccessor TokenCacheAccessor { get; } = new UapTokenCacheAccessor(new UapCryptographyManager());

        /// <inheritdoc />
        public ICryptographyManager CryptographyManager { get; } = new UapCryptographyManager();
    }
}
