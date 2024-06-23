!define NAME "fuse X"
;!define VERSION "x.y.z"

!define NODE_VERSION "16.20.0"
!define NODE_MSI "node-v${NODE_VERSION}-x64.msi"
!define NODE_URL "https://nodejs.org/dist/v${NODE_VERSION}/${NODE_MSI}"
!define NPM_DIR "$APPDATA\npm"

!define JDK_MSI "OpenJDK11U-jdk_x64_windows_hotspot_11.0.16.1_1.msi"
!define JDK_URL "https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.16.1%2B1/${JDK_MSI}"
!define JDK_SUFFIX "Eclipse Adoptium\jdk-11.0.16.101-hotspot"

!define FUSE_STUDIO_NAME "${NAME} ${VERSION}"
!define FUSE_STUDIO_TGZ "fuse-x-studio-win-${VERSION}.tgz"
!define FUSE_STUDIO_DIR "${NPM_DIR}\node_modules\@fuse-x\studio-win"
!define FUSE_STUDIO "${FUSE_STUDIO_DIR}\bin\Release\fuse-studio.exe"
!define FUSE "${FUSE_STUDIO_DIR}\bin\Release\fuse.exe"
!define UNO "${FUSE_STUDIO_DIR}\node_modules\@fuse-open\uno\bin\uno.exe"

!define REG_KEY "Software\Fuseapps\${NAME}\setup"
!define TEMP_DIR "$TEMP\fuse-setup"
!define WRAP "${TEMP_DIR}\wrap.cmd"

!define INSTALL_ANDROID     '"${WRAP}" npm install android-build-tools@1.x -g -f --prefix "${NPM_DIR}"'
!define INSTALL_FUSE_STUDIO '"${WRAP}" npm install "${TEMP_DIR}\${FUSE_STUDIO_TGZ}" -g -f --prefix "${NPM_DIR}"'

Unicode True
Name "${NAME}"
OutFile "fuse-x-${VERSION}-win.exe"
InstallDirRegKey HKCU "${REG_KEY}" ""
RequestExecutionLevel admin
SetCompressor lzma
SpaceTexts none
Target amd64-unicode
BrandingText " "

;--------------------------------
; Interface Settings

!define MUI_WELCOMEFINISHPAGE_BITMAP "data\modern-xl-install.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "data\modern-xl-uninstall.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP_NOSTRETCH "data\modern-xl-install.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP_NOSTRETCH "data\modern-xl-uninstall.bmp"

!include "ModernXL.nsh"
!include "MUI2.nsh"

!define MUI_ICON "data\icon.ico"
!define MUI_UNICON "data\icon.ico"

!define MUI_ABORTWARNING
!define MUI_UNABORTWARNING

!define MUI_WELCOMEPAGE_TITLE_3LINES

!define MUI_COMPONENTSPAGE_NODESC

!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_UNFINISHPAGE_NOAUTOCLOSE

!define MUI_FINISHPAGE_TITLE_3LINES
!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_FUNCTION "LaunchFuseStudio"
!define MUI_FINISHPAGE_LINK "Learn more at fuse-x.com"
!define MUI_FINISHPAGE_LINK_LOCATION "https://fuse-x.com/docs/learn/"

;--------------------------------
; Pages

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "data\license.txt"
!insertmacro MUI_PAGE_COMPONENTS
;!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
;!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
;!insertmacro MUI_UNPAGE_FINISH

;--------------------------------
; Languages

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "Korean"
;!insertmacro MUI_LANGUAGE "Norwegian"

;--------------------------------
; Install types

InstType "Typical"
InstType "Complete"
InstType "Minimal"

;--------------------------------
; Installer Sections

!include "LogicLib.nsh"
!include "sections.nsh"

Section "-wrap"

  SetOutPath "${TEMP_DIR}"
  File "copy-unoconfig.js"
  File "wrap.cmd"

SectionEnd

SectionGroup "fuse X"

Section "Node.js" SEC_NODE
SectionIn 1 2

  ; Installed later.

SectionEnd

