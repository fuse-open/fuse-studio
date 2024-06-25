// NOTES:
// - pFilenameStackEntry was removed. Not necessary.

//================================================================
// Header Implementation
//================================================================

// File Inclusions
//================================================================
#include <windows.h>
#include <windowsx.h>
#include <shlobj.h>
#include <commdlg.h>
#include <cderr.h>
#include <shlwapi.h>
#include <olectl.h>
#include <stdio.h>
#include <tchar.h>
#include <shellapi.h>
#include "resource.h"

#include "pluginapi.h" // nsis plugin

// Defines Based on File Inclusions
//================================================================
#ifndef IDC_HAND
  #define IDC_HAND MAKEINTRESOURCE(32649)
#endif

#ifndef BS_TYPEMASK
  #define BS_TYPEMASK 0x0000000FL
#endif

#ifndef USER_TIMER_MAXIMUM
  #define USER_TIMER_MAXIMUM 0x7FFFFFFF
#endif
#ifndef USER_TIMER_MINIMUM
  #define USER_TIMER_MINIMUM 0x0000000A
#endif

#ifndef COLOR_MENUHILIGHT
  #define COLOR_MENUHILIGHT 29
#endif

// Compiler Options
//================================================================

// Force INLINE code
//----------------------------------------------------------------
// Use for functions only called from one place to possibly reduce some code
// size.  Allows the source code to remain readable by leaving the function
// intact.
#ifdef _MSC_VER
  #define INLINE __forceinline
#else
  #define INLINE inline
#endif

//================================================================
// Classes Definitions
//================================================================

// NotifyQueue (Allows notification messages to have queue)
//================================================================
struct NotifyQueue {
  int iNotifyId;
  bool bNotifyType; // 0 = page, 1 = control. Page notifications are hardcodedly handled by the plug-in.
  int iField;
};

// TableEntry (Table used to convert strings to numbers)
//================================================================
struct TableEntry {
  TCHAR *pszName;
  int   nValue;
};

// IOExWindowStorage (Storage structure for window information)
//================================================================
// all allocated buffers must be first in the struct
// when adding more allocated buffers to IOExControlStorage, don't forget to change this define
#define IOEX_WINDOW_BUFFERS 4
struct IOExWindowStorage {
  LPTSTR pszTitle;
  LPTSTR pszState;        //Window state.
  LPTSTR pszNotify;       //Set which window notifications to use.
  LPTSTR pszNotifyResult; //INI Files change: Notify->NotifyResult.
                          //Idea: Maybe I should add another "Notify" key name so that it could
                          //be used for selecting which page notification should be accepted, and
                          //then use the NOTIFY_TIMEOUT (for example) so that the "nTimeOut" be activated.
  int nRect;
  int bRTL;
  int iMUnit;
  int nTimeOut;

  int iDlgResource; //If used, then pszRect is used. Default is 105.

  RECT RectUDU;
  RECT RectPx;
};

// IOExControlStorage (Storage structure for control information)
//================================================================
// all allocated buffers must be first in the struct
// when adding more allocated buffers to IOExControlStorage, don't forget to change this define
#define FIELD_BUFFERS 15
struct IOExControlStorage {
  TCHAR  *pszText;
  TCHAR  *pszState;
  TCHAR  *pszRoot;

  TCHAR  *pszListItems;
  TCHAR  *pszFilter;

  TCHAR   *pszValidateText;

  TCHAR  *pszFontName;

  TCHAR  *pszToolTipText;
  TCHAR  *pszToolTipTitle;

  TCHAR *pszHeaderItems;
  TCHAR *pszHeaderItemsWidth;
  TCHAR *pszHeaderItemsAlign;

  TCHAR *pszStateImageList;
  TCHAR *pszSmallImageList;
  TCHAR *pszLargeImageList;

  int    nRefFields;

  int    nMinLength;
  int    nMaxLength;

  int    nFontHeight;
  int    nFontWidth;
  bool   nFontBold;
  bool   nFontItalic;
  bool   nFontUnderline;
  bool   nFontStrikeOut;

  int    nListItemsHeight;

  COLORREF crTxtColor;
  COLORREF crBgColor;
  COLORREF crSelTxtColor;
  COLORREF crSelBgColor;

  COLORREF crReadOnlyTxtColor;
  COLORREF crReadOnlyBgColor;

  COLORREF crDisTxtColor;
  COLORREF crDisBgColor;
  COLORREF crDisSelTxtColor;
  COLORREF crDisSelBgColor;

  COLORREF crTxtShwColor;
  COLORREF crSelTxtShwColor;

  COLORREF crDisTxtShwColor;
  COLORREF crDisSelTxtShwColor;

  COLORREF crMonthOutColor;
  COLORREF crMonthTrailingTxtColor;

  int  nType;
  RECT RectUDU;
  RECT RectPx;

  int nFlags;
  int nNotify;
  int nNotifyCursor;
  int nAlign;
  int nVAlign;
  int nTxtAlign;
  int nTxtVAlign;

  HWND hwnd;
  UINT nControlID;

  HWND      hwToolTip;
  int       nToolTipFlags;
  int       nToolTipIcon;
  COLORREF  crToolTipTxtColor;
  COLORREF  crToolTipBgColor;
  int       nToolTipMaxWidth;

  INT_PTR    nParentIdx;      // this is used to store original windowproc.
  int        nDataType;       // used by FIELD_RICHTEXT and FIELD_IMAGE.
  LPPICTURE  nImageInterface; // used by FIELD_IMAGE controls for gif, jpeg and wmf files.
  HANDLE     hImage;          // this is used by image field to save the handle to the image.
  HIMAGELIST hStateImageList; // handle to state image list.
  //HIMAGELIST hSmallImageList;
  //HIMAGELIST hLargeImageList;
  HWND       hEditControl;    // handle to a temporary edit control for controls which use FLAG_EDITLABELS flag.

  int    nField;

  UINT nWindowID; //Connects with the the IOExWindowStorage structure.
};

//TODO: Remove 9 key names to disable, enable or change buttons text. Reason: This is
//      located outside of the InstallOptionsEx window, so it shouldn't touch it.
//      For compatibility purposes, a specific function should be created that does this.

//TODO: Add Tooltip control feature custom positioning of the tooltip itself in the dialog.
//TODO: Make Tooltip commands separate from the control individual and make them individual controls.

//TODO: Add flags for Image control to determine image file types.

//================================================================
// Variables/Defines Definitions
//================================================================

// Plug-in
//================================================================

// General
//----------------------------------------------------------------
HINSTANCE m_hInstance = NULL;

int initCalled; //If the page initialization was called before

// Message Loop
//----------------------------------------------------------------

// Misc.
MSG msg; //message previous than the current one
int g_done; // specifies if the message loop should end

// Notification Flags Queue
#define NOTIFY_QUEUE_AMOUNT_MAX 2
#define NOTIFY_TYPE_PAGE 0
#define NOTIFY_TYPE_CONTROL 1
int g_nNotifyQueueAmountMax;
NotifyQueue *g_aNotifyQueue = NULL; // MaxItems = NOTIFY_QUEUE_AMOUNT_MAX
NotifyQueue *g_aNotifyQueueTemp = NULL; // MaxItems = 1

// Determine direction of page change
bool g_is_cancel,g_is_back,g_is_timeout;

