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

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// The builder class to construct a MsalConfiguration object which can be used to initialize a PublicClientApplication or ConfidentialClientApplication.
    /// </summary>
    public sealed class MsalConfigurationBuilder
    {
        internal MsalConfigurationBuilder(MsalConfiguration msalConfiguration)
        {
            Config = msalConfiguration;
        }

        internal MsalConfiguration Config { get; }

        /// <summary>
        /// Read the configuration from a JSON stream according to the schema.
        /// </summary>
        /// <param name="json">The JSON to parse in string format.</param>
        /// <returns>An MsalConfigurationBuilder object which can be further modified.</returns>
        public static MsalConfigurationBuilder CreateFromJson(string json)
        {
            var config = new MsalConfiguration();
            return new MsalConfigurationBuilder(config);
        }

        /// <summary>
        /// Creates a new MsalConfigurationBuilder with the base required parameters of clientId and redirectUri.
        /// Other values will be set to defaults and can be overridden by other methods of the MsalConfigurationBuilder.
        /// </summary>
        /// <param name="clientId">The ClientId of the application to reference.</param>
        /// <param name="redirectUri">The RedirectUri of the application to reference.</param>
        /// <returns></returns>
        public static MsalConfigurationBuilder Create(string clientId, string redirectUri)
        {
            var config = new MsalConfiguration(clientId, redirectUri);
            return new MsalConfigurationBuilder(config);
        }
    }
}