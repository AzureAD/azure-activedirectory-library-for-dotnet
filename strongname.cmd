@echo off
REM ******************************************
REM ** 
REM ** Locate folders and disable strongname 
REM ** 
REM ******************************************

echo options: debug (default), release
echo args: %1%

set bconfig=debug
if '%1' NEQ '' (set bconfig=%1%)

set gotoFolder=adal\src\Microsoft.IdentityModel.Clients.ActiveDirectory\bin\%bconfig%\net45
echo config: %gotoFolder%
pushd %gotoFolder%

sn.exe -Vr Microsoft.IdentityModel.Clients.ActiveDirectory.dll
sn.exe -Vr Microsoft.Identity.Core.dll

popd