// Config file
//================================================================
TCHAR *pszFilename = NULL; // File name
TCHAR *szResult; // Key Name (Temporary)
const TCHAR *pszAppName; // Section Name (Temporary)

// Dialog
//================================================================

// NSIS Window
//----------------------------------------------------------------

// Properties

// / Window handles
HWND hMainWindow      = NULL;
HWND hConfigWindow    = NULL; //Dialog Window

// / Main window button handles
HWND hCancelButton    = NULL;
HWND hNextButton      = NULL;
HWND hBackButton      = NULL;

// Settings

// / Button texts
TCHAR *pszCancelButtonText = NULL; // CancelButtonText
TCHAR *pszNextButtonText   = NULL; // NextButtonText
TCHAR *pszBackButtonText   = NULL; // BackButtonText

// / Enable buttons?
int bNextEnabled   = FALSE; // NextEnabled
int bBackEnabled   = FALSE; // BackEnabled
int bCancelEnabled = FALSE; // CancelEnabled

// / Show buttons?
int bNextShow      = FALSE; // NextShow
int bBackShow      = FALSE; // BackShow
int bCancelShow    = FALSE; // CancelShow

// Temporary

// Were buttons on previous page shown?
int old_cancel_visible;
int old_back_visible;
int old_next_visible;

// Were buttons on previous page enabled?
int old_cancel_enabled;
int old_back_enabled;
int old_next_enabled;

// Previous page callback
static WNDPROC lpWndProcOld;

// IOEx Window Related
//----------------------------------------------------------------

// Controls Storage
IOExControlStorage *pFields = NULL;

// Temporary Variables

// / Control Field
TCHAR *pszTitle = NULL; //temp variable - probably not needed w/ the new classes

// / Timer
int nTimeOutTemp;
UINT_PTR g_timer_id = 14; // This can be any number except 0.
DWORD g_timer_cur_time;
bool g_timer_activated;

// Variable Buffer Size
// - Should be > MAX_PATH to prevent excessive growing.
// - Includes last '\0' character.
// - Default supports most of controls needs.
#define BUFFER_SIZE 8192 // 8KB

// Default Rectangle Number
#define DEFAULT_RECT 1018

// Default Control Styles
#define DEFAULT_STYLES (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS)
#define RTL_EX_STYLES (WS_EX_RTLREADING | WS_EX_LEFTSCROLLBAR)

// Page Settings
int g_nBufferSize;  // BufferSize
int bMUnit = 0;     // MUnit
int bRTL = 0;       // RTL
int nRectId = 0;    // RectId
int nNumFields = 0; // NumFields (not sure if to continue compatibility)
int nTimeOut;       // TimeOut

// Control Settings

// / Type Flags

//================================================================
// Field Types Definitions
//================================================================
// NB - the order of this list is important - see below

#define FIELD_INVALID       (0)
#define FIELD_HLINE         (1)
#define FIELD_VLINE         (2)
#define FIELD_LABEL         (3)
#define FIELD_GROUPBOX      (4)
#define FIELD_IMAGE         (5)
#define FIELD_PROGRESSBAR   (6)
#define FIELD_LINK          (7)
#define FIELD_BUTTON        (8)
#define FIELD_UPDOWN        (9)
#define FIELD_CHECKBOX      (10)
#define FIELD_RADIOBUTTON   (11)
#define FIELD_TEXT          (12)
#define FIELD_IPADDRESS     (13)
#define FIELD_RICHTEXT      (14)
#define FIELD_COMBOBOX      (15)
#define FIELD_DATETIME      (16)
#define FIELD_LISTBOX       (17)
#define FIELD_LISTVIEW      (18)
#define FIELD_TREEVIEW      (19)
#define FIELD_TRACKBAR      (20)
#define FIELD_MONTHCALENDAR (21)

#define FIELD_SETFOCUS     FIELD_CHECKBOX // First field that qualifies for having the initial keyboard focus
#define FIELD_CHECKLEN     FIELD_TEXT     // First field to have length of state value checked against MinLen/MaxLen

// / / Control Types Table

    static TableEntry TypeTable[] = {
    { _T("Label"),         FIELD_LABEL         },
    { _T("GroupBox"),      FIELD_GROUPBOX      },
    { _T("Image"),         FIELD_IMAGE         },
    { _T("Icon"),          FIELD_IMAGE         }, // For compatibility w/ IO
    { _T("Bitmap"),        FIELD_IMAGE         }, // For compatibility w/ IO
    { _T("Animation"),     FIELD_IMAGE         }, // For compatibility w/ IO
    { _T("ProgressBar"),   FIELD_PROGRESSBAR   },
    { _T("Link"),          FIELD_LINK          },
    { _T("CheckBox"),      FIELD_CHECKBOX      },
    { _T("RadioButton"),   FIELD_RADIOBUTTON   },
    { _T("Button"),        FIELD_BUTTON        },
    { _T("UpDown"),        FIELD_UPDOWN        },
    { _T("Text"),          FIELD_TEXT          },
    { _T("Edit"),          FIELD_TEXT          }, // Same as TEXT
    { _T("Password"),      FIELD_TEXT          },
    { _T("IPAddress"),     FIELD_IPADDRESS     },
    { _T("RichText"),      FIELD_RICHTEXT      },
    { _T("RichEdit"),      FIELD_RICHTEXT      }, // Same as RICHTEXT
    { _T("DropList"),      FIELD_COMBOBOX      },
    { _T("ComboBox"),      FIELD_COMBOBOX      },
    { _T("DateTime"),      FIELD_DATETIME      },
    { _T("ListBox"),       FIELD_LISTBOX       },
    { _T("ListView"),      FIELD_LISTVIEW      },
    { _T("TreeView"),      FIELD_TREEVIEW      },
    { _T("TrackBar"),      FIELD_TRACKBAR      },
    { _T("MonthCalendar"), FIELD_MONTHCALENDAR },
    { _T("HLINE"),         FIELD_HLINE         },
    { _T("VLINE"),         FIELD_VLINE         },
    { NULL,                0                   }
  };

// / Flags

