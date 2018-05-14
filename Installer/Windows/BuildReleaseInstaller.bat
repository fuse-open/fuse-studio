@echo off

call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\vcvarsall.bat"
msbuild Installer.msbuild /target:PrepareAndBuildInstaller /p:ReleaseConfiguration=Release || goto ERROR

REM msbuild Installer.msbuild /target:BuildInstaller /p:ReleaseConfiguration=Release

:SUCCESS
exit /b 0

:ERROR
echo Build failed
exit /b 1
