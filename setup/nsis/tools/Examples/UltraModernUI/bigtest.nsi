;NSIS Ultra Modern User Interface
;Big example Script
;Originally Written by Joost Verburg

;--------------------------------
;General

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "BigTest.exe"
  SetCompressor /FINAL lzma

  BrandingText "UltraModernUI for NSIS"

  ;Default installation folder
  InstallDir "$DESKTOP\UltraModernUI Test"

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Inclusion

  !include "UMUI.nsh"

;--------------------------------
;Interface Settings

  !define UMUI_SKIN "blue"
  !define UMUI_BGSKIN "blue"

  !define UMUI_USE_INSTALLOPTIONSEX
  
;  !define MUI_COMPONENTSPAGE_NODESC
  !define MUI_COMPONENTSPAGE_SMALLDESC
  !define UMUI_NOLEFTIMAGE

  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING

  !define UMUI_USE_ALTERNATE_PAGE
  !define UMUI_USE_UNALTERNATE_PAGE

  !define UMUI_PARAMS_REGISTRY_ROOT HKCU
  !define UMUI_PARAMS_REGISTRY_KEY "Software\UltraModernUI Test"
  
  !define UMUI_INSTALLDIR_REGISTRY_VALUENAME "InstallDir"    ;Replace the InstallDirRegKey instruction and automatically save the $INSTDIR variable

  ;Remember the installer language
  !define UMUI_LANGUAGE_REGISTRY_VALUENAME "Installer Language"

;--------------------------------
;Pages

  Var STARTMENU_FOLDER

  !insertmacro MUI_PAGE_WELCOME

  Page custom CustomPageA
    
    !define MUI_LICENSEPAGE_CHECKBOX
  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"

    !define UMUI_INFORMATIONPAGE_USE_RICHTEXTFORMAT
  !insertmacro UMUI_PAGE_INFORMATION "${NSISDIR}\Docs\UltraModernUI\ReadMe.rtf"
;  !insertmacro UMUI_PAGE_INFORMATION "${NSISDIR}\Docs\UltraModernUI\License.txt"
    
  !insertmacro UMUI_PAGE_SETUPTYPE
    
  !insertmacro MUI_PAGE_COMPONENTS
  
  !insertmacro MUI_PAGE_DIRECTORY
  
    !define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU" 
    !define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\UltraModernUI Test" 
    !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
  !insertmacro UMUI_PAGE_ALTERNATIVESTARTMENU "Application" $STARTMENU_FOLDER
  
    !define UMUI_CONFIRMPAGE_TEXTBOX confirm_function
  !insertmacro UMUI_PAGE_CONFIRM
  
    !define UMUI_INSTFILEPAGE_ENABLE_CANCEL_BUTTON cancel_function
  !insertmacro MUI_PAGE_INSTFILES

    !define MUI_FINISHPAGE_RUN "C:\Notepad.exe"
    !define MUI_FINISHPAGE_SHOWREADME "C:\Notepad.exe"
    !define MUI_FINISHPAGE_LINK "UltraModernUI Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net"
  !insertmacro MUI_PAGE_FINISH

    !define UMUI_ABORTPAGE_LINK "UltraModernUI Home Page"
    !define UMUI_ABORTPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net"
  !insertmacro UMUI_PAGE_ABORT

  
  !insertmacro MUI_UNPAGE_WELCOME
  
  !insertmacro MUI_UNPAGE_COMPONENTS
  
  !insertmacro MUI_UNPAGE_CONFIRM
  
  !insertmacro MUI_UNPAGE_INSTFILES
  
    !define MUI_FINISHPAGE_LINK "UltraModernUI Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net"
  !insertmacro MUI_UNPAGE_FINISH
  
    !define UMUI_ABORTPAGE_LINK "UltraModernUI Home Page"
    !define UMUI_ABORTPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net"
  !insertmacro UMUI_UNPAGE_ABORT


Function cancel_function
  MessageBox MB_OK "Delete all files"
FunctionEnd

Function confirm_function
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_INSTCONFIRM_TEXTBOX_DESTINATION_LOCATION)"
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $INSTDIR"
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE ""
  
  ;Only if StartMenu Folder is selected
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_INSTCONFIRM_TEXTBOX_START_MENU_FOLDER)"
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $STARTMENU_FOLDER"
    !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE ""
  !insertmacro MUI_STARTMENU_WRITE_END

  ;For the setuptype page
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_SETUPTYPE_TITLE):"
  !insertmacro UMUI_GET_CHOOSEN_SETUP_TYPE_TEXT
  Pop $R0
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $R0"
FunctionEnd



Function CustomPageA
  !insertmacro MUI_INSTALLOPTIONS_EXTRACT "ioA.ini"
  !insertmacro MUI_HEADER_TEXT "def" "abc"
  !insertmacro MUI_INSTALLOPTIONS_DISPLAY "ioA.ini"
FunctionEnd


;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "French"
  !insertmacro MUI_LANGUAGE "English"
  !insertmacro MUI_RESERVEFILE_LANGDLL


;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  sleep 10000
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BigNSISTest" "DisplayName" "$(^NameDA)"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BigNSISTest" "DisplayIcon" "$INSTDIR\Uninstall.exe,0"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BigNSISTest" "UninstallString" "$INSTDIR\Uninstall.exe"
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

Section Uninstall

  DeleteRegKey ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}"
    
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\BigNSISTest"

  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  Sleep 1000
SectionEnd


;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecDummy ${LANG_ENGLISH} "A test section."
  LangString DESC_SecDummy ${LANG_FRENCH} "Une section de test."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDummy} $(DESC_SecDummy)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

Function un.onInit

  !insertmacro MUI_UNGETLANGUAGE
  
FunctionEnd

Function .onInit

  !insertmacro MUI_LANGDLL_DISPLAY

FunctionEnd