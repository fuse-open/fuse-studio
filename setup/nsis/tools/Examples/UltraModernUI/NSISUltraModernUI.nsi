; o-----------------------------------------------o
; | NSIS 3.03 + Ultra-Modern User Interface 2.0b5 |
; (-----------------------------------------------)
; | Installer script.                             |
; | Written by SyperPat                           |
; o-----------------------------------------------o

;--------------------------------
;General

  !define /date NOW "%Y-%m-%d"
  !define NAME "UltraModernUI"

  !define UMUI_VERSION "2.0b5"
  !define UMUI_VERBUILD "2.0_${NOW}"

  !define VER_MAJOR 3
  !define VER_MINOR 04
  !define VER_REVISION 0
  !define VER_BUILD 0
  !define VER_REV_STR ""

  !define VERSION "${VER_MAJOR}.${VER_MINOR}${VER_REV_STR}"
  !if "${NSIS_VERSION}" != "v${VERSION}"
    !error "VER_MAJOR, VER_MINOR and VER_REV_STR defines does not match the current NSIS version: ${NSIS_VERSION}"
  !endif

  !define /date VERIPV "200.%Y.%m.%d"
  VIProductVersion "${VERIPV}"
  VIAddVersionKey ProductName "NSIS (Nullsoft Scriptable Install System) with the Ultra-Modern User Interface."
  VIAddVersionKey ProductVersion "${UMUI_VERSION}"
  VIAddVersionKey Comments "This package also include some plugins used by UMUI to extend the possibilities of NSIS."
  VIAddVersionKey LegalTrademarks "NSIS and Ultra-Modern UI are released under the zlib/libpng license: http://nsis.sf.net/License"
  VIAddVersionKey LegalCopyright "Copyright © 2005-2019 SuperPat"
  VIAddVersionKey FileDescription "NSIS (Nullsoft Scriptable Install System) with the Ultra-Modern User Interface."
  VIAddVersionKey FileVersion "${UMUI_VERBUILD}"


;--------------------------------
;Configuration

  ; The name of the installer
  Name "NSIS ${VERSION} and Ultra-Modern UI ${UMUI_VERSION}"

  ; The file to write
  OutFile "NSIS_${VERSION}_UltraModernUI_${UMUI_VERSION}.exe"

  SetCompressor /FINAL /SOLID lzma

  ;Generate unicode installer
  Unicode True

  ;Windows vista compatibility
  RequestExecutionLevel admin

  BrandingText "$(^NameDA)"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\NSIS"


;--------------------------------
;Include Ultra-Modern UI between others

  !include "UMUI.nsh"
  !include "Sections.nsh"

;--------------------------------
;Definitions

  !define SHCNE_ASSOCCHANGED 0x8000000
  !define SHCNF_IDLIST 0

;--------------------------------
;Interface Settings

  !define UMUI_SKIN "blue"

  !define UMUI_USE_INSTALLOPTIONSEX

  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING

  !define UMUI_PAGEBGIMAGE
  !define UMUI_UNPAGEBGIMAGE

  !define UMUI_USE_ALTERNATE_PAGE ; For Welcome finish abort pages
  !define UMUI_USE_UNALTERNATE_PAGE

  !define MUI_COMPONENTSPAGE_SMALLDESC

  !define UMUI_DEFAULT_SHELLVARCONTEXT all
 
  !define UMUI_ENABLE_DESCRIPTION_TEXT

;--------------------------------
;Registry Settings

  !define UMUI_PARAMS_REGISTRY_ROOT HKLM
  !define UMUI_PARAMS_REGISTRY_KEY "Software\NSIS"

  !define UMUI_LANGUAGE_REGISTRY_VALUENAME "UMUI_InstallerLanguage"
  !define UMUI_SHELLVARCONTEXT_REGISTRY_VALUENAME "UMUI_ShellVarContext"

  !define UMUI_UNINSTALLPATH_REGISTRY_VALUENAME "UMUI_UninstallPath"
  !define UMUI_UNINSTALL_FULLPATH "$INSTDIR\UninstallUMUI.exe"
  !define UMUI_INSTALLERFULLPATH_REGISTRY_VALUENAME "UMUI_InstallPath"

  !define UMUI_VERSION_REGISTRY_VALUENAME "UMUI_Version"
  !define UMUI_VERBUILD_REGISTRY_VALUENAME "UMUI_VerBuild"

  !define UMUI_PREUNINSTALL_FUNCTION preuninstall_function

  InstallDirRegKey ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" ""

;--------------------------------
;Reserve Files

  !insertmacro MUI_RESERVEFILE_INSTALLOPTIONS


;--------------------------------
;Pages

  !insertmacro UMUI_PAGE_MULTILANGUAGE

    !define UMUI_MAINTENANCEPAGE_MODIFY
    !define UMUI_MAINTENANCEPAGE_REPAIR
    !define UMUI_MAINTENANCEPAGE_REMOVE
    !define UMUI_MAINTENANCEPAGE_CONTINUE_SETUP
  !insertmacro UMUI_PAGE_MAINTENANCE

    !define UMUI_UPDATEPAGE_REMOVE
    !define UMUI_UPDATEPAGE_CONTINUE_SETUP
  !insertmacro UMUI_PAGE_UPDATE

    !define UMUI_WELCOMEPAGE_ALTERNATIVETEXT
  !insertmacro MUI_PAGE_WELCOME

    !define MUI_LICENSEPAGE_CHECKBOX
  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"

    !define UMUI_INFORMATIONPAGE_USE_RICHTEXTFORMAT
  !insertmacro UMUI_PAGE_INFORMATION "${NSISDIR}\Docs\UltraModernUI\ReadMe.rtf"

    !define UMUI_SETUPTYPEPAGE_MINIMAL "$(UMUI_TEXT_SETUPTYPE_MINIMAL_TITLE)"
    !define UMUI_SETUPTYPEPAGE_STANDARD "$(UMUI_TEXT_SETUPTYPE_STANDARD_TITLE)"
    !define UMUI_SETUPTYPEPAGE_COMPLETE "$(UMUI_TEXT_SETUPTYPE_COMPLETE_TITLE)"
    !define UMUI_SETUPTYPEPAGE_DEFAULTCHOICE ${UMUI_COMPLETE}
    !define UMUI_SETUPTYPEPAGE_REGISTRY_VALUENAME "UMUI_SetupType"
  !insertmacro UMUI_PAGE_SETUPTYPE

    !define UMUI_COMPONENTSPAGE_INSTTYPE_REGISTRY_VALUENAME "UMUI_InstType"
    !define UMUI_COMPONENTSPAGE_REGISTRY_VALUENAME "UMUI_Components"
  !insertmacro MUI_PAGE_COMPONENTS

  !insertmacro MUI_PAGE_DIRECTORY

    !define UMUI_ADDITIONALTASKS_REGISTRY_VALUENAME "UMUI_Tasks"
  !insertmacro UMUI_PAGE_ADDITIONALTASKS addtasks_function

    !define UMUI_CONFIRMPAGE_TEXTBOX confirm_function
  !insertmacro UMUI_PAGE_CONFIRM

  !insertmacro MUI_PAGE_INSTFILES

    !define MUI_FINISHPAGE_SHOWREADME "${NSISDIR}\Docs\UltraModernUI\Readme.html"
    !define MUI_FINISHPAGE_LINK "Ultra-Modern UI Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro MUI_PAGE_FINISH

    !define UMUI_ABORTPAGE_LINK "Ultra-Modern UI Home Page"
    !define UMUI_ABORTPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro UMUI_PAGE_ABORT


  !insertmacro UMUI_UNPAGE_MULTILANGUAGE

    !define UMUI_MAINTENANCEPAGE_MODIFY
    !define UMUI_MAINTENANCEPAGE_REPAIR
    !define UMUI_MAINTENANCEPAGE_REMOVE
    !define UMUI_MAINTENANCEPAGE_CONTINUE_SETUP
  !insertmacro UMUI_UNPAGE_MAINTENANCE

  !insertmacro MUI_UNPAGE_WELCOME

  !insertmacro MUI_UNPAGE_CONFIRM

  !insertmacro MUI_UNPAGE_INSTFILES

    !define MUI_FINISHPAGE_LINK "Ultra-Modern UI Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro MUI_UNPAGE_FINISH

    !define UMUI_ABORTPAGE_LINK "Ultra-Modern UI Home Page"
    !define UMUI_ABORTPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro UMUI_UNPAGE_ABORT


;--------------------------------
;Languages

; first language is the default language if the system language is not in this list
  !insertmacro MUI_LANGUAGE "English"

; Other UMUI translated languages
  !insertmacro MUI_LANGUAGE "Bulgarian"
  !insertmacro MUI_LANGUAGE "Czech"
  !insertmacro MUI_LANGUAGE "French"
  !insertmacro MUI_LANGUAGE "German"
  !insertmacro MUI_LANGUAGE "Greek"
  !insertmacro MUI_LANGUAGE "Hungarian"
  !insertmacro MUI_LANGUAGE "Italian"
  !insertmacro MUI_LANGUAGE "Japanese"
  !insertmacro MUI_LANGUAGE "Lithuanian"
  !insertmacro MUI_LANGUAGE "Polish"
  !insertmacro MUI_LANGUAGE "Russian"
  !insertmacro MUI_LANGUAGE "Slovenian"
  !insertmacro MUI_LANGUAGE "Spanish"
  !insertmacro MUI_LANGUAGE "Turkish"
; Other UMUI partially translated language
  !insertmacro MUI_LANGUAGE "PortugueseBR"

; Other untranslated languages but usable even so.
  !insertmacro MUI_LANGUAGE "SpanishInternational"
  !insertmacro MUI_LANGUAGE "SimpChinese"
  !insertmacro MUI_LANGUAGE "TradChinese"
  !insertmacro MUI_LANGUAGE "Korean"
  !insertmacro MUI_LANGUAGE "Dutch"
  !insertmacro MUI_LANGUAGE "Danish"
  !insertmacro MUI_LANGUAGE "Swedish"
  !insertmacro MUI_LANGUAGE "Norwegian"
  !insertmacro MUI_LANGUAGE "NorwegianNynorsk"
  !insertmacro MUI_LANGUAGE "Finnish"
  !insertmacro MUI_LANGUAGE "Portuguese"
  !insertmacro MUI_LANGUAGE "Ukrainian"
  !insertmacro MUI_LANGUAGE "Slovak"
  !insertmacro MUI_LANGUAGE "Croatian"
  !insertmacro MUI_LANGUAGE "Thai"
  !insertmacro MUI_LANGUAGE "Romanian"
  !insertmacro MUI_LANGUAGE "Latvian"
  !insertmacro MUI_LANGUAGE "Macedonian"
  !insertmacro MUI_LANGUAGE "Estonian"
  !insertmacro MUI_LANGUAGE "Serbian"
  !insertmacro MUI_LANGUAGE "SerbianLatin"
  !insertmacro MUI_LANGUAGE "Arabic"
  !insertmacro MUI_LANGUAGE "Farsi"
  !insertmacro MUI_LANGUAGE "Hebrew"
  !insertmacro MUI_LANGUAGE "Indonesian"
  !insertmacro MUI_LANGUAGE "Mongolian"
  !insertmacro MUI_LANGUAGE "Luxembourgish"
  !insertmacro MUI_LANGUAGE "Albanian"
  !insertmacro MUI_LANGUAGE "Breton"
  !insertmacro MUI_LANGUAGE "Belarusian"
  !insertmacro MUI_LANGUAGE "Icelandic"
  !insertmacro MUI_LANGUAGE "Malay"
  !insertmacro MUI_LANGUAGE "Bosnian"
  !insertmacro MUI_LANGUAGE "Kurdish"
  !insertmacro MUI_LANGUAGE "Irish"
  !insertmacro MUI_LANGUAGE "Uzbek"
  !insertmacro MUI_LANGUAGE "Galician"
  !insertmacro MUI_LANGUAGE "Afrikaans"
  !insertmacro MUI_LANGUAGE "Catalan"
  !insertmacro MUI_LANGUAGE "Esperanto"
  !insertmacro MUI_LANGUAGE "Asturian"
  !insertmacro MUI_LANGUAGE "Basque"
  !insertmacro MUI_LANGUAGE "Pashto"
  !insertmacro MUI_LANGUAGE "ScotsGaelic"
  !insertmacro MUI_LANGUAGE "Vietnamese"
  !insertmacro MUI_LANGUAGE "Welsh"
  !insertmacro MUI_LANGUAGE "Corsican"
  !insertmacro MUI_LANGUAGE "Tatar"