Section "VC++ Redistributables"
SectionIn 1 2

  ; https://stackoverflow.com/questions/12206314/detect-if-visual-c-redistributable-for-visual-studio-2012-is-installed

  ReadRegStr $1 HKLM "SOFTWARE\Classes\Installer\Products\1926E8D15D0BCE53481466615F760A7F" "NUL:"
  ${If} $0 != "NUL:"
    DetailPrint "vcredist 2010 (x64) is already installed"
    Goto installed_2010
  ${EndIf}

  DetailPrint "Installing vcredist 2010 (x64)"
  NScurl::http GET https://download.microsoft.com/download/1/6/5/165255E7-1014-4D0A-B094-B6A430A6BFFC/vcredist_x64.exe vcredist_2010_x64.exe /CANCEL /INSIST /Zone.Identifier /END
  ExecWait "${TEMP_DIR}\vcredist_2010_x64.exe /q /norestart"
  Delete "${TEMP_DIR}\vcredist_2010_x64.exe"

installed_2010:
  ReadRegStr $1 HKLM "SOFTWARE\Classes\Installer\Dependencies\{ca67548a-5ebe-413a-b50c-4b9ceb6d66c6}" "NUL:"
  ${If} $0 != "NUL:"
    DetailPrint "vcredist 2012 (x64) is already installed"
    Goto installed_2012
  ${EndIf}

  DetailPrint "Installing vcredist 2012 (x64)"
  NScurl::http get https://download.microsoft.com/download/1/6/B/16B06F60-3B20-4FF2-B699-5E9B7962F9AE/VSU_4/vcredist_x64.exe vcredist_2012_x64.exe /CANCEL /INSIST /Zone.Identifier /END
  ExecWait "${TEMP_DIR}\vcredist_2012_x64.exe /q /norestart"
  Delete "${TEMP_DIR}\vcredist_2012_x64.exe"

installed_2012:
  ReadRegStr $1 HKLM "SOFTWARE\Classes\Installer\Dependencies\{050d4fc8-5d48-4b8f-8972-47c82c46020f}" "NUL:"
  ${If} $0 != "NUL:"
    DetailPrint "vcredist 2013 (x64) is already installed"
    Goto installed_2013
  ${EndIf}

  DetailPrint "Installing vcredist 2013 (x64)"
  NScurl::http get https://download.microsoft.com/download/2/E/6/2E61CFA4-993B-4DD4-91DA-3737CD5CD6E3/vcredist_x64.exe vcredist_2013_x64.exe /CANCEL /INSIST /Zone.Identifier /END
  ExecWait "${TEMP_DIR}\vcredist_2013_x64.exe /install /quiet /norestart"
  Delete "${TEMP_DIR}\vcredist_2013_x64.exe"

installed_2013:
SectionEnd

Section "fuse X" SEC0000
SectionIn 1 2 3 RO

  ExecDos::exec 'cmd /c ""${WRAP}" npm --version"' ''
  Pop $0

  ${If} $0 == 0
    DetailPrint "Node.js is already installed"
    Goto install_fuse
  ${ElseIf} ${SectionIsSelected} ${SEC_NODE}
    Goto install_node
  ${EndIf}

  DetailPrint "Please install Node.js and try again"
  MessageBox MB_ICONQUESTION|MB_YESNO "Node.js is required, but could not be found.$\r$\n$\r$\nDo you want to install Node.js now?" /SD IDNO IDYES install_node IDNO abort_install

install_node:
  DetailPrint "Installing Node.js"
  NScurl::http get "${NODE_URL}" "${TEMP_DIR}\${NODE_MSI}" /CANCEL /INSIST /Zone.Identifier /END
  ExecWait 'msiexec.exe /i "${TEMP_DIR}\${NODE_MSI}" /qn'
  Delete "${TEMP_DIR}\${NODE_MSI}"
  Goto install_fuse

abort_install:
  Call Failed

