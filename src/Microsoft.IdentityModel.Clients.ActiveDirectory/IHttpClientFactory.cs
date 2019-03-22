// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License.

using System.Net.Http;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    /// <summary>
    /// Http Client Factory
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Method returning an Http client that will be used to
        /// communicate with Azure AD. This enables advanced scenarios. 
        /// See https://aka.ms/adal-net-application-configuration
        /// </summary>
        /// <returns>An Http client</returns>
        HttpClient GetHttpClient();
    }
}