; Other unicode only untranslated languages but usable even so.
  !insertmacro MUI_LANGUAGE "Armenian"
  !insertmacro MUI_LANGUAGE "Georgian"
  !insertmacro MUI_LANGUAGE "Hindi"


;--------------------------------
;Installer Types

InstType "$(UMUI_TEXT_SETUPTYPE_MINIMAL_TITLE)"
InstType "$(UMUI_TEXT_SETUPTYPE_STANDARD_TITLE)"
InstType "$(UMUI_TEXT_SETUPTYPE_COMPLETE_TITLE)"


;--------------------------------
;Installer Sections

SectionGroup /e "NSIS ${VERSION}" SecNSIS

!macro InstallStub stub
    File ..\..\Stubs\${stub}-x86-ansi
    File ..\..\Stubs\${stub}-x86-unicode
!macroend

Section "NSIS Core Files (required)" SecCore

  SetDetailsPrint textonly
  DetailPrint "Installing NSIS Core Files..."
  SetDetailsPrint listonly

  SectionIn 1 2 3 RO
  SetOutPath $INSTDIR
  RMDir /r $SMPROGRAMS\NSIS

  IfFileExists $INSTDIR\nsisconf.nsi "" +2
  Rename $INSTDIR\nsisconf.nsi $INSTDIR\nsisconf.nsh
  SetOverwrite off
  File ..\..\nsisconf.nsh

  SetOverwrite on
  File ..\..\makensis.exe
  File ..\..\makensisw.exe
  File ..\..\COPYING
  File ..\..\NSIS.chm
  !pragma verifychm "..\..\NSIS.chm"
  !if /FileExists "..\..\NSIS.exe"
    !if /FileExists "..\..\NSIS.exe.manifest"
      File "..\..\NSIS.exe.manifest"
    !endif
  !else
    !define NO_NSISMENU_HTML 1
    !makensis '-v2 "NSISMenu.nsi" "-XOutFile ..\..\NSIS.exe"' = 0
  !endif
  File ..\..\NSIS.exe

  SetOutPath $INSTDIR\Bin
  File ..\..\Bin\makensis.exe
!ifdef USE_NEW_ZLIB
  File ..\..\Bin\zlib.dll
!else
  File ..\..\Bin\zlib1.dll
!endif

  SetOutPath $INSTDIR\Stubs
  File ..\..\Stubs\uninst
  !insertmacro InstallStub bzip2
  !insertmacro InstallStub bzip2_solid
  !insertmacro InstallStub lzma
  !insertmacro InstallStub lzma_solid
  !insertmacro InstallStub zlib
  !insertmacro InstallStub zlib_solid


  SetOutPath $INSTDIR\Include
  File ..\..\Include\WinMessages.nsh
  File ..\..\Include\Sections.nsh
  File ..\..\Include\Library.nsh
  File ..\..\Include\UpgradeDLL.nsh
  File ..\..\Include\LogicLib.nsh
  File ..\..\Include\StrFunc.nsh
  File ..\..\Include\Colors.nsh
  File ..\..\Include\FileFunc.nsh
  File ..\..\Include\TextFunc.nsh
  File ..\..\Include\WordFunc.nsh
  File ..\..\Include\WinVer.nsh
  File ..\..\Include\x64.nsh
  File ..\..\Include\Memento.nsh
  File ..\..\Include\LangFile.nsh
  File ..\..\Include\InstallOptions.nsh
  File ..\..\Include\MultiUser.nsh
  File ..\..\Include\VB6RunTime.nsh
  File ..\..\Include\Util.nsh
  File ..\..\Include\WinCore.nsh

  SetOutPath $INSTDIR\Include\Win
  File ..\..\Include\Win\WinDef.nsh
  File ..\..\Include\Win\WinError.nsh
  File ..\..\Include\Win\WinNT.nsh
  File ..\..\Include\Win\WinUser.nsh
  File ..\..\Include\Win\Propkey.nsh

  SetOutPath $INSTDIR\Docs\StrFunc
  File ..\..\Docs\StrFunc\StrFunc.txt

  SetOutPath $INSTDIR\Docs\MultiUser
  File ..\..\Docs\MultiUser\Readme.html

  SetOutPath $INSTDIR\Docs\makensisw
  File ..\..\Docs\makensisw\*.txt

  !ifndef NO_NSISMENU_HTML
    SetOutPath $INSTDIR\Menu
    File ..\..\Menu\*.html
    SetOutPath $INSTDIR\Menu\images
    File ..\..\Menu\images\header.gif
    File ..\..\Menu\images\line.gif
    File ..\..\Menu\images\site.gif
  !endif

  Delete $INSTDIR\makensis.htm
  Delete $INSTDIR\Docs\*.html
  Delete $INSTDIR\Docs\style.css
  RMDir $INSTDIR\Docs

  SetOutPath $INSTDIR\Bin
  !if /FileExists "..\..\Bin\LibraryLocal.exe"
    File ..\..\Bin\LibraryLocal.exe
  !endif
  !if /FileExists "..\..\Bin\RegTool-x86.bin"
    File ..\..\Bin\RegTool-x86.bin
  !else
    File ..\..\Bin\RegTool.bin
  !endif

  CreateDirectory $INSTDIR\Plugins\x86-ansi
  CreateDirectory $INSTDIR\Plugins\x86-unicode

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\TypeLib.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\TypeLib.dll

  ReadRegStr $R0 HKCR ".nsi" ""
  StrCmp $R0 "NSISFile" 0 +2
    DeleteRegKey HKCR "NSISFile"

  WriteRegStr HKCR ".nsi" "" "NSIS.Script"
  WriteRegStr HKCR "NSIS.Script" "" "NSIS Script File"
  WriteRegStr HKCR "NSIS.Script\DefaultIcon" "" "$INSTDIR\makensisw.exe,1"
  ReadRegStr $R0 HKCR "NSIS.Script\shell\open\command" ""
  StrCmp $R0 "" 0 no_nsiopen
    WriteRegStr HKCR "NSIS.Script\shell" "" "open"
    WriteRegStr HKCR "NSIS.Script\shell\open\command" "" 'notepad.exe "%1"'
  no_nsiopen:
  WriteRegStr HKCR "NSIS.Script\shell\compile" "" "Compile NSIS Script"
  WriteRegStr HKCR "NSIS.Script\shell\compile\command" "" '"$INSTDIR\makensisw.exe" "%1"'
  WriteRegStr HKCR "NSIS.Script\shell\compile-compressor" "" "Compile NSIS Script (Choose Compressor)"
  WriteRegStr HKCR "NSIS.Script\shell\compile-compressor\command" "" '"$INSTDIR\makensisw.exe" /ChooseCompressor "%1"'

  ReadRegStr $R0 HKCR ".nsh" ""
  StrCmp $R0 "NSHFile" 0 +2
    DeleteRegKey HKCR "NSHFile"

  WriteRegStr HKCR ".nsh" "" "NSIS.Header"
  WriteRegStr HKCR "NSIS.Header" "" "NSIS Header File"
  WriteRegStr HKCR "NSIS.Header\DefaultIcon" "" "$INSTDIR\makensisw.exe,1"
  ReadRegStr $R0 HKCR "NSIS.Header\shell\open\command" ""
  StrCmp $R0 "" 0 no_nshopen
    WriteRegStr HKCR "NSIS.Header\shell" "" "open"
    WriteRegStr HKCR "NSIS.Header\shell\open\command" "" 'notepad.exe "%1"'
  no_nshopen:

  System::Call 'Shell32::SHChangeNotify(i ${SHCNE_ASSOCCHANGED}, i ${SHCNF_IDLIST}, i 0, i 0)'

SectionEnd

Section "Script Examples" SecExample

  SetDetailsPrint textonly
  DetailPrint "Installing Script Examples..."
  SetDetailsPrint listonly

  SectionIn 2 3
  SetOutPath $INSTDIR\Examples
  File ..\..\Examples\makensis.nsi
  File ..\..\Examples\example1.nsi
  File ..\..\Examples\example2.nsi
  File ..\..\Examples\viewhtml.nsi
  File ..\..\Examples\waplugin.nsi
  File ..\..\Examples\bigtest.nsi
  File ..\..\Examples\primes.nsi
  File ..\..\Examples\rtest.nsi
  File ..\..\Examples\gfx.nsi
  File ..\..\Examples\one-section.nsi
  File ..\..\Examples\languages.nsi
  File ..\..\Examples\Library.nsi
  File ..\..\Examples\VersionInfo.nsi
  File ..\..\Examples\UserVars.nsi
  File ..\..\Examples\LogicLib.nsi
  File ..\..\Examples\silent.nsi
  File ..\..\Examples\StrFunc.nsi
  File ..\..\Examples\FileFunc.nsi
  File ..\..\Examples\FileFunc.ini
  File ..\..\Examples\FileFuncTest.nsi
  File ..\..\Examples\TextFunc.nsi
  File ..\..\Examples\TextFunc.ini
  File ..\..\Examples\TextFuncTest.nsi
  File ..\..\Examples\WordFunc.nsi
  File ..\..\Examples\WordFunc.ini
  File ..\..\Examples\WordFuncTest.nsi
  File ..\..\Examples\Memento.nsi
  File /nonfatal ..\..\Examples\unicode.nsi
  File /nonfatal ..\..\Examples\NSISMenu.nsi

  SetOutPath $INSTDIR\Examples\Plugin
  File ..\..\Examples\Plugin\exdll.c
  File ..\..\Examples\Plugin\exdll.dpr
  File ..\..\Examples\Plugin\exdll.dsp
  File ..\..\Examples\Plugin\exdll.dsw
  File ..\..\Examples\Plugin\exdll_with_unit.dpr
  File ..\..\Examples\Plugin\exdll-vs2008.sln
  File ..\..\Examples\Plugin\exdll-vs2008.vcproj
  File ..\..\Examples\Plugin\extdll.inc
  File ..\..\Examples\Plugin\nsis.pas

  SetOutPath $INSTDIR\Examples\Plugin\nsis
  File ..\..\Examples\Plugin\nsis\pluginapi.h
  File /nonfatal ..\..\Examples\Plugin\nsis\pluginapi*.lib
  File ..\..\Examples\Plugin\nsis\api.h
  File ..\..\Examples\Plugin\nsis\nsis_tchar.h

