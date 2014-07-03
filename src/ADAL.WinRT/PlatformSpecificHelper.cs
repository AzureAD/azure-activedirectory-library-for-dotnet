﻿//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal static class PlatformSpecificHelper
    {
        public static string GetProductName()
        {
            return "WinRT";
        }

        public static string GetEnvironmentVariable(string variable)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            return localSettings.Values.ContainsKey(variable) ? localSettings.Values[variable].ToString() : null;
        }

        public static AuthenticationResult ProcessServiceError(string error, string errorDescription)
        {
            return new AuthenticationResult(AuthenticationStatus.ServiceError, error, errorDescription);
        }

        public static string PlatformSpecificToLower(this string input)
        {
            // WinRT does not have the overload with CultureInfo parameter
            return input.ToLower();
        }

        public async static Task<bool> IsUserLocal()
        {
            if (!Windows.System.UserProfile.UserInformation.NameAccessAllowed)
            {
                throw new AdalException(AdalError.CannotAccessUserInformation);
            }

            try
            {
                return string.IsNullOrEmpty(await Windows.System.UserProfile.UserInformation.GetDomainNameAsync());
            }
            catch (UnauthorizedAccessException)
            {
                // This mostly means Enterprise capability is missing, so WIA cannot be used and
                // we return true to add form auth parameter in the caller.
                return true;
            }
        }

        public async static Task<string> GetUserPrincipalNameAsync()
        {
            if (!Windows.System.UserProfile.UserInformation.NameAccessAllowed)
            {
                throw new AdalException(AdalError.CannotAccessUserInformation);
            }

            try
            {
                return await Windows.System.UserProfile.UserInformation.GetPrincipalNameAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new AdalException(AdalError.UnauthorizedUserInformationAccess, ex);
            }
        }

        internal static string CreateSha256Hash(string input)
        {
            IBuffer inputBuffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);

            var hasher = HashAlgorithmProvider.OpenAlgorithm("SHA256");
            IBuffer hashed = hasher.HashData(inputBuffer);

            return CryptographicBuffer.EncodeToBase64String(hashed);
        }

        public static bool IsDomainJoined()
        {
            IReadOnlyList<HostName> hostNamesList = Windows.Networking.Connectivity.NetworkInformation
                .GetHostNames();

            foreach (var entry in hostNamesList)
            {
                if (entry.Type == Windows.Networking.HostNameType.DomainName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
