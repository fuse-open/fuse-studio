;NSIS Ultra Modern User Interface
;Serial Number Page Example Script
;Written by SuperPat

;--------------------------------
;General

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "SerialNumber.exe"

  ;Default installation folder
  InstallDir "$DESKTOP\UltraModernUI Test"
  
  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;UltraModern Include

  !include "UMUI.nsh"

;--------------------------------
;Interface Settings

  !define UMUI_SKIN "red"

  !define UMUI_USE_INSTALLOPTIONSEX
  
  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING
  
;--------------------------------
;Registry Settings

  !define UMUI_PARAMS_REGISTRY_ROOT HKLM
  !define UMUI_PARAMS_REGISTRY_KEY "Software\UltraModernUI_Test"

;--------------------------------
;Reserve Files

  !insertmacro MUI_RESERVEFILE_INSTALLOPTIONS
  ReserveFile "${NSISDIR}\Contrib\UltraModernUI\Ini\serialnumber.ini"
  
;--------------------------------
;Pages

  !define MUI_PAGE_CUSTOMFUNCTION_LEAVE leave_serial_function
!insertmacro UMUI_PAGE_SERIALNUMBER serial_function
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro UMUI_PAGE_ABORT


Function serial_function

;Get the windows name and organisation
!insertmacro UMUI_SERIALNUMBERPAGE_GET_WINDOWS_REGISTRED_OWNER R0
!insertmacro UMUI_SERIALNUMBERPAGE_GET_WINDOWS_REGISTRED_ORGANIZATION R1

StrCpy $R2 "11111-22222-33333-44444-55555"

; ID:        A unique identifiant for this serial number
; STR:       555 for 3 inputs of 5 characters each, 0 for unlimited chars
; FLAGS:     "", NODASHS, TOUPPER, TOLOWER, NUMBERS, CANBEEMPTY
; DEFAULT:   The défault value if not already saved in the registry XXXXX-XXXXX-XXXXX OR XXXXXXXXXXXXXXX
; LABEL
  !define UMUI_SERIALNUMBERPAGE_SERIAL_REGISTRY_VALUENAME "RegName"
!insertmacro UMUI_SERIALNUMBERPAGE_ADD_LABELEDSERIAL REGNAME       0     "TOUPPER"             $R0 "$(UMUI_TEXT_SERIALNUMBER_NAME)"
  !define UMUI_SERIALNUMBERPAGE_SERIAL_REGISTRY_VALUENAME "Organisation"
!insertmacro UMUI_SERIALNUMBERPAGE_ADD_LABELEDSERIAL ORGANISATION   0     "CANBEEMPTY|TOLOWER"   $R1 "$(UMUI_TEXT_SERIALNUMBER_ORGANIZATION)"
!insertmacro UMUI_SERIALNUMBERPAGE_ADD_HLINE
  !define UMUI_SERIALNUMBERPAGE_SERIAL_REGISTRY_VALUENAME "SerialNumber"
!insertmacro UMUI_SERIALNUMBERPAGE_ADD_LABELEDSERIAL SERIAL         55555 "NUMBERS|NODASHS"     $R2 "$(UMUI_TEXT_SERIALNUMBER_SERIALNUMBER)"

FunctionEnd


Function leave_serial_function

  !insertmacro UMUI_SERIALNUMBER_GET REGNAME R0
  
  StrLen $0 $R0
  IntCmp $0 5 0 0 +3 
    MessageBox MB_OK "The size of name is not enough (must be > 5)"
    Abort

; Apply a verification algorithm of the serial number and use Abort if error

FunctionEnd
  
  
;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"


;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  sleep 1000

SectionEnd
