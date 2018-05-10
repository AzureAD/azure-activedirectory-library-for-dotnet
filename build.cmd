echo off
echo options: debug (default), release
echo args: %1%

set bconfig=debug
if '%1' NEQ '' (set bconfig=%1%)
echo config: %bconfig%

msbuild Combined.NoWinRT.sln /t:build /p:configuration=%bconfig%
