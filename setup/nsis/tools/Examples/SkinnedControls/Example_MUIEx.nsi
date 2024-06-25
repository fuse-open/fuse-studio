; example_MUIEx.nsi

;--------------------------------
;General

  !define NAME "SkinnedControls test"
  
;--------------------------------
;Configuration

  ; The name of the installer
  Name "${NAME}"

  ; The file to write
  OutFile "${NAME}.exe"
  
  SetCompressor /FINAL /SOLID lzma
  
  ;Windows vista compatibility
  RequestExecutionLevel admin

  BrandingText "${NAME}"

  ;Default installation folder
  InstallDir "$DESKTOP\${NAME}"

;--------------------------------
;Registry Settings

  !define UMUI_PARAMS_REGISTRY_ROOT HKLM
  !define UMUI_PARAMS_REGISTRY_KEY "${NAME}"
  
  InstallDirRegKey ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}" ""

;--------------------------------
;Include Modern UI

  !include "MUIEx.nsh"

;--------------------------------
;Interface Settings

  !define UMUI_USE_INSTALLOPTIONSEX

  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING

  !define MUI_COMPONENTSPAGE_NODESC

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
  !insertmacro MUI_PAGE_DIRECTORY
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
 
  !insertmacro MUI_LANGUAGE "English"
  !insertmacro MUI_LANGUAGE "French"

;--------------------------------
; The stuff to install

Section "${NAME}"
  SectionIn RO

  SetOutPath $INSTDIR
  WriteUninstaller "uninstall.exe"

SectionEnd

;--------------------------------
; Uninstaller

Section "Uninstall"

  RMDir /r "$INSTDIR"
  
SectionEnd

;--------------------------------------
; Components and Confirm page functions


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
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      - ${NAME}"

FunctionEnd


;-------------------------
; Init functions

Function .onInit
  !insertmacro UMUI_MULTILANG_GET
FunctionEnd

Function un.onInit
  !insertmacro UMUI_MULTILANG_GET
FunctionEnd
