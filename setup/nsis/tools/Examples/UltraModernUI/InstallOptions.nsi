;NSIS Ultra Modern User Interface
;InstallOptionsEx and InstallOptions compatibility mode Example Script
;Written by SyperPat

;--------------------------------
;General

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "InstallOptions.exe"

  ;Default installation folder
  InstallDir "$DESKTOP\UltraModernUI Test"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\UltraModernUI Test" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING
  
  ;Active InstallOptionsEx
  !define UMUI_USE_INSTALLOPTIONSEX

  !define UMUI_SKIN "blue"
  
  !define UMUI_PAGEBGIMAGE
  !define UMUI_UNPAGEBGIMAGE  
  
  
;---------------------
;Include UltraModernUI

  !include "UMUI.nsh"  


;--------------------------------
;Reserve Files
  
  ;These files should be inserted before other files in the data block
  ;Keep these lines before any File command
  ;Only for solid compression (by default, solid compression is enabled for BZIP2 and LZMA)
  
  ReserveFile "ioA.ini"
  ReserveFile "ioB.ini"
  !insertmacro MUI_RESERVEFILE_INSTALLOPTIONS
  
  
;--------------------------------
;Pages

    !define MUI_LICENSEPAGE_TEXT_TOP "All the action takes place on the next page..."
  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"
  Page custom CustomPageA
  !insertmacro MUI_PAGE_COMPONENTS
  Page custom CustomPageB
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro UMUI_PAGE_ABORT

;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"


;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
 
SectionEnd

;--------------------------------
;Installer Functions

Function .onInit

  ;Extract InstallOptions INI files
  !insertmacro MUI_INSTALLOPTIONS_EXTRACT "ioA.ini"
  !insertmacro MUI_INSTALLOPTIONS_EXTRACT "ioB.ini"
  
FunctionEnd

LangString TEXT_IO_TITLE ${LANG_ENGLISH} "InstallOptions page"
LangString TEXT_IO_SUBTITLE ${LANG_ENGLISH} "This is a page created using the InstallOptions plug-in."

Function CustomPageA

  ; Does not show page if setup cancelled, required only if UMUI_PAGE_ABORT inserted
  !insertmacro UMUI_ABORT_IF_INSTALLFLAG_IS ${UMUI_CANCELLED}

  !insertmacro MUI_HEADER_TEXT "$(TEXT_IO_TITLE)" "$(TEXT_IO_SUBTITLE)"
  !insertmacro INSTALLOPTIONS_DISPLAY "ioA.ini"

FunctionEnd

Var HWND
Function CustomPageB

  ; Does not show page if setup cancelled, required only if UMUI_PAGE_ABORT inserted
  !insertmacro UMUI_ABORT_IF_INSTALLFLAG_IS ${UMUI_CANCELLED}

  !insertmacro MUI_HEADER_TEXT "$(TEXT_IO_TITLE)" "$(TEXT_IO_SUBTITLE)"

  !insertmacro MUI_INSTALLOPTIONS_INITDIALOG "ioB.ini"
  Pop $HWND ;HWND of dialog
  
  !insertmacro UMUI_IOPAGEBGTRANSPARENT_INIT $HWND ; set page background color

  GetDlgItem $0 $HWND 1200
;  !insertmacro UMUI_IOPAGECTLTRANSPARENT_INIT $0  ; set label colors
  SetCtlColors $0 00FF00 "transparent"
  
  GetDlgItem $0 $HWND 1201
  !insertmacro UMUI_IOPAGECTLTRANSPARENT_INIT $0  ; set checkbox colors
  
  GetDlgItem $0 $HWND 1202
  !insertmacro UMUI_IOPAGEINPUTCTL_INIT $0  ; set text colors
  
  !insertmacro MUI_INSTALLOPTIONS_SHOW

FunctionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecDummy ${LANG_ENGLISH} "A test section."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDummy} $(DESC_SecDummy)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...

  Delete "$INSTDIR\Uninstall.exe"

  RMDir "$INSTDIR"

  DeleteRegKey /ifempty HKCU "Software\UltraModernUI Test"

SectionEnd