SectionEnd

SectionGroup "User Interfaces" SecInterfaces

Section "Modern User Interface" SecInterfacesModernUI

  SetDetailsPrint textonly
  DetailPrint "Installing User Interfaces | Modern User Interface..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath "$INSTDIR\Examples\Modern UI"
  File "..\..\Examples\Modern UI\Basic.nsi"
  File "..\..\Examples\Modern UI\HeaderBitmap.nsi"
  File "..\..\Examples\Modern UI\MultiLanguage.nsi"
  File "..\..\Examples\Modern UI\StartMenu.nsi"
  File "..\..\Examples\Modern UI\WelcomeFinish.nsi"

  SetOutPath "$INSTDIR\Contrib\Modern UI"
  File "..\..\Contrib\Modern UI\System.nsh"
  File "..\..\Contrib\Modern UI\ioSpecial.ini"

  SetOutPath "$INSTDIR\Docs\Modern UI"
  File "..\..\Docs\Modern UI\Readme.html"
  File "..\..\Docs\Modern UI\Changelog.txt"
  File "..\..\Docs\Modern UI\License.txt"

  SetOutPath "$INSTDIR\Docs\Modern UI\images"
  File "..\..\Docs\Modern UI\images\header.gif"
  File "..\..\Docs\Modern UI\images\screen1.png"
  File "..\..\Docs\Modern UI\images\screen2.png"
  File "..\..\Docs\Modern UI\images\open.gif"
  File "..\..\Docs\Modern UI\images\closed.gif"

  SetOutPath $INSTDIR\Contrib\UIs
  File "..\..\Contrib\UIs\modern.exe"
  File "..\..\Contrib\UIs\modern_headerbmp.exe"
  File "..\..\Contrib\UIs\modern_headerbmpr.exe"
  File "..\..\Contrib\UIs\modern_nodesc.exe"
  File "..\..\Contrib\UIs\modern_smalldesc.exe"

  SetOutPath $INSTDIR\Include
  File "..\..\Include\MUI.nsh"

  SetOutPath "$INSTDIR\Contrib\Modern UI 2"
  File "..\..\Contrib\Modern UI 2\Deprecated.nsh"
  File "..\..\Contrib\Modern UI 2\Interface.nsh"
  File "..\..\Contrib\Modern UI 2\Localization.nsh"
  File "..\..\Contrib\Modern UI 2\MUI2.nsh"
  File "..\..\Contrib\Modern UI 2\Pages.nsh"

  SetOutPath "$INSTDIR\Contrib\Modern UI 2\Pages"
  File "..\..\Contrib\Modern UI 2\Pages\Components.nsh"
  File "..\..\Contrib\Modern UI 2\Pages\Directory.nsh"
  File "..\..\Contrib\Modern UI 2\Pages\Finish.nsh"
  File "..\..\Contrib\Modern UI 2\Pages\InstallFiles.nsh"
  File "..\..\Contrib\Modern UI 2\Pages\License.nsh"
  File "..\..\Contrib\Modern UI 2\Pages\StartMenu.nsh"
  File "..\..\Contrib\Modern UI 2\Pages\UninstallConfirm.nsh"
  File "..\..\Contrib\Modern UI 2\Pages\Welcome.nsh"

  SetOutPath "$INSTDIR\Docs\Modern UI 2"
  File "..\..\Docs\Modern UI 2\Readme.html"
  File "..\..\Docs\Modern UI 2\License.txt"

  SetOutPath "$INSTDIR\Docs\Modern UI 2\images"
  File "..\..\Docs\Modern UI 2\images\header.gif"
  File "..\..\Docs\Modern UI 2\images\screen1.png"
  File "..\..\Docs\Modern UI 2\images\screen2.png"
  File "..\..\Docs\Modern UI 2\images\open.gif"
  File "..\..\Docs\Modern UI 2\images\closed.gif"

  SetOutPath $INSTDIR\Include
  File "..\..\Include\MUI2.nsh"

SectionEnd

Section "Default User Interface" SecInterfacesDefaultUI

  SetDetailsPrint textonly
  DetailPrint "Installing User Interfaces | Default User Interface..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath "$INSTDIR\Contrib\UIs"
  File "..\..\Contrib\UIs\default.exe"

SectionEnd

Section "Tiny User Interface" SecInterfacesTinyUI

  SetDetailsPrint textonly
  DetailPrint "Installing User Interfaces | Tiny User Interface..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath "$INSTDIR\Contrib\UIs"
  File "..\..\Contrib\UIs\sdbarker_tiny.exe"

SectionEnd

SectionGroupEnd

Section "Graphics" SecGraphics

  SetDetailsPrint textonly
  DetailPrint "Installing Graphics..."
  SetDetailsPrint listonly

  SectionIn 2 3

  RMDir $INSTDIR\Contrib\Icons
  SetOutPath $INSTDIR\Contrib\Graphics
  File /r "..\..\Contrib\Graphics\*.ico"
  File /r "..\..\Contrib\Graphics\*.bmp"
SectionEnd

Section "Language Files" SecLangFiles

  SetDetailsPrint textonly
  DetailPrint "Installing Language Files..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath "$INSTDIR\Contrib\Language files"
  File "..\..\Contrib\Language files\*.nlf"

  SetOutPath $INSTDIR\Bin
  File ..\..\Bin\MakeLangID.exe

  !insertmacro SectionFlagIsSet ${SecInterfacesModernUI} ${SF_SELECTED} mui nomui
  mui:
    SetOutPath "$INSTDIR\Contrib\Language files"
    File "..\..\Contrib\Language files\*.nsh"
  nomui:

SectionEnd

SectionGroup "Tools" SecTools

Section "Zip2Exe" SecToolsZ2E

  SetDetailsPrint textonly
  DetailPrint "Installing Tools | Zip2Exe..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Bin
  File ..\..\Bin\zip2exe.exe
  SetOutPath $INSTDIR\Contrib\zip2exe
  File ..\..\Contrib\zip2exe\Base.nsh
  File ..\..\Contrib\zip2exe\Modern.nsh
  File ..\..\Contrib\zip2exe\Classic.nsh

SectionEnd

SectionGroupEnd

SectionGroup "Plug-ins" SecPluginsPlugins

Section "Banner plugin" SecPluginsBanner

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | Banner..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\Banner.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\Banner.dll
  SetOutPath $INSTDIR\Docs\Banner
  File ..\..\Docs\Banner\Readme.txt
  SetOutPath $INSTDIR\Examples\Banner
  File ..\..\Examples\Banner\Example.nsi
SectionEnd

Section "Language DLL plugin" SecPluginsLangDLL

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | Language DLL..."
  SetDetailsPrint listonly

  SectionIn 2 3
  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\LangDLL.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\LangDLL.dll
SectionEnd

Section "nsExec plugin" SecPluginsnsExec

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | nsExec..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\nsExec.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\nsExec.dll
  SetOutPath $INSTDIR\Docs\nsExec
  File ..\..\Docs\nsExec\nsExec.txt
  SetOutPath $INSTDIR\Examples\nsExec
  File ..\..\Examples\nsExec\test.nsi
SectionEnd

Section "Splash plugin" SecPluginsSplash

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | Splash..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\splash.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\splash.dll
  SetOutPath $INSTDIR\Docs\Splash
  File ..\..\Docs\Splash\splash.txt
  SetOutPath $INSTDIR\Examples\Splash
  File ..\..\Examples\Splash\Example.nsi
SectionEnd

Section "AdvSplash plugin" SecPluginsSplashT

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | AdvSplash..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\advsplash.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\advsplash.dll
  SetOutPath $INSTDIR\Docs\AdvSplash
  File ..\..\Docs\AdvSplash\advsplash.txt
  SetOutPath $INSTDIR\Examples\AdvSplash
  File ..\..\Examples\AdvSplash\Example.nsi
SectionEnd

Section "BgImage plugin" SecPluginsBgImage

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | BgImage..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\BgImage.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\BgImage.dll
  SetOutPath $INSTDIR\Docs\BgImage
  File ..\..\Docs\BgImage\BgImage.txt
  SetOutPath $INSTDIR\Examples\BgImage
  File ..\..\Examples\BgImage\Example.nsi
SectionEnd

Section "InstallOptions plugin" SecPluginsIO

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | InstallOptions..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\InstallOptions.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\InstallOptions.dll
  SetOutPath $INSTDIR\Docs\InstallOptions
  File ..\..\Docs\InstallOptions\Readme.html
  File ..\..\Docs\InstallOptions\Changelog.txt
  SetOutPath $INSTDIR\Examples\InstallOptions
  File ..\..\Examples\InstallOptions\test.ini
  File ..\..\Examples\InstallOptions\test.nsi
  File ..\..\Examples\InstallOptions\testimgs.ini
  File ..\..\Examples\InstallOptions\testimgs.nsi
  File ..\..\Examples\InstallOptions\testlink.ini
  File ..\..\Examples\InstallOptions\testlink.nsi
  File ..\..\Examples\InstallOptions\testnotify.ini
  File ..\..\Examples\InstallOptions\testnotify.nsi
SectionEnd

Section "Math plugin" SecPluginsMath

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | Math..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\Math.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\Math.dll
  SetOutPath $INSTDIR\Docs\Math
  File ..\..\Docs\Math\Math.txt
  SetOutPath $INSTDIR\Examples\Math
  File ..\..\Examples\Math\math.nsi
  File ..\..\Examples\Math\mathtest.txt
  File ..\..\Examples\Math\mathtest.nsi
  File ..\..\Examples\Math\mathtest.ini

SectionEnd

Section "NSISdl plugin" SecPluginsNSISDL

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | NSISdl..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\nsisdl.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\nsisdl.dll
  SetOutPath $INSTDIR\Docs\NSISdl
  File ..\..\Docs\NSISdl\ReadMe.txt
  File ..\..\Docs\NSISdl\License.txt
SectionEnd

Section "System plugin" SecPluginsSystem

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | System..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\System.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\System.dll
  SetOutPath $INSTDIR\Docs\System
  File ..\..\Docs\System\System.html
  File ..\..\Docs\System\WhatsNew.txt
  SetOutPath $INSTDIR\Examples\System
  File ..\..\Examples\System\Resource.dll
  File ..\..\Examples\System\SysFunc.nsh
  File ..\..\Examples\System\System.nsh
  File ..\..\Examples\System\System.nsi
SectionEnd

