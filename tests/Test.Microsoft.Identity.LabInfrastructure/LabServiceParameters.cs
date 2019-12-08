// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Microsoft.Identity.LabInfrastructure
{
    public enum MFA
    {
        None,
        MfaOnAll,
        AutoMfaOnAll
    }

    public enum ProtectionPolicy
    {
        None,
        CA,
        CADJ,
        MAM,
        MDM,
        MDMCA,
        MAMCA,
        MAMSPO
    }

    public enum HomeDomain //Must add ".com" to end for lab queury
    {
        None,
        MsidLab2,
        MsidLab3,
        MsidLab4
    }

    public enum HomeUPN //Must replace "_" with "@" add ".com" to end for lab queury
    {
        None,
        GidLab_Msidlab2,
        GidLab_Msidlab3,
        GidLab_Msidlab4,
    }

    public enum AzureEnvironment
    {
        azurecloud,
        azureb2ccloud,
        azurechinacloud,
        azuregermanycloud,
        azureppe,
        azureusgovernment
    }

    public enum SignInAudience
    {
        AzureAdMyOrg,
        AzureAdMultipleOrgs,
        AzureAdAndPersonalMicrosoftAccount
    }
}
