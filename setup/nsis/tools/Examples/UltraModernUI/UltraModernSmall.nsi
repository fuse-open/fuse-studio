;NSIS Ultra Modern User Interface
;UltraModernUI Small Example Script
;Written by SuperPat

;--------------------------------
;General

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "UltraModern Small.exe"

  ;Default installation folder
  InstallDir "$DESKTOP\UltraModernUI Test"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\UltraModernUI Test" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Include UltraModernUI

  !include "UMUI.nsh"

;--------------------------------
;Interface Settings
  
  !define UMUI_SKIN "Blue"

  !define UMUI_ULTRAMODERN_SMALL

  ; If you want to change the left image:
  ;!define MUI_WELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Contrib\UltraModernUI\Skins\blue\Left.bmp"
  ;!define MUI_UNWELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Contrib\UltraModernUI\Skins\blue\Left.bmp"
  
  !define UMUI_WELCOMEFINISHABORTPAGE_USE_IMAGE
  
  !define UMUI_PAGEBGIMAGE

  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING

  !define UMUI_USE_ALTERNATE_PAGE
  !define UMUI_USE_UNALTERNATE_PAGE
  
  
;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  
  !define MUI_FINISHPAGE_RUN "blabla.exe"
  !define MUI_FINISHPAGE_RUN_TEXT "run blabla"
  !define MUI_FINISHPAGE_SHOWREADME "blabla.htm"
  !define MUI_FINISHPAGE_SHOWREADME_TEXT "blabla readme"
  !define MUI_FINISHPAGE_LINK "UltraModernUI Home Page"
  !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro MUI_PAGE_FINISH

  !define UMUI_ABORTPAGE_LINK "UltraModernUI Home Page"
  !define UMUI_ABORTPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro UMUI_PAGE_ABORT


  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
  !define MUI_FINISHPAGE_LINK "UltraModernUI Home Page"
  !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro MUI_UNPAGE_FINISH

  !define UMUI_ABORTPAGE_LINK "UltraModernUI Home Page"
  !define UMUI_ABORTPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro UMUI_UNPAGE_ABORT
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  
  ;Store installation folder
  WriteRegStr HKCU "Software\UltraModernUI Test" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

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