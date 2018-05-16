Rem -- Todo: Move to Powershell
echo off
echo Usage:
echo param1 (configuration options): debug(d), release(r) .. default is debug
echo param2 (target options): build(b), restore (r), clean(c) .. default is build 
echo param3 (include sample apps): blank=components and utests, not blank=everything
echo Calling args: %1%, %2%, %3%

set bconfig=debug
if '%1' NEQ '' (set bconfig=%1%)
if '%1' EQU 'd' (set bconfig=debug)
if '%1' EQU 'r' (set bconfig=release)
set btarget=build
if '%2' NEQ '' (set btarget=%2%)
if '%2' EQU 'b' (set btarget=build)
if '%2' EQU 'r' (set btarget=retore)
if '%2' EQU 'c' (set btarget=clean)
set bsampleapps=
if '%3' NEQ '' (set bsampleapps='1')
echo Building using: target: %btarget%, configuration: %bconfig%, %bsampleapps%

if '%3' NEQ '' (
  msbuild Combined.NoWinRT.sln /t:build /p:configuration=%bconfig% 
  )
else (
  msbuild CoreAndUTests.sln /t:build /p:configuration=%bconfig%
  )


