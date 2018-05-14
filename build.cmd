echo off
echo options: debug (default), release
echo args: %1%

set bconfig=debug
if '%1' NEQ '' (set bconfig=%1%)
echo config: %bconfig%

Rem TODO: Find all .csproj files and build those instead of .sln files.
Rem TODO: set build $(Platform)

goto :buildCore
:doneBuildingCore
goto :buildMsal
:doneBuildingMsal
goto :buildAdal
:doneBuildingAdal


Rem goto :buildAdalTestApps
:doneBuildingAdalTestapps
Rem goto :buildMsalTestApps
:doneBuildingMsalTestApps


goto :eof


:buildCore

pushd core\src
Rem set csprojFile=Microsoft.Identity.Core.VS2015.csproj
Rem msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
Rem msbuild %csprojFile% /t:build /p:configuration=%bconfig%

set csprojFile=Microsoft.Identity.Core.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd core\tests\Test.Microsoft.Identity.Core.Unit
set csprojFile=Test.Microsoft.Identity.Core.Unit.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

goto :doneBuildingCore

:buildAdal

pushd adal\src\Microsoft.IdentityModel.Clients.ActiveDirectory.Platform
set csprojFile=ADAL.WinRT.VS2015.Platform.csproj
Rem msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
Rem msbuild %csprojFile% /t:build /p:configuration=%bconfig%

set csprojFile=Microsoft.IdentityModel.Clients.ActiveDirectory.Platform.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\src\Microsoft.IdentityModel.Clients.ActiveDirectory
set csprojFile=ADAL.WinRT.VS2015.csproj
Rem msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
Rem msbuild %csprojFile% /t:build /p:configuration=%bconfig%

set csprojFile=Microsoft.IdentityModel.Clients.ActiveDirectory.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\tests\Test.ADAL.NET.Common
set csprojFile=Test.ADAL.NET.Common.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\tests\Test.ADAL.NET.Integration
set csprojFile=Test.ADAL.NET.Integration.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\tests\Test.ADAL.NET.Unit
set csprojFile=Test.ADAL.NET.Unit.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

goto doneBuildingAdal



:buildMsal

pushd msal\src\Microsoft.Identity.Client
set csprojFile=Microsoft.Identity.Client.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\Test.MSAL.NET.Unit
set csprojFile=Test.MSAL.NET.Unit.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

goto :doneBuildingMsal


:buildAdalTestApps
pushd adal\automation\WinFormsAutomationApp
set csprojFile=WinFormsAutomationApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\devApps\AdalAndroidTestApp
set csprojFile=AdalAndroidTestApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\devApps\AdalAndroidTestApp
set csprojFile=AdalAndroidTestApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\devApps\AdalCoreCLRTestApp
set csprojFile=AdalCoreCLRTestApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\devApps\AdalDesktopTestApp
set csprojFile=AdalDesktopTestApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\devApps\AdaliOSTestApp
set csprojFile=AdaliOSTestApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\devApps\AdalUniversalTestApp\AdalUniversalTestApp.Windows
set csprojFile=AdalUniversalTestApp.Windows.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\devApps\XFormsApp
set csprojFile=XFormsApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd adal\devApps\XFormsApp.iOS
set csprojFile=XFormsApp.iOS.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

goto doneBuildingAdalTestapps


:buildMsalTestApps

pushd msal\samples\desktop\SampleApp
set csprojFile=SampleApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\AutomationApp
set csprojFile=AutomationApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\dev apps\DesktopTestApp
set csprojFile=DesktopTestApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\dev apps\NetCoreTestApp
set csprojFile=NetCoreTestApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\dev apps\WebTestApp\WebApi
set csprojFile=WebApi.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\dev apps\WebTestApp\WebApi
set csprojFile=WebApi.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\dev apps\WebTestApp\WebApp
set csprojFile=WebApp.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\dev apps\XForms\XForms
set csprojFile=XForms.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\dev apps\XForms\XForms.Android
set csprojFile=Android.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

pushd msal\tests\dev apps\XForms\XForms.UWP
set csprojFile=XForms.UWP.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd

goto :doneBuildingMsalTestApps

:buildmsaliOSApp
pushd msal\tests\dev apps\XForms\XForms.iOS
set csprojFile=XForms.iOS.csproj
msbuild %csprojFile% /t:restore /p:configuration=%bconfig%
msbuild %csprojFile% /t:build /p:configuration=%bconfig%
popd
goto :doneBuildingMsaliOSApp

rem msbuild Combined.NoWinRT.sln /t:build /p:configuration=%bconfig%

