;NSIS Ultra Modern User Interface
;Additional Tasks Page Example Script
;Written by SuperPat


;--------------------------------
;General

  ;Name and file
  Name "UltraModernUI Test"
  OutFile "AdditionalTasks.exe"

  ;Default installation folder
  InstallDir "$DESKTOP\UltraModernUI Test"
  
  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;UltraModern Include

!include "UMUI.nsh"

;--------------------------------
;Interface Settings

  !define UMUI_SKIN "green"

  !define UMUI_PAGEBGIMAGE
  !define UMUI_UNPAGEBGIMAGE

  !define MUI_ABORTWARNING
  !define MUI_UNABORTWARNING
  
  !define UMUI_PARAMS_REGISTRY_ROOT HKCU
  !define UMUI_PARAMS_REGISTRY_KEY "Software\UltraModernUI Test"
  
  !define UMUI_INSTALLDIR_REGISTRY_VALUENAME "InstallDir"    ;Replace the InstallDirRegKey instruction and automatically save the $INSTDIR variable

  !define UMUI_ADDITIONALTASKS_REGISTRY_VALUENAME "Tasks"

;--------------------------------
;Pages

  Var STARTMENU_FOLDER


  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\UltraModernUI\License.txt"

  !insertmacro MUI_PAGE_COMPONENTS

  !insertmacro MUI_PAGE_DIRECTORY

    !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
  !insertmacro UMUI_PAGE_ALTERNATIVESTARTMENU "Application" $STARTMENU_FOLDER

  !insertmacro UMUI_PAGE_ADDITIONALTASKS addtasks_function

    !define UMUI_CONFIRMPAGE_TEXTBOX confirm_function
  !insertmacro UMUI_PAGE_CONFIRM

  !insertmacro MUI_PAGE_INSTFILES

  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES


Function addtasks_function
  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_LABEL "$(UMUI_TEXT_ADDITIONALTASKS_ADDITIONAL_ICONS)"
  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK DESKTOP 1 "$(UMUI_TEXT_ADDITIONALTASKS_CREATE_DESKTOP_ICON)"
  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK QUICK_LAUNCH 1 "$(UMUI_TEXT_ADDITIONALTASKS_CREATE_QUICK_LAUNCH_ICON)"

  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_EMPTYLINE

  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_LABEL "$(UMUI_TEXT_ADDITIONALTASKS_ADVANCED_PARAMETERS)"
  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK STARTUP 1 "$(UMUI_TEXT_ADDITIONALTASKS_LAUNCH_PROGRAM_AT_WINDOWS_STARTUP)"

  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_LABEL "$(UMUI_TEXT_ADDITIONALTASKS_FILE_ASSOCIATION)"
  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK ASSOCIATE 0 "$(UMUI_TEXT_ADDITIONALTASKS_ASSOCIATE_WITH) .UMUI $(UMUI_TEXT_ADDITIONALTASKS_ASSOCIATE_WITH_END)"

  !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_LINE

  ; only if a directory has been selected in the STARTMENU page
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_LABEL "$(UMUI_TEXT_SHELL_VAR_CONTEXT)"
    UserInfo::GetAccountType
    Pop $R0
    StrCmp $R0 "Guest" 0 notLimited
      !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK_RADIO CURRENT 1 "$(UMUI_TEXT_SHELL_VAR_CONTEXT_ONLY_FOR_CURRENT_USER)"
      Goto endShellVarContext
    notLimited:
      !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK_RADIO ALL 1 "$(UMUI_TEXT_SHELL_VAR_CONTEXT_FOR_ALL_USERS)"
      !insertmacro UMUI_ADDITIONALTASKSPAGE_ADD_TASK_RADIO CURRENT 0 "$(UMUI_TEXT_SHELL_VAR_CONTEXT_ONLY_FOR_CURRENT_USER)"
    endShellVarContext:
  !insertmacro MUI_STARTMENU_WRITE_END

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
 
  ;Only if one at least of the six checks is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED DESKTOP|QUICK_LAUNCH|STARTUP|ASSOCIATE|ALL|CURRENT
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "$(UMUI_TEXT_ADDITIONALTASKS_TITLE):"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ;Only if one at least of additional icon check is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED DESKTOP|QUICK_LAUNCH
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $(UMUI_TEXT_ADDITIONALTASKS_ADDITIONAL_ICONS)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ;Only if the first check is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED DESKTOP
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "            $(UMUI_TEXT_ADDITIONALTASKS_CREATE_DESKTOP_ICON)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ;Only if the second check is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED QUICK_LAUNCH
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "            $(UMUI_TEXT_ADDITIONALTASKS_CREATE_QUICK_LAUNCH_ICON)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ;Only if start programm at windows startup check is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED STARTUP
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $(UMUI_TEXT_ADDITIONALTASKS_ADVANCED_PARAMETERS)"
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "            $(UMUI_TEXT_ADDITIONALTASKS_LAUNCH_PROGRAM_AT_WINDOWS_STARTUP)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF 
  ;Only if file association check is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED ASSOCIATE
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $(UMUI_TEXT_ADDITIONALTASKS_FILE_ASSOCIATION)"
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "            $(UMUI_TEXT_ADDITIONALTASKS_ASSOCIATE_WITH) .UMUI $(UMUI_TEXT_ADDITIONALTASKS_ASSOCIATE_WITH_END)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF 
  ; only if a directory has been selected in the STARTMENU page
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED ALL|CURRENT
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "      $(UMUI_TEXT_SHELL_VAR_CONTEXT)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ; only if for all user radio is selected
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED ALL
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "            $(UMUI_TEXT_SHELL_VAR_CONTEXT_FOR_ALL_USERS)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ; only if for current user is selected
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED CURRENT
  !insertmacro UMUI_CONFIRMPAGE_TEXTBOX_ADDLINE "            $(UMUI_TEXT_SHELL_VAR_CONTEXT_ONLY_FOR_CURRENT_USER)"
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF 

FunctionEnd


  
;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"
  !insertmacro MUI_LANGUAGE "French"


;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ;Only if the first check is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED DESKTOP

  !insertmacro UMUI_ADDITIONALTASKS_ENDIF

  ;Only if the second check is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED QUICK_LAUNCH

  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  
  ;Only if the third check is checked  
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED STARTUP

  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  
  
  ;Only if the file association is checked
  ;  For an exemple, see this page: http://nsis.sourceforge.net/FileAssoc
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED ASSOCIATE
  
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF

  
  ;set shellvar context
  ; only if all user is selected
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED ALL
    SetShellVarContext all
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  ; only if current user is selected
  !insertmacro UMUI_ADDITIONALTASKS_IF_CKECKED CURRENT
    SetShellVarContext current
  !insertmacro UMUI_ADDITIONALTASKS_ENDIF
  
  ;After set shell var context, Only if StartMenu Folder is selected
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    ;create start menu and desktop shortsuts
  !insertmacro MUI_STARTMENU_WRITE_END


SectionEnd

Section Uninstall

  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  
  DeleteRegKey ${UMUI_PARAMS_REGISTRY_ROOT} "${UMUI_PARAMS_REGISTRY_KEY}"
  
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

