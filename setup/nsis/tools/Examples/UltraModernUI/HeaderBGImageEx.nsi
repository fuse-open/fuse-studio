;NSIS Modern User Interface Extended
;Header Background Image Example Script
;Written by SuperPat

;--------------------------------
;General

  ;Name and file
  Name "Modern UI Extended Test"
  OutFile "HeaderBGImage.exe"

  ;Default installation folder
  InstallDir "$DESKTOP\Modern UI Extended Test"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\Modern UI Extended Test" ""

;--------------------------------
;Interface Configuration

  !define UMUI_HEADERBGIMAGE
  ; If you want to change the header image
;  !define MUI_HEADERBGIMAGE_BITMAP "${NSISDIR}\Contrib\Graphics\UltraModernUI\HeaderBG.bmp"

  !define MUI_TEXTCOLOR FFFFFF
  !define MUI_BGCOLOR 6783cf
  !define UMUI_HEADERTEXT_COLOR 003366

  ; If you want to change the WelcomeFinishAbort image
;  !define MUI_WELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Wizard\win.bmp"
;  !define MUI_UNWELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Wizard\win.bmp"

  ; text and background colors for all Input,List,Treeview (except for licencepage and instfile page)
  !define UMUI_TEXT_INPUTCOLOR 003366
  !define UMUI_BGINPUTCOLOR F0F0FF
  
  !define MUI_LICENSEPAGE_BGCOLOR ${UMUI_BGINPUTCOLOR}
  !define MUI_INSTFILESPAGE_COLORS "${UMUI_TEXT_INPUTCOLOR} ${UMUI_BGINPUTCOLOR}"
  
  ; Use skinned buttons
  !define UMUI_DISABLED_BUTTON_TEXT_COLOR 666666
  !define UMUI_SELECTED_BUTTON_TEXT_COLOR 000066
  !define UMUI_BUTTON_TEXT_COLOR 003366
  !define UMUI_BUTTONIMAGE_BMP "${NSISDIR}\Contrib\UltraModernUI\Skins\blue\Button.bmp"
  !define UMUI_UNBUTTONIMAGE_BMP "${NSISDIR}\Contrib\UltraModernUI\Skins\blue\Button.bmp"
  
  
  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING
  
;--------------------------------
;Include Modern UI

  !include "MUIEx.nsh"
  
;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\Modern UI\License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH
  !insertmacro UMUI_PAGE_ABORT
  
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH
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
  WriteRegStr HKCU "Software\Modern UI Test" "" $INSTDIR
  
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

  DeleteRegKey /ifempty HKCU "Software\Modern UI Test"

SectionEnd