﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Identity.Test.LabInfrastructure
{
    public class LabApiConstants
    {
        public const string MobileDeviceManagementWithConditionalAccess = "mdmca";
        public const string MobileAppManagementWithConditionalAccess = "mamca";
        public const string MobileAppManagement = "mam";
        public const string MultiFactorAuthentication = "mfa";
        public const string License = "license";
        public const string FederationProvider = "federationProvider";
        public const string FederatedUser = "isFederated";
        public const string UserType = "usertype";
        public const string External = "external";
        public const string B2CProvider = "b2cProvider";
        public const string B2CLocal = "local";
        public const string B2CFacebook = "facebook";
        public const string B2CGoogle = "google";
        public const string UserContains = "usercontains";
        public const string AppName = "AppName";

        public const string True = "true";
        public const string False = "false";
        public const string None = "none"; // for mfa false
        public const string MfaOnAll = "mfaonall"; // for mfa true

        public const string BetaEndpoint = "http://api.msidlab.com/api/userbeta";

        public const string LabUserCredentialEndpoint = "https://msidlab.com/api/LabUserSecret";
        public const string LabAppEndpoint = "https://msidlab.com/api/app/";
        public const string LabInfoEndpoint = "https://msidlab.com/api/Lab/";
        public const string LabEndPoint = "https://user.msidlab.com/api/user";
        public const string CreateLabUser = "https://request.msidlab.com/api/CreateLabUser";
    }
}