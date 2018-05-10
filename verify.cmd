@echo off

echo .
echo Android SDK's required: 25, 26 and 27
pushd %ProgramFiles(x86)%\Android\android-sdk\platforms
echo . 
echo Available SDK's:
echo %ProgramFiles(x86)%\Android\android-sdk\platforms
echo .
echo Open SDK Manager:
echo  %ProgramFiles(x86)%\Android\android-sdk\SdkManager.exe
echo .
echo Validating available SDK's ...
if not exist android-25 echo Please install Android SDK v25 (Open SDK Manager from above location)
if not exist android-26 echo Please install Android SDK v26 (Open SDK Manager from above location)
if not exist android-27 echo Please install Android SDK v27 (Open SDK Manager from above location)
echo ... validation done.
popd

Rem JDK
Rem e.g.: C:\Program Files\Java\jdk1.8.0_152
REM HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft
Rem HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\JavaSoft\Java Development Kit\1.8.0_152

Rem MSBuild
Rem C:\Program Files (x86)\MSBuild\14.0\bin\amd64\

Rem validate that VS is installed:
Rem HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\DevDiv\vs\Servicing\14.0
Rem HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\DevDiv\vs\Servicing\15.0


Rem validate VS components:
Rem HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0_Config\InstalledProducts\GitHubPackage
Rem HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0_Config\InstalledProducts\NetCoreToolsPackage

Rem HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0_Config\InstalledProducts\iOSPackage
Rem HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0_Config\InstalledProducts\AndroidPackage
Rem HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0_Config\InstalledProducts\XamarinAndroidPackage
Rem HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0_Config\InstalledProducts\XamarinIOSPackage
Rem HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0_Config\InstalledProducts\XamarinShellPackage


Rem Validate that Xamarin is installed
Rem HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Xamarin


Rem Validate that the right versions of .net target packages are installed

