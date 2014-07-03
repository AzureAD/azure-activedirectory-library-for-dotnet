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

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal static partial class ActiveDirectoryAuthenticationError
    {
        public const string UnauthorizedUserInformationAccess = "unauthorized_user_information_access";
        public const string CannotAccessUserInformation = "user_information_access_failed";
    }

    internal static partial class ActiveDirectoryAuthenticationErrorMessage
    {
        public const string CannotAccessUserInformation = "Cannot access user information. Check machine's Privacy settings or initialize UserCredential with userId";
        public const string RedirectUriAppIdMismatch = "The return URI provided does not match the app's ID";
        public const string RedirectUriUnsupportedWithPromptBehaviorNever = "PromptBehavior.Never is supported in SSO mode only (i.e. no redirectUri)";
        public const string UnauthorizedUserInformationAccess = "Unauthorized accessing user information. Check application's 'Enterprise Authentication' capability";
    }
}
