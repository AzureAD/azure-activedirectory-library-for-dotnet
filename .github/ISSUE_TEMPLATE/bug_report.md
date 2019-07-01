---
name: Bug report
about: Create a report to help us improve
---

**MSAL is the recommended auth library for use with the Microsoft identity platform**

No new features will be implemented on ADAL. The team's efforts are on improving [MSAL](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet), the next-gen auth library. MSAL's wiki contains a [migration guide](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Adal-to-Msal) from ADAL.

Only regressions, high severity issues and security issues will be fixed on ADAL. Other issues are likely to have already been fixed in MSAL. 

If you think that your issue falls into the above categories, please fill in the form below.

**Which Version of ADAL are you using ?**
Note that to get help, you need to run the latest preview or non-preview version
For MSAL, please log issues to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet
<!-- E.g. ADAL 3.19.8, ADAL 4.7.0-preview -->

**Which platform has the issue?**
<!-- Ex: net45, netcore, UWP, xamarin android, xamarin iOS -->

**What authentication flow has the issue?**
* Desktop / Mobile
    * [ ] Interactive
    * [ ] Integrated Windows Auth
    * [ ] Username Password
    * [ ] Device code flow (browserless)
* Web App
    * [ ] Authorization code
    * [ ] OBO
* Web API
    * [ ] OBO

Other? - please describe;

**Is this a new or existing app?**
<!-- Ex:
a. The app is in production, and I have upgraded to a new version of ADAL
b. The app is in production, uses the same version of ADAL, but started seeing this issue
c. This is a new app or experiment
-->

**Repro**

```csharp
var your = (code) => here;
```

**Expected behavior**
A clear and concise description of what you expected to happen (or code).

**Actual behavior**
A clear and concise description of what happens, e.g. exception is thrown, UI freezes  

**Possible Solution**
<!--- Only if you have suggestions on a fix for the bug -->

**Additional context/ Logs / Screenshots**
Add any other context about the problem here, such as logs and screebshots. Logging is described at https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Logging-in-ADAL.Net. Don't post logs containing PII on GitHub!
