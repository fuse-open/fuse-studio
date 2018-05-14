@echo off

sh %~dp0run-tests.sh %* || goto ERROR
exit /b 0

:ERROR
echo Please use Cmder/MSYS to run this script (http://gooseberrycreative.com/cmder/)
pause