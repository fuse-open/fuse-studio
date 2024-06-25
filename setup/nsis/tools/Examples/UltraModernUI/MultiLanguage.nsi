;NSIS Ultra Modern User Interface
;Multilanguage Page Example Script
;Written SuperPat

;--------------------------------
;General

  ;Generate unicode installer
  Unicode True

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "MultiLanguage.exe"

  ;Default installation folder
  InstallDir "$DESKTOP\UltraModernUI Test"

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Include UltraModernUI

  !include "UMUI.nsh"
;  !include "MUIEx.nsh"

;--------------------------------
;Interface Settings

  !define UMUI_USE_INSTALLOPTIONSEX

  !define MUI_ABORTWARNING

  !define UMUI_PARAMS_REGISTRY_ROOT HKCU 
  !define UMUI_PARAMS_REGISTRY_KEY "Software\UltraModernUI Test" 
  
  !define UMUI_INSTALLDIR_REGISTRY_VALUENAME "InstallDir"    ;Replace the InstallDirRegKey instruction and automatically save the $INSTDIR variable
  
  !define UMUI_LANGUAGE_REGISTRY_VALUENAME "Language"

  !define UMUI_USE_ALTERNATE_PAGE

  ; always show the MultiLanguage page or the LangDLLDialog
  ;!define UMUI_LANGUAGE_ALWAYSSHOW

;--------------------------------
;Pages
    
  !insertmacro UMUI_PAGE_MULTILANGUAGE
  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro UMUI_UNPAGE_MULTILANGUAGE
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
;Languages

; first language is the default language if the system language is not in this list
  !insertmacro MUI_LANGUAGE "English"

; Other UMUI translated languages
  !insertmacro MUI_LANGUAGE "Bulgarian"
  !insertmacro MUI_LANGUAGE "Czech"
  !insertmacro MUI_LANGUAGE "French"
  !insertmacro MUI_LANGUAGE "German"
  !insertmacro MUI_LANGUAGE "Greek"
  !insertmacro MUI_LANGUAGE "Hungarian"
  !insertmacro MUI_LANGUAGE "Italian"
  !insertmacro MUI_LANGUAGE "Japanese"
  !insertmacro MUI_LANGUAGE "Lithuanian"
  !insertmacro MUI_LANGUAGE "Polish"
  !insertmacro MUI_LANGUAGE "Russian"
  !insertmacro MUI_LANGUAGE "Slovenian"
  !insertmacro MUI_LANGUAGE "Spanish"
  !insertmacro MUI_LANGUAGE "Turkish"
; Other UMUI partially translated language
  !insertmacro MUI_LANGUAGE "PortugueseBR"

; Other untranslated languages but usable even so.
  !insertmacro MUI_LANGUAGE "SpanishInternational"
  !insertmacro MUI_LANGUAGE "SimpChinese"
  !insertmacro MUI_LANGUAGE "TradChinese"
  !insertmacro MUI_LANGUAGE "Korean"
  !insertmacro MUI_LANGUAGE "Dutch"
  !insertmacro MUI_LANGUAGE "Danish"
  !insertmacro MUI_LANGUAGE "Swedish"
  !insertmacro MUI_LANGUAGE "Norwegian"
  !insertmacro MUI_LANGUAGE "NorwegianNynorsk"
  !insertmacro MUI_LANGUAGE "Finnish"
  !insertmacro MUI_LANGUAGE "Portuguese"
  !insertmacro MUI_LANGUAGE "Ukrainian"
  !insertmacro MUI_LANGUAGE "Slovak"
  !insertmacro MUI_LANGUAGE "Croatian"
  !insertmacro MUI_LANGUAGE "Thai"
  !insertmacro MUI_LANGUAGE "Romanian"
  !insertmacro MUI_LANGUAGE "Latvian"
  !insertmacro MUI_LANGUAGE "Macedonian"
  !insertmacro MUI_LANGUAGE "Estonian"
  !insertmacro MUI_LANGUAGE "Serbian"
  !insertmacro MUI_LANGUAGE "SerbianLatin"
  !insertmacro MUI_LANGUAGE "Arabic"
  !insertmacro MUI_LANGUAGE "Farsi"
  !insertmacro MUI_LANGUAGE "Hebrew"
  !insertmacro MUI_LANGUAGE "Indonesian"
  !insertmacro MUI_LANGUAGE "Mongolian"
  !insertmacro MUI_LANGUAGE "Luxembourgish"
  !insertmacro MUI_LANGUAGE "Albanian"
  !insertmacro MUI_LANGUAGE "Breton"
  !insertmacro MUI_LANGUAGE "Belarusian"
  !insertmacro MUI_LANGUAGE "Icelandic"
  !insertmacro MUI_LANGUAGE "Malay"
  !insertmacro MUI_LANGUAGE "Bosnian"
  !insertmacro MUI_LANGUAGE "Kurdish"
  !insertmacro MUI_LANGUAGE "Irish"
  !insertmacro MUI_LANGUAGE "Uzbek"
  !insertmacro MUI_LANGUAGE "Galician"
  !insertmacro MUI_LANGUAGE "Afrikaans"
  !insertmacro MUI_LANGUAGE "Catalan"
  !insertmacro MUI_LANGUAGE "Esperanto"
  !insertmacro MUI_LANGUAGE "Asturian"
  !insertmacro MUI_LANGUAGE "Basque"
  !insertmacro MUI_LANGUAGE "Pashto"
  !insertmacro MUI_LANGUAGE "ScotsGaelic"
  !insertmacro MUI_LANGUAGE "Vietnamese"
  !insertmacro MUI_LANGUAGE "Welsh"
  !insertmacro MUI_LANGUAGE "Corsican"
  !insertmacro MUI_LANGUAGE "Tatar"

; Other unicode only untranslated languages but usable even so.
  !insertmacro MUI_LANGUAGE "Armenian"
  !insertmacro MUI_LANGUAGE "Georgian"
  !insertmacro MUI_LANGUAGE "Hindi"

  
;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd



;--------------------------------
;Descriptions

  ;USE A LANGUAGE STRING IF YOU WANT YOUR DESCRIPTIONS TO BE LANGAUGE SPECIFIC

  ;Assign descriptions to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDummy} "A test section."
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

 
;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...

  Delete "$INSTDIR\Uninstall.exe"

  RMDir "$INSTDIR"

  DeleteRegKey ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}"

SectionEnd

;--------------------------------
;Installer Functions

Function .onInit
  !insertmacro UMUI_MULTILANG_GET
FunctionEnd

;--------------------------------
;Uninstaller Functions

Function un.onInit
  !insertmacro UMUI_MULTILANG_GET
FunctionEnd