install_fuse:
  File ${FUSE_STUDIO_TGZ}

  ExecDos::exec 'taskkill /f /t /im unohost.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse-tray.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse-lang.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse-studio.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse-preview.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse.exe' ''

  ;NPM should handle removal of old versions already.
  ;RMDir /r /REBOOTOK "${FUSE_STUDIO_DIR}"

  DetailPrint "Installing ${FUSE_STUDIO_NAME}"
  ExecDos::exec /DETAILED 'cmd /c "${INSTALL_FUSE_STUDIO}"' ''
  Pop $0

  ${If} $0 != 0
    ;Try one more time.
    DetailPrint "Reinstalling ${FUSE_STUDIO_NAME}"
    ExecDos::exec /DETAILED 'cmd /c "${INSTALL_FUSE_STUDIO}"' ''
    Pop $0

    ${If} $0 != 0
      Delete "${TEMP_DIR}\${FUSE_STUDIO_TGZ}"
      Call Failed
    ${EndIf}
  ${EndIf}

  Delete "${TEMP_DIR}\${FUSE_STUDIO_TGZ}"

  SetOutPath "$INSTDIR"
  WriteRegStr HKCU "${REG_KEY}" "" $INSTDIR
  WriteUninstaller "$INSTDIR\uninstall.exe"

  ; https://nsis.sourceforge.io/mediawiki/index.php?title=Add_uninstall_information_to_Add/Remove_Programs
  WriteRegStr HKCU \
    "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" \
    "DisplayName" "${NAME}"
  WriteRegStr HKCU \
    "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" \
    "DisplayIcon" "${FUSE_STUDIO}"
  WriteRegStr HKCU \
    "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" \
    "DisplayVersion" "${VERSION}"
  WriteRegDWORD HKCU \
    "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" \
    "EstimatedSize" 194560 ; ~190MB
  WriteRegStr HKCU \
    "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" \
    "UninstallString" "$INSTDIR\uninstall.exe"

  FileOpen $9 your-files.txt w
  FileWrite $9 "Your files are located at:$\r$\n"
  FileWrite $9 "${FUSE_STUDIO_DIR}$\r$\n"
  FileClose $9

  CreateDirectory "$SMPROGRAMS\${NAME}"
  CreateShortCut "$SMPROGRAMS\${NAME}\${NAME}.lnk" "${FUSE_STUDIO}" "" "${FUSE_STUDIO}"
  CreateShortCut "$SMPROGRAMS\${NAME}\Uninstall ${NAME}.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe"

SectionEnd

Section "Warm-up"
SectionIn 1 2

  DetailPrint "Warming up"
  ExecDos::exec /DETAILED 'cmd /c ""${WRAP}" "${UNO}" build dotnet "${FUSE_STUDIO_DIR}\empty""' ''
  Pop $0

  ${If} $0 != 0
    SetDetailsView show
    MessageBox MB_ICONEXCLAMATION|MB_OK "Warm-up failed."
  ${EndIf}

SectionEnd

SectionGroupEnd
SectionGroup "Android Support"

Section "Java Development Kit" SEC_JAVA
SectionIn 1 2

  ; Installed later.

SectionEnd

Section "Android Build Tools"
SectionIn 1 2

  ExecDos::exec 'cmd /c ""${WRAP}" javac -version"' ''
  Pop $0

  ${If} $0 == 0
    DetailPrint "JDK is already installed"
    Goto install_android
  ${ElseIf} ${SectionIsSelected} ${SEC_JAVA}
    Goto install_java
  ${EndIf}

  DetailPrint "Please install Java Development Kit and try again"
  MessageBox MB_ICONQUESTION|MB_YESNO "Java Development Kit is required, but could not be found.$\r$\n$\r$\nDo you want to install Java Development Kit now?" /SD IDNO IDYES install_java IDNO install_android

install_java:
  DetailPrint "Installing Java Development Kit"
  NScurl::http get "${JDK_URL}" "${TEMP_DIR}\${JDK_MSI}" /CANCEL /INSIST /Zone.Identifier /END
  ExecWait 'msiexec.exe /i "${TEMP_DIR}\${JDK_MSI}" /qn'
  Delete "${TEMP_DIR}\${JDK_MSI}"

  ; Use JDK installation in %PROGRAMW6432% (x64).
  Var /GLOBAL JAVA_HOME
  ExpandEnvStrings $JAVA_HOME "%PROGRAMW6432%\${JDK_SUFFIX}"
  System::Call 'Kernel32::SetEnvironmentVariable(t, t)i ("JAVA_HOME", "$JAVA_HOME").r0'
  DetailPrint "JAVA_HOME: $JAVA_HOME"