// / / All controls
#define FLAG_DISABLED          0x00000001
#define FLAG_GROUP             0x00000002
#define FLAG_NOTABSTOP         0x00000004
#define FLAG_HSCROLL           0x00000008
#define FLAG_VSCROLL           0x00000010
#define FLAG_MULTISELECT       0x00000020
#define FLAG_READONLY          0x00000040
// / / Image controls
#define FLAG_RESIZETOFIT       0x00000080
#define FLAG_TRANSPARENT       0x00000100
// / / ProgressBar controls
#define FLAG_SMOOTH            0x00000080
//      FLAG_VSCROLL           0x00000010
// / / CheckBox/RadioButton controls
//      FLAG_READONLY          0x00000040
#define FLAG_3STATE            0x00000080 // CheckBox
// / / Button controls
#define FLAG_OPEN_FILEREQUEST  0x00000100
#define FLAG_SAVE_FILEREQUEST  0x00000200
#define FLAG_DIRREQUEST        0x00000400
#define FLAG_COLORREQUEST      0x00000800
#define FLAG_FONTREQUEST       0x00001000
#define FLAG_FILE_MUST_EXIST   0x00002000
#define FLAG_FILE_EXPLORER     0x00004000
#define FLAG_FILE_HIDEREADONLY 0x00008000
#define FLAG_WARN_IF_EXIST     0x00010000
#define FLAG_PATH_MUST_EXIST   0x00020000
#define FLAG_PROMPT_CREATE     0x00040000
#define FLAG_BITMAP            0x00080000
#define FLAG_ICON              0x00100000
//      FLAG_MULTISELECT       0x00000020
// / / UpDown controls
//      FLAG_HSCROLL           0x00000008
#define FLAG_WRAP              0x00000080
// / / Text/Password/RichText controls
#define FLAG_ONLYNUMBERS       0x00000080
#define FLAG_MULTILINE         0x00000100
#define FLAG_WANTRETURN        0x00000200
#define FLAG_NOWORDWRAP        0x00000400
#define FLAG_PASSWORD          0x00000800 // Text/RichText
// / / DropList/ComboBox controls
//      FLAG_VSCROLL           0x00000010
#define FLAG_DROPLIST          0x00000080 // ComboBox
// / / DateTime controls
#define FLAG_UPDOWN            0x00000080
// / / ListBox controls
//      FLAG_MULTISELECT       0x00000020
#define FLAG_EXTENDEDSELECT    0x00000080
//      FLAG_VSCROLL           0x00000010
// / / TreeView/ListView controls
#define FLAG_CHECKBOXES        0x00000080
#define FLAG_EDITLABELS        0x00000100
#define FLAG_ICON_VIEW         0x00000200 // ListView
#define FLAG_LIST_VIEW         0x00000400 // ListView
#define FLAG_REPORT_VIEW       0x00000800 // ListView
#define FLAG_SMALLICON_VIEW    0x00001000 // ListView
// / / TrackBar controls
#define FLAG_NO_TICKS          0x00000080
//      FLAG_VSCROLL           0x00000010
// / / MonthCalendar controls
#define FLAG_NOTODAY           0x00000080
#define FLAG_WEEKNUMBERS       0x00000100

#define FLAG_FOCUS             0x10000000 // Controls that can receive focus
#define FLAG_DRAW_TEMP         0x40000000 // Temporary flag for
                                          // drawing process
#define FLAG_CUSTOMDRAW_TEMP   0x80000000 // Temporary flag for
                                          // custom drawing process

// / - Control Flags Table
    static TableEntry FlagTable[] = {
    // All controls
    { _T("DISABLED"),          FLAG_DISABLED          },
    { _T("GROUP"),             FLAG_GROUP             },
    { _T("NOTABSTOP"),         FLAG_NOTABSTOP         },

    // Image controls
    { _T("RESIZETOFIT"),       FLAG_RESIZETOFIT       },
    { _T("TRANSPARENT"),       FLAG_TRANSPARENT       },

    // ProgressBar controls
    { _T("SMOOTH"),            FLAG_SMOOTH            },
    { _T("VSCROLL"),           FLAG_VSCROLL           },

    // CheckBox/RadioButton controls
    { _T("READONLY"),          FLAG_READONLY          },
    { _T("3STATE"),            FLAG_3STATE            }, // Except CheckBox

    // Button controls
    { _T("OPEN_FILEREQUEST"),  FLAG_OPEN_FILEREQUEST  },
    { _T("SAVE_FILEREQUEST"),  FLAG_SAVE_FILEREQUEST  },
    { _T("REQ_SAVE"),          FLAG_SAVE_FILEREQUEST  },
    { _T("DIRREQUEST"),        FLAG_DIRREQUEST        },
    { _T("COLORREQUEST"),      FLAG_COLORREQUEST      },
    { _T("FONTREQUEST"),       FLAG_FONTREQUEST       },
    { _T("FILE_MUST_EXIST"),   FLAG_FILE_MUST_EXIST   },
    { _T("FILE_EXPLORER"),     FLAG_FILE_EXPLORER     },
    { _T("FILE_HIDEREADONLY"), FLAG_FILE_HIDEREADONLY },
    { _T("WARN_IF_EXIST"),     FLAG_WARN_IF_EXIST     },
    { _T("PATH_MUST_EXIST"),   FLAG_PATH_MUST_EXIST   },
    { _T("PROMPT_CREATE"),     FLAG_PROMPT_CREATE     },
    { _T("BITMAP"),            FLAG_BITMAP            },
    { _T("ICON"),              FLAG_ICON              },

    // UpDown controls
    { _T("HSCROLL"),           FLAG_HSCROLL           },
    { _T("WRAP"),              FLAG_WRAP              },

    // Text/Password/RichText controls
    { _T("ONLY_NUMBERS"),      FLAG_ONLYNUMBERS       },
    { _T("MULTILINE"),         FLAG_MULTILINE         },
    { _T("WANTRETURN"),        FLAG_WANTRETURN        },
    { _T("NOWORDWRAP"),        FLAG_NOWORDWRAP        },
//    { _T("HSCROLL"),           FLAG_HSCROLL           },
//    { _T("VSCROLL"),           FLAG_VSCROLL           },
//    { _T("READONLY"),          FLAG_READONLY          },
    { _T("PASSWORD"),          FLAG_PASSWORD          }, //Except Password

    // DropList/ComboBox controls
//    { _T("VSCROLL"),           FLAG_VSCROLL           },
    { _T("DROPLIST"),          FLAG_DROPLIST          }, //Except DropList

    // DateTime controls
    { _T("UPDOWN"),            FLAG_UPDOWN            },

    // ListBox controls
    { _T("MULTISELECT"),       FLAG_MULTISELECT       },
    { _T("EXTENDEDSELECT"),    FLAG_EXTENDEDSELECT    },
    { _T("EXTENDEDSELCT"),     FLAG_EXTENDEDSELECT    },
//    { _T("VSCROLL"),           FLAG_VSCROLL           },

    // ListView/TreeView controls
    { _T("CHECKBOXES"),        FLAG_CHECKBOXES        },
    { _T("EDITLABELS"),        FLAG_EDITLABELS        },
//    { _T("MULTISELECT"),       FLAG_MULTISELECT       },
    { _T("LIST_VIEW"),         FLAG_LIST_VIEW         },
    { _T("ICON_VIEW"),         FLAG_ICON_VIEW         },
    { _T("SMALLICON_VIEW"),    FLAG_SMALLICON_VIEW    },
    { _T("REPORT_VIEW"),       FLAG_REPORT_VIEW       },

    // TrackBar controls
    { _T("NO_TICKS"),          FLAG_NO_TICKS          },
//    { _T("VSCROLL"),           FLAG_VSCROLL           },

    // MonthCalendar controls
    { _T("NOTODAY"),           FLAG_NOTODAY           },
    { _T("WEEKNUMBERS"),       FLAG_WEEKNUMBERS       },

      // Null
    { NULL,                    0                      }
  };

// / Notification Flags

#define NOTIFY_NONE                    0x00000000

// / / Control Notification Flags

