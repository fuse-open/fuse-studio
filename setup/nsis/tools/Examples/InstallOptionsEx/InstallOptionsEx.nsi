; o-----------------------------------------------------o
; |   InstallOptionsEx 2.4.5 beta 3                     |
; (-----------------------------------------------------)
; | Installer script.      / A plug-in for NSIS 2 and 3 |
; |                       ------------------------------|
; | By SuperPat                                         |
; o-----------------------------------------------------o

;--------------------------------
;General

  !define VERSION "2.4.5b3"
  !define NAME "NSIS InstallOptionsEx plugin"

  !define /date VERIPV "2.4.5.3"
  VIProductVersion "${VERIPV}"
  VIAddVersionKey ProductName "InstallOptionsEx plugin for NSIS (Nullsoft Scriptable Install System)."
  VIAddVersionKey ProductVersion "${VERSION}"
  VIAddVersionKey Comments "InstallOptionsEx plugin for NSIS (Nullsoft Scriptable Install System)."
  VIAddVersionKey LegalTrademarks "InstallOptionsEx is released under the zlib/libpng license: http://nsis.sf.net/License"
  VIAddVersionKey LegalCopyright "Copyright � 2005-2019 SuperPat, Copyright � 2004-2005 Diego Pedroso"
  VIAddVersionKey FileDescription "InstallOptionsEx allow you to create custom pages for NSIS installers, to prompt the user for extra information."
  VIAddVersionKey FileVersion "${VERIPV}"
  
;--------------------------------
;Configuration

  ; The name of the installer
  Name "${NAME} version ${VERSION}"

  ; The file to write
  OutFile "InstallOptionsEx${VERSION}.exe"

  ;Generate unicode installer
  Unicode True

  SetCompressor /FINAL /SOLID lzma
  
  ;Windows vista compatibility
  RequestExecutionLevel admin

  BrandingText "${NAME} version ${VERSION}"

  ;Default installation folder
  InstallDir $PROGRAMFILES\NSIS

;--------------------------------
;Include UltraModernUI between others

  !include "MUIEx.nsh"
  !include "Sections.nsh"

;--------------------------------
;Interface Settings

  !define UMUI_USE_INSTALLOPTIONSEX

  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING

  !define MUI_COMPONENTSPAGE_SMALLDESC
  
  !define MUI_LICENSEPAGE_RADIOBUTTONS
  
  !define MUI_HEADERIMAGE

;--------------------------------
;Registry Settings

  !define UMUI_PARAMS_REGISTRY_ROOT HKLM
  !define UMUI_PARAMS_REGISTRY_KEY "Software\NSIS"
  
  InstallDirRegKey ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" ""

;--------------------------------
;Reserve Files

  !insertmacro MUI_RESERVEFILE_INSTALLOPTIONS

;--------------------------------
;Pages

  Var STARTMENU_FOLDER

    !define UMUI_MULTILANGUAGEPAGE_TITLE_3LINES
  !insertmacro UMUI_PAGE_MULTILANGUAGE
    !define UMUI_WELCOMEPAGE_ALTERNATIVETEXT
    !define MUI_WELCOMEPAGE_TITLE_3LINES
  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "..\..\Docs\InstallOptionsEx\License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
    !define UMUI_ALTERNATIVESTARTMENUPAGE_SETSHELLVARCONTEXT
    !define UMUI_ALTERNATIVESTARTMENUPAGE_USE_TREEVIEW
    !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
    !define MUI_STARTMENUPAGE_DEFAULTFOLDER "NSIS\Contrib"
  !insertmacro UMUI_PAGE_ALTERNATIVESTARTMENU Application $STARTMENU_FOLDER
;  !insertmacro MUI_PAGE_DIRECTORY
    !define UMUI_CONFIRMPAGE_TEXTBOX confirm_function
  !insertmacro UMUI_PAGE_CONFIRM
  !insertmacro MUI_PAGE_INSTFILES
    !define MUI_FINISHPAGE_TITLE_3LINES
    !define MUI_FINISHPAGE_SHOWREADME "${NSISDIR}\Docs\InstallOptionsEx\Readme.html"
    !define MUI_FINISHPAGE_LINK "InstallOptionsEx Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://nsis-ioex.sourceforge.net/"
  !insertmacro MUI_PAGE_FINISH
    !define UMUI_ABORTPAGE_TITLE_3LINES
    !define UMUI_ABORTPAGE_LINK "InstallOptionsEx Home Page"
    !define UMUI_ABORTPAGE_LINK_LOCATION "http://nsis-ioex.sourceforge.net/"
  !insertmacro UMUI_PAGE_ABORT
 
    !define UMUI_WELCOMEPAGE_ALTERNATIVETEXT
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
    !define MUI_FINISHPAGE_TITLE_3LINES
    !define MUI_FINISHPAGE_LINK "InstallOptionsEx Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://nsis-ioex.sourceforge.net/"
  !insertmacro MUI_UNPAGE_FINISH
    !define UMUI_ABORTPAGE_TITLE_3LINES
    !define UMUI_ABORTPAGE_LINK "InstallOptionsEx Home Page"
    !define UMUI_ABORTPAGE_LINK_LOCATION "http://nsis-ioex.sourceforge.net/"
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
; The stuff to install

