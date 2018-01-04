set BUILD_PATH=bin\%BUILD_BUILDCONFIGURATION%
set SIGNED_TARGET=%BUILD_ARTIFACTSTAGINGDIRECTORY%\SignedProductBinaries
set NUGET_TARGET=%BUILD_ARTIFACTSTAGINGDIRECTORY%
set NUGET_SYMBOLS_PLACEHOLDER=%BUILD_ARTIFACTSTAGINGDIRECTORY%\symbols
set NUGET_TEMP_TARGET=%BUILD_SOURCESDIRECTORY%\NuGet-Temp
set LIBRARY_NAME=Microsoft.Identity.Client
setlocal ENABLEDELAYEDEXPANSION

md %NUGET_TEMP_TARGET%
md %NUGET_SYMBOLS_PLACEHOLDER%

@echo Move symbols package to placeholder
move /Y %NUGET_TARGET%\*.symbols.nupkg %NUGET_SYMBOLS_PLACEHOLDER%

@echo ==========================
@echo Decompressing nuget...
for %%i in (%NUGET_TARGET%\*.nupkg) do (
	set NUGET_NAME=%%~ni
	7z e %NUGET_TARGET%\%%~ni.nupkg -o%NUGET_TEMP_TARGET%\ -y
)

@echo Saved nuget name as %NUGET_NAME%
@echo ==========================
@echo Copying signed files to %NUGET_TEMP_TARGET%\lib ...
ROBOCOPY %SIGNED_TARGET%\ %NUGET_TEMP_TARGET%\lib\ *.dll /E


del %NUGET_TARGET%\*.CodeAnalysisLog.xml /s
del %NUGET_TARGET%\*.lastcodeanalysissucceeded /s


@echo ==========================
@echo Creating NuGet Package....
7z a %NUGET_TARGET%\%NUGET_NAME%.zip -r %NUGET_TEMP_TARGET%\*
copy /Y %NUGET_TARGET%\%NUGET_NAME%.zip %NUGET_TARGET%\%NUGET_NAME%.nupkg

@echo Move symbols package back to original location
move /Y %NUGET_SYMBOLS_PLACEHOLDER%\*.symbols.nupkg %NUGET_TARGET%\

@echo ====================================
@echo MSAL-NET nuget creation script complete with no errors
@echo ====================================
exit 0

:ERROREXIT
@echo **********************************************
@echo MSAL-NET Build failed to create nuget properly
@echo MSAL-NET Postbuild Script Complete WITH ERRORS
@echo **********************************************
exit 1