// / / / Kill and set focus
#define NOTIFY_CONTROL_ONSETFOCUS      0x00000001
#define NOTIFY_CONTROL_ONKILLFOCUS     0x00000002
// / / / Open or close drop lists
#define NOTIFY_CONTROL_ONLISTOPEN      0x00000004
#define NOTIFY_CONTROL_ONLISTCLOSE     0x00000008
// / / / Change a control selection
#define NOTIFY_CONTROL_ONSELCHANGE     0x00000010
#define NOTIFY_CONTROL_ONTEXTCHANGE    0x00000020
#define NOTIFY_CONTROL_ONTEXTUPDATE    0x00000040
#define NOTIFY_CONTROL_ONTEXTSELCHANGE 0x00000080
// / / / Mouse clicks
#define NOTIFY_CONTROL_ONCLICK         0x00000100
#define NOTIFY_CONTROL_ONDBLCLICK      0x00000200
#define NOTIFY_CONTROL_ONRCLICK        0x00000400
#define NOTIFY_CONTROL_ONRDBLCLICK     0x00000800
// / / / Text being truncated
#define NOTIFY_CONTROL_ONTEXTTRUNCATE  0x00001000
// / / / Using text scrollbar
#define NOTIFY_CONTROL_ONTEXTVSCROLL   0x00002000
// / / / Image control of animation type start/end
#define NOTIFY_CONTROL_ONSTART         0x00004000
#define NOTIFY_CONTROL_ONSTOP          0x00008000

// / / - Control Notification Flags Table
  static TableEntry ControlNotifyTable[] = {
    { _T("ONSETFOCUS"),     NOTIFY_CONTROL_ONSETFOCUS     },
    { _T("ONKILLFOCUS"),    NOTIFY_CONTROL_ONKILLFOCUS    },
    { _T("ONLISTOPEN"),     NOTIFY_CONTROL_ONLISTOPEN     },
    { _T("ONLISTCLOSE"),    NOTIFY_CONTROL_ONLISTCLOSE    },
    { _T("ONSELCHANGE"),    NOTIFY_CONTROL_ONSELCHANGE    },
    { _T("ONTEXTCHANGE"),   NOTIFY_CONTROL_ONTEXTCHANGE   },
    { _T("ONTEXTUPDATE"),   NOTIFY_CONTROL_ONTEXTUPDATE   },
    { _T("ONTEXTSELCHANGE"),NOTIFY_CONTROL_ONTEXTSELCHANGE},
    { _T("ONTEXTTRUNCATE"), NOTIFY_CONTROL_ONTEXTTRUNCATE },
    { _T("ONCLICK"),        NOTIFY_CONTROL_ONCLICK        },
    { _T("ONDBLCLICK"),     NOTIFY_CONTROL_ONDBLCLICK     },
    { _T("ONRCLICK"),       NOTIFY_CONTROL_ONRCLICK       },
    { _T("ONRDBLCLICK"),    NOTIFY_CONTROL_ONRDBLCLICK    },
    { _T("ONTEXTVSCROLL"),  NOTIFY_CONTROL_ONTEXTVSCROLL  },
    { _T("ONSTART"),        NOTIFY_CONTROL_ONSTART        },
    { _T("ONSTOP"),         NOTIFY_CONTROL_ONSTOP         },
    { NULL,                 0                             }
  };

// / / Page Notification Flags

// / / / Page clicks
#define NOTIFY_PAGE_ONNEXT          0x00000001
#define NOTIFY_PAGE_ONBACK          0x00000002
#define NOTIFY_PAGE_ONCANCEL        0x00000004
// / / / Timeout feature
#define NOTIFY_PAGE_ONTIMEOUT       0x00000008

// / / - Page Notification Flags Table
  static TableEntry PageNotifyTable[] = {
    { _T("ONNEXT"),         NOTIFY_PAGE_ONNEXT            },
    { _T("ONBACK"),         NOTIFY_PAGE_ONBACK            },
    { _T("ONCANCEL"),       NOTIFY_PAGE_ONCANCEL          },
    { _T("ONTIMEOUT"),      NOTIFY_PAGE_ONTIMEOUT         },
    { NULL,                 0                             }
  };

// / Alignment Flags

// / / Horizontal
#define ALIGN_LEFT             0x00000001
#define ALIGN_CENTER           0x00000002
#define ALIGN_RIGHT            0x00000004

// / / Vertical
#define VALIGN_TOP             0x00000001
#define VALIGN_CENTER          0x00000002
#define VALIGN_BOTTOM          0x00000004

// / Text Alignment Flags

// / / Horizontal
#define ALIGN_TEXT_LEFT        0x00000001
#define ALIGN_TEXT_CENTER      0x00000002
#define ALIGN_TEXT_RIGHT       0x00000004
#define ALIGN_TEXT_JUSTIFY     0x00000008
// / / Vertical
#define VALIGN_TEXT_TOP        0x00000001
#define VALIGN_TEXT_CENTER     0x00000002
#define VALIGN_TEXT_BOTTOM     0x00000004
#define VALIGN_TEXT_JUSTIFY    0x00000008

// / Image Type Flags

#define IMAGE_TYPE_BITMAP    0
#define IMAGE_TYPE_ICON      1
#define IMAGE_TYPE_CURSOR    2
#define IMAGE_TYPE_ANIMATION 3
#define IMAGE_TYPE_OLE       4
#define IMAGE_TYPE_GDIPLUS   5

//================================================================
// Function Definitions
//================================================================

// Memory Functions (w/ Implementation)
//================================================================
void *WINAPI MALLOC(int len) { return (void*)GlobalAlloc(GPTR,len); }
void WINAPI FREE(void *d) { if (d) GlobalFree((HGLOBAL)d); }

// String Functions/Macros
//================================================================

// WARNING: lstrcpy(str, "") != ZeroMemory. lstrcpy assigns '\0' to
//          first character only. It also ignores extra '\0'
//          characters.

#define strcpy(x,y) lstrcpy(x,y)
#define strncpy(x,y,z) lstrcpyn(x,y,z)
#define strlen(x) lstrlen(x)
#define strdup(x) STRDUP(x)
#define stricmp(x,y) lstrcmpi(x,y)
//#define abs(x) ((x) < 0 ? -(x) : (x))

TCHAR *WINAPI STRDUP(const TCHAR *c)
{
  TCHAR *t=(TCHAR*)MALLOC((strlen(c)+1)*sizeof(TCHAR));
  return strcpy(t,c);
}

// Turn a pair of chars into a word
// Turn four chars into a dword
// NOTE: These codes are only when comparing WORD's and DWORD's.
#ifdef __BIG_ENDIAN__ // Not very likely, but, still...
  #define CHAR2_TO_WORD(a,b) (((WORD)(b))|((a)<<8))
  #define CHAR4_TO_DWORD(a,b,c,d) (((DWORD)CHAR2_TO_WORD(c,d))|(CHAR2_TO_WORD(a,b)<<16))
#else
  #define CHAR2_TO_WORD(a,b) (((WORD)(a))|((b)<<8))
  #define CHAR2_TO_DWORD(a,b) (((DWORD)(a))|((b)<<16))
  #define CHAR4_TO_DWORD(a,b,c,d) (((DWORD)CHAR2_TO_WORD(a,b))|(CHAR2_TO_WORD(c,d)<<16))
#endif

// Integer Functions
//================================================================
void WINAPI myitoa(TCHAR *s, int d);
void WINAPI mycrxtoa(TCHAR *s, int x);
void WINAPI myltoa(TCHAR *s, long l);

// Debug Functions
//================================================================
int WINAPI myMessageBox(TCHAR lpText);
int WINAPI myMessageBoxI(int i);

// Configuration File (.ini File) Functions
//================================================================
// TODO: Needs new header specially for configuration file reading
//       for greater support to configuration files for other
//       Operating Systems.

