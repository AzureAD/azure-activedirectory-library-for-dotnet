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
using System.Collections.Generic;

namespace Test.ADAL.NET.Common
{
	public static class AdalTestConstants
    {
        public const string DefaultResource = "resource1";
        public const string AnotherResource = "resource2";
        public const string DefaultAdfsAuthorityTenant = "https://login.contoso.com/adfs/";
        public const string DefaultAuthorityHomeTenant = "https://login.microsoftonline.com/home/";
        public const string SomeTenantId = "some-tenant-id";
        public const string TenantSpecificAuthority = "https://login.microsoftonline.com/" + SomeTenantId + "/";
        public const string DefaultAuthorityGuestTenant = "https://login.microsoftonline.com/guest/";
        public const string DefaultAuthorityCommonTenant = "https://login.microsoftonline.com/common/";
        public const string DefaultAuthorityBlackforestTenant = "https://login.microsoftonline.de/common/";
        public const string DefaultClientId = "client_id";
        public const string DefaultUniqueId = "unique_id";
        public const string DefaultThumbprint = "some_thumbprint";
        public const string DefaultDisplayableId = "displayable@id.com";
        public static readonly Uri DefaultRedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");
        public const bool DefaultRestrictToSingleUser = false;
        public const string DefaultClientSecret = "client_secret";
        public const string DefaultPassword = "password";
        public const bool DefaultExtendedLifeTimeEnabled = false;
        public const bool PositiveExtendedLifeTimeEnabled = true;
        public const string ErrorSubCode = "ErrorSubCode";
        public const string CloudAudienceUrnMicrosoft = "urn:federation:MicrosoftOnline";
        public const string CloudAudienceUrn = "urn:federation:Blackforest";
        public const string TokenEndPoint = "oauth2/token";
        public const string UserRealmEndPoint = "userrealm";
        public const string DiscoveryEndPoint = "discovery/instance";
        public const string DefaultRefreshTokenValue = "RefreshTokenValue";
        public const string DefaultAuthorizationCode = "DefaultAuthorizationCode";
        public const string MSGraph = "https://graph.microsoft.com";

        public const string DefaultUniqueIdentifier = "testUniqueIdentifier";
        public const string DefaultUniqueTenantIdentifier = "testUniqueTenantIdentifier";

        public static string GetTokenEndpoint(string Authority)
        {
            return Authority + TokenEndPoint;
        }

        public static string GetUserRealmEndpoint(string Authority)
        {
            return Authority + UserRealmEndPoint;
        }

        public static string GetDiscoveryEndpoint(string Authority)
        {
            return Authority + DiscoveryEndPoint;
        }
    }

    public static class StringValue
    {
        public const string NotProvided = "NotProvided";
        public const string NotReady = "Not Ready";
        public const string Null = "NULL";
    }

    public static class UserType
    {
        public const string NonFederated = "NonFederated";
        public const string Federated = "Federated";
    }

    public static class CacheType
    {
        public const string Default = "Default";
        public const string Null = "Null";
        public const string Constant = "Constant";
        public const string InMemory = "InMemory";
    }

    public enum ValidateAuthorityIndex
    {
        NotProvided = 0,
        Yes = 1,
        No = 2
    }

    public enum CacheTypeIndex
    {
        NotProvided = 0,
        Default = 1,
        Null = 2,
        InMemory = 3
    }
}