Section "nsDialogs plugin" SecPluginsDialogs

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | nsDialogs..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\nsDialogs.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\nsDialogs.dll
  SetOutPath $INSTDIR\Examples\nsDialogs
  File ..\..\Examples\nsDialogs\example.nsi
  File ..\..\Examples\nsDialogs\InstallOptions.nsi
  File ..\..\Examples\nsDialogs\timer.nsi
  File ..\..\Examples\nsDialogs\welcome.nsi
  SetOutPath $INSTDIR\Include
  File ..\..\Include\nsDialogs.nsh
  SetOutPath $INSTDIR\Docs\nsDialogs
  File ..\..\Docs\nsDialogs\Readme.html

SectionEnd

Section "StartMenu plugin" SecPluginsStartMenu

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | StartMenu..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\StartMenu.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\StartMenu.dll
  SetOutPath $INSTDIR\Docs\StartMenu
  File ..\..\Docs\StartMenu\Readme.txt
  SetOutPath $INSTDIR\Examples\StartMenu
  File ..\..\Examples\StartMenu\Example.nsi
SectionEnd

Section "UserInfo plugin" SecPluginsUserInfo

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | UserInfo..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\UserInfo.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\UserInfo.dll
  SetOutPath $INSTDIR\Examples\UserInfo
  File ..\..\Examples\UserInfo\UserInfo.nsi
SectionEnd

Section "Dialer plugin" SecPluginsDialer

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | Dialer..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\Dialer.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\Dialer.dll
  SetOutPath $INSTDIR\Docs\Dialer
  File ..\..\Docs\Dialer\Dialer.txt
SectionEnd

Section "VPatch plugin" SecPluginsVPatch

  SetDetailsPrint textonly
  DetailPrint "Installing Plug-ins | VPatch..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath $INSTDIR\Plugins\x86-ansi
  File ..\..\Plugins\x86-ansi\VPatch.dll
  SetOutPath $INSTDIR\Plugins\x86-unicode
  File ..\..\Plugins\x86-unicode\VPatch.dll
  SetOutPath $INSTDIR\Examples\VPatch
  File ..\..\Examples\VPatch\example.nsi
  File ..\..\Examples\VPatch\oldfile.txt
  File ..\..\Examples\VPatch\newfile.txt
  File ..\..\Examples\VPatch\patch.pat
  SetOutPath $INSTDIR\Docs\VPatch
  File ..\..\Docs\VPatch\Readme.html
  SetOutPath $INSTDIR\Bin
  File ..\..\Bin\GenPat.exe
  SetOutPath $INSTDIR\Include
  File ..\..\Include\VPatchLib.nsh
SectionEnd

SectionGroupEnd

Section -post

  ; When Modern UI is installed:
  ; * Always install the English language file
  ; * Always install default icons / bitmaps

  !insertmacro SectionFlagIsSet ${SecInterfacesModernUI} ${SF_SELECTED} mui nomui

    mui:

    SetDetailsPrint textonly
    DetailPrint "Configuring Modern UI..."
    SetDetailsPrint listonly

    !insertmacro SectionFlagIsSet ${SecLangFiles} ${SF_SELECTED} langfiles nolangfiles

      nolangfiles:

      SetOutPath "$INSTDIR\Contrib\Language files"
      File "..\..\Contrib\Language files\English.nlf"
      SetOutPath "$INSTDIR\Contrib\Language files"
      File "..\..\Contrib\Language files\English.nsh"

    langfiles:

    !insertmacro SectionFlagIsSet ${SecGraphics} ${SF_SELECTED} graphics nographics

      nographics:

      SetOutPath $INSTDIR\Contrib\Graphics
      SetOutPath $INSTDIR\Contrib\Graphics\Checks
      File "..\..\Contrib\Graphics\Checks\modern.bmp"
      SetOutPath $INSTDIR\Contrib\Graphics\Icons
      File "..\..\Contrib\Graphics\Icons\modern-install.ico"
      File "..\..\Contrib\Graphics\Icons\modern-uninstall.ico"
      SetOutPath $INSTDIR\Contrib\Graphics\Header
      File "..\..\Contrib\Graphics\Header\nsis.bmp"
      SetOutPath $INSTDIR\Contrib\Graphics\Wizard
      File "..\..\Contrib\Graphics\Wizard\win.bmp"

    graphics:

  nomui:

  SetDetailsPrint textonly
  DetailPrint "Creating Registry Keys..."
  SetDetailsPrint listonly

  SetOutPath $INSTDIR

  WriteRegStr HKLM "Software\NSIS" "" $INSTDIR
!ifdef VER_MAJOR & VER_MINOR & VER_REVISION & VER_BUILD
  WriteRegDword HKLM "Software\NSIS" "VersionMajor" "${VER_MAJOR}"
  WriteRegDword HKLM "Software\NSIS" "VersionMinor" "${VER_MINOR}"
  WriteRegDword HKLM "Software\NSIS" "VersionRevision" "${VER_REVISION}"
  WriteRegDword HKLM "Software\NSIS" "VersionBuild" "${VER_BUILD}"
!endif

  WriteRegExpandStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "UninstallString" "$INSTDIR\uninst-nsis.exe"
  ;WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "QuietUninstallString" '"$INSTDIR\uninst-nsis.exe" /S' ; Ideally WACK would use this
  WriteRegExpandStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "InstallLocation" "$INSTDIR"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "DisplayName" "Nullsoft Install System"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "DisplayIcon" "$INSTDIR\NSIS.exe,0"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "DisplayVersion" "${VERSION}"
!ifdef VER_MAJOR & VER_MINOR & VER_REVISION
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "VersionMajor" "${VER_MAJOR}" ; Required by WACK
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "VersionMinor" "${VER_MINOR}.${VER_REVISION}" ; Required by WACK
!endif
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "Publisher" "Nullsoft and Contributors" ; Required by WACK
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "URLInfoAbout" "http://nsis.sourceforge.net/"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "HelpLink" "http://nsis.sourceforge.net/Support"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "NoModify" "1"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS" "NoRepair" "1"

  SetOutPath "$INSTDIR"
  File ..\..\uninst-nsis.exe

  SetDetailsPrint both

SectionEnd

SectionGroupEnd

;--------------------------------
; UMUI Sections

SectionGroup /e "NSIS Ultra-Modern User Interface ${UMUI_VERSION}" SecUMUIG

Section "NSIS Ultra-Modern User Interface Core Files (required)" SecUMUI

  SetDetailsPrint textonly
  DetailPrint "Installing Ultra-Modern UI..."
  SetDetailsPrint listonly

  SectionIn RO

  SectionIn 1 2 3

  SetOutPath "$INSTDIR\"
  !if ! /FileExists "..\..\NSISUMUI.exe"
    !makensis '-v2 "NSISUMUIMenu.nsi" "-XOutFile ..\..\NSISUMUI.exe"' = 0
  !endif
  File ..\..\NSISUMUI.exe

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\"
  File "..\..\Contrib\UltraModernUI\UMUI.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Ini"
  File "..\..\Contrib\UltraModernUI\Ini\*.ini"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Language files\"
  File "..\..\Contrib\UltraModernUI\Language files\*.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File /r "..\..\Contrib\UltraModernUI\BGSkins\"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\blue.nsh"
  File "..\..\Contrib\UltraModernUI\Skins\blue2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\blue\"
  File "..\..\Contrib\UltraModernUI\Skins\blue\*.bmp"

  SetOutPath "$INSTDIR\Docs\UltraModernUI\"
  File "..\..\Docs\UltraModernUI\*.*"
  SetOutPath "$INSTDIR\Docs\UltraModernUI\images\"
  File "..\..\Docs\UltraModernUI\images\*.png"
  File "..\..\Docs\UltraModernUI\images\*.gif"

  SetOutPath "$INSTDIR\Examples\UltraModernUI\"
  File "*.nsi"
  File "*.ini"
  File "*.txt"

  SetOutPath "$INSTDIR\Contrib\UIs\UltraModernUI\"
  File "..\..\Contrib\UIs\UltraModernUI\*.exe"

  SetOutPath "$INSTDIR\Contrib\Graphics\UltraModernUI\"
  File "..\..\Contrib\Graphics\UltraModernUI\*.*"

  SetOutPath "$INSTDIR\Include\"
  File "..\..\Include\UMUI.nsh"
  File "..\..\Include\MUIEx.nsh"


  ;CreateShortCuts
  ; only if all user is selected
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED ALL
    SetShellVarContext all
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ; only if current user is selected
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED CURRENT
    SetShellVarContext current
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF 

  SetOutPath "$INSTDIR\"
  CreateShortCut "$SMPROGRAMS\NSIS.lnk" "$INSTDIR\NSISUMUI.exe"

  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED DESKTOP
    CreateShortCut "$DESKTOP\NSIS.lnk" "$INSTDIR\NSISUMUI.exe"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF


  ;Create uninstaller
  WriteUninstaller "$INSTDIR\UninstallUMUI.exe"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "DisplayName" "$(^Name)"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "DisplayIcon" "$INSTDIR\UninstallUMUI.exe,0"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "UninstallString" "$INSTDIR\UninstallUMUI.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "Publisher" "SuperPat"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "HelpLink" "http://ultramodernui.sourceforge.net/"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "URLInfoAbout" "http://ultramodernui.sourceforge.net/"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "URLUpdateInfo" "http://ultramodernui.sourceforge.net/"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "ModifyPath" '"$INSTDIR\UninstallUMUI.exe" /modify'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "DisplayVersion" "${UMUI_VERSION}"

SectionEnd

Section "Skins for Ultra-Modern UI" SecSkins

  SetDetailsPrint textonly
  DetailPrint "Installing Ultra-Modern UI | Skins..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\brown.nsh"
  File "..\..\Contrib\UltraModernUI\Skins\brown2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\brown\"
  File "..\..\Contrib\UltraModernUI\Skins\brown\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\darkgreen.nsh"
  File "..\..\Contrib\UltraModernUI\Skins\darkgreen2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\darkgreen\"
  File "..\..\Contrib\UltraModernUI\Skins\darkgreen\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\gray.nsh"
  File "..\..\Contrib\UltraModernUI\Skins\gray2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\gray\"
  File "..\..\Contrib\UltraModernUI\Skins\gray\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\green.nsh"
  File "..\..\Contrib\UltraModernUI\Skins\green2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\green\"
  File "..\..\Contrib\UltraModernUI\Skins\green\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\purple.nsh"
  File "..\..\Contrib\UltraModernUI\Skins\purple2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\purple\"
  File "..\..\Contrib\UltraModernUI\Skins\purple\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\red.nsh"
  File "..\..\Contrib\UltraModernUI\Skins\red2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\red\"
  File "..\..\Contrib\UltraModernUI\Skins\red\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftBlue.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\SoftBlue\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftBlue\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftBrown.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\SoftBrown\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftBrown\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftGray.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\SoftGray\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftGray\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftGreen.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\SoftGreen\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftGreen\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftPurple.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\SoftPurple\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftPurple\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftRed.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\Skins\SoftRed\"
  File "..\..\Contrib\UltraModernUI\Skins\SoftRed\*.bmp"