DWORD WINAPI myGetProfileString(LPCTSTR lpKeyName);
TCHAR * WINAPI myGetProfileStringDup(LPCTSTR lpKeyName);
int WINAPI myGetProfileInt(LPCTSTR lpKeyName, INT nDefault);
TCHAR * WINAPI myGetProfileListItemsDup(LPCTSTR lpKeyName, int nStrFlags);
TCHAR * WINAPI myGetProfileFilterItemsDup(LPCTSTR lpKeyName);

TCHAR* WINAPI IOtoa(TCHAR *str);
TCHAR* WINAPI IOtoaFolder(TCHAR *str);
void WINAPI ConvertNewLines(TCHAR *str);

// Input Functions
//================================================================
// Convert a string to internal types
void WINAPI ConvertVariables(TCHAR *str, int nMaxChars);
void WINAPI LItoIOLI(LPTSTR str, int nMaxChars, int nStrFlags, int iControlType);
void WINAPI LItoFI(LPTSTR str, int nMaxChars);
TCHAR* WINAPI IOLItoLI(LPTSTR str, int nMaxChars, int iControlType);
int WINAPI IOLI_NextItem(LPTSTR str, LPTSTR *strstart, LPTSTR *strend, int nStrFlags);
void WINAPI IOLI_RestoreItemStructure(LPTSTR str, LPTSTR *strstart, LPTSTR *strend, int nFlags);

// Convert a string to respective integer
int WINAPI LookupToken(TableEntry*, TCHAR*);
int WINAPI LookupTokens(TableEntry*, TCHAR*);
TCHAR* WINAPI LookupTokenName(TableEntry*, int);

// Find the index of a control
int WINAPI FindControlIdx(UINT id);

// Window Functions
//================================================================
LRESULT WINAPI mySendMessage(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);
void WINAPI mySetFocus(HWND hWnd);
void WINAPI mySetWindowText(HWND hWnd, LPCTSTR pszText);
void WINAPI myGetWindowText(HWND hWnd, LPTSTR pszText);

bool ConvertDlgRect(HWND hWnd, RECT* pRect, int iFrom, int iTo);

// Notification Queue Functions
//================================================================
int WINAPI AddNotifyQueueItem(NotifyQueue * nqItem);
int WINAPI RemoveNotifyQueueItem();

#define Notification(ID, code) \
  if ( (pField->nNotify & ID) == ID && codeNotify == code ) \
  { \
    g_aNotifyQueueTemp->iNotifyId = ID; \
    isNotified = true; \
    dontSimulateNext = false; \
  }

// Other Functions
//================================================================
bool WINAPI FileExists(LPTSTR pszFile); // Alternative - needs testing to see performance.

//================================================================
// Function Implementations
//================================================================

#ifndef LEGACY_PLUGIN

// Plugin callback function
//================================================================
  static UINT_PTR PluginCallback(enum NSPIM msg)
  {
     return 0;
  }

#endif

// Integer Functions
//================================================================
void WINAPI myitoa(TCHAR *s, int d)
{
  wsprintf(s,_T("%d"),d);
}

void WINAPI mycrxtoa(TCHAR *s, int x)
{
  wsprintf(s,_T("%#06x"),x);
}

void WINAPI myltoa(TCHAR *s, long l)
{
  wsprintf(s,_T("%ld"),l);
}

// Debug Functions
//================================================================
int WINAPI myMessageBox(LPCTSTR lpText)
{
  return MessageBox(0,lpText,0,MB_OK);
}

int WINAPI myMessageBoxI(int i)
{
  LPTSTR lpText = (LPTSTR)MALLOC(g_nBufferSize);
  myitoa(lpText, i);
  int nMsgBoxResult = MessageBox(0,lpText,0,MB_OK);
  FREE(lpText);
  return nMsgBoxResult;
}

// Configuration File (.ini) Functions
//================================================================
DWORD WINAPI myGetProfileString(LPCTSTR lpKeyName)
{
  *szResult = _T('\0');
  int nSize = GetPrivateProfileString(pszAppName, lpKeyName, _T(""), szResult, g_nBufferSize, pszFilename);
  if(nSize)
    ConvertVariables(szResult, g_nBufferSize);
  return nSize;
}

TCHAR * WINAPI myGetProfileStringDup(LPCTSTR lpKeyName)
{
  int nSize = myGetProfileString(lpKeyName);

  if (nSize)
    return strdup(szResult);  // uses STRDUP
  else
    return NULL;
}

int WINAPI myGetProfileInt(LPCTSTR lpKeyName, INT nDefault)
{
  LPTSTR str;
  int num;

  str = myGetProfileStringDup(lpKeyName);

  if(str)
  num = myatoi(str);
  else
    num = nDefault;

  FREE(str);
  return num;
}

TCHAR * WINAPI myGetProfileListItemsDup(LPCTSTR lpKeyName, int nStrFlags, int iControlType)
{
  int size;
  LPTSTR str = (LPTSTR)MALLOC(g_nBufferSize*sizeof(TCHAR)); // + end list char

  size = myGetProfileString(lpKeyName);

  if(size)
  {
    LItoIOLI(szResult, g_nBufferSize, nStrFlags, iControlType);

    size = lstrlen(szResult)+1;

    lstrcpy(str, szResult);

    str[size] = '\1';
    str[size+1] = '\0';
  }

  return str;
}

TCHAR * WINAPI myGetProfileFilterItemsDup(LPCTSTR lpKeyName)
{
  int size;
  LPTSTR str = (LPTSTR)MALLOC(g_nBufferSize); // + end list char

  size = myGetProfileString(lpKeyName);

  if(size)
  {
    size = lstrlen(szResult)+1;

    lstrcpy(str, szResult);
    LItoFI(str, g_nBufferSize);
  }

  return str;
}


/**
 * ConvertNewLines takes a string and turns escape sequences written
 * as separate chars e.g. "\\t" into the special char they represent
 * '\t'.  The transformation is done in place.
 *
 * @param str [in/out] The string to convert.
 */
void WINAPI ConvertNewLines(TCHAR *str) {
  TCHAR *p1, *p2, *p3;
  TCHAR tch0, tch1, nch;

  if (!str)
    return;

  p1 = p2 = str;

  while ((tch0 = *p1) != 0)
  {
    nch = 0;  // new translated char
    if (tch0 == _T('\\'))
    {
      tch1 = *(p1+1);
      
      if      (tch1 == _T('t'))  nch = _T('\t');
      else if (tch1 == _T('n'))  nch = _T('\n');
      else if (tch1 == _T('r'))  nch = _T('\r');
      else if (tch1 == _T('\\')) nch = _T('\\');
    }

    // Was it a special char?
    if (nch)
    {
      *p2++ = nch;
      p1   += 2;
    }
    else
    {
      // For MBCS
      p3 = CharNext(p1);
      while (p1 < p3)
        *p2++ = *p1++;
    }
  }

  *p2 = 0;
}

