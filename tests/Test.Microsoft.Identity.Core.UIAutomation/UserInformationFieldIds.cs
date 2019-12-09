// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Test.LabInfrastructure;

namespace Test.Microsoft.Identity.Core.UIAutomation
{
    public class UserInformationFieldIds
    {
        public string PasswordInputId { get; set; }
        public string SignInButtonId { get; set; }

        public void DetermineFieldIds(LabUser user)
        {
            if (user.UserType == UserType.Federated)
            {
                switch (user.FederationProvider)
                {
                    case FederationProvider.AdfsV3:
                    case FederationProvider.AdfsV4:
                        // We use the same IDs for ADFSv3 and ADFSv4
                        PasswordInputId = CoreUiTestConstants.AdfsV4WebPasswordID;
                        SignInButtonId = CoreUiTestConstants.AdfsV4WebSubmitID;
                        break;
                    case FederationProvider.AdfsV2:
                        PasswordInputId = CoreUiTestConstants.AdfsV2WebPasswordInputId;
                        SignInButtonId = CoreUiTestConstants.AdfsV2WebSubmitButtonId;
                        break;
                    default:
                        break;
                }

                return;
            }

            PasswordInputId = CoreUiTestConstants.WebPasswordID;
            SignInButtonId = CoreUiTestConstants.WebSubmitID;
        }
    }
}