SectionEnd

Section "BackGround Skins for Ultra-Modern UI" SecBGSkins

  SetDetailsPrint textonly
  DetailPrint "Installing Ultra-Modern UI | BackGround Skins..."
  SetDetailsPrint listonly

  SectionIn 2 3

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\blue.nsh"
  File "..\..\Contrib\UltraModernUI\BGSkins\blue2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\blue\"
  File "..\..\Contrib\UltraModernUI\BGSkins\blue\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\brown.nsh"
  File "..\..\Contrib\UltraModernUI\BGSkins\brown2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\brown\"
  File "..\..\Contrib\UltraModernUI\BGSkins\brown\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\darkgreen.nsh"
  File "..\..\Contrib\UltraModernUI\BGSkins\darkgreen2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\darkgreen\"
  File "..\..\Contrib\UltraModernUI\BGSkins\darkgreen\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\gray.nsh"
  File "..\..\Contrib\UltraModernUI\BGSkins\gray2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\gray\"
  File "..\..\Contrib\UltraModernUI\BGSkins\gray\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\green.nsh"
  File "..\..\Contrib\UltraModernUI\BGSkins\green2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\green\"
  File "..\..\Contrib\UltraModernUI\BGSkins\green\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\purple.nsh"
  File "..\..\Contrib\UltraModernUI\BGSkins\purple2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\purple\"
  File "..\..\Contrib\UltraModernUI\BGSkins\purple\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\red.nsh"
  File "..\..\Contrib\UltraModernUI\BGSkins\red2.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\red\"
  File "..\..\Contrib\UltraModernUI\BGSkins\red\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftBlue.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftBlue\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftBlue\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftBrown.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftBrown\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftBrown\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftGray.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftGray\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftGray\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftGreen.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftGreen\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftGreen\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftPurple.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftPurple\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftPurple\*.bmp"

  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftRed.nsh"
  SetOutPath "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftRed\"
  File "..\..\Contrib\UltraModernUI\BGSkins\SoftRed\*.bmp"

SectionEnd


SectionGroup /e "Plugins" SecGroupPlugins

  Section "SkinnedControls plugin version 1.4" SecSkinnedControls

    SetDetailsPrint textonly
    DetailPrint "Installing Ultra-Modern UI | Plugins | SkinnedControls..."
    SetDetailsPrint listonly

    SectionIn RO

    SectionIn 1 2 3

    SetOutPath $INSTDIR\Plugins\x86-ansi
    File "..\..\Plugins\x86-ansi\SkinnedControls.dll"

    SetOutPath $INSTDIR\Plugins\x86-unicode
    File "..\..\Plugins\x86-unicode\SkinnedControls.dll"

    SetOutPath $INSTDIR\Examples\SkinnedControls
    File "..\SkinnedControls\*.nsi"

    SetOutPath $INSTDIR\Contrib\SkinnedControls\skins
    File "..\..\Contrib\SkinnedControls\skins\*.bmp"

    SetOutPath $INSTDIR\Contrib\UIs
    File "..\..\Contrib\UIs\modern_sb.exe"
    File "..\..\Contrib\UIs\default_sb.exe"

    SetOutPath $INSTDIR\Docs\SkinnedControls
    File "..\..\Docs\SkinnedControls\*.*"
    SetOutPath "$INSTDIR\Docs\SkinnedControls\images\"
    File "..\..\Docs\SkinnedControls\images\*.png"
    File "..\..\Docs\SkinnedControls\images\*.gif"

  SectionEnd

  Section "SkinnedControls plugin Sources Code" SecSkinnedControlsSources

    SetDetailsPrint textonly
    DetailPrint "Installing Ultra-Modern UI | Plugins | SkinnedControls Soucres Code..."
    SetDetailsPrint listonly

    SectionIn 3

    SetOutPath $INSTDIR\Contrib\SkinnedControls
    File "..\..\Contrib\SkinnedControls\*.h"
    File "..\..\Contrib\SkinnedControls\*.c"
    File "..\..\Contrib\SkinnedControls\SkinnedControls.sln"
    File "..\..\Contrib\SkinnedControls\SkinnedControls.vcproj"
    SetOutPath $INSTDIR\Contrib\SkinnedControls\coolsb
    File "..\..\Contrib\SkinnedControls\coolsb\*.h"
    File "..\..\Contrib\SkinnedControls\coolsb\*.c"
    File "..\..\Contrib\SkinnedControls\coolsb\detours.lib"
    File "..\..\Contrib\SkinnedControls\coolsb\coolsb.vcproj"

  SectionEnd


  Section "InstallOptionsEx plugin version 2.4.5 beta 3" SecInstallOptionsEx

    SetDetailsPrint textonly
    DetailPrint "Installing Ultra-Modern UI | Plugins | InstallOptionsEx..."
    SetDetailsPrint listonly

    SectionIn 2 3

    SetOutPath $INSTDIR\Plugins\x86-ansi
    File "..\..\Plugins\x86-ansi\InstallOptionsEx.dll"

    SetOutPath $INSTDIR\Plugins\x86-unicode
    File "..\..\Plugins\x86-unicode\InstallOptionsEx.dll"

    SetOutPath $INSTDIR\Examples\InstallOptionsEx
    File "..\InstallOptionsEx\*.nsi"
    File "..\InstallOptionsEx\*.ini"

    SetOutPath $INSTDIR\Docs\InstallOptionsEx
    File "..\..\Docs\InstallOptionsEx\*.*"

  SectionEnd

  Section "InstallOptionsEx plugin Sources Code" SecInstallOptionsExSources

    SetDetailsPrint textonly
    DetailPrint "Installing Ultra-Modern UI | Plugins | InstallOptionsEx Soucres Code..."
    SetDetailsPrint listonly

    SectionIn 3

  SetOutPath $INSTDIR\Contrib\InstallOptionsEx
  File "..\..\Contrib\InstallOptionsEx\*.cpp"
  File "..\..\Contrib\InstallOptionsEx\*.c"
  File "..\..\Contrib\InstallOptionsEx\*.h"
  File "..\..\Contrib\InstallOptionsEx\ioptdll.rc"
  File "..\..\Contrib\InstallOptionsEx\io.sln"
  File "..\..\Contrib\InstallOptionsEx\io.vcproj"
  SetOutPath $INSTDIR\Contrib\InstallOptionsEx\Controls
  File "..\..\Contrib\InstallOptionsEx\Controls\*.h"

  SectionEnd


  Section "nsArray plugin version 1.1.1.7" SecnsArray

    SetDetailsPrint textonly
    DetailPrint "Installing Ultra-Modern UI | Plugins | nsArray..."
    SetDetailsPrint listonly

    SectionIn 2 3

    SetOutPath $INSTDIR\Plugins\x86-ansi   
    File "..\..\Plugins\x86-ansi\nsArray.dll"

    SetOutPath $INSTDIR\Plugins\x86-unicode
    File "..\..\Plugins\x86-unicode\nsArray.dll"

    SetOutPath $INSTDIR\Include
    File "..\..\Include\nsArray.nsh"

    SetOutPath $INSTDIR\Docs\nsArray
    File "..\..\Docs\nsArray\*.*"

    SetOutPath $INSTDIR\Examples\nsArray
    File "..\nsArray\*.nsi"

  SectionEnd

  Section "nsArray plugin Sources Code" SecnsArraySources

    SetDetailsPrint textonly
    DetailPrint "Installing Ultra-Modern UI | Plugins | nsArray Soucres Code..."
    SetDetailsPrint listonly

    SectionIn 3

    SetOutPath $INSTDIR\Contrib\nsArray
    File "..\..\Contrib\nsArray\*.*"

  SectionEnd

SectionGroupEnd

SectionGroupEnd


;--------------------------------
;Uninstall Section(s)