Section "${NAME}, examples & docs" SecIOEx
  SectionIn RO
  
  ReadRegDword $0 HKLM Software\NSIS "VersionMajor"
  StrCmp $0 3 0 +6
    ; NSIS 3
    SetOutPath $INSTDIR\Plugins\x86-ansi
    File "..\..\Plugins\x86-ansi\InstallOptionsEx.dll"
    SetOutPath $INSTDIR\Plugins\x86-unicode
    File "..\..\Plugins\x86-unicode\InstallOptionsEx.dll"
    Goto +3
    
    ; NSIS 2
    SetOutPath $INSTDIR\Plugins
    File "..\..\Plugins\x86-ansi\InstallOptionsEx.dll"
  
  SetOutPath $INSTDIR\Examples\InstallOptionsEx
  File "*.nsi"
  File "*.ini"
  
  SetOutPath $INSTDIR\Docs\InstallOptionsEx
  File "..\..\Docs\InstallOptionsEx\*.*"

  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "DisplayName" "${NAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "DisplayIcon" "$INSTDIR\uninstall_InstallOptionsEx.exe,0"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "UninstallString" "$INSTDIR\uninstall_InstallOptionsEx.exe"
  WriteUninstaller "uninstall_InstallOptionsEx.exe"

  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    SetOutPath "$SMPROGRAMS\$STARTMENU_FOLDER\"
    CreateShortCut "$SMPROGRAMS\$STARTMENU_FOLDER\InstallOptionsEx Readme.lnk" "$INSTDIR\Docs\InstallOptionsEx\Readme.html"
    CreateShortCut "$SMPROGRAMS\$STARTMENU_FOLDER\Uninstall InstallOptionsEx.lnk" "$INSTDIR\uninstall_InstallOptionsEx.exe"
  !insertmacro MUI_STARTMENU_WRITE_END

SectionEnd

Section "Source Code" SecSrc
    
  SetOutPath $INSTDIR\Contrib\InstallOptionsEx
  File "..\..\Contrib\InstallOptionsEx\*.c"
  File "..\..\Contrib\InstallOptionsEx\*.h"
  File "..\..\Contrib\InstallOptionsEx\ioptdll.rc"
  File "..\..\Contrib\InstallOptionsEx\io.sln"
  File "..\..\Contrib\InstallOptionsEx\io.vcproj"
  SetOutPath $INSTDIR\Contrib\InstallOptionsEx\Controls
  File "..\..\Contrib\InstallOptionsEx\Controls\*.h"
    
SectionEnd

;--------------------------------
; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}"
  DeleteRegKey HKLM "SOFTWARE\NSIS\${NAME}"

  Delete "$INSTDIR\Plugins\InstallOptionsEx.dll"
  Delete "$INSTDIR\Plugins\x86-ansi\InstallOptionsEx.dll"
  Delete "$INSTDIR\Plugins\x86-unicode\InstallOptionsEx.dll"
  RMDir /r "$INSTDIR\Contrib\InstallOptionsEx"
  Delete "$INSTDIR\Docs\InstallOptionsEx\*.*"
  RMDir "$INSTDIR\Docs\InstallOptionsEx"
  Delete "$INSTDIR\Examples\InstallOptionsEx\*.*"
  RMDir "$INSTDIR\Examples\InstallOptionsEx"
  Delete "$INSTDIR\uninstall_InstallOptionsEx.exe"
  RMDir "$INSTDIR"

  !insertmacro MUI_STARTMENU_GETFOLDER Application $STARTMENU_FOLDER
  Delete "$SMPROGRAMS\$STARTMENU_FOLDER\InstallOptionsEx Readme.lnk"
  Delete "$SMPROGRAMS\$STARTMENU_FOLDER\Uninstall InstallOptionsEx.lnk"
  RMDir "$SMPROGRAMS\$STARTMENU_FOLDER"
  RMDir "$SMPROGRAMS\$STARTMENU_FOLDER\.."
  
SectionEnd

;--------------------------------------
; Components and Confirm page functions

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecIOEx} "Install the ANSI and UNICODE version of the plugin, the documentation and examples of the InstallOptionsEx NSIS plugin."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecSrc} "The Sources code of the InstallOptionsEx plugin."
!insertmacro MUI_FUNCTION_DESCRIPTION_END


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

  ;Only if StartMenu Floder is selected
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application

    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_INSTCONFIRM_TEXTBOX_START_MENU_FOLDER)"
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $STARTMENU_FOLDER"
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE ""

    ;ShellVarContext
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_SHELL_VAR_CONTEXT)"
    !insertmacro UMUI_GETSHELLVARCONTEXT
    Pop $1
    StrCmp $1 "all" 0 current
      !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $(UMUI_TEXT_SHELL_VAR_CONTEXT_FOR_ALL_USERS)"
      Goto endsvc
    current:
      !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $(UMUI_TEXT_SHELL_VAR_CONTEXT_ONLY_FOR_CURRENT_USER)"
    endsvc:
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE ""

  !insertmacro MUI_STARTMENU_WRITE_END


  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_INSTCONFIRM_TEXTBOX_COMPNENTS)"

  !insertmacro confirm_addline IOEx
  !insertmacro confirm_addline Src

FunctionEnd

;--------------------------------
; Init functions

Function .onInit
  
  ; Check NSIS Version
  ReadRegDword $0 HKLM Software\NSIS "VersionMajor"
  StrCmp $0 3 okNSIS 0
    StrCmp $0 2 0 +3
      ReadRegDword $0 HKLM Software\NSIS "VersionMinor"
      IntCmp $0 42 okNSIS 0 okNSIS
  
        MessageBox MB_OK|MB_ICONSTOP "NSIS version 2.42 or NSIS 3 are not installed on your computer.$\r$\nPlease, install NSIS (http://nsis.sourceforge.net) and then re-execute this install.$\n$\rThis install will stop."
        Quit

  okNSIS:
  ClearErrors

  !insertmacro UMUI_MULTILANG_GET
FunctionEnd

Function un.onInit
  !insertmacro UMUI_MULTILANG_GET
FunctionEnd