TCHAR* WINAPI IOtoa(TCHAR *pszBuffer)
{
  if (!pszBuffer)
    return pszBuffer;

  TCHAR *pszBuf2 = (TCHAR*)MALLOC((strlen(pszBuffer)+1)*2*sizeof(TCHAR)); // double the size, consider the worst case, all chars are \r\n
  TCHAR *p1, *p2;
  for (p1 = pszBuffer, p2 = pszBuf2; *p1; p1 = CharNext(p1), p2 = CharNext(p2))
  {
    switch (*p1) {
      case _T('\t'):
    *p2++ = _T('\\');
    *p2   = _T('t');
    break;
      case _T('\n'):
        *p2++ = _T('\\');
        *p2   = _T('n');
        break;
      case _T('\r'):
        *p2++ = _T('\\');
        *p2   = _T('n');
        break;
      case _T('\\'):
        *p2 = _T('\\');
        // Jim Park: used to be p2++ but that's a bug that works because
        // CharNext()'s behavior at terminating null char.  But still
        // definitely, unsafe.
      default:
        lstrcpyn(p2, p1, CharNext(p1) - p1 + 1);
        break;
    }
  }
  *p2 = 0;

  return pszBuf2;
}

TCHAR* WINAPI IOtoaFolder(TCHAR *pszBuffer)
{
  if (!pszBuffer)
    return pszBuffer;

  TCHAR *pszBuf2 = (TCHAR*)MALLOC((strlen(pszBuffer)+1)*2*sizeof(TCHAR)); // double the size, consider the worst case, all chars are \r\n
  TCHAR *p1, *p2;
  for (p1 = pszBuffer, p2 = pszBuf2; *p1; p1 = CharNext(p1), p2 = CharNext(p2))
  {
    switch (*p1) {
      case _T('\t'):
    *p2++ = _T('\\');
    *p2   = _T('t');
    break;
      case _T('\n'):
        *p2++ = _T('\\');
        *p2   = _T('n');
        break;
      case _T('\r'):
        *p2++ = _T('\\');
        *p2   = _T('n');
        break;
      case _T('\\'):
        *p2 = _T('\\');
        // Jim Park: used to be p2++ but that's a bug that works because
        // CharNext()'s behavior at terminating null char.  But still
        // definitely, unsafe.
      default:
        lstrcpyn(p2, p1, CharNext(p1) - p1 + 1);
        break;
    }
  }
  *p2 = 0;

  return pszBuf2;
}


// Input Functions
//================================================================
void WINAPI ConvertVariables(TCHAR *str, int nMaxChars) {

  if (!str)
    return;

  TCHAR *p1, *p2, *p3, *p4, *p5;

  p1 = p2 = str;
  p4 = p5 = (TCHAR*)MALLOC(nMaxChars*sizeof(TCHAR));

  while (*p1 && lstrlen(p2) <= nMaxChars)
  {
  if(*(LPBYTE)p1 == '$')
  {
    p1++;

    if(*(LPBYTE)p1 == '$')
    {
      p1++;

      *p2 = '$';
      p2++;
    }
    else if(*(LPBYTE)p1 >= '0' && *(LPBYTE)p1 <= '9')
    {
      p1++;

      if(!(p4 = getuservariable(myatoi(p1))))
        p4 = _T("");

      while (*p4 && lstrlen(p2) <= nMaxChars)
      {
        p5 = CharNext(p4);
        while (p4 < p5 && lstrlen(p2) <= nMaxChars)
        {
          *p2++ = *p4++;
        }
      }
    }
    else if(*(LPBYTE)p1 == 'R')
    {
      p1++;

      if(*(LPBYTE)p1 >= '0' && *(LPBYTE)p1 <= '9')
      {
        p1++;

        if(!(p4 = getuservariable(myatoi(p1)+ 10)))
          p4 = _T("");

        while (*p4 && lstrlen(p2) <= nMaxChars)
        {
          p5 = CharNext(p4);
          while (p4 < p5 && lstrlen(p2) <= nMaxChars)
          {
            *p2++ = *p4++;
          }
        }
      }
      else
      {
        p1 -= 2;

        p3 = CharNext(p1);
        while (p1 < p3 && lstrlen(p2) <= nMaxChars)
          *p2++ = *p1++;
      }
    }
    else
    {
      p1--;

      p3 = CharNext(p1);
      while (p1 < p3 && lstrlen(p2) <= nMaxChars)
        *p2++ = *p1++;
    }
  }
  else
  {
      p3 = CharNext(p1);
      while (p1 < p3 && lstrlen(p2) <= nMaxChars)
        *p2++ = *p1++;
  }
  }

  *p2 = 0;

  FREE(p4);
}

// This function converts a list items (not necessarily from the user)
// and converts characters to be used internally. It converts:
// '|' -> '\x01'
// '{' -> '\x02'
// '}' -> '\x03'
// '\\|' -> '|'
// '\\{' -> '{'
// '\\}' -> '}'

// This is done because it becomes easier to manipulate the search
// for strings into a list of items.

// A list of items could be somewhat called an array, except that
// it doesn't hold the exact location of the items, but gives
// enough reference to where the item starts or ends.

void WINAPI LItoIOLI(TCHAR *str, int nMaxChars, int nStrFlags, int iControlType)
{
  //nStrFlags
  // 0 = Accept '|', '{' and '}' characters.
  // 1 = Except '|'.
  // 2 = Except '{' and '}'.

  if (!str)
    return;

  TCHAR *p1, *p2, *p3;

  p1 = p2 = str;

  while (*p1 && lstrlen(p2) <= nMaxChars)
  {
    if (*(LPWORD)p1 == CHAR2_TO_WORD('\\', '\\'))
  {
    *p2 = '\\';
    p1 += 2;
    p2++;
  }
    else if (*(LPWORD)p1 == CHAR2_TO_WORD('\\', 'r') && (iControlType == FIELD_COMBOBOX || iControlType == FIELD_LISTBOX))
  {
    *p2 = '\r';
    p1 += 2;
    p2++;
  }
    else if (*(LPWORD)p1 == CHAR2_TO_WORD('\\', 'n') && (iControlType == FIELD_COMBOBOX || iControlType == FIELD_LISTBOX))
  {
    *p2 = '\n';
    p1 += 2;
    p2++;
  }
    else if (*(LPWORD)p1 == CHAR2_TO_WORD('\\', 't') && (iControlType == FIELD_COMBOBOX || iControlType == FIELD_LISTBOX))
  {
    *p2 = '\t';
    p1 += 2;
    p2++;
  }
    else if (*(LPWORD)p1 == CHAR2_TO_WORD('\\', '|'))
  {
    *p2 = '|';
    p1 += 2;
    p2++;
  }
  else if (*(LPWORD)p1 == CHAR2_TO_WORD('\\', '{'))
  {
    *p2 = '{';
    p1 += 2;
    p2++;
  }
  else if (*(LPWORD)p1 == CHAR2_TO_WORD('\\', '}'))
  {
    *p2 = '}';
    p1 += 2;
    p2++;
  }
    else if (*(LPBYTE)p1 == '|' && nStrFlags != 1)
  {
    *p2 = '\x01';
    p1++;
    p2++;
  }
  else if (*(LPBYTE)p1 == '{' && nStrFlags != 2)
  {
    *p2 = '\x02';
    p1++;
    p2++;
  }
  else if (*(LPBYTE)p1 == '}' && nStrFlags != 2)
  {
    *p2 = '\x03';
    p1++;
    p2++;
  }
    else
  {
      p3 = CharNext(p1);
      while (p1 < p3)
        *p2++ = *p1++;
    }
  }

  *p2 = 0;
}