!macro removeUMUIfiles

  ;uninstall SkinnedControls Plugin
  Delete "$INSTDIR\Plugins\SkinnedControls.dll"
  Delete "$INSTDIR\Plugins\x86-ansi\SkinnedControls.dll"
  Delete "$INSTDIR\Plugins\x86-unicode\SkinnedControls.dll"
  Delete "$INSTDIR\Contrib\UIs\modern_sb.exe"
  Delete "$INSTDIR\Contrib\UIs\default_sb.exe"
  RMDir /r "$INSTDIR\Contrib\SkinnedControls"
  Delete "$INSTDIR\Docs\SkinnedControls\*.*"
  RMDir "$INSTDIR\Docs\SkinnedControls"
  Delete "$INSTDIR\Examples\SkinnedControls\*.nsi"
  RMDir "$INSTDIR\Examples\SkinnedControls"

  ;uninstall InstallOptionsEx Plugin
  Delete "$INSTDIR\Plugins\InstallOptionsEx*.dll"
  Delete "$INSTDIR\Plugins\x86-ansi\InstallOptionsEx.dll"
  Delete "$INSTDIR\Plugins\x86-unicode\InstallOptionsEx.dll"
  RMDir /r "$INSTDIR\Contrib\InstallOptionsEx"
  Delete "$INSTDIR\Docs\InstallOptionsEx\*.*"
  RMDir "$INSTDIR\Docs\InstallOptionsEx"
  Delete "$INSTDIR\Examples\InstallOptionsEx\*.*"
  RMDir "$INSTDIR\Examples\InstallOptionsEx"

  ;uninstall nsArray Plugin
  Delete "$INSTDIR\Plugins\x86-ansi\nsArray.dll"
  Delete "$INSTDIR\Plugins\x86-unicode\nsArray.dll"
  Delete "$INSTDIR\Include\nsArray.nsh"
  Delete "$INSTDIR\Contrib\nsArray\*.*"
  RMDir "$INSTDIR\Contrib\nsArray"
  Delete "$INSTDIR\Docs\nsArray\*.*"
  RMDir "$INSTDIR\Docs\nsArray"
  Delete "$INSTDIR\Examples\nsArray\*.nsi"
  RMDir "$INSTDIR\Examples\nsArray"

  ;uninstall NSISArray Plugin
  IfFileExists $INSTDIR\Plugins\NSISArray.dll "" noNSISArray
    Delete "$INSTDIR\Plugins\NSISArray*.dll"
    Delete "$INSTDIR\Include\NSISArray.nsh"
    Delete "$INSTDIR\Contrib\NSISArray\*.*"
    RMDir "$INSTDIR\Contrib\NSISArray"
    Delete "$INSTDIR\Docs\NSISArray\*.*"
    RMDir "$INSTDIR\Docs\NSISArray"
    Delete "$INSTDIR\Examples\NSISArray\*.nsi"
    RMDir "$INSTDIR\Examples\NSISArray"
  noNSISArray:

  Delete "$INSTDIR\Include\UMUI.nsh"
  Delete "$INSTDIR\Include\MUIEx.nsh"
  RMDir "$INSTDIR\Include"

  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\AdditionalTasks.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\AlternateWelcomeFinishAbort.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\AlternateWelcomeFinishAbortImage.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\AlternativeStartMenu.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\AlternativeStartMenuApplication.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\Confirm.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\Information.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\ioSpecial.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\Maintenance.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\MaintenanceSetupType.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\MaintenanceUpdateSetupType.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\SerialNumber.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\WelcomeFinishAbort.ini"
  Delete "$INSTDIR\Contrib\UltraModernUI\Ini\WelcomeFinishAbortImage.ini"
  RMDir "$INSTDIR\Contrib\UltraModernUI\Ini"

  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\blue"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\brown"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\darkgreen"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\gray"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\green"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\purple"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\red"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftBlue"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftBrown"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftGray"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftGreen"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftPurple"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftRed"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\blue2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\blue.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\brown2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\brown.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\darkgreen2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\darkgreen.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\gray2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\gray.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\green2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\green.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\purple2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\purple.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\red2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\red.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftBlue.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftBrown.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftGray.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftGreen.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftPurple.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\BGSkins\SoftRed.nsh"
  RMDir "$INSTDIR\Contrib\UltraModernUI\BGSkins"

  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\blue"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\brown"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\darkgreen"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\gray"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\green"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\purple"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\red"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\SoftBlue"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\SoftBrown"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\SoftGray"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\SoftGreen"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\SoftPurple"
  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Skins\SoftRed"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\blue2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\blue.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\brown2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\brown.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\darkgreen2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\darkgreen.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\gray2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\gray.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\green2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\green.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\purple2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\purple.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\red2.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\red.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\SoftBlue.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\SoftBrown.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\SoftGray.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\SoftGreen.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\SoftPurple.nsh"
  Delete "$INSTDIR\Contrib\UltraModernUI\Skins\SoftRed.nsh"
  RMDir "$INSTDIR\Contrib\UltraModernUI\Skins"

  RMDir /r "$INSTDIR\Contrib\UltraModernUI\Language files"

  Delete "$INSTDIR\Contrib\UltraModernUI\UMUI.nsh"
  RMDir "$INSTDIR\Contrib\UltraModernUI\"
  RMDir "$INSTDIR\Contrib"

  RMDir /r "$INSTDIR\Docs\UltraModernUI\"
  RMDir "$INSTDIR\Docs"

  RMDir /r "$INSTDIR\Examples\UltraModernUI\"
  RMDir "$INSTDIR\Examples"

  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\modern_bigdesc.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\modern_headerbgimage.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\modern_sb.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\UltraModern.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\UltraModern_bigdesc.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\UltraModern_nodesc.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\UltraModern_noleftimage.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\UltraModern_sb.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\UltraModern_small.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\UltraModern_small_sb.exe"
  Delete "$INSTDIR\Contrib\UIs\UltraModernUI\UltraModern_smalldesc.exe"
  RMDir "$INSTDIR\Contrib\UIs\UltraModernUI\"
  RMDir "$INSTDIR\Contrib\UIs"

  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Complete.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\CompleteEx.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Continue.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\ContinueEx.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Custom.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\CustomEx.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\HeaderBG.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Minimal.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\MinimalEx.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Modify.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\ModifyEx.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Remove.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\RemoveEx.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Repair.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\RepairEx.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Standard.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\StandardEx.bmp"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Icon2.ico"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\Icon.ico"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\install.ico"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\installEx.ico"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\UnIcon2.ico"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\UnIcon.ico"
  Delete "$INSTDIR\Contrib\Graphics\UltraModernUI\uninstall.ico"
  RMDir "$INSTDIR\Contrib\Graphics\UltraModernUI\"
  RMDir "$INSTDIR\Contrib\Graphics"

  Delete "$INSTDIR\NSISUMUI.exe"
  Delete "$INSTDIR\UninstallUMUI.exe"

!macroend


Section Uninstall

  SetDetailsPrint textonly
  DetailPrint "Uninstalling NSIS and Ultra-Modern UI..."
  SetDetailsPrint listonly

  IfFileExists $INSTDIR\makensis.exe nsis_installed
    MessageBox MB_YESNO "It does not appear that NSIS is installed in the directory '$INSTDIR'.$\r$\nContinue anyway (not recommended)?" IDYES nsis_installed
    Abort "Uninstall aborted by user"
  nsis_installed:

  !insertmacro removeUMUIfiles

  Delete "$SMPROGRAMS\NSIS.lnk"
  Delete "$DESKTOP\NSIS.lnk"

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}"

  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_SHELLVARCONTEXT_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_LANGUAGE_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "UMUI_SetupType" ;"${UMUI_SETUPTYPEPAGE_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "UMUI_InstType" ;"${UMUI_COMPONENTSPAGE_INSTTYPE_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "UMUI_Components" ;"${UMUI_COMPONENTSPAGE_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_ADDITIONALTASKS_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_VERSION_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_VERBUILD_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_INSTALLERFULLPATH_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_UNINSTALLPATH_REGISTRY_VALUENAME}"
  DeleteRegKey /ifempty ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}"


  SetDetailsPrint textonly
  DetailPrint "Uninstalling NSIS Development Shell Extensions..."
  SetDetailsPrint listonly

  SetDetailsPrint textonly
  DetailPrint "Deleting Registry Keys..."
  SetDetailsPrint listonly

  ReadRegStr $R0 HKCR ".nsi" ""
  StrCmp $R0 "NSIS.Script" 0 +2
    DeleteRegKey HKCR ".nsi"

  ReadRegStr $R0 HKCR ".nsh" ""
  StrCmp $R0 "NSIS.Header" 0 +2
    DeleteRegKey HKCR ".nsh"

  DeleteRegKey HKCR "NSIS.Script"
  DeleteRegKey HKCR "NSIS.Header"

  System::Call 'Shell32::SHChangeNotify(i ${SHCNE_ASSOCCHANGED}, i ${SHCNF_IDLIST}, i 0, i 0)'

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS"
  DeleteRegKey HKLM "Software\NSIS"

  SetDetailsPrint textonly
  DetailPrint "Deleting Files..."
  SetDetailsPrint listonly

  Delete $INSTDIR\makensis.exe
  Delete $INSTDIR\makensisw.exe
  Delete $INSTDIR\NSIS.exe
  Delete $INSTDIR\NSIS.exe.manifest
  Delete $INSTDIR\license.txt
  Delete $INSTDIR\COPYING
  Delete $INSTDIR\uninst-nsis.exe
  Delete $INSTDIR\nsisconf.nsi
  Delete $INSTDIR\nsisconf.nsh
  Delete $INSTDIR\NSIS.chm
  RMDir /r $INSTDIR\Bin
  RMDir /r $INSTDIR\Contrib
  RMDir /r $INSTDIR\Docs
  RMDir /r $INSTDIR\Examples
  RMDir /r $INSTDIR\Include
  RMDir /r $INSTDIR\Menu
  RMDir /r $INSTDIR\Plugins
  RMDir /r $INSTDIR\Stubs
  RMDir /r $INSTDIR

  SetDetailsPrint both

SectionEnd


;--------------------------------
;Installer Sections Declaration dans Description

!insertmacro UMUI_DECLARECOMPONENTS_BEGIN
  !insertmacro UMUI_COMPONENT SecNSIS
  !insertmacro UMUI_COMPONENT SecCore
  !insertmacro UMUI_COMPONENT SecExample
  !insertmacro UMUI_COMPONENT SecInterfaces
  !insertmacro UMUI_COMPONENT SecInterfacesModernUI
  !insertmacro UMUI_COMPONENT SecInterfacesDefaultUI
  !insertmacro UMUI_COMPONENT SecInterfacesTinyUI
  !insertmacro UMUI_COMPONENT SecTools
  !insertmacro UMUI_COMPONENT SecToolsZ2E
  !insertmacro UMUI_COMPONENT SecGraphics
  !insertmacro UMUI_COMPONENT SecLangFiles
  !insertmacro UMUI_COMPONENT SecPluginsPlugins
  !insertmacro UMUI_COMPONENT SecPluginsBanner
  !insertmacro UMUI_COMPONENT SecPluginsLangDLL
  !insertmacro UMUI_COMPONENT SecPluginsnsExec
  !insertmacro UMUI_COMPONENT SecPluginsSplash
  !insertmacro UMUI_COMPONENT SecPluginsSplashT
  !insertmacro UMUI_COMPONENT SecPluginsSystem
  !insertmacro UMUI_COMPONENT SecPluginsMath
  !insertmacro UMUI_COMPONENT SecPluginsDialer
  !insertmacro UMUI_COMPONENT SecPluginsIO
  !insertmacro UMUI_COMPONENT SecPluginsDialogs
  !insertmacro UMUI_COMPONENT SecPluginsStartMenu
  !insertmacro UMUI_COMPONENT SecPluginsBgImage
  !insertmacro UMUI_COMPONENT SecPluginsUserInfo
  !insertmacro UMUI_COMPONENT SecPluginsNSISDL
  !insertmacro UMUI_COMPONENT SecPluginsVPatch

  !insertmacro UMUI_COMPONENT SecUMUI
  !insertmacro UMUI_COMPONENT SecSkins
  !insertmacro UMUI_COMPONENT SecBGSkins
  !insertmacro UMUI_COMPONENT SecSkinnedControls
  !insertmacro UMUI_COMPONENT SecSkinnedControlsSources
  !insertmacro UMUI_COMPONENT SecInstallOptionsEx
  !insertmacro UMUI_COMPONENT SecInstallOptionsExSources
  !insertmacro UMUI_COMPONENT SecnsArray
  !insertmacro UMUI_COMPONENT SecnsArraySources
!insertmacro UMUI_DECLARECOMPONENTS_END

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecNSIS} "The NSIS package version ${VER_MAJOR}.${VER_MINOR}"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecCore} "The core files required to use NSIS (compiler etc.)"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecExample} "Example installation scripts that show you how to use NSIS"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecInterfaces} "User interface designs that can be used to change the installer look and feel"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecInterfacesModernUI} "A modern user interface like the wizards of recent Windows versions"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecInterfacesDefaultUI} "The default NSIS user interface which you can customize to make your own UI"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecInterfacesTinyUI} "A tiny version of the default user interface"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecTools} "Tools that help you with NSIS development"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecToolsZ2E} "A utility that converts a ZIP file to a NSIS installer"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecGraphics} "Icons, checkbox images and other graphics"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecLangFiles} "Language files used to support multiple languages in an installer"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsPlugins} "Useful plugins that extend NSIS's functionality"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsBanner} "Plugin that lets you show a banner before installation starts"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsLangDLL} "Plugin that lets you add a language select dialog to your installer"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsnsExec} "Plugin that executes console programs and prints its output in the NSIS log window or hides it"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsSplash} "Splash screen add-on that lets you add a splash screen to an installer"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsSplashT} "Splash screen add-on with transparency support that lets you add a splash screen to an installer"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsSystem} "Plugin that lets you call Win32 API or external DLLs"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsMath} "Plugin that lets you evaluate complicated mathematical expressions"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsDialer} "Plugin that provides internet connection functions"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsIO} "Plugin that lets you add custom pages to an installer"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsDialogs} "Plugin that lets you add custom pages to an installer"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsStartMenu} "Plugin that lets the user select the start menu folder"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsBgImage} "Plugin that lets you show a persistent background image plugin and play sounds"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsUserInfo} "Plugin that that gives you the user name and the user account type"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsNSISDL} "Plugin that lets you create a web based installer"
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPluginsVPatch} "Plugin that lets you create patches to upgrade older files"

  !insertmacro MUI_DESCRIPTION_TEXT ${SecUMUI} "The Utra-Modern User Interface for NSIS."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecSkins} "A lot of skins for the Utra-Modern User Interface."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecBGSkins} "A lot of background skins for the Utra-Modern User Interface."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecGroupPlugins} "Install very useful NSIS plugins used by Ultra-Modern UI to extend the possibilities of NSIS."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecSkinnedControls} "This NSIS plugin, writing by SuperPat, allow you to skin all buttons and scrollbars of your installer.$\n$\rIt's used by default with the UltraModern style."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecSkinnedControlsSources} "The Sources code of the SkinnedControls plugin."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecInstallOptionsEx} "This NSIS plugin, writing by deguix and SuperPat is an expanded version of the original InstallOptions plugin containing a lot of new features.$\nThis plugin is supported natively by Ultra-Modern UI."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecInstallOptionsExSources} "The Sources code of the InstallOptionsEx plugin."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecnsArray} "This NSIS plugin, writing by Afrow UK, add the support of the array in NSIS. It comes with plenty of functions for managing your arrays.$\nThis plugin is used with the AlternativeStartMenu and MultiLanguages pages of Ultra-Modern UI."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecnsArraySources} "The Sources code of the nsArray plugin."
