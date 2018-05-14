@echo off

call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\vcvarsall.bat"

signtool sign /d "Fuse Setup" /t http://timestamp.digicert.com /f "C:\Users\Emil\Documents\cert\MySPC.pfx" "BuildOutput\Release\FuseSetup.exe" || goto ERROR
signtool verify /pa "BuildOutput\Release\FuseSetup.exe" || goto ERROR

:SUCCESS
exit /b 0

:ERROR
echo Sign failed
exit /b 1