install_android:
  DetailPrint "Installing Android Build Tools"
  ExecDos::exec /DETAILED 'cmd /c "${INSTALL_ANDROID}"' ''
  Pop $0

  ${If} $0 != 0
    SetDetailsView show
    MessageBox MB_ICONEXCLAMATION|MB_OK "Android Build Tools failed to install."
  ${EndIf}

  ; Copy generated .unoconfig to %PROGRAMDATA% (#92).
  ExecDos::exec /DETAILED 'cmd /c ""${WRAP}" node "${TEMP_DIR}\copy-unoconfig.js""' ''
  Pop $0

  ${If} $0 != 0
    SetDetailsView show
    MessageBox MB_ICONEXCLAMATION|MB_OK "Failed to copy unoconfig file."
  ${EndIf}

SectionEnd

SectionGroupEnd
SectionGroup /e "Text Editor Plugins"

Section "Visual Studio Code"
SectionIn 2

  ExecDos::exec 'cmd /c ""${WRAP}" code --version"' ''
  Pop $0

  ${If} $0 == 0
    DetailPrint "Visual Studio Code is already installed"
    Goto install_extension
  ${EndIf}

  DetailPrint "Please install Visual Studio Code"
  MessageBox MB_ICONQUESTION|MB_YESNO "The 'code' command was not found on your computer.$\r$\n$\r$\nDo you want to install Visual Studio Code now?" /SD IDNO IDYES install_code IDNO install_extension

install_code:
  ExecShell "open" "https://code.visualstudio.com/download/"
  MessageBox MB_ICONINFORMATION|MB_OK "Please follow instructions on https://code.visualstudio.com/download/ to install Visual Studio Code."

install_extension:
  DetailPrint "Installing Visual Studio Code extension"
  ExecDos::exec /DETAILED 'cmd /c ""${WRAP}" "${FUSE}" install vscode-extension"' ''
  Pop $0

  ${If} $0 != 0
    SetDetailsView show
    MessageBox MB_ICONEXCLAMATION|MB_OK "The Visual Studio Code extension failed to install.$\r$\n$\r$\nYou can reinstall the extension later from the Tools menu in fuse X."
  ${EndIf}

SectionEnd

Section "Sublime Text 3"
SectionIn 2

  IfFileExists "$SMPROGRAMS\Sublime Text 3.lnk" 0 +2
  Goto install_plugin

  ExecDos::exec 'cmd /c ""${WRAP}" subl --version"' ''
  Pop $0

  ${If} $0 == 0
    DetailPrint "Sublime Text is already installed"
    Goto install_plugin
  ${EndIf}

  DetailPrint "Please install Sublime Text"
  MessageBox MB_ICONQUESTION|MB_YESNO "The 'subl' command was not found on your computer.$\r$\n$\r$\nDo you want to install Sublime Text now?" /SD IDNO IDYES install_sublime IDNO install_plugin

install_sublime:
  ExecShell "open" "https://www.sublimetext.com/3"
  MessageBox MB_ICONINFORMATION|MB_OK "Please follow instructions on https://www.sublimetext.com/3 to install Sublime Text."

install_plugin:
  DetailPrint "Installing Sublime Text plugin"
  ExecDos::exec /DETAILED 'cmd /c ""${WRAP}" "${FUSE}" install sublime-plugin"' ''
  Pop $0

  ${If} $0 != 0
    SetDetailsView show
    MessageBox MB_ICONEXCLAMATION|MB_OK "The Sublime Text plugin failed to install.$\r$\n$\r$\nYou can reinstall the plugin later from the Tools menu in fuse X."
  ${EndIf}

SectionEnd

Section "Atom"
SectionIn 2

  DetailPrint "Installing Atom plugin"
  ExecDos::exec /DETAILED 'cmd /c ""${WRAP}" "${FUSE}" install atom-plugin"' ''
  Pop $0

  ${If} $0 != 0
    SetDetailsView show
    MessageBox MB_ICONEXCLAMATION|MB_OK "The Atom plugin failed to install.$\r$\n$\r$\nYou can reinstall the plugin later from the Tools menu in fuse X."
  ${EndIf}

SectionEnd

SectionGroupEnd

Section "-chmod"

  DetailPrint "Setting permissions"

  ; Reset permissions to All Users (not Administrator).
  AccessControl::GrantOnFile "${FUSE_STUDIO_DIR}" "(S-1-5-32-545)" "FullAccess"
  Pop $0

  ${If} $0 == "error"
    SetDetailsView show
    MessageBox MB_ICONEXCLAMATION|MB_OK "Failed to set permissions."
  ${EndIf}

SectionEnd

;--------------------------------
; Uninstaller Section

  Var PROGRAMDATA

Section "Uninstall"

  ExecDos::exec 'taskkill /f /t /im unohost.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse-tray.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse-lang.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse-studio.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse-preview.exe' ''
  ExecDos::exec 'taskkill /f /t /im fuse.exe' ''

  RMDir /r /REBOOTOK "$INSTDIR"
  RMDir /r /REBOOTOK "$SMPROGRAMS\${NAME}"
  RMDir /r /REBOOTOK "${FUSE_STUDIO_DIR}"
  RMDir /r /REBOOTOK "${TEMP_DIR}"

  Delete /REBOOTOK "${NPM_DIR}\fuse"
  Delete /REBOOTOK "${NPM_DIR}\fuse.cmd"
  Delete /REBOOTOK "${NPM_DIR}\fuse.ps1"
  Delete /REBOOTOK "${NPM_DIR}\fuse-x"
  Delete /REBOOTOK "${NPM_DIR}\fuse-x.cmd"
  Delete /REBOOTOK "${NPM_DIR}\fuse-x.ps1"
  Delete /REBOOTOK "${NPM_DIR}\uno"
  Delete /REBOOTOK "${NPM_DIR}\uno.cmd"
  Delete /REBOOTOK "${NPM_DIR}\uno.ps1"

  DeleteRegKey HKCU "${REG_KEY}"
  DeleteRegKey HKCU "Software\Classes\fusestudio" ; Legacy URL scheme.
  DeleteRegKey HKCU "Software\Classes\fuse-x"
  ; https://nsis.sourceforge.io/mediawiki/index.php?title=Add_uninstall_information_to_Add/Remove_Programs
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}"

  MessageBox MB_ICONQUESTION|MB_YESNO "Do you want to delete your configuration and log files?" /SD IDNO IDYES remove_userdata IDNO done