!insertmacro MUI_FUNCTION_DESCRIPTION_END


;--------------------------------
; Pages functions

Function addtasks_function
  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_LABEL "$(UMUI_TEXT_ADDITIONALTASKS_ADDITIONAL_ICONS)"
  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK DESKTOP 1 "$(UMUI_TEXT_ADDITIONALTASKS_CREATE_DESKTOP_ICON)"

  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_LINE

  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_LABEL "$(UMUI_TEXT_SHELL_VAR_CONTEXT)"

  UserInfo::GetAccountType
  Pop $R0
  StrCmp $R0 "Guest" 0 notLimited
    !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK_RADIO CURRENT 1 "$(UMUI_TEXT_SHELL_VAR_CONTEXT_ONLY_FOR_CURRENT_USER)"
    Goto endShellVarContext
  notLimited:
    !insertmacro UMUI_GETSHELLVARCONTEXT
    Pop $R0
    StrCmp $R0 "current" 0 allShellVarContext
      !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK_RADIO ALL 0 "$(UMUI_TEXT_SHELL_VAR_CONTEXT_FOR_ALL_USERS)"
      !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK_RADIO CURRENT 1 "$(UMUI_TEXT_SHELL_VAR_CONTEXT_ONLY_FOR_CURRENT_USER)"
      Goto endShellVarContext
    allShellVarContext:
      !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK_RADIO ALL 1 "$(UMUI_TEXT_SHELL_VAR_CONTEXT_FOR_ALL_USERS)"
      !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK_RADIO CURRENT 0 "$(UMUI_TEXT_SHELL_VAR_CONTEXT_ONLY_FOR_CURRENT_USER)"
  endShellVarContext:
  ClearErrors

FunctionEnd

!macro confirm_addline section

  SectionGetFlags ${Sec${section}} $1
  IntOp $1 $1 & ${SF_SELECTED}
  IntCmp $1 ${SF_SELECTED} 0 n${section} n${section}
    SectionGetText ${Sec${section}} $1
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "    - $1"
  n${section}:

!macroend

Function confirm_function

  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_INSTCONFIRM_TEXTBOX_DESTINATION_LOCATION)"
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $INSTDIR"
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE ""

  ;For the setuptype page
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_SETUPTYPE_TITLE):"
  !insertmacro UMUI_GET_CHOOSEN_SETUP_TYPE_TEXT
  Pop $R0
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $R0"
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE ""


  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_INSTCONFIRM_TEXTBOX_COMPNENTS)"

  !insertmacro confirm_addline NSIS
  !insertmacro confirm_addline Core
  !insertmacro confirm_addline Example
  !insertmacro confirm_addline Interfaces
  !insertmacro confirm_addline InterfacesModernUI
  !insertmacro confirm_addline InterfacesDefaultUI
  !insertmacro confirm_addline InterfacesTinyUI
  !insertmacro confirm_addline Tools
  !insertmacro confirm_addline ToolsZ2E
  !insertmacro confirm_addline Graphics
  !insertmacro confirm_addline LangFiles
  !insertmacro confirm_addline PluginsBanner
  !insertmacro confirm_addline PluginsLangDLL
  !insertmacro confirm_addline PluginsnsExec
  !insertmacro confirm_addline PluginsSplash
  !insertmacro confirm_addline PluginsSplashT
  !insertmacro confirm_addline PluginsSystem
  !insertmacro confirm_addline PluginsMath
  !insertmacro confirm_addline PluginsDialer
  !insertmacro confirm_addline PluginsIO
  !insertmacro confirm_addline PluginsDialogs
  !insertmacro confirm_addline PluginsStartMenu
  !insertmacro confirm_addline PluginsBgImage
  !insertmacro confirm_addline PluginsUserInfo
  !insertmacro confirm_addline PluginsNSISDL
  !insertmacro confirm_addline PluginsVPatch

  !insertmacro confirm_addline UMUI
  !insertmacro confirm_addline Skins
  !insertmacro confirm_addline BGSkins
  !insertmacro confirm_addline SkinnedControls
  !insertmacro confirm_addline SkinnedControlsSources
  !insertmacro confirm_addline InstallOptionsEx
  !insertmacro confirm_addline InstallOptionsExSources
  !insertmacro confirm_addline nsArray
  !insertmacro confirm_addline nsArraySources


  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE ""
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_ADDITIONALTASKS_TITLE):"
  ;Only if one at least of additional icon check is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED DESKTOP
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $(UMUI_TEXT_ADDITIONALTASKS_ADDITIONAL_ICONS)"
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "            $(UMUI_TEXT_ADDITIONALTASKS_CREATE_DESKTOP_ICON)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ;ShellVarContext
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $(UMUI_TEXT_SHELL_VAR_CONTEXT)"
  ; only if for all user radio is selected
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED ALL
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "            $(UMUI_TEXT_SHELL_VAR_CONTEXT_FOR_ALL_USERS)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ; only if for current user is selected
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED CURRENT
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "            $(UMUI_TEXT_SHELL_VAR_CONTEXT_ONLY_FOR_CURRENT_USER)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF 

FunctionEnd

