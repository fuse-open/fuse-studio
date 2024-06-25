;NSIS Ultra Modern User Interface
;Alternative Start Menu Folder Selection Example Script
;Written by SuperPat

;--------------------------------
;General

  ;Generate unicode installer
  ;Unicode True

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "StartMenu.exe"

  ;Default installation folder
  InstallDir "$DESKTOP\UltraModernUI Test"
  
  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Include UltraModernUI

  !include "UMUI.nsh"

;--------------------------------
;Variables

  Var MUI_TEMP
  Var STARTMENU_FOLDER

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING

  !define UMUI_USE_INSTALLOPTIONSEX

  !define UMUI_PARAMS_REGISTRY_ROOT HKCU
  !define UMUI_PARAMS_REGISTRY_KEY "Software\UltraModernUI Test"
  
  !define UMUI_INSTALLDIR_REGISTRY_VALUENAME "InstallDir"    ;Replace the InstallDirRegKey instruction and automatically save the $INSTDIR variable

  !define UMUI_SHELLVARCONTEXT_REGISTRY_VALUENAME "UMUI ShellVarContext" 
  !define UMUI_DEFAULT_SHELLVARCONTEXT all

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"
  !insertmacro MUI_PAGE_DIRECTORY
  
    !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
    !define MUI_STARTMENUPAGE_DEFAULTFOLDER "UltraModernUI Test"
    !define UMUI_ALTERNATIVESTARTMENUPAGE_SETSHELLVARCONTEXT
    !define UMUI_ALTERNATIVESTARTMENUPAGE_USE_TREEVIEW
  !insertmacro UMUI_PAGE_ALTERNATIVESTARTMENU Application $STARTMENU_FOLDER
;  !insertmacro MUI_PAGE_STARTMENU Application $STARTMENU_FOLDER
   
  !insertmacro MUI_PAGE_COMPONENTS
  
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
 
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    
    ;Create shortcuts
    CreateDirectory "$SMPROGRAMS\$STARTMENU_FOLDER"
    CreateShortCut "$SMPROGRAMS\$STARTMENU_FOLDER\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
  
  !insertmacro MUI_STARTMENU_WRITE_END

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
  
  !insertmacro MUI_STARTMENU_GETFOLDER Application $MUI_TEMP
 
  Delete "$SMPROGRAMS\$MUI_TEMP\Uninstall.lnk"
  
  ;Delete empty start menu parent diretories
  StrCpy $MUI_TEMP "$SMPROGRAMS\$MUI_TEMP"
 
  startMenuDeleteLoop:
  ClearErrors
    RMDir $MUI_TEMP
    GetFullPathName $MUI_TEMP "$MUI_TEMP\.."
    
    IfErrors startMenuDeleteLoopDone
  
    StrCmp $MUI_TEMP $SMPROGRAMS startMenuDeleteLoopDone startMenuDeleteLoop
  startMenuDeleteLoopDone:

  DeleteRegKey ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}"

SectionEnd