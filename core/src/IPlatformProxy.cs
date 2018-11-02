﻿using System;
using System.Threading.Tasks;
using Microsoft.Identity.Core.Cache;

namespace Microsoft.Identity.Core
{
    /// <summary>
    /// Common operations for extracting platform / operating system specifics
    /// </summary>
    internal interface IPlatformProxy
    {
        /// <summary>
        /// Gets the device model. On some TFMs this is not returned for security reasonons.
        /// </summary>
        /// <returns>device model or null</returns>
        string GetDeviceModel();

        string GetEnvironmentVariable(string variable);

        string GetOperatingSystem();

        string GetProcessorArchitecture();

        /// <summary>
        /// Gets the upn of the user currently logged into the OS
        /// </summary>
        /// <returns></returns>
        Task<string> GetUserPrincipalNameAsync();

        /// <summary>
        /// Returns true if the current OS logged in user is AD or AAD joined.
        /// </summary>
        /// <returns></returns>
        bool IsDomainJoined();

        Task<bool> IsUserLocalAsync(RequestContext requestContext);

        /// <summary>
        /// Returns the name of the calling assembly
        /// </summary>
        /// <returns></returns>
        string GetCallingApplicationName();

        /// <summary>
        /// Returns the version of the calling assembly
        /// </summary>
        /// <returns></returns>
        string GetCallingApplicationVersion();

        /// <summary>
        /// Returns a device identifier. Varies by platform.
        /// </summary>
        /// <returns></returns>
        string GetDeviceId();

        void ValidateRedirectUri(Uri redirectUri, RequestContext requestContext);
        string GetRedirectUriAsString(Uri redirectUri, RequestContext requestContext);
        string GetDefaultRedirectUri(string correlationId);

        string GetProductName();

        ILegacyCachePersistence CreateLegacyCachePersistence();

        ITokenCacheAccessor CreateTokenCacheAccessor();

        ICryptographyManager CryptographyManager { get; }
    }
}
