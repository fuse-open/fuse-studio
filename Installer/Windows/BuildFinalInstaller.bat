@echo off

call BuildInstaller.bat --release

:SUCCESS
exit /b 0

:ERROR
echo Build failed
exit /b 1
