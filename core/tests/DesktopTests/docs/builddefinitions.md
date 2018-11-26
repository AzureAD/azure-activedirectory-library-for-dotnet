# Build Definitions

Several build definitions have been set up in the IDDP project. Here is a brief outline of the definitions that exist today, what triggers
have been configured for them and what they do at a high-level.

## Continuous Integration `/CI`

### [ADAL Objective-C](https://identitydivision.visualstudio.com/IDDP/_build/index?definitionId=64)

This build is triggered for each commit into the `dev` branch of of [_azure-activedirectory-library-for-objc_](https://github.com/AzureAD/azure-activedirectory-library-for-android) repository.

The resulting automation app is published the Olympus file share (\\\\olympusshare\\Olympus\\apps\\DevEx\\ios\\`BRANCH_NAME`) and updates a special file named _latest_ to the latest **successful** build number.

## End-to-end Automation Testing `/E2E Tests`

### [ADAL Android E2E (Olympus)](https://identitydivision.visualstudio.com/IDDP/_build/index?definitionId=13)

This build is triggered for each commit into the `master` branch of [_azure-activedirectory-library-for-android_](https://github.com/AzureAD/azure-activedirectory-library-for-android) repository.

`azure-activedirectory-library-for-android/master` is built. All Android automation tests are performed using Olympus. No code signing is performed.

### [ADAL iOS E2E (Olympus)](https://identitydivision.visualstudio.com/IDDP/_build/index?definitionId=61)

This build has **no** external triggers; it must be run manually.

All iOS automation tests are performed using Olympus using the automation test app specified at queue time. The path is computed by setting the `AutomationApp.Branch` and `AutomationApp.BuildNumber` build variables.
Setting `AutomationApp.BuildNumber` to "latest" will select the latest successful build for the given branch.

The path is of the format: _\\\\olympusshare\\Olympus\\apps\\DevEx\\ios\\`AutomationApp.Branch`\\`AutomationApp.BuildNumber`\\Release-iphoneos\\ADALAutomation.app_

### [TestAutomation CI](https://identitydivision.visualstudio.com/IDDP/_build/index?definitionId=35)

This build is triggered for each commit into the `master` branch of the [_TestAutomation_](https://identitydivision.visualstudio.com/IDDP/_git/TestAutomation) repository.

`TestAutomation/master` is built. One automation test per platform is performed using Olympus. No code signing is performed.

### [TestAutomation PR](https://identitydivision.visualstudio.com/IDDP/_build/index?definitionId=36)

This build is triggered whenever a pull request is created against the [_TestAutomation_](https://identitydivision.visualstudio.com/IDDP/_git/TestAutomation) repository.

The subject branch of the pull request is built. No tests are performed. No code signing is performed. A successful build is required to complete the pull request.

## Release Builds `/Release Builds`

–

## User Builds `/users`

This build definition directory is open for users to create their own definitions. Please create your build definitions under `users/YOUR_ALIAS/`.
