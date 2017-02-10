//----------------------------------------------------------------------
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

namespace Test.ADAL.Common
{
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

    public class TestConstants
    {
        public static readonly string DefaultResource = "resource1";
        public static readonly string AnotherResource = "resource2";
        public static readonly string DefaultAdfsAuthorityTenant = "https://login.contoso.com/adfs/";
        public static readonly string DefaultAuthorityHomeTenant = "https://login.microsoftonline.com/home/";
        public static readonly string DefaultAuthorityGuestTenant = "https://login.microsoftonline.com/guest/";
        public static readonly string DefaultAuthorityCommonTenant = "https://login.microsoftonline.com/common/";
        public static readonly string DefaultClientId = "client_id";
        public static readonly string DefaultUniqueId = "unique_id";
        public static readonly string DefaultDisplayableId = "displayable@id.com";
        public static readonly Uri DefaultRedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");
        public static readonly bool DefaultRestrictToSingleUser = false;
        public static readonly string DefaultClientSecret = "client_secret";
        public static readonly string DefaultPassword = "password";
        public static readonly bool DefaultExtendedLifeTimeEnabled = false;
        public static readonly bool PositiveExtendedLifeTimeEnabled = true;
        public static readonly string ErrorSubCode = "ErrorSubCode";
        public static readonly string CloudAudienceUrnMicrosoft = "urn:federation:MicrosoftOnline";
        public static readonly string CloudAudienceUrn = "urn:federation:Blackforest";
    }
}