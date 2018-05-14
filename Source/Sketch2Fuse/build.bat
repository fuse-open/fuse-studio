@echo off
pushd "%~dp0"

if "%1" == "--release" (
	set config="Release"
) else (
	set config="Debug"
)

REM If we already have a VS environment set up, just use that
if defined VSINSTALLDIR (
	goto :BUILD
)

set NUGETEXE="..\..\.nuget\NuGet.exe"

REM First try to locate VS 2017. Since a user can have multiple installations
REM of VS 2017 at the same time a utility named vswhere to find a useful candidate.
if not exist "packages\vswhere\tools\vswhere.exe" (
	%NUGETEXE% install vswhere -ExcludeVersion -OutputDirectory packages
 )

for /f "usebackq tokens=1* delims=: " %%i in (`packages\vswhere\tools\vswhere -products * -latest -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" (
	set vsdir=%%j
  )
)

if exist "%vsdir%\Common7\Tools\VsMSBuildCmd.bat" (
	call "%vsdir%\Common7\Tools\VsMSBuildCmd.bat"
	rem Running VsMSBuildCmd unfortunately changes current dir to %HOMEPATH%
	rem We have already pushd so lets just cd back
	cd "%~dp0"
	goto :BUILD
)

REM Fall back to VS 2015 Environment setup
if not defined VSINSTALLDIR (
	if defined VS140COMNTOOLS (
		call "%VS140COMNTOOLS%\vsvars32.bat"
		goto :BUILD
	)
)

:BUILD
REM Nuget restore
echo Nuget restore
%NUGETEXE% restore

REM Build Sketch2Fuse
echo Build %config%
msbuild /m /p:Configuration=%config% Sketch2Fuse.sln || goto ERROR
pause

:SUCCESS
popd && exit /b 0

:ERROR
pause
popd && exit /b 1
