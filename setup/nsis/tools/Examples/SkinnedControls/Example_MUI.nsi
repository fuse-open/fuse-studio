; example_MUI.nsi

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
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING
  !define MUI_COMPONENTSPAGE_NODESC
  !define MUI_CUSTOMFUNCTION_GUIINIT myGUIInit
  !define MUI_CUSTOMFUNCTION_UNGUIINIT un.myGUIInit
  !define MUI_LICENSEPAGE_RADIOBUTTONS
  !define MUI_HEADERIMAGE
 

;--------------------------------
;Pages

  Var STARTMENU_FOLDER

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "..\..\Docs\SkinnedControls\license.txt"
  !insertmacro MUI_PAGE_COMPONENTS
    !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
    !define MUI_STARTMENUPAGE_DEFAULTFOLDER "${NAME}"
  !insertmacro MUI_PAGE_STARTMENU Application $STARTMENU_FOLDER
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
    !define MUI_FINISHPAGE_SHOWREADME "${NSISDIR}\Docs\SkinnedControls\Readme.html"
    !define MUI_FINISHPAGE_LINK "SkinnedControls Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro MUI_PAGE_FINISH
 
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
    !define MUI_FINISHPAGE_LINK "SkinnedControls Home Page"
    !define MUI_FINISHPAGE_LINK_LOCATION "http://ultramodernui.sourceforge.net/"
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

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

;--------------------------------
; Skin buttons and scrollbars

; Workaround for unskinned Detail button in instfiles page
ChangeUI IDD_INSTFILES "${NSISDIR}\Contrib\UIs\modern_sb.exe"
; XPStyle must be set to Off if you use the previous workaround otherwise the detail button will disappear
XPStyle Off

Function .onInit
  InitPluginsDir

  File "/oname=$PLUGINSDIR\button.bmp" "${NSISDIR}\Contrib\SkinnedControls\skins\defaultbtn.bmp"
  File "/oname=$PLUGINSDIR\scrollbar.bmp" "${NSISDIR}\Contrib\SkinnedControls\skins\defaultsb.bmp"
FunctionEnd

Function myGUIInit
  ; start the plugin
  ; the /disabledtextcolor, /selectedtextcolor and /textcolor parameters are optionnal
  SkinnedControls::skinit \
        /disabledtextcolor=808080 \
        /selectedtextcolor=000080 \
        /textcolor=000000 \
        "/scrollbar=$PLUGINSDIR\scrollbar.bmp" \
        "/button=$PLUGINSDIR\button.bmp"
FunctionEnd

Function un.onInit
  InitPluginsDir
  File "/oname=$PLUGINSDIR\button.bmp" "${NSISDIR}\Contrib\SkinnedControls\skins\defaultbtn.bmp"
  File "/oname=$PLUGINSDIR\scrollbar.bmp" "${NSISDIR}\Contrib\SkinnedControls\skins\defaultsb.bmp"
FunctionEnd

Function un.myGUIInit
  ; start the plugin
  ; the /disabledtextcolor, /selectedtextcolor and /textcolor parameters are optionnal
  SkinnedControls::skinit \
        /disabledtextcolor=808080 \
        /selectedtextcolor=000080 \
        /textcolor=000000 \
        "/scrollbar=$PLUGINSDIR\scrollbar.bmp" \
        "/button=$PLUGINSDIR\button.bmp"
FunctionEnd