// This function is specially used by the "Filter" value name
// to convert a list items (not necessarily from the user) to
// the supported syntax for the filter of directories. It converts:
// '|' -> '\0'
// '\\|' -> '|'
// the ending '\0' -> '\0\0'

void WINAPI LItoFI(TCHAR *str, int nMaxChars)
{
  if (!str)
    return;

  TCHAR *p1, *p2, *p3;

  p1 = p2 = str;

  while (*p1 && lstrlen(p2) <= nMaxChars)
  {
    if (*(LPWORD)p1 == CHAR2_TO_WORD('\\', '|'))
  {
    *p2 = '|';
    p1 += 2;
    p2++;
  }
  else if (*(LPBYTE)p1 == '\0')
  {
    *p2 = '\0';
    p1++;
  }
  else if (*(LPBYTE)p1 == '|')
  {
    *p2 = '\0';
    p1++;
    p2++;
  }
    else
  {
      p3 = CharNext(p1);
      while (p1 < p3)
        *p2++ = *p1++;
    }
  }

  *p2 = 0;
}

// This function converts a list items used internally to one
// to be returned to the user. It converts:
// '|' -> '\\|'
// '{' -> '\\{'
// '}' -> '\\}'
// '\x01' -> '|'
// '\x02' -> '{'
// '\x03' -> '}'

// This is done because the user needs to get the string back to
// be reused again on the next call to the plug-in.

TCHAR* WINAPI IOLItoLI(TCHAR *str, int nMaxChars, int iControlType)
{
  if (!str)
    return str;

  TCHAR *newstr = (TCHAR*)MALLOC(lstrlen(str)*2+1);
  TCHAR *p1, *p2;

  for (p1 = str, p2 = newstr; *p1; p1 = CharNext(p1), p2 = CharNext(p2))
  {
    if (*(LPBYTE)p1 == '\\')
  {
        *(LPWORD)p2 = CHAR2_TO_WORD('\\', '\\');
        p2++;
    }
  else if (*(LPBYTE)p1 == '\r' && (iControlType == FIELD_LISTBOX || iControlType == FIELD_COMBOBOX))
  {
        *(LPWORD)p2 = CHAR2_TO_WORD('\\', 'r');
        p2++;
  }
  else if (*(LPBYTE)p1 == '\n' && (iControlType == FIELD_LISTBOX || iControlType == FIELD_COMBOBOX))
  {
        *(LPWORD)p2 = CHAR2_TO_WORD('\\', 'n');
        p2++;
  }
  else if (*(LPBYTE)p1 == '\t' && (iControlType == FIELD_LISTBOX || iControlType == FIELD_COMBOBOX))
  {
        *(LPWORD)p2 = CHAR2_TO_WORD('\\', 't');
        p2++;
  }
  else if (*(LPBYTE)p1 == '|')
  {
        *(LPWORD)p2 = CHAR2_TO_WORD('\\', '|');
        p2++;
  }
  else if (*(LPBYTE)p1 == '{')
  {
        *(LPWORD)p2 = CHAR2_TO_WORD('\\', '{');
        p2++;
  }
  else if (*(LPBYTE)p1 == '}')
  {
        *(LPWORD)p2 = CHAR2_TO_WORD('\\', '}');
        p2++;
  }
  else if (*(LPBYTE)p1 == '\x01')
  {
        *(LPBYTE)p2 = '|';
        p2++;
  }
  else if (*(LPBYTE)p1 == '\x02')
  {
        *(LPBYTE)p2 = '{';
        p2++;
  }
  else if (*(LPBYTE)p1 == '\x03')
  {
        *(LPBYTE)p2 = '}';
        p2++;
  }
  else
  {
        strncpy(p2, p1, CharNext(p1) - p1 + 1);
    }
  }
  *p2 = 0;
  return newstr;
}

int WINAPI IOLI_NextItem(LPTSTR str, TCHAR **strstart, TCHAR **strend, int nStrFlags)
{
  int nFlags = 0;
  // nFlags
  // 0 = The string is in format "ListItems". It's not an item.
  // 1 = It's a normal item.
  // 2 = Item ending w/ '\x02' - '{'.
  // 3 = Item ending w/ '\x03' - '}'.
  // 4 = End of string, but there is string.
  // 5 = End of string + item starting w/ '\x03' - '}'.
  // 6 = End of string.

  // nStrFlags
  // 0 = Accept '|', '{' and '}' characters.
  // 1 = Accept only '|'.
  // 2 = Accept only '{' and '}'.

  if (!str)
    return nFlags;

  //strend = current item in the list

  if(*(LPBYTE)*strend == '\x03')
  nFlags = 5;
  else
  nFlags = 4;

  if(*(LPBYTE)*strend == '\0')
  nFlags = 6;

  while (*(LPBYTE)*strend)
  {
  if(*(LPBYTE)*strend == '\x01')
  {
      if(nStrFlags == 2)
    *(LPBYTE)*strend = '|';
      else
    {
      *(LPBYTE)*strend = '\0';
    return 1;
    }
  }

  if(*(LPBYTE)*strend == '\x02')
  {
      if(nStrFlags == 1)
    *(LPBYTE)*strend = '{';
    else
    {
      *(LPBYTE)*strend = '\0';
    return 2;
    }
  }

  if(*(LPBYTE)*strend == '\x03')
  {
      if(nStrFlags == 1)
    *(LPBYTE)*strend = '}';
    else
    {
      *(LPBYTE)*strend = '\0';
    return 3;
    }
  }

    *strend = CharNext(*strend);
  }

  return nFlags;
}

void WINAPI IOLI_RestoreItemStructure(LPTSTR str, LPTSTR *strstart, LPTSTR *strend, int nFlags)
{
  // nFlags
  // 0 = The string is in format "ListItems". It's not an item.
  // 1 = It's a normal item.
  // 2 = Item ending w/ '\x02' - '{'.
  // 3 = Item ending w/ '\x03' - '}'.
  // 4 = End of string.

  switch(nFlags)
  {
    case 1:
  {
    **strend = '\x01';
    *strstart = ++*strend;
    break;
  }
    case 2:
  {
    **strend = '\x02';
    *strstart = ++*strend;
    break;
  }
    case 3:
  {
    **strend = '\x03';
    *strstart = ++*strend;
    break;
  }
  }

  return;
}

TCHAR* WINAPI LookupTokenName(TableEntry* psTable_, int nValue)
{
  for (int i = 0; psTable_[i].nValue; i++)
    if (nValue == psTable_[i].nValue)
      return psTable_[i].pszName;
  return 0;
}

/**
 * Looks up a single token in the psTable_ and returns its mapped numerical value.
 *
 * @param psTable_ The lookup table.
 * @param pszToken_ The token to lookup.
 * @return The integer value related to the token, otherwise 0.
 */
int WINAPI LookupToken(TableEntry* psTable_, TCHAR* pszToken_)
{
  for (int i = 0; psTable_[i].pszName; i++)
    if (!lstrcmpi(pszToken_, psTable_[i].pszName))
      return psTable_[i].nValue;
  return 0;
}

/**
 * In a string of tokens separated by vertical bars '|', look them up in the
 * Lookup Table psTable and return their logical OR of their subsequent
 * integer values.
 * 
 * @param psTable_ The lookup table to search in.
 * @param pszToken String of tokens separated by '|' whose values are to be
 * ORed together.
 * @return The ORed value of the token values.  If no tokens were found, it
 * will return 0.
 */
