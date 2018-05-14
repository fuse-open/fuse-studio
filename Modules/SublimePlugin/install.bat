@ECHO off

SETLOCAL EnableDelayedExpansion

SET "sublime_package_dir=%appdata%\Sublime Text 3\Installed Packages"
SET "fuse_package_file=%sublime_package_dir%\Fuse.sublime-package"
SET "fuse_version_file=%sublime_package_dir%\FuseVersion"

:check_pack_install
IF EXIST "%fuse_version_file%" ( CALL :loadetag )
IF "%1"=="-s" ( GOTO checkForUpdateOrInstall )
IF "%1"=="--status" ( GOTO checkForUpdateOrInstall ) ELSE ( GOTO install )

:setdefaultetag
ECHO "None" > "%fuse_version_file%"
GOTO:eof

:loadetag
SET /p fuse_etag=<"%fuse_version_file%"
SET fuse_etag=%fuse_etag: =%
GOTO:eof

:install
IF NOT EXIST "%sublime_package_dir%" (
	md "%sublime_package_dir%"
	IF %ERRORLEVEL% NEQ 0 (
		ECHO Couldn't create "%sublime_package_dir%"!
		EXIT /B %ERRORLEVEL%
		GOTO:eof
	)
)

ECHO Installing sublime plugin...
for /f "delims=" %%A in ('call .\tools\curl.exe -I -s -L --header "If-None-Match: %fuse_etag%" "https://go.fusetools.com/latest-sublime-plugin-2"') do call :checkHeader %%A %%B
IF %ERRORLEVEL% NEQ 0 (
	ECHO Download failed!
	GOTO error
	)
call .\tools\curl.exe -s -L -o ^"%fuse_package_file%^" "https://go.fusetools.com/latest-sublime-plugin-2"
IF %ERRORLEVEL% NEQ 0 (
	ECHO Download failed!
	GOTO error
	)
GOTO:eof

:checkForUpdateOrInstall
IF NOT EXIST "%fuse_version_file%" (
	GOTO no_plugin
	)
for /f "delims=" %%A in ('call .\tools\curl.exe -I -s -L --header "If-None-Match: %fuse_etag%" "https://go.fusetools.com/latest-sublime-plugin-2"') do call :checkHeaderUpdate %%A %%B

IF %ERRORLEVEL% NEQ 0 (
	rem ECHO Check for update failed!
	rem GOTO error
	EXIT /B %ERRORLEVEL%
	)
GOTO:eof

:no_plugin
CALL :cleanup
EXIT /B 100

:error
ECHO Failed to install the Sublime plugin
CALL :cleanup
EXIT /B 1

:noupdate
ECHO Package up to date
CALL :cleanup
EXIT /B 0

:updateavailable
ECHO A package update is available
CALL :cleanup
EXIT /B 200

:success
ECHO Successfully installed the sublime plugin
CALL :cleanup
EXIT /B 0

:checkHeader
if "%2"=="304" GOTO noupdate
IF "%1"=="Etag:" IF NOT "%2"=="%fuse_etag%" CALL :updateetag %2
GOTO:eof

:checkHeaderUpdate
if "%2"=="304" GOTO noupdate
IF "%1"=="Etag:" IF NOT "%2"=="%fuse_etag%" GOTO updateavailable
GOTO:eof

:updateetag
IF EXIST "%fuse_version_file%" DEL "%fuse_version_file%"
ECHO %1 > "%fuse_version_file%"
GOTO success

:cleanup
ENDLOCAL
GOTO:eof