// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License.

using System.Net.Http;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    /// <summary>
    /// Factory responsible for creating HttpClient(s); 
    /// .Net recommends to use a single instance of HttpClient
    /// </summary>
    /// <remarks>
    /// Implementations must be thread safe. Consider creating and configuring an HttpClient in the constructor
    /// of the factory and return the same object in <see cref="GetHttpClient"/>
    /// </remarks>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Method returning an Http client that will be used to
        /// communicate with Azure AD. This enables advanced scenarios. 
        /// See https://aka.ms/adal-net-application-configuration
        /// </summary>
        /// <returns>An Http client</returns>
        /// <remarks>Implementations must be thread safe</remarks>
        HttpClient GetHttpClient();
    }
}