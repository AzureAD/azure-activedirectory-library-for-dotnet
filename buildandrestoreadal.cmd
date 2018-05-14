echo off
echo options: debug (default), release
echo args: %1%

set bconfig=debug
if '%1' NEQ '' (set bconfig=%1%)
echo config: %bconfig%

pushd adal\src\Microsoft.IdentityModel.Clients.ActiveDirectory.Platform
msbuild Microsoft.IdentityModel.Clients.ActiveDirectory.Platform.csproj /t:restore /p:configuration=%bconfig%
msbuild Microsoft.IdentityModel.Clients.ActiveDirectory.Platform.csproj /t:build /p:configuration=%bconfig%
popd