int WINAPI LookupTokens(TableEntry* psTable_, TCHAR* pszTokens_)
{
  int n = 0;
  TCHAR *pszStart = pszTokens_;
  TCHAR *pszEnd = pszTokens_;
  for (;;) {
    TCHAR c = *pszEnd;
    if (c == _T('|') || c == _T('\0')) {
      *pszEnd = _T('\0');
      n |= LookupToken(psTable_, pszStart);
      *pszEnd = c;
      if (!c)
        break;
      pszStart = ++pszEnd;
    }
    else
      pszEnd = CharNext(pszEnd);
  }
  return n;
}

int WINAPI FindControlIdx(UINT id)
{
  for (int nIdx = 0; nIdx < nNumFields; nIdx++)
    if (id == pFields[nIdx].nControlID)
      return nIdx;
  return -1;
}

// Window Functions
//================================================================
LRESULT WINAPI mySendMessage(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam)
{
  return SendMessage(hWnd, Msg, wParam, lParam);
}

void WINAPI mySetFocus(HWND hWnd)
{
  mySendMessage(hMainWindow, WM_NEXTDLGCTL, (WPARAM)hWnd, TRUE);
}

void WINAPI mySetWindowText(HWND hWnd, LPCTSTR pszText)
{
  if (pszText)
    SetWindowText(hWnd, pszText);
}

void WINAPI myGetWindowText(HWND hWnd, LPTSTR pszText)
{
  GetWindowText(hWnd, pszText, GetWindowTextLength(hWnd)+1);
}

bool ConvertDlgRect(HWND hWnd, RECT* pRect, int iFrom, int iTo)
{
  // iFrom and iTo:
  // 0x0 = Pixels
  // 0x1 = Dialog units

  if(iFrom == 0x0 && iTo == 0x1)
  {
    RECT pTempRect = {0, 0, 4, 8};

    if(!MapDialogRect(hWnd, &pTempRect))
    return 0;
    int baseUnitY = pTempRect.bottom;
    int baseUnitX = pTempRect.right;

    pRect->left = (pRect->left * baseUnitX) / 4;
    pRect->right = (pRect->right * baseUnitX) / 4;

    pRect->top = (pRect->top * baseUnitX) / 8;
    pRect->bottom = (pRect->bottom * baseUnitX) / 8;
  }
  else if(iFrom == 0x1 && iTo == 0x0)
  {
    if(!MapDialogRect(hWnd, pRect))
      return 0;
  }

  return 1;
}

//================================================================
// Notify Queue Functions Definitions
//================================================================
int WINAPI AddNotifyQueueItem(NotifyQueue * nqItem)
{
  int id = NOTIFY_QUEUE_AMOUNT_MAX - 1;

  while(id >= 0 && g_aNotifyQueue[id].iNotifyId == NOTIFY_NONE)
  {
    --id;
  }

  ++id;

  if(id > NOTIFY_QUEUE_AMOUNT_MAX - 1)
    return -1;

  g_aNotifyQueue[id].iNotifyId = nqItem[0].iNotifyId;
  g_aNotifyQueue[id].iField = nqItem[0].iField;
  g_aNotifyQueue[id].bNotifyType = nqItem[0].bNotifyType;

  nqItem[0].iNotifyId = NOTIFY_NONE;
  nqItem[0].iField = 0;
  nqItem[0].bNotifyType = NOTIFY_TYPE_PAGE;

  return 0;
}

int WINAPI RemoveNotifyQueueItem()
{
  int id = 0;

  while(++id && id < NOTIFY_QUEUE_AMOUNT_MAX)
  {
    g_aNotifyQueue[id-1].iNotifyId = g_aNotifyQueue[id].iNotifyId;
    g_aNotifyQueue[id-1].iField = g_aNotifyQueue[id].iField;
    g_aNotifyQueue[id-1].bNotifyType = g_aNotifyQueue[id].bNotifyType;

    g_aNotifyQueue[id].iNotifyId = NOTIFY_NONE;
    g_aNotifyQueue[id].iField = 0;
    g_aNotifyQueue[id].bNotifyType = NOTIFY_TYPE_PAGE;
  }
  return 0;
}

// Other Functions
//================================================================

// Alternative for "PathFileExists" from shlwapi.h.
bool WINAPI FileExists(LPTSTR pszFile)
{
  HANDLE h;
  static WIN32_FIND_DATA fd;
  // Avoid a "There is no disk in the drive" error box on empty removable drives
  SetErrorMode(SEM_NOOPENFILEERRORBOX | SEM_FAILCRITICALERRORS);
  h = FindFirstFile(pszFile,&fd);
  SetErrorMode(0);
  if (h != INVALID_HANDLE_VALUE)
    return TRUE;
  return FALSE;
}

//================================================================
// Post-Call Functions (see InstallOptions.cpp)
//================================================================

//Pre function part
int WINAPI createCfgDlg();

//Show function part
void WINAPI showCfgDlg();

BOOL CALLBACK ParentWndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);
BOOL CALLBACK DialogWindowProc(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam);
int WINAPI ControlWindowProc(HWND hWin, UINT uMsg, WPARAM wParam, LPARAM lParam);
LRESULT WINAPI NotifyProc(HWND hWnd, UINT id, HWND hwndCtl, UINT codeNotify);

//Reading and writing operations
int WINAPI ReadSettings();
bool WINAPI SaveSettings(void);

//Other main functions
bool INLINE ValidateFields();

//================================================================
// Include Control Implementations
//================================================================

#include "Controls\Label.h"          // FIELD_LABEL
#include "Controls\GroupBox.h"       // FIELD_GROUPBOX
#include "Controls\Image.h"          // FIELD_IMAGE
#include "Controls\ProgressBar.h"    // FIELD_PROGRESSBAR
#include "Controls\Link.h"           // FIELD_LINK
#include "Controls\Button.h"         // FIELD_BUTTON
#include "Controls\UpDown.h"         // FIELD_UPDOWN
#include "Controls\CheckBox.h"       // FIELD_CHECKBOX
#include "Controls\RadioButton.h"    // FIELD_RADIOBUTTON
#include "Controls\Text.h"           // FIELD_TEXT
#include "Controls\IPAddress.h"      // FIELD_IPADDRESS
#include "Controls\RichText.h"       // FIELD_RICHTEXT
#include "Controls\ComboBox.h"       // FIELD_COMBOBOX
#include "Controls\DateTime.h"       // FIELD_DATETIME
#include "Controls\ListBox.h"        // FIELD_LISTBOX
#include "Controls\ListView.h"       // FIELD_LISTVIEW
#include "Controls\TreeView.h"       // FIELD_TREEVIEW
#include "Controls\TrackBar.h"       // FIELD_TRACKBAR
#include "Controls\MonthCalendar.h"  // FIELD_MONTHCALENDAR

#pragma message(" ")
#pragma message("----------------------------------------------------")
#ifndef LEGACY_PLUGIN
	#pragma message(" Info: Compilation using the new NSIS plugin API")
#else
	#pragma message(" Info: Compilation using the legacy NSIS plugin API")
#endif
#pragma message("----------------------------------------------------")
#pragma message(" ")
