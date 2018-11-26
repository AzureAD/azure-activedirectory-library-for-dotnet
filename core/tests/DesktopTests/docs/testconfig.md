# Test Configuration

The end-to-end tests in the _TestAutomation_ repository can be configured in various ways; pointing to different WebDriver endpoints,
selecting different devices, using specific automation app versions, etc.

## _config*.json_ files

The primary way of configuring the test runs is via _config*.json_ files in the _TestAutomation_ repository. These files can currently be found
under the _/AutomationCore/_ directory.

Several _config*.json_ files can be combined/overlaid together to produce a single 'effective' configuration.
This is done automatically by the [`Microsoft.Extensions.Configuration`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) package.
The order in which configuration sources are combined determines the effective configuration, with properties from sources added later overriding values set previously.

Currently there are three configuration files:

- _config.json_ : this serves as a common base configuration and is always included first
- _config.vsts.json_ : this is combined with _config.json_ when the tests detect they're running on a VSTS agent
  - see the [VSTS configuration](#vsts-configuration) section below for more information about VSTS-specific configuration.
- _config.local.json_ : this is combined with _config.json_ when **not** executing on a VSTS agent

The general stucture of a _config.json_ file is outlined below:

```json
{
    // These are examples of platform profile sections.
    // Currently there are only three possibilities which the tests can choose from: Android, iOS, and Desktop.
    "android": { /*...*/ },
    "ios": { /*...*/ },
    "desktop": { /*...*/ },

    // Azure Key Vault section
    "keyVault": { /*...*/ }
}
```

### `<PLATFORM_PROFILE_NAME>`

These sections contain a 'profile' against which tests should be run against. Currently there is one profile per platform (Android, iOS and Desktop).
The profiles contain a few properties which control the way the test framework should behave, as well as the `deviceCapabilities` section which is sent directly
to the WebDriver endpoint (`serverUrl`) as Desired Capabilities.

```json
"PLATFORM_PROFILE_NAME": {
    // The WebDriver endpoint the platform profile should target
    "serverUrl": "<WEBDRIVER_ENDPOINT>",

    // Appium & Olympus desired capabilities to send to the WD endpoint
    "deviceCapabilities": {
        "platform": "android",
        "app": "<PATH_TO_APP>",
        "olympusTeam": "ADAL_DevEX"
        // ...
    },

    // Toggles whether or not the Driver.HideKeyboard command should be ignored.
    // This is important when targeting devices/emulators without a soft-keyboard.
    "canHideKeyboard": true,

    // WebDriver endpoint communication timeout in seconds
    "commandTimeout": "120"
}
```

### `keyVault`

The `keyVault` section contains the configuration nessesary for accessing the MSIDLabs Key Vaults (which contain the Lab user account passwords).

```json
"keyVault": {
    // The URL of the Azure Key Vault
    "url": "<KEYVAULT_URL>",

    // One of either "ClientCertificate" or "UserCredentials"
    "authType": "<AUTH_TYPE>",

    // The Azure AD Application ID which has been configured to access the Key Vault
    "clientId": "<AAD_APPLICATION_ID>",

    // The thumbprint of the X509 certificate used when authType = ClientCertificate
    "certThumbprint": "<X509_THUMBPRINT>"
}
```

## Environment Variables

As well as the _config*.json_ files you can also use environment variables as a configuration source. The environment variables are folded into
the configuration _last_ and a such will override any configuration values set in the _config*.json_ files.

The environment variable name syntax for configuration properties is a concatenatation of JSON properties with the colon (`:`) character.

For example this JSON configuration..

```json
{
    "android": {
        "deviceCapabilities": {
            "app": "/tmp/my.app"
        }
    }
}
```

..is equivalent to the `android:deviceCapabilities:app` environment variable with the value `"/tmp/my.app"`.

## VSTS Configuration

When queuing a new build on VSTS, the dialog box has a section named "Variables". Clicking "Add variable" allows the configuration of specific variables without the need to check in changes to the JSON files (useful for testing).

Variables added in this manner should be of the format: `android:deviceCapabilities:deviceName`. Note the colons and the platform-specific prefix; see the section above for more details.

There is a limited whitelist of variables that can be set - see the whitelist in [`ConfigurationProvider.cs`](https://identitydivision.visualstudio.com/IDDP/_git/TestAutomation?path=%2FAutomationCore%2FConfiguration%2FConfigurationProvider.cs&version=GBmaster) for details. These are drawn from the [Appium](https://github.com/appium/appium/blob/master/docs/en/writing-running-appium/caps.md) and [Olympus](http://sharepoint/sites/bingclients/ARIAClients/_layouts/15/WopiFrame2.aspx?sourcedoc=%7Be49d0fb2-df5e-4448-a7ef-6e32c065520e%7D&action=edit&wd=target%28%2FDesired%20Capabilities%20Cheat-sheet%7C2a8a2807-e564-48ad-b0ea-e6c22b267d21%2F%29) device capability listings.

### Computed `app` path

When the tests detect* that they are being exectuted on a VSTS agent, the `deviceCapabilities:app` configuration property is computed at runtime.
This property is set after _config.json_, but before the _config.vsts.json_ and environment variable sources are added meaning that you can still override
the app path using these mechanisms, if desired.

_*Detection of whether the tests are running in on a VSTS agent is by the presence of the `BUILD_BUILDURI` environment variable._

The path varies slightly by platform, but they all follow the same basic structure:

Platform|App Path
--------|--------
Android |_\\\\olympusshare\\Olympus\\apps\\DevEx\\android\\`BUILD_NUMBER`\\apk\\automationtestapp-debug.apk_
iOS     | \* see below \*
Desktop |_\\\\olympusshare\\Olympus\\apps\\DevEx\\dotnet\\adal\\`BUILD_NUMBER`\\adalApp\\WinFormsAutomationApp.exe_

The required `BUILD_NUMBER` environment variable, along with several other [pre-defined variables](https://www.visualstudio.com/en-us/docs/build/define/variables#predefined-variables), are set by VSTS automatically.

The [build definitions](builddefinitions.md) which run the end-to-end tests will publish the test automation app to these paths before executing the tests.

#### iOS App Path

Because we must build the authentication library and automation app on a Mac, but can only build the E2E tests on Windows, we cannot build both the tests and automation app within the same build definition.

Instead the E2E build definition assumes that a pre-built automation app exists. The `ios:deviceCapabilities:app` variable is computed within the build definition (and not in code). For more information see [here](builddefinitions.md).

