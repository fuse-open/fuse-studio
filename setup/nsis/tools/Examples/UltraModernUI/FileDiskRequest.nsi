;NSIS Ultra Modern User Interface
;FileDiskRequest Page Example Script
;Written by SuperPat

;--------------------------------
;General

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "FileDiskRequest.exe"

  ;Default installation folder
  InstallDir "$DESKTOP\UltraModernUI Test"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\UltraModernUI Test" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user
  
;--------------------------------
;Include UltraModernUI

  !include "UMUI.nsh"
;  !include "MUIEx.nsh"

;--------------------------------
;Interface Settings

 !define UMUI_SKIN "blue"
 
 !define MUI_ABORTWARNING
 !define MUI_UNABORTWARNING
 
  !define UMUI_PAGEBGIMAGE
  !define UMUI_UNPAGEBGIMAGE
  
;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"
  !insertmacro MUI_PAGE_DIRECTORY
  
  var dir
  !define UMUI_FILEDISKREQUESTPAGE_VARIABLE dir
  !define UMUI_FILEDISKREQUESTPAGE_FILE_NAME "FileDiskRequest.exe" ; Want to seatch a file. 
  !insertmacro UMUI_PAGE_FILEDISKREQUEST
  
  var dir2
  !define UMUI_FILEDISKREQUESTPAGE_VARIABLE dir2
  !define UMUI_FILEDISKREQUESTPAGE_FILE_NAME "FileDiskRequest.exe"
  !define UMUI_FILEDISKREQUESTPAGE_DISK_NAME "CD 1" ; Want to seatch the "CD 1" disk containing the this file
  !insertmacro UMUI_PAGE_FILEDISKREQUEST

  !insertmacro MUI_PAGE_INSTFILES
  
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  MessageBox MB_OK "Your file was found in $dir"
  MessageBox MB_OK "The CD 1 is in $dir2"

SectionEnd


; Set the defaut directory
Function .onInit

  StrCpy $dir $EXEDIR
  StrCpy $dir2 $EXEDIR
  
FunctionEnd