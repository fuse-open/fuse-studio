@echo off

:: Paths added during installation.
set PATH=%APPDATA%\npm;%PATH%
set PATH=%JAVA_HOME%\bin;%PATH%
set PATH=%PROGRAMFILES%\Git\cmd;%PATH%
set PATH=%PROGRAMFILES%\nodejs;%PATH%
set PATH=%PROGRAMW6432%\Eclipse Adoptium\jdk-11.0.16.101-hotspot\bin;%PATH%
set PATH=%PROGRAMW6432%\Android\Android Studio\jre\bin;%PATH%
set PATH=%PROGRAMW6432%\Git\cmd;%PATH%
set PATH=%PROGRAMW6432%\Microsoft VS Code\bin;%PATH%
set PATH=%PROGRAMW6432%\Sublime Text 3;%PATH%
set PATH=%PROGRAMW6432%\nodejs;%PATH%

:: Run command.
%*
