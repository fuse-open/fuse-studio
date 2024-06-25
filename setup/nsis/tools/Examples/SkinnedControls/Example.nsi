; example.nsi

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

  LicenseData "..\..\Docs\SkinnedControls\license.txt"

;--------------------------------
; Pages

  Page license
  Page components
  Page directory
  Page instfiles

  UninstPage uninstConfirm
  UninstPage instfiles

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
ChangeUI IDD_INSTFILES "${NSISDIR}\Contrib\UIs\default_sb.exe"
; XPStyle must not be set to On if you use the previous workaround otherwise the detail button will disappear

Function .onInit
  InitPluginsDir
  File "/oname=$PLUGINSDIR\button.bmp" "${NSISDIR}\Contrib\SkinnedControls\skins\defaultbtn.bmp"
  File "/oname=$PLUGINSDIR\scrollbar.bmp" "${NSISDIR}\Contrib\SkinnedControls\skins\defaultsb.bmp"
FunctionEnd

Function .onGUIInit
Push "test"
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

Function un.onGUIInit
  ; start the plugin
  ; the /disabledtextcolor, /selectedtextcolor and /textcolor parameters are optionnal
  SkinnedControls::skinit \
        /disabledtextcolor=808080 \
        /selectedtextcolor=000080 \
        /textcolor=000000 \
        "/scrollbar=$PLUGINSDIR\scrollbar.bmp" \
        "/button=$PLUGINSDIR\button.bmp"
FunctionEnd
