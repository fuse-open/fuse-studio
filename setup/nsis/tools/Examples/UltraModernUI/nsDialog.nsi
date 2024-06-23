;NSIS Ultra Modern User Interface
;InstallOptionsEx and InstallOptions compatibility mode Example Script
;Written by SyperPat

;--------------------------------
;General

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "nsDialogs.exe"

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

  !define UMUI_SKIN "blue2"
  
  !define UMUI_PAGEBGIMAGE
  !define UMUI_UNPAGEBGIMAGE  
  
  
;---------------------
;Include UltraModernUI

  !include "UMUI.nsh"  
  !include nsDialogs.nsh
  !include LogicLib.nsh

    !define MUI_LICENSEPAGE_TEXT_TOP "All the action takes place on the next page..."
  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"
  Page custom nsDialogsPage
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
;Variables

Var BUTTON
Var EDIT
Var CHECKBOX

;--------------------------------
;Installer Functions

LangString TEXT_NSD_TITLE ${LANG_ENGLISH} "nsDialog page"
LangString TEXT_NSD_SUBTITLE ${LANG_ENGLISH} "This is a page created using the nsDialog plug-in."

Function nsDialogsPage

  ; Does not show page if setup cancelled, required only if UMUI_PAGE_ABORT inserted
  !insertmacro UMUI_ABORT_IF_INSTALLFLAG_IS ${UMUI_CANCELLED}

  !insertmacro MUI_HEADER_TEXT "$(TEXT_NSD_TITLE)" "$(TEXT_NSD_SUBTITLE)"

  nsDialogs::Create 1018
  Pop $0

  !insertmacro UMUI_IOPAGEBGTRANSPARENT_INIT $0 ; set page background color

  GetFunctionAddress $0 OnBack
  nsDialogs::OnBack $0

  ${NSD_CreateButton} 0 0 100% 12u Test
  Pop $BUTTON
  GetFunctionAddress $0 OnClick
  nsDialogs::OnClick $BUTTON $0

  ${NSD_CreateText} 0 35 100% 12u hello
  Pop $EDIT
  GetFunctionAddress $0 OnChange
  nsDialogs::OnChange $EDIT $0

  !insertmacro UMUI_IOPAGEINPUTCTL_INIT $EDIT ; set input text and background color
  
  ${NSD_CreateCheckbox} 0 -50 100% 8u Test
  Pop $CHECKBOX
  GetFunctionAddress $0 OnCheckbox
  nsDialogs::OnClick $CHECKBOX $0

  !insertmacro UMUI_IOPAGECTLTRANSPARENT_INIT $CHECKBOX ; set checkbox and background color
  
  ${NSD_CreateLabel} 0 40u 75% 40u "* Type `hello there` above.$\n* Click the button.$\n* Check the checkbox.$\n* Hit the Back button."
  Pop $0

  !insertmacro UMUI_IOPAGECTLTRANSPARENT_INIT $0 ; set label and background color
  

  nsDialogs::Show

FunctionEnd

Function OnClick

  Pop $0 # HWND

  MessageBox MB_OK clicky

FunctionEnd

Function OnChange

  Pop $0 # HWND

  System::Call user32::GetWindowText(p$EDIT,t.r0,i${NSIS_MAX_STRLEN})

  ${If} $0 == "hello there"
    MessageBox MB_OK "right back at ya"
  ${EndIf}

FunctionEnd

Function OnBack

  MessageBox MB_YESNO "are you sure?" IDYES +2
  Abort

FunctionEnd

Function OnCheckbox

  Pop $0 # HWND

  MessageBox MB_OK "checkbox clicked"

FunctionEnd
