;NSIS Ultra Modern User Interface
;Setup Type Page Example Script
;Written by SuperPat

;--------------------------------
;Include Modern UI

  !include "UMUI.nsh"

;--------------------------------
;General

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "SetupType.exe"

  ;Default installation folder
  InstallDir "$DESKTOP\UltraModernUI Test"

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Pages

;  !define UMUI_SKIN "purple"

  !define UMUI_USE_INSTALLOPTIONSEX
  !define UMUI_PAGEBGIMAGE
  
  !define UMUI_PARAMS_REGISTRY_ROOT HKCU
  !define UMUI_PARAMS_REGISTRY_KEY "Software\UltraModernUI Test"

  !define UMUI_INSTALLDIR_REGISTRY_VALUENAME "InstallDir"    ;Replace the InstallDirRegKey instruction and automatically save the $INSTDIR variable

  !define UMUI_COMPONENTSINSTTYPE_REGISTRY_VALUENAME "insttype"
  !define UMUI_COMPONENTS_REGISTRY_VALUENAME "components"


  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"
  
    !define UMUI_SETUPTYPEPAGE_MINIMAL "$(UMUI_TEXT_SETUPTYPE_MINIMAL_TITLE)"
    !define UMUI_SETUPTYPEPAGE_STANDARD "$(UMUI_TEXT_SETUPTYPE_STANDARD_TITLE)"
    !define UMUI_SETUPTYPEPAGE_COMPLETE "$(UMUI_TEXT_SETUPTYPE_COMPLETE_TITLE)"
    !define UMUI_SETUPTYPEPAGE_DEFAULTCHOICE ${UMUI_COMPLETE}
    !define UMUI_SETUPTYPE_REGISTRY_VALUENAME "SetupType"
  !insertmacro UMUI_PAGE_SETUPTYPE

    !define UMUI_COMPONENTSPAGE_INSTTYPE_REGISTRY_VALUENAME "insttype"
    !define UMUI_COMPONENTSPAGE_REGISTRY_VALUENAME "components"
  !insertmacro MUI_PAGE_COMPONENTS
  
  !insertmacro MUI_PAGE_DIRECTORY
  
    !define UMUI_CONFIRMPAGE_TEXTBOX confirm_function
  !insertmacro UMUI_PAGE_CONFIRM
  !insertmacro MUI_PAGE_INSTFILES
  
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
 

;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"


;--------------------------------
;Installer Types

InstType "$(UMUI_TEXT_SETUPTYPE_MINIMAL_TITLE)"
InstType "$(UMUI_TEXT_SETUPTYPE_STANDARD_TITLE)"
InstType "$(UMUI_TEXT_SETUPTYPE_COMPLETE_TITLE)"


;--------------------------------
;Installer Sections

Section "Section 1" SecS1
  SectionIn RO
  SectionIn 1 2 3

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd


Section "Section 2" SecS2
  SectionIn 2 3

SectionEnd

Section "Section 3" SecS3
  SectionIn 3

SectionEnd


!insertmacro UMUI_DECLARECOMPONENTS_BEGIN
  !insertmacro UMUI_COMPONENT SecS1
  !insertmacro UMUI_COMPONENT SecS2
  !insertmacro UMUI_COMPONENT SecS3
!insertmacro UMUI_DECLARECOMPONENTS_END

 
;--------------------------------
;Confirm page function

!include "Sections.nsh"
  
!macro confirm_addline section
  SectionGetFlags ${Sec${section}} $1
  IntOp $1 $1 & ${SF_SELECTED}
  IntCmp $1 ${SF_SELECTED} 0 n${section} n${section}
  SectionGetText ${Sec${section}} $1
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "   - $1"
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

  !insertmacro confirm_addline S1
  !insertmacro confirm_addline S2
  !insertmacro confirm_addline S3
FunctionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecDummy ${LANG_ENGLISH} "A test section."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecS1} $(DESC_SecDummy)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...

  Delete "$INSTDIR\Uninstall.exe"

  RMDir "$INSTDIR"

  DeleteRegKey ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}"

SectionEnd