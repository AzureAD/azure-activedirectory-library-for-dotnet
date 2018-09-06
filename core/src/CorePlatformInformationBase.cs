//----------------------------------------------------------------------
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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core
{
    internal abstract class CorePlatformInformationBase
    {
        public static CorePlatformInformationBase Instance { get; set; }

        public const string DefaultRedirectUri = "urn:ietf:wg:oauth:2.0:oob";
        public abstract string GetProductName();
        public abstract string GetEnvironmentVariable(string variable);
        public abstract string GetProcessorArchitecture();
        public abstract string GetOperatingSystem();
        public abstract string GetDeviceModel();

        public abstract string GetAssemblyFileVersionAttribute();

        public abstract Task<bool> IsUserLocalAsync(RequestContext requestContext);

        private static readonly Regex ClientVersionRegex = new Regex(@"Version=[\d]+.[\d+]+.[\d]+.[\d]+", RegexOptions.CultureInvariant);

        public virtual bool IsDomainJoined()
        {
            return false;
        }

        public virtual void ValidateRedirectUri(Uri redirectUri, RequestContext requestContext)
        {
            if (redirectUri == null)
            {
                throw new ArgumentNullException(nameof(redirectUri));
            }
        }

        public virtual string GetRedirectUriAsString(Uri redirectUri, RequestContext requestContext)
        {
            return redirectUri.OriginalString;
        }

        public virtual string GetDefaultRedirectUri(string correlationId)
        {
            return DefaultRedirectUri;
        }

        public static string GetClientVersion()
        {
            string fullVersion = typeof(UriParamsHelper).GetTypeInfo().Assembly.FullName;
            Match match = ClientVersionRegex.Match(fullVersion);
            if (match.Success)
            {
                string[] version = match.Groups[0].Value.Split(new[] { '=' }, StringSplitOptions.None);
                return version[1];
            }

            return null;
        }

        public static string GetAssemblyFileVersion()
        {
            return CorePlatformInformationBase.Instance.GetAssemblyFileVersionAttribute();
        }

        public static string GetAssemblyInformationalVersion()
        {
            AssemblyInformationalVersionAttribute attribute =
                typeof(UriParamsHelper).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return (attribute != null) ? attribute.InformationalVersion : string.Empty;
        }
    }
}