remove_userdata:
  RMDir /r /REBOOTOK "$LOCALAPPDATA\fuse X"
  RMDir /r /REBOOTOK "$PROGRAMDATA\fuse X"

done:
SectionEnd

;--------------------------------
; Installer Functions

Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY
  SectionSetFlags ${SEC0000} 17
  ; Prefer installing in %PROGRAMW6432% (x64).
  ExpandEnvStrings $INSTDIR "%PROGRAMW6432%\${NAME}"
FunctionEnd

Function .onGUIEnd
  RMDir /r /REBOOTOK "${TEMP_DIR}"
FunctionEnd

Function Failed
  DetailPrint "Install failed."
  SetDetailsView show
  Abort
FunctionEnd

Function LaunchFuseStudio
  ; http://mdb-blog.blogspot.com/2013/01/nsis-lunch-program-as-user-from-uac.html
  Exec '"$WINDIR\explorer.exe" "${FUSE_STUDIO}"'
FunctionEnd

;--------------------------------
; Uninstaller Functions

Function un.onInit
  !insertmacro MUI_LANGDLL_DISPLAY
  ; https://nsis-dev.github.io/NSIS-Forums/html/t-354968.html
  SetShellVarContext all
  StrCpy $PROGRAMDATA "$APPDATA"
  SetShellVarContext current
FunctionEnd
