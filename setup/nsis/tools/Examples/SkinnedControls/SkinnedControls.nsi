; o-----------------------------------------------------o
; |   SkinnedControls 1.4                               |
; (-----------------------------------------------------)
; | Installer script.      / A plug-in for NSIS 2 and 3 |
; |                       ------------------------------|
; | By SuperPat                                         |
; o-----------------------------------------------------o

;--------------------------------
;General

  !define VERSION "1.4"
  !define NAME "NSIS SkinnedControls plugin"

  !define /date VERIPV "1.4"
  VIProductVersion "${VERIPV}.0.0"
  VIAddVersionKey "ProductName" "SkinnedControls plugin for NSIS (Nullsoft Scriptable Install System)."
  VIAddVersionKey "LegalTrademarks" "SkinnedControls is released under the zlib/libpng license"
  VIAddVersionKey "LegalCopyright" "Copyright � 2005-2019 SuperPat, Based on wansis @ Saivert"
  VIAddVersionKey "FileDescription" "SkinnedControls allow you to can skin all buttons and scroll bars of your installer."
  VIAddVersionKey "FileVersion" "${VERIPV}"
  
;--------------------------------
;Configuration

  ; The name of the installer
  Name "${NAME} version ${VERSION}"

  ; The file to write
  OutFile "SkinnedControls${VERSION}.exe"

  ;Generate unicode installer
  Unicode True

  SetCompressor /FINAL /SOLID lzma
  
  ;Windows vista compatibility
  RequestExecutionLevel admin

  BrandingText "${NAME} version ${VERSION}"

  ;Default installation folder
  InstallDir $PROGRAMFILES\NSIS

;--------------------------------
;Registry Settings

  !define UMUI_PARAMS_REGISTRY_ROOT HKLM
  !define UMUI_PARAMS_REGISTRY_KEY "Software\NSIS"
  
  InstallDirRegKey ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" ""

;--------------------------------
;Include Modern UI

  !include "MUIEx.nsh"
  !include "Sections.nsh"

;--------------------------------
;Interface Settings

  !define UMUI_USE_INSTALLOPTIONSEX

  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING

  !define UMUI_COMPONENTSPAGE_BIGDESC

  !define MUI_LICENSEPAGE_RADIOBUTTONS

  !define MUI_HEADERIMAGE

;--------------------------------
; Skin buttons and scrollbars

  !define UMUI_BUTTONIMAGE_BMP "${NSISDIR}\Contrib\SkinnedControls\skins\defaultbtn.bmp"
  !define UMUI_SCROLLBARIMAGE_BMP "${NSISDIR}\Contrib\SkinnedControls\skins\defaultsb.bmp"

  !define UMUI_DISABLED_BUTTON_TEXT_COLOR 808080
  !define UMUI_SELECTED_BUTTON_TEXT_COLOR 000080
  !define UMUI_BUTTON_TEXT_COLOR 000000

;--------------------------------
;Pages

  Var STARTMENU_FOLDER
  
  !insertmacro UMUI_PAGE_MULTILANGUAGE
    !define UMUI_WELCOMEPAGE_ALTERNATIVETEXT
  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "..\..\Docs\SkinnedControls\license.txt"
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
    !define MUI_FINISHPAGE_SHOWREADME "${NSISDIR}\Docs\SkinnedControls\Readme.html"
    !define MUI_FINISHPAGE_LINK "SkinnedControls Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro MUI_PAGE_FINISH
    !define UMUI_ABORTPAGE_LINK "SkinnedControls Home Page"
    !define UMUI_ABORTPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro UMUI_PAGE_ABORT
 
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
    !define MUI_FINISHPAGE_LINK "SkinnedControls Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro MUI_UNPAGE_FINISH
    !define UMUI_ABORTPAGE_LINK "SkinnedControls Home Page"
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
; The stuff to install

Section "${NAME}, examples & docs" SecSC
  SectionIn RO
  
  ReadRegDword $0 HKLM Software\NSIS "VersionMajor"
  StrCmp $0 3 0 +6
    ; NSIS 3
    SetOutPath $INSTDIR\Plugins\x86-ansi
    File "..\..\Plugins\x86-ansi\SkinnedControls.dll"
    SetOutPath $INSTDIR\Plugins\x86-unicode
    File "..\..\Plugins\x86-unicode\SkinnedControls.dll"
    Goto +3
    
    ; NSIS 2
    SetOutPath $INSTDIR\Plugins
    File "..\..\Plugins\x86-ansi\SkinnedControls.dll"
    
  SetOutPath $INSTDIR\Examples\SkinnedControls
  File "*.nsi"

  SetOutPath $INSTDIR\Contrib\SkinnedControls\skins
  File "..\..\Contrib\SkinnedControls\skins\*.bmp"
  
  SetOutPath $INSTDIR\Docs\SkinnedControls
  File "..\..\Docs\SkinnedControls\*.*"
  SetOutPath "$INSTDIR\Docs\SkinnedControls\images\"
  File "..\..\Docs\SkinnedControls\images\*.png"
  File "..\..\Docs\SkinnedControls\images\*.gif"

  SetOutPath $INSTDIR\Contrib\UIs
  File "..\..\Contrib\UIs\modern_sb.exe"
  File "..\..\Contrib\UIs\default_sb.exe"

  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "DisplayName" "${NAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "DisplayIcon" "$INSTDIR\uninstall_SkinnedControls.exe,0"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}" "UninstallString" "$INSTDIR\uninstall_SkinnedControls.exe"
  WriteUninstaller "uninstall_SkinnedControls.exe"
  
  SetOutPath "$SMPROGRAMS\NSIS\Contrib\"
  CreateShortCut "$SMPROGRAMS\NSIS\Contrib\SkinnedControls Readme.lnk" "$INSTDIR\Docs\SkinnedControls\Readme.html"
  CreateShortCut "$SMPROGRAMS\NSIS\Contrib\Uninstall SkinnedControls.lnk" "$INSTDIR\uninstall_SkinnedControls.exe"

SectionEnd

Section "Source Code" SecSrc
    
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

;--------------------------------
; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}"
  DeleteRegKey HKLM "SOFTWARE\NSIS\${NAME}"

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
  
  Delete "$SMPROGRAMS\NSIS\Contrib\SkinnedControls Readme.lnk"
  Delete "$SMPROGRAMS\NSIS\Contrib\Uninstall SkinnedControls.lnk"
  RMDir "$SMPROGRAMS\NSIS\Contrib"
  RMDir "$SMPROGRAMS\NSIS"
  Delete "$INSTDIR\uninstall_SkinnedControls.exe"
  
SectionEnd

;--------------------------------------
; Components and Confirm page functions

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecSC} "Install the ANSI and UNICODE version of the plugin, default images and the documentation and examples of the SkinnedControls NSIS plugin."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecSrc} "The Sources code of the SkinnedControls plugin."
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

  !insertmacro confirm_addline SC
  !insertmacro confirm_addline Src

FunctionEnd


;-------------------------
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