Function preuninstall_function

  IfFileExists $INSTDIR\makensis.exe nsis_installed
    MessageBox MB_YESNO "It does not appear that NSIS is installed in the directory '$INSTDIR'.$\r$\nContinue anyway (not recommended)?" IDYES nsis_installed
    Abort "Install aborted by user"
  nsis_installed:

  SetDetailsPrint textonly
  DetailPrint "Uninstalling NSIS and Ultra-Modern UI..."
  SetDetailsPrint listonly

  !insertmacro removeUMUIfiles

  Delete "$SMPROGRAMS\NSIS.lnk"
  Delete "$DESKTOP\NSIS.lnk"

  ClearErrors
  ReadRegStr $R0 ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "UMUI_StartMenuFolder"
  IfErrors checkNext 0
    StrCpy $R1 $R0 "" -8 ; copy last height chars
    StrCmpS $R1 "\Contrib" 0 removeShortcuts
      StrCpy $R0 $R0 -8  ; remove \Contrib
      Goto removeShortcuts
  checkNext:
    ReadRegStr $R0 ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "StartMenuFolder"
    IfErrors noStartMenu removeShortcuts
    removeShortcuts:
      SetShellVarContext all   
      Delete "$SMPROGRAMS\$R0\Contrib\zip2exe (Create SFX).lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\AdvSplash Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Banner Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\BgImage Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\InstallOptions Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\MakeNSISw Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Math Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Modern UI 2 Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Modern UI Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\nsDialogs Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\nsExec Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\NSISdl Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Splash Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\StartMenu Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\System Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\VPatch Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\NSISArray Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\UltraModernUI Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\SkinnedControls Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\nsArray Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\InstallOptionsEx Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\InstallOptions Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Uninstall UltraModernUI.lnk"
      RMDir "$SMPROGRAMS\$R0\Contrib"
      Delete "$SMPROGRAMS\$R0\Uninstall NSIS.lnk"
      Delete "$SMPROGRAMS\$R0\NSIS Menu.lnk"
      Delete "$SMPROGRAMS\$R0\NSIS Examples Directory.lnk"
      Delete "$SMPROGRAMS\$R0\NSIS Documentation.lnk"
      Delete "$SMPROGRAMS\$R0\MakeNSISW (Compiler GUI).lnk"
      Delete "$SMPROGRAMS\$R0\NSIS Site.url"
      Delete "$SMPROGRAMS\$R0\Uninstall UltraModernUI.lnk"
      Delete "$SMPROGRAMS\$R0\NSIS.lnk"
      RMDir "$SMPROGRAMS\$R0"

      SetShellVarContext current
      Delete "$SMPROGRAMS\$R0\Contrib\zip2exe (Create SFX).lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\AdvSplash Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Banner Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\BgImage Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\InstallOptions Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\MakeNSISw Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Math Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Modern UI 2 Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Modern UI Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\nsDialogs Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\nsExec Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\NSISdl Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Splash Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\StartMenu Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\System Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\VPatch Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\NSISArray Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\UltraModernUI Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\SkinnedControls Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\nsArray Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\InstallOptionsEx Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\InstallOptions Readme.lnk"
      Delete "$SMPROGRAMS\$R0\Contrib\Uninstall UltraModernUI.lnk"
      RMDir "$SMPROGRAMS\$R0\Contrib"
      Delete "$SMPROGRAMS\$R0\Uninstall NSIS.lnk"
      Delete "$SMPROGRAMS\$R0\NSIS Menu.lnk"
      Delete "$SMPROGRAMS\$R0\NSIS Examples Directory.lnk"
      Delete "$SMPROGRAMS\$R0\NSIS Documentation.lnk"
      Delete "$SMPROGRAMS\$R0\MakeNSISW (Compiler GUI).lnk"
      Delete "$SMPROGRAMS\$R0\NSIS Site.url"
      Delete "$SMPROGRAMS\$R0\Uninstall UltraModernUI.lnk"
      Delete "$SMPROGRAMS\$R0\NSIS.lnk"
      RMDir "$SMPROGRAMS\$R0"
  noStartMenu:


  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}"

  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_SHELLVARCONTEXT_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_LANGUAGE_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "UMUI_SetupType" ;"${UMUI_SETUPTYPEPAGE_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "UMUI_InstType" ;"${UMUI_COMPONENTSPAGE_INSTTYPE_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "UMUI_Components" ;"${UMUI_COMPONENTSPAGE_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_ADDITIONALTASKS_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_VERSION_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_VERBUILD_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_INSTALLERFULLPATH_REGISTRY_VALUENAME}"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "${UMUI_UNINSTALLPATH_REGISTRY_VALUENAME}"
  ; No more used registry keys
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "UMUI_StartMenuFolder"
  DeleteRegValue ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" "StartMenuFolder"


  ReadRegStr $R0 HKCR ".nsi" ""
  StrCmp $R0 "NSIS.Script" 0 +2
    DeleteRegKey HKCR ".nsi"

  ReadRegStr $R0 HKCR ".nsh" ""
  StrCmp $R0 "NSIS.Header" 0 +2
    DeleteRegKey HKCR ".nsh"

  DeleteRegKey HKCR "NSIS.Script"
  DeleteRegKey HKCR "NSIS.Header"

  System::Call 'Shell32::SHChangeNotify(i ${SHCNE_ASSOCCHANGED}, i ${SHCNF_IDLIST}, i 0, i 0)'

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\NSIS"

  Delete $INSTDIR\makensis.exe
  Delete $INSTDIR\makensisw.exe
  Delete $INSTDIR\NSIS.exe
  Delete $INSTDIR\NSIS.exe.manifest
  Delete $INSTDIR\license.txt
  Delete $INSTDIR\COPYING
  Delete $INSTDIR\uninst-nsis.exe
  Delete $INSTDIR\NSIS.chm

  RMDir /r $INSTDIR\Stubs

  Delete $INSTDIR\Include\WinMessages.nsh
  Delete $INSTDIR\Include\Sections.nsh
  Delete $INSTDIR\Include\Library.nsh
  Delete $INSTDIR\Include\UpgradeDLL.nsh
  Delete $INSTDIR\Include\LogicLib.nsh
  Delete $INSTDIR\Include\StrFunc.nsh
  Delete $INSTDIR\Include\Colors.nsh
  Delete $INSTDIR\Include\FileFunc.nsh
  Delete $INSTDIR\Include\TextFunc.nsh
  Delete $INSTDIR\Include\WordFunc.nsh
  Delete $INSTDIR\Include\WinVer.nsh
  Delete $INSTDIR\Include\x64.nsh
  Delete $INSTDIR\Include\Memento.nsh
  Delete $INSTDIR\Include\LangFile.nsh
  Delete $INSTDIR\Include\InstallOptions.nsh

  RMDir /r $INSTDIR\Docs\StrFunc
  RMDir /r $INSTDIR\Docs\makensisw
  RMDir /r $INSTDIR\Menu

  RMDir /r $INSTDIR\Bin

  Delete $INSTDIR\Examples\makensis.nsi
  Delete $INSTDIR\Examples\example1.nsi
  Delete $INSTDIR\Examples\example2.nsi
  Delete $INSTDIR\Examples\viewhtml.nsi
  Delete $INSTDIR\Examples\waplugin.nsi
  Delete $INSTDIR\Examples\bigtest.nsi
  Delete $INSTDIR\Examples\primes.nsi
  Delete $INSTDIR\Examples\rtest.nsi
  Delete $INSTDIR\Examples\gfx.nsi
  Delete $INSTDIR\Examples\one-section.nsi
  Delete $INSTDIR\Examples\languages.nsi
  Delete $INSTDIR\Examples\Library.nsi
  Delete $INSTDIR\Examples\VersionInfo.nsi
  Delete $INSTDIR\Examples\UserVars.nsi
  Delete $INSTDIR\Examples\LogicLib.nsi
  Delete $INSTDIR\Examples\silent.nsi
  Delete $INSTDIR\Examples\StrFunc.nsi
  Delete $INSTDIR\Examples\FileFunc.nsi
  Delete $INSTDIR\Examples\FileFunc.ini
  Delete $INSTDIR\Examples\FileFuncTest.nsi
  Delete $INSTDIR\Examples\TextFunc.nsi
  Delete $INSTDIR\Examples\TextFunc.ini
  Delete $INSTDIR\Examples\TextFuncTest.nsi
  Delete $INSTDIR\Examples\WordFunc.nsi
  Delete $INSTDIR\Examples\WordFunc.ini
  Delete $INSTDIR\Examples\WordFuncTest.nsi

  RMDir /r "$INSTDIR\Examples\Modern UI"
  RMDir /r "$INSTDIR\Contrib\Modern UI"
  RMDir /r "$INSTDIR\Docs\Modern UI"
  Delete $INSTDIR\Include\MUI.nsh

  RMDir /r "$INSTDIR\Examples\Modern UI 2"
  RMDir /r "$INSTDIR\Contrib\Modern UI 2"
  RMDir /r "$INSTDIR\Docs\Modern UI 2"
  Delete $INSTDIR\Include\MUI2.nsh

  Delete $INSTDIR\Contrib\UIs\default.exe
  Delete $INSTDIR\Contrib\UIs\modern.exe
  Delete $INSTDIR\Contrib\UIs\modern_headerbmp.exe
  Delete $INSTDIR\Contrib\UIs\modern_headerbmpr.exe
  Delete $INSTDIR\Contrib\UIs\modern_nodesc.exe
  Delete $INSTDIR\Contrib\UIs\modern_smalldesc.exe
  Delete $INSTDIR\Contrib\UIs\sdbarker_tiny.exe

  RMDir /r $INSTDIR\Contrib\Graphics

  RMDir /r "$INSTDIR\Contrib\Language files\"

  RMDir /r $INSTDIR\Contrib\zip2exe

  Delete $INSTDIR\Plugins\Banner.dll
  Delete $INSTDIR\Plugins\x86-ansi\Banner.dll
  Delete $INSTDIR\Plugins\x86-unicode\Banner.dll
  RMDir /r "$INSTDIR\Docs\Banner\"
  RMDir /r "$INSTDIR\Examples\Banner\"

  Delete $INSTDIR\Plugins\TypeLib.dll
  Delete $INSTDIR\Plugins\x86-ansi\TypeLib.dll
  Delete $INSTDIR\Plugins\x86-unicode\TypeLib.dll
  
  Delete $INSTDIR\Plugins\LangDLL.dll
  Delete $INSTDIR\Plugins\x86-ansi\LangDLL.dll
  Delete $INSTDIR\Plugins\x86-unicode\LangDLL.dll

  Delete $INSTDIR\Plugins\nsExec.dll
  Delete $INSTDIR\Plugins\x86-ansi\nsExec.dll
  Delete $INSTDIR\Plugins\x86-unicode\nsExec.dll
  RMDir /r $INSTDIR\Docs\nsExec
  RMDir /r $INSTDIR\Examples\nsExec

  Delete $INSTDIR\Plugins\splash.dll
  Delete $INSTDIR\Plugins\x86-ansi\splash.dll
  Delete $INSTDIR\Plugins\x86-unicode\splash.dll
  RMDir /r $INSTDIR\Docs\Splash
  RMDir /r $INSTDIR\Examples\Splash

  Delete $INSTDIR\Plugins\advsplash.dll
  Delete $INSTDIR\Plugins\x86-ansi\advsplash.dll
  Delete $INSTDIR\Plugins\x86-unicode\advsplash.dll
  RMDir /r $INSTDIR\Docs\AdvSplash
  RMDir /r $INSTDIR\Examples\AdvSplash

  Delete $INSTDIR\Plugins\BgImage.dll
  Delete $INSTDIR\Plugins\x86-ansi\BgImage.dll
  Delete $INSTDIR\Plugins\x86-unicode\BgImage.dll
  RMDir /r $INSTDIR\Docs\BgImage
  RMDir /r $INSTDIR\Examples\BgImage

  Delete $INSTDIR\Plugins\InstallOptions.dll
  Delete $INSTDIR\Plugins\x86-ansi\InstallOptions.dll
  Delete $INSTDIR\Plugins\x86-unicode\InstallOptions.dll
  RMDir /r $INSTDIR\Docs\InstallOptions
  RMDir /r $INSTDIR\Examples\InstallOptions

  Delete $INSTDIR\Plugins\Math.dll
  Delete $INSTDIR\Plugins\x86-ansi\Math.dll
  Delete $INSTDIR\Plugins\x86-unicode\Math.dll
  RMDir /r $INSTDIR\Docs\Math
  RMDir /r $INSTDIR\Examples\Math

  Delete $INSTDIR\Plugins\nsisdl.dll
  Delete $INSTDIR\Plugins\x86-ansi\nsisdl.dll
  Delete $INSTDIR\Plugins\x86-unicode\nsisdl.dll
  RMDir /r $INSTDIR\Docs\NSISdl

  Delete $INSTDIR\Plugins\System.dll
  Delete $INSTDIR\Plugins\x86-ansi\System.dll
  Delete $INSTDIR\Plugins\x86-unicode\System.dll
  RMDir /r $INSTDIR\Docs\System
  RMDir /r $INSTDIR\Examples\System

  Delete $INSTDIR\Plugins\nsDialogs.dll
  Delete $INSTDIR\Plugins\x86-ansi\nsDialogs.dll
  Delete $INSTDIR\Plugins\x86-unicode\nsDialogs.dll
  RMDir /r $INSTDIR\Examples\nsDialogs
  Delete $INSTDIR\Include\nsDialogs.nsh
  RMDir /r $INSTDIR\Docs\nsDialogs

  Delete $INSTDIR\Plugins\StartMenu.dll
  Delete $INSTDIR\Plugins\x86-ansi\StartMenu.dll
  Delete $INSTDIR\Plugins\x86-unicode\StartMenu.dll
  RMDir /r $INSTDIR\Docs\StartMenu
  RMDir /r $INSTDIR\Examples\StartMenu

  Delete $INSTDIR\Plugins\UserInfo.dll
  Delete $INSTDIR\Plugins\x86-ansi\UserInfo.dll
  Delete $INSTDIR\Plugins\x86-unicode\UserInfo.dll
  RMDir /r $INSTDIR\Examples

  Delete $INSTDIR\Plugins\Dialer.dll
  Delete $INSTDIR\Plugins\x86-ansi\Dialer.dll
  Delete $INSTDIR\Plugins\x86-unicode\Dialer.dll
  RMDir /r $INSTDIR\Docs\Dialer

  Delete $INSTDIR\Plugins\VPatch.dll
  Delete $INSTDIR\Plugins\x86-ansi\VPatch.dll
  Delete $INSTDIR\Plugins\x86-unicode\VPatch.dll
  RMDir /r $INSTDIR\Examples\VPatch
  RMDir /r $INSTDIR\Docs\VPatch
  Delete $INSTDIR\Include\VPatchLib.nsh

  RMDir "$INSTDIR"

  SetDetailsPrint both

FunctionEnd


;--------------------------------
;Init Functions

Function .onInit

  !insertmacro UMUI_MULTILANG_GET

  ; Change default InstallDir to C:\ProgramData on Windows Vista and more
  ClearErrors
  IfFileExists $INSTDIR endCheckVersion 0
    ReadRegStr $0 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" CurrentVersion
    IfErrors endCheckVersion 0 ; If not WinNT
      IntCmp $0 6 0 endCheckVersion 0 ; If version >= 6
        SetShellVarContext all
        StrCpy $INSTDIR "$APPDATA\NSIS"
  endCheckVersion:

FunctionEnd

Function un.onInit

  !insertmacro UMUI_MULTILANG_GET

FunctionEnd