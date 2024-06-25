/*
o------------------------------------------------------------------------------------o
|InstallOptionsEx 2.4.5 beta 3                                                       |
|Based on InstallOptions 2.47 (Git version: d20b99c082a99304a542c2fa6cf4abd381ca263a)|
(------------------------------------------------------------------------------------)
| Main source code.                                     / A plug-in for NSIS 2 and 3 |
|                                                      ------------------------------|
| By deguix (see copyright notes on readme)                                          |
| And by SuperPat since 2.4.5 beta 1                                                 |
| And by Tallmaris (Leandro Tramma): Initial unicode compatibility                   |
o------------------------------------------------------------------------------------o
*/

#include "InstallerOptions.h"

//Implementation
extern "C" void main(){}

//================================================================
// DLLMain
//================================================================
extern "C" BOOL WINAPI DllMain(HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
  m_hInstance=(HINSTANCE) hInst;
  return TRUE;
}

//================================================================
// External Functions
//================================================================

// TODO: Add another call for changing item properties.
// TODO: Add another call for adding external controls.

// TODO: Expand functions to only show a specific main window.

// show Function
//================================================================
extern "C" void __declspec(dllexport) show(HWND hwndParent, int string_size,
                                      TCHAR *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  if (!initCalled) {
    pushstring(_T("error"));
    return;
  }
  initCalled--;
  showCfgDlg();
}


// make_unicode Function
//================================================================
#ifdef _UNICODE
// convert ini file to Unicode so that WritePrivateProfileString can write Unicode strings in it
extern "C" void __declspec(dllexport) make_unicode(HWND hwndParent, int string_size,
                                      TCHAR *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  TCHAR filename[MAX_PATH];
  popstring(filename);
  HANDLE hFile = CreateFile(filename, GENERIC_READ|GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
  if (hFile != INVALID_HANDLE_VALUE)
  {
    DWORD fSize = GetFileSize(hFile, NULL);
    LPSTR lpBuffer = (LPSTR) MALLOC(fSize);
    if (lpBuffer)
    {
        DWORD bytes;
        ReadFile(hFile, lpBuffer, fSize, &bytes, NULL);
        if ((bytes < 2) || (lpBuffer[0] != '\xFF') || (lpBuffer[1] != '\xFE')) // file is not already Unicode
        {
            LPWSTR lpWide = (LPWSTR) MALLOC((bytes+1)*2);
            if (lpWide)
            {
                int cch = MultiByteToWideChar(CP_ACP, 0, lpBuffer, bytes, lpWide, bytes+1);
                if (cch)
                {
                    SetFilePointer(hFile, 0, NULL, FILE_BEGIN);
                    WriteFile(hFile, "\xFF\xFE", 2, &bytes, NULL); // write Unicode BOM
                    WriteFile(hFile, lpWide, cch*2, &bytes, NULL);
                    SetEndOfFile(hFile);
                }
                FREE(lpWide);
            }
        }
        FREE(lpBuffer);
    }
    CloseHandle(hFile);
  }
}
#endif

// dialog Function
//================================================================
extern "C" void __declspec(dllexport) dialog(HWND hwndParent, int string_size,
                                      TCHAR *variables, stack_t **stacktop)
{
  hMainWindow=hwndParent;
  EXDLL_INIT();
  if (initCalled) {
    pushstring(_T("error"));
    return;
  }
  if (createCfgDlg())
    return;
  popstring(NULL);
  showCfgDlg();
}

// initDialog Function
//================================================================

#ifndef LEGACY_PLUGIN
  extern "C" void __declspec(dllexport) initDialog(HWND hwndParent, int string_size,
                                        TCHAR *variables, stack_t **stacktop,
                                        extra_parameters *extra)
#else
  extern "C" void __declspec(dllexport) initDialog(HWND hwndParent, int string_size,
                                        TCHAR *variables, stack_t **stacktop)
#endif
{
  printf("CIAO");
  hMainWindow=hwndParent;

  EXDLL_INIT();

#ifndef LEGACY_PLUGIN
  extra->RegisterPluginCallback(m_hInstance, PluginCallback);
#endif

  if (initCalled) {
    pushstring(_T("error"));
    return;
  }
  if (createCfgDlg())
    return;
  initCalled++;
}

//  setFocus Function
//================================================================
#ifndef LEGACY_PLUGIN
  extern "C" void __declspec(dllexport) setFocus(HWND hwndParent, int string_size,
                                        TCHAR *variables, stack_t **stacktop,
                                        extra_parameters *extra)
#else
  extern "C" void __declspec(dllexport) setFocus(HWND hwndParent, int string_size,
                                        TCHAR *variables, stack_t **stacktop)
#endif
{
  EXDLL_INIT();

#ifndef LEGACY_PLUGIN
  extra->RegisterPluginCallback(m_hInstance, PluginCallback);
#endif

  // Get the HWND of the control
  TCHAR szhwCtrl[10];
  popstring(szhwCtrl);

  // Convert the string into an HWND
  HWND hwnd = 0;
  _stscanf_s(szhwCtrl,_T("%d"),&hwnd);

  // Change Focus
  mySetFocus(hwnd);
}

//  skipValidation Function
//================================================================
#ifndef LEGACY_PLUGIN
  extern "C" void __declspec(dllexport) skipValidation(HWND hwndParent, int string_size,
                                        TCHAR *variables, stack_t **stacktop,
                                        extra_parameters *extra)
#else
  extern "C" void __declspec(dllexport) skipValidation(HWND hwndParent, int string_size,
                                        TCHAR *variables, stack_t **stacktop)
#endif
{
  EXDLL_INIT();

#ifndef LEGACY_PLUGIN
  extra->RegisterPluginCallback(m_hInstance, PluginCallback);
#endif

  g_is_cancel = true;
}



//================================================================
// Post-Call Function Implementations
//================================================================





//----------------------------------------------------------------
// Pre function part
//================================================================

BOOL MakeTransparentAreaFromBitmap(HWND hwCtrl, HBITMAP hBitmap)
{
    // based on AdvSplash's SetTransparentRegion
    BITMAP bm;
//    HBITMAP hBitmap = (HBITMAP) pField->hImage;

    if (GetObject(hBitmap, sizeof(bm), &bm))
    {
      HDC dc;
      int x, y;
      HRGN region, cutrgn;
      BITMAPINFO bmi;
      int size = bm.bmWidth * bm.bmHeight * sizeof(int);
      int *bmp = (int *) MALLOC(size);
      if (bmp)
      {
        bmi.bmiHeader.biBitCount = 32;
        bmi.bmiHeader.biCompression = BI_RGB;
        bmi.bmiHeader.biHeight = bm.bmHeight;
        bmi.bmiHeader.biPlanes = 1;
        bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
        bmi.bmiHeader.biWidth = bm.bmWidth;
        bmi.bmiHeader.biClrUsed = 0;
        bmi.bmiHeader.biClrImportant = 0;

        dc = CreateCompatibleDC(NULL);
        SelectObject(dc, hBitmap);

        x = GetDIBits(dc, hBitmap, 0, bm.bmHeight, bmp, &bmi, DIB_RGB_COLORS);

        region = CreateRectRgn(0, 0, bm.bmWidth, bm.bmHeight);

        int keycolor = *bmp & 0xFFFFFF;

        // Search for transparent pixels
        for (y = bm.bmHeight - 1; y >= 0; y--) {
            for (x = 0; x < bm.bmWidth;) {
            if ((*bmp & 0xFFFFFF) == keycolor) {
                int j = x;
                while ((x < bm.bmWidth) && ((*bmp & 0xFFFFFF) == keycolor)) {
                    bmp++, x++;
                }

                // Cut transparent pixels from the original region
                cutrgn = CreateRectRgn(j, y, x, y + 1);
                CombineRgn(region, region, cutrgn, RGN_XOR);
                DeleteObject(cutrgn);
            } else {
                bmp++, x++;
            }
          }
        }

        // Set resulting region.
        SetWindowRgn(hwCtrl, region, TRUE);
        DeleteObject(region);
        DeleteObject(dc);
        FREE(bmp);
        return TRUE;
      }
    }
  //}
  return FALSE;
}

int WINAPI createCfgDlg()
{
  // TODO: Make NSIS controls be handled by the plug-in as an option

// Initialize Variables
//================================================================
  g_is_back=false;
  g_is_cancel=false;
  g_is_timeout=false;

// Detect If A Main Window Exists
//================================================================
  HWND mainwnd = hMainWindow;
  if (!mainwnd)
  {
    popstring(NULL);
    pushstring(_T("error finding mainwnd"));
    return 1; // cannot be used in silent mode unfortunately.
  }

// Detect If Settings Were Loaded
//================================================================
  if (!g_stacktop || !*g_stacktop || !(pszFilename = (*g_stacktop)->text) || !pszFilename[0] || !ReadSettings())
  {
    popstring(NULL);
    pushstring(_T("error finding config"));
    return 1;
  }

// Detect If Child Window Exists
//================================================================
  HWND childwnd=GetDlgItem(mainwnd,nRectId);
  if (!childwnd)
  {
    popstring(NULL);
    pushstring(_T("error finding childwnd"));
    return 1;
  }

// NSIS Main Buttons Configurations
//================================================================
  hCancelButton = GetDlgItem(mainwnd,IDCANCEL);
  hNextButton = GetDlgItem(mainwnd,IDOK);
  hBackButton = GetDlgItem(mainwnd,3);

  mySetWindowText(hCancelButton,pszCancelButtonText);
  mySetWindowText(hNextButton,pszNextButtonText);
  mySetWindowText(hBackButton,pszBackButtonText);

  if(bNextShow!=-1)
    old_next_visible=ShowWindow(hNextButton,bNextShow?SW_SHOWNA:SW_HIDE);
  else
  {
    old_next_visible=ShowWindow(hNextButton,SW_SHOWNA);
    ShowWindow(hNextButton,old_next_visible?SW_SHOWNA:SW_HIDE);
  }

  if(bBackShow!=-1)
    old_back_visible=ShowWindow(hBackButton,bBackShow?SW_SHOWNA:SW_HIDE);
  else
  {
    old_back_visible=ShowWindow(hBackButton,SW_SHOWNA);
    ShowWindow(hBackButton,old_back_visible?SW_SHOWNA:SW_HIDE);
  }

  if(bCancelShow==-1)
    old_cancel_visible=ShowWindow(hCancelButton,bCancelShow?SW_SHOWNA:SW_HIDE);
  else
  {
    old_cancel_visible=ShowWindow(hCancelButton,SW_SHOWNA);
    ShowWindow(hCancelButton,old_cancel_visible?SW_SHOWNA:SW_HIDE);
  }

  old_next_enabled = IsWindowEnabled(hNextButton);
  old_back_enabled = IsWindowEnabled(hBackButton);
  old_cancel_enabled = IsWindowEnabled(hCancelButton);

  if (bNextEnabled!=-1)
    EnableWindow(hNextButton,bNextEnabled);
  else
    EnableWindow(hNextButton,old_next_enabled);

  if (bNextShow!=-1)
    ShowWindow(hNextButton,bNextShow?SW_SHOWNA:SW_HIDE);
  else
    ShowWindow(hNextButton,old_next_visible?SW_SHOWNA:SW_HIDE);


  if (bBackEnabled!=-1)
    EnableWindow(hBackButton,bBackEnabled);
  else
    EnableWindow(hBackButton,old_back_enabled);

  if (bBackShow!=-1)
    ShowWindow(hBackButton,bBackShow?SW_SHOWNA:SW_HIDE);
  else
    ShowWindow(hBackButton,old_back_visible?SW_SHOWNA:SW_HIDE);


  if (bCancelEnabled!=-1)
  {
    EnableWindow(hCancelButton,bCancelEnabled);
    if (bCancelEnabled)
        EnableMenuItem(GetSystemMenu(mainwnd, FALSE), SC_CLOSE, MF_BYCOMMAND | MF_ENABLED);
    else
        EnableMenuItem(GetSystemMenu(mainwnd, FALSE), SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
  }
  else
    EnableWindow(hCancelButton,old_cancel_enabled);

  if (bCancelShow!=-1)
    ShowWindow(hCancelButton,bCancelShow?SW_SHOWNA:SW_HIDE);
  else
    ShowWindow(hCancelButton,old_cancel_visible?SW_SHOWNA:SW_HIDE);

// Create Window
//================================================================
  HFONT hFont = (HFONT)mySendMessage(mainwnd, WM_GETFONT, 0, 0);

  // Prevent WM_COMMANDs from being processed while we are building
  g_done = 1;

  // TODO: Use RegisterClassEx to create true GUI windows (not dialogs - CreateDialog - like now)
  //       http://www.functionx.com/win32/Lesson05.htm

  // TODO: Make loops for each main window/dialog and each control to create.

  int mainWndWidth, mainWndHeight;
  hConfigWindow=CreateDialog(m_hInstance,MAKEINTRESOURCE(IDD_DIALOG1),mainwnd,DialogWindowProc);
  if (!hConfigWindow)
  {
    popstring(NULL);
    pushstring(_T("error creating dialog"));
    return 1;
  }

  RECT dialog_r;
  GetWindowRect(childwnd,&dialog_r);
  MapWindowPoints(0, mainwnd, (LPPOINT) &dialog_r, 2);
  mainWndWidth = dialog_r.right - dialog_r.left;
  mainWndHeight = dialog_r.bottom - dialog_r.top;
  SetWindowPos(
    hConfigWindow,
    0,
    dialog_r.left,
    dialog_r.top,
    mainWndWidth,
    mainWndHeight,
    SWP_NOZORDER|SWP_NOACTIVATE
  );
  // Sets the font of IO window to be the same as the main window
  mySendMessage(hConfigWindow, WM_SETFONT, (WPARAM)hFont, TRUE);

  BOOL fFocused = FALSE;
  BOOL fFocusedByFlag = FALSE;

// Identify Styles For Each Control Type
//================================================================

  for (int nIdx = 0; nIdx < nNumFields; nIdx++) {
    IOExControlStorage *pField = pFields + nIdx;

    static struct {
      const TCHAR* pszClass;
      DWORD dwStyle;
      DWORD dwRTLStyle;
      DWORD dwExStyle;
      DWORD dwRTLExStyle;
    } ClassTable[] = {
    { _T("STATIC"),           // FIELD_HLINE
      DEFAULT_STYLES | SS_ETCHEDHORZ | SS_SUNKEN,
      DEFAULT_STYLES | SS_ETCHEDHORZ | SS_SUNKEN,
      WS_EX_TRANSPARENT,
      WS_EX_TRANSPARENT | RTL_EX_STYLES },
    { _T("STATIC"),           // FIELD_VLINE
      DEFAULT_STYLES | SS_ETCHEDVERT | SS_SUNKEN,
      DEFAULT_STYLES | SS_ETCHEDVERT | SS_SUNKEN,
      WS_EX_TRANSPARENT,
      WS_EX_TRANSPARENT | RTL_EX_STYLES },
    { _T("STATIC"),           // FIELD_LABEL
      DEFAULT_STYLES | SS_OWNERDRAW | SS_NOTIFY,
      DEFAULT_STYLES | SS_OWNERDRAW | SS_NOTIFY,
      WS_EX_TRANSPARENT,
      WS_EX_TRANSPARENT | RTL_EX_STYLES },
    { _T("BUTTON"),           // FIELD_GROUPBOX
      DEFAULT_STYLES | BS_GROUPBOX,
      DEFAULT_STYLES | BS_GROUPBOX | BS_RIGHT,
      WS_EX_TRANSPARENT,
      WS_EX_TRANSPARENT | RTL_EX_STYLES },
    { _T("IMAGE"),            // FIELD_IMAGE // Representation for both "Static" and "Animation" controls.
      DEFAULT_STYLES,
      DEFAULT_STYLES,
      0,
      RTL_EX_STYLES },
    { PROGRESS_CLASS,         // FIELD_PROGRESSBAR
      DEFAULT_STYLES,
      DEFAULT_STYLES,
      0,
      RTL_EX_STYLES },
    { _T("BUTTON"),           // FIELD_LINK
      DEFAULT_STYLES | WS_TABSTOP | BS_OWNERDRAW | BS_NOTIFY,
      DEFAULT_STYLES | WS_TABSTOP | BS_OWNERDRAW | BS_RIGHT | BS_NOTIFY,
      0,
      RTL_EX_STYLES },
    { _T("BUTTON"),           // FIELD_BUTTON
      DEFAULT_STYLES | WS_TABSTOP | BS_MULTILINE | BS_NOTIFY,
      DEFAULT_STYLES | WS_TABSTOP | BS_MULTILINE | BS_NOTIFY,
      0,
      RTL_EX_STYLES },
    { UPDOWN_CLASS,           // FIELD_UPDOWN
      DEFAULT_STYLES | WS_TABSTOP | UDS_ARROWKEYS | UDS_NOTHOUSANDS | UDS_SETBUDDYINT | UDS_ALIGNRIGHT,
      DEFAULT_STYLES | WS_TABSTOP | UDS_ARROWKEYS | UDS_NOTHOUSANDS | UDS_SETBUDDYINT | UDS_ALIGNLEFT,
      0,
      RTL_EX_STYLES },
    { _T("BUTTON"),           // FIELD_CHECKBOX
      DEFAULT_STYLES | WS_TABSTOP | BS_TEXT | BS_VCENTER | BS_AUTOCHECKBOX | BS_MULTILINE | BS_NOTIFY,
      DEFAULT_STYLES | WS_TABSTOP | BS_TEXT | BS_VCENTER | BS_AUTOCHECKBOX | BS_MULTILINE | BS_RIGHT | BS_LEFTTEXT | BS_NOTIFY,
      0,
      RTL_EX_STYLES },
    { _T("BUTTON"),           // FIELD_RADIOBUTTON
      DEFAULT_STYLES | WS_TABSTOP | BS_TEXT | BS_VCENTER | BS_AUTORADIOBUTTON | BS_MULTILINE | BS_NOTIFY,
      DEFAULT_STYLES | WS_TABSTOP | BS_TEXT | BS_VCENTER | BS_AUTORADIOBUTTON | BS_MULTILINE | BS_RIGHT | BS_LEFTTEXT | BS_NOTIFY,
      0,
      RTL_EX_STYLES },
    { _T("EDIT"),             // FIELD_TEXT
      DEFAULT_STYLES | WS_TABSTOP | ES_AUTOHSCROLL,
      DEFAULT_STYLES | WS_TABSTOP | ES_AUTOHSCROLL | ES_RIGHT,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | RTL_EX_STYLES | WS_EX_LEFTSCROLLBAR },
    { WC_IPADDRESS,           // FIELD_IPADDRESS
      WS_CHILD | WS_TABSTOP | WS_VISIBLE,
      WS_CHILD | WS_TABSTOP | WS_VISIBLE,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | RTL_EX_STYLES },
    { RICHEDIT_CLASS,         // FIELD_RICHTEXT // Representation for the actual class name (depends on version)
      DEFAULT_STYLES | WS_TABSTOP | ES_AUTOHSCROLL,
      DEFAULT_STYLES | WS_TABSTOP | ES_AUTOHSCROLL | ES_RIGHT,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | RTL_EX_STYLES | WS_EX_LEFTSCROLLBAR },
    { _T("COMBOBOX"),         // FIELD_COMBOBOX
      DEFAULT_STYLES | WS_TABSTOP | WS_VSCROLL | WS_CLIPCHILDREN | CBS_OWNERDRAWFIXED | CBS_HASSTRINGS | CBS_AUTOHSCROLL,
      DEFAULT_STYLES | WS_TABSTOP | WS_VSCROLL | WS_CLIPCHILDREN | CBS_OWNERDRAWFIXED | CBS_HASSTRINGS | CBS_AUTOHSCROLL,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | WS_EX_RIGHT | RTL_EX_STYLES | WS_EX_LEFTSCROLLBAR },
    { DATETIMEPICK_CLASS,     // FIELD_DATETIME
      DEFAULT_STYLES | WS_TABSTOP,
      DEFAULT_STYLES | WS_TABSTOP | DTS_RIGHTALIGN,
      0,
      RTL_EX_STYLES | WS_EX_LEFTSCROLLBAR },
    { _T("LISTBOX"),          // FIELD_LISTBOX
      DEFAULT_STYLES | WS_TABSTOP | WS_VSCROLL | LBS_OWNERDRAWFIXED | LBS_HASSTRINGS | LBS_NOINTEGRALHEIGHT | LBS_NOTIFY,
      DEFAULT_STYLES | WS_TABSTOP | WS_VSCROLL | LBS_OWNERDRAWFIXED | LBS_HASSTRINGS | LBS_NOINTEGRALHEIGHT | LBS_NOTIFY,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
      WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | WS_EX_RIGHT | RTL_EX_STYLES | WS_EX_LEFTSCROLLBAR },
    { WC_LISTVIEW,            // FIELD_LISTVIEW
      DEFAULT_STYLES | WS_TABSTOP | LVS_SHOWSELALWAYS | LVS_SINGLESEL,
      DEFAULT_STYLES | WS_TABSTOP | LVS_SHOWSELALWAYS | LVS_SINGLESEL,
      WS_EX_CLIENTEDGE,
      WS_EX_CLIENTEDGE | RTL_EX_STYLES | WS_EX_LEFTSCROLLBAR },
    { WC_TREEVIEW,            // FIELD_TREEVIEW
      DEFAULT_STYLES | WS_TABSTOP | TVS_DISABLEDRAGDROP | TVS_HASBUTTONS | TVS_HASLINES | TVS_LINESATROOT | TVS_SHOWSELALWAYS,
      DEFAULT_STYLES | WS_TABSTOP | TVS_DISABLEDRAGDROP | TVS_HASBUTTONS | TVS_HASLINES | TVS_LINESATROOT | TVS_SHOWSELALWAYS,
      WS_EX_CLIENTEDGE,
      WS_EX_CLIENTEDGE | RTL_EX_STYLES | WS_EX_LEFTSCROLLBAR },
    { TRACKBAR_CLASS,         // FIELD_TRACKBAR
      DEFAULT_STYLES | WS_TABSTOP | TBS_AUTOTICKS,
      DEFAULT_STYLES | WS_TABSTOP | TBS_AUTOTICKS,
      0,
      RTL_EX_STYLES },
    { MONTHCAL_CLASS,         // FIELD_MONTHCALENDAR
      DEFAULT_STYLES | WS_TABSTOP,
      DEFAULT_STYLES | WS_TABSTOP,
      0,
      RTL_EX_STYLES }
    };

    if (pField->nType < 1 || pField->nType > (int)(_countof(ClassTable)))
      continue;

    DWORD dwStyle, dwExStyle;
    if (bRTL) {
      dwStyle = ClassTable[pField->nType - 1].dwRTLStyle;
      dwExStyle = ClassTable[pField->nType - 1].dwRTLExStyle;
    }
    else {
      dwStyle = ClassTable[pField->nType - 1].dwStyle;
      dwExStyle = ClassTable[pField->nType - 1].dwExStyle;
    }

    // Required because we want to change this for FIELD_IMAGE image types.
    LPTSTR pszClass = (LPTSTR)MALLOC(64 * sizeof(TCHAR));
    strcpy(pszClass, ClassTable[pField->nType - 1].pszClass);

// Convert From User Defined Units
//================================================================

    pField->RectPx = pField->RectUDU;
    // MapDialogRect uses the font used when a dialog is created, and ignores
    // any subsequent WM_SETFONT messages (like we used above); so use the main
    // NSIS window for the conversion, instead of this one.
    if(!bMUnit)
      MapDialogRect(mainwnd, &pField->RectPx);

// Implement support for negative coordinates
//================================================================

    if (pField->RectUDU.left < 0)
      pField->RectPx.left += mainWndWidth;
    if (pField->RectUDU.right < 0)
      pField->RectPx.right += mainWndWidth;
    if (pField->RectUDU.top < 0)
      pField->RectPx.top += mainWndHeight;
    if (pField->RectUDU.bottom < 0)
      pField->RectPx.bottom += mainWndHeight;

// Implement support for RTL
//================================================================

    if (bRTL) {
      int right = pField->RectPx.right;
      pField->RectPx.right = mainWndWidth - pField->RectPx.left;
      pField->RectPx.left = mainWndWidth - right;
    }

// Initialize Controls Before Showing Them Up
//================================================================
    switch(pField->nType)
    {
      case FIELD_IMAGE:
      // Integrated control for icons, cursors, bitmaps and videos w/o sound.
      {
        // Videos cannot be handled here. Only one message suffices for this,
        // and this message has to have a control already created to work.

        int nHeight = pField->RectPx.bottom - pField->RectPx.top;
        int nWidth = pField->RectPx.right - pField->RectPx.left;

        // Step 1: Load from file or from resource
        //--------------------------------------------------------
        LPTSTR pFileExtension = PathFindExtension(pField->pszText);
        int nImageType;

        // Handle icons first from executables and dlls
        if(PathFileExists(pField->pszText) && (stricmp(pFileExtension, _T(".exe")) == 0 || stricmp(pFileExtension, _T(".dll")) == 0))
        {
          nImageType = IMAGE_ICON;

          if(myatoi(pField->pszState) < 0)
            pField->pszState = _T("0");

          HICON hIcon = NULL;

          ExtractIconEx(
            pField->pszText,
            (UINT) myatoi(pField->pszState),
            nWidth >= 32 && nHeight >= 32 ? &hIcon : NULL,
            (nWidth >= 16 && nWidth < 32) && (nHeight >= 16 && nHeight < 32) ? &hIcon : NULL, 1);
          pField->hImage = (HBITMAP)hIcon;

          if(pField->nFlags & FLAG_RESIZETOFIT)
            pField->hImage = (HBITMAP)CopyImage(pField->hImage, nImageType, nWidth, nHeight, 0);
        }
        else if(PathFileExists(pField->pszText) && (stricmp(pFileExtension, _T(".ico")) == 0 || stricmp(pFileExtension, _T(".cur")) == 0 || stricmp(pFileExtension, _T(".ani")) == 0 || stricmp(pFileExtension, _T(".bmp")) == 0))
        {
          if(stricmp(pFileExtension, _T(".ico")) == 0)
            nImageType = IMAGE_ICON;
          else if(stricmp(pFileExtension, _T(".cur")) == 0 || stricmp(pFileExtension, _T(".ani")) == 0)
            nImageType = IMAGE_CURSOR;
          else if(stricmp(pFileExtension, _T(".bmp")) == 0)
            nImageType = IMAGE_BITMAP;

          pField->hImage = (HBITMAP)LoadImage(m_hInstance, pField->pszText, nImageType, (pField->nFlags & FLAG_RESIZETOFIT) ? nWidth : 0, (pField->nFlags & FLAG_RESIZETOFIT) ? nHeight : 0, LR_LOADFROMFILE);
        }
        else if(PathFileExists(pField->pszText) && stricmp(pFileExtension, _T(".avi")) == 0)
          pField->nDataType = IMAGE_TYPE_ANIMATION;
        else if(PathFileExists(pField->pszText) && (
                 //Raster Images (OLE)
                 stricmp(pFileExtension, _T(".gif")) == 0 ||
                 stricmp(pFileExtension, _T(".jpg")) == 0 ||
                 stricmp(pFileExtension, _T(".jpeg")) == 0))
                 //Vector Images (OLE not supported yet)
                 //stricmp(pFileExtension, _T(".wmf")) == 0
        {
          if(stricmp(pFileExtension, _T(".gif")) == 0 ||
             stricmp(pFileExtension, _T(".jpg")) == 0 ||
             stricmp(pFileExtension, _T(".jpeg")) == 0)
          pField->nDataType = IMAGE_TYPE_OLE;

          //if(pField->nDataType == IMAGE_TYPE_OLE)
          //{
            HANDLE hFile = CreateFile(pField->pszText, GENERIC_READ, 0, NULL, OPEN_EXISTING, 0, NULL);
            
            if (hFile == INVALID_HANDLE_VALUE)
              break;

            DWORD nFileSize = GetFileSize(hFile, 0);
            HGLOBAL hFileGlobal = GlobalAlloc(GPTR, nFileSize);

            if (!hFileGlobal)
            {
              CloseHandle(hFile);
              break;
            }

            LPVOID lpFileLocked = GlobalLock(hFileGlobal);
            if (!lpFileLocked)
            {
              CloseHandle(hFile);
              GlobalFree(hFileGlobal);
              break;
            }

            ReadFile(hFile, lpFileLocked, nFileSize, &nFileSize, 0);

            GlobalUnlock(hFileGlobal);
            CloseHandle(hFile);

            LPSTREAM lpStream;
            if (CreateStreamOnHGlobal(hFileGlobal, FALSE, &lpStream) != S_OK || !lpStream)
            {
              GlobalFree(hFileGlobal);
              break;
            }

            if (OleLoadPicture(lpStream, 0, FALSE, IID_IPicture, (void **)&pField->nImageInterface) != S_OK)
              pField->nImageInterface = NULL;

            lpStream->Release();
            GlobalFree(hFileGlobal);

            if (!pField->nImageInterface)
              break;

            pField->nImageInterface->get_Handle((OLE_HANDLE *)&pField->hImage);

            if (pField->hImage)
              pField->hImage = (HBITMAP)CopyImage(pField->hImage, IMAGE_BITMAP, (pField->nFlags & FLAG_RESIZETOFIT) ? nWidth : 0, (pField->nFlags & FLAG_RESIZETOFIT) ? nHeight : 0, LR_COPYRETURNORG);

            pField->nImageInterface->Release();
          //}
        }
        else
        {
          struct TrioTableEntry {
            int   nDataType;
            const TCHAR *pszName;
            int   nValue;
          };

          // Icon Flags
          //-------------------------------
          // These below are resource numbers. Needs to use MAKEINTRESOURCE later.
          static TrioTableEntry IconTable[] =
          {
            // Icon Flags
            { IMAGE_ICON, _T("APPLICATION"), 32512 },
            { IMAGE_ICON, _T("EXCLAMATION"), 32515 },
            { IMAGE_ICON, _T("INFORMATION"), 32516 },
            { IMAGE_ICON, _T("QUESTION"),    32514 },
            { IMAGE_ICON, _T("STOP"),        32513 },
            { IMAGE_ICON, _T("WINLOGO"),     32517 },

            // Cursor Flags
            { IMAGE_CURSOR, _T("APPSTARTING"), 32650 },
            { IMAGE_CURSOR, _T("ARROW"),       32512 },
            { IMAGE_CURSOR, _T("CROSS"),       32515 },
            { IMAGE_CURSOR, _T("HAND"),        32649 },
            { IMAGE_CURSOR, _T("HELP"),        32651 },
            { IMAGE_CURSOR, _T("IBEAM"),       32513 },
            { IMAGE_CURSOR, _T("NO"),          32648 },
            { IMAGE_CURSOR, _T("SIZEALL"),     32646 },
            { IMAGE_CURSOR, _T("SIZENESW"),    32643 },
            { IMAGE_CURSOR, _T("SIZENS"),      32645 },
            { IMAGE_CURSOR, _T("SIZENWSE"),    32642 },
            { IMAGE_CURSOR, _T("SIZEWE"),      32644 },
            { IMAGE_CURSOR, _T("UPARROW"),     32516 },
            { IMAGE_CURSOR, _T("WAIT"),        32514 },

            // Bitmap Flags
            { IMAGE_BITMAP, _T("BTNCORNERS"),  32758 },
            { IMAGE_BITMAP, _T("BTSIZE"),      32761 },
            { IMAGE_BITMAP, _T("CHECK"),       32760 },
            { IMAGE_BITMAP, _T("CHECKBOXES"),  32759 },
            { IMAGE_BITMAP, _T("CLOSE"),       32754 },
            { IMAGE_BITMAP, _T("COMBO"),       32738 },
            { IMAGE_BITMAP, _T("DNARROW"),     32752 },
            { IMAGE_BITMAP, _T("DNARROWD"),    32742 },
            { IMAGE_BITMAP, _T("DNARROWI"),    32736 },
            { IMAGE_BITMAP, _T("LFARROW"),     32750 },
            { IMAGE_BITMAP, _T("LFARROWD"),    32740 },
            { IMAGE_BITMAP, _T("LFARROWI"),    32734 },
            { IMAGE_BITMAP, _T("MNARROW"),     32739 },
            { IMAGE_BITMAP, _T("REDUCE"),      32749 },
            { IMAGE_BITMAP, _T("REDUCED"),     32746 },
            { IMAGE_BITMAP, _T("RESTORE"),     32747 },
            { IMAGE_BITMAP, _T("RESTORED"),    32744 },
            { IMAGE_BITMAP, _T("RGARROW"),     32751 },
            { IMAGE_BITMAP, _T("RGARROWD"),    32741 },
            { IMAGE_BITMAP, _T("RGARROWI"),    32735 },
            { IMAGE_BITMAP, _T("SIZE"),        32766 },
            { IMAGE_BITMAP, _T("UPARROW"),     32753 },
            { IMAGE_BITMAP, _T("UPARROWD"),    32743 },
            { IMAGE_BITMAP, _T("UPARROWI"),    32737 },
            { IMAGE_BITMAP, _T("ZOOM"),        32748 },
            { IMAGE_BITMAP, _T("ZOOMD"),       32745 },

            { IMAGE_ICON, NULL, 0 } //NSIS application icon.
          };

          WORD nIcon = 103;
          nImageType = IMAGE_ICON;
          HINSTANCE hInstance = NULL;

          //LookupToken adapted to TrioTableEntry.
          for (int i = 0; IconTable[i].pszName; i++)
            if (!lstrcmpi(pField->pszState, IconTable[i].pszName))
            {
              nImageType = IconTable[i].nDataType;
              nIcon = IconTable[i].nValue;
            }

          pField->hImage = (HBITMAP)LoadImage((nIcon == 103) ? GetModuleHandle(0) : NULL, MAKEINTRESOURCE(nIcon), nImageType, 0, 0, LR_SHARED);

          if(pField->nFlags & FLAG_RESIZETOFIT)
            pField->hImage = (HBITMAP)CopyImage(pField->hImage, nImageType, nWidth, nHeight, LR_COPYFROMRESOURCE);
        }

        if(nImageType == IMAGE_BITMAP)
          pField->nDataType = IMAGE_TYPE_BITMAP;
        else if(nImageType == IMAGE_ICON)
          pField->nDataType = IMAGE_TYPE_ICON;
        else if(nImageType == IMAGE_CURSOR)
          pField->nDataType = IMAGE_TYPE_CURSOR;
        //IMAGE_TYPE_ANIMATION and IMAGE_TYPE_OLE were already set at this point.

        // Step 2: Transform into specific internal controls
        //--------------------------------------------------------
        switch(pField->nDataType)
        {
          case IMAGE_TYPE_BITMAP:
          case IMAGE_TYPE_OLE:
          case IMAGE_TYPE_GDIPLUS:
          {
            pszClass = _T("STATIC");
            dwStyle |= SS_BITMAP | SS_NOTIFY;
            break;
          }
          case IMAGE_TYPE_ICON:
          case IMAGE_TYPE_CURSOR:
          {
            pszClass = _T("STATIC");
            dwStyle |= SS_ICON | SS_NOTIFY;
            break;
          }
          case IMAGE_TYPE_ANIMATION:
          {
            pszClass = ANIMATE_CLASS;
            dwStyle |= ACS_TIMER | ACS_CENTER;
            break;
          }
        }
        break;
      }
      case FIELD_RICHTEXT:
      {
        // Load the dll for the RichText in the memory
        //--------------------------------------------------------
        if(!LoadLibrary(_T("riched20.dll"))) //Version 2
        {
          LoadLibrary(_T("riched32.dll")); //Version 1

          pszClass = _T("RichEdit");
        }
      }
    }

// Assign To Each Control Type Optional Flags
//================================================================
    TCHAR *title = pField->pszText;

    switch (pField->nType) {
      case FIELD_IMAGE:
        title = NULL; // otherwise it is treated as the name of a resource
        if(pField->nDataType == IMAGE_TYPE_ANIMATION)
          if (pField->nFlags & FLAG_TRANSPARENT)
            dwStyle |= ACS_TRANSPARENT;    
        break;
      case FIELD_LABEL:
        if(!(pField->nNotify & NOTIFY_CONTROL_ONCLICK) && !(pField->nNotify & NOTIFY_CONTROL_ONDBLCLICK))
          dwStyle &= ~SS_NOTIFY;

        if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
          dwStyle &= ~SS_OWNERDRAW;
        break;
      case FIELD_CHECKBOX:
        if (pField->nFlags & FLAG_READONLY)
        {
          dwStyle &= ~BS_AUTOCHECKBOX;
          if (pField->nFlags & FLAG_3STATE)
            dwStyle |= BS_3STATE;
          else
            dwStyle |= BS_CHECKBOX;
        }
        else
        {
          if (pField->nFlags & FLAG_3STATE)
          {
            dwStyle &= ~BS_AUTOCHECKBOX;
            dwStyle |= BS_AUTO3STATE;
          }
        }
        break;
      case FIELD_RADIOBUTTON:
        if (pField->nFlags & FLAG_READONLY)
        {
          dwStyle &= ~BS_AUTORADIOBUTTON;
          dwStyle |= BS_RADIOBUTTON;
        }
        break;
      case FIELD_TEXT:
        title = pField->pszState;
      case FIELD_RICHTEXT:
        // Microsoft says that password style cannot be used with
        // multiline style, but in use, these can be used together.
        // (Thank you Microsoft.)
        if (pField->nFlags & FLAG_PASSWORD)
          dwStyle |= ES_PASSWORD;
        if (pField->nFlags & FLAG_ONLYNUMBERS)
          dwStyle |= ES_NUMBER;
        if (pField->nFlags & FLAG_WANTRETURN)
          dwStyle |= ES_WANTRETURN;
        if (pField->nFlags & FLAG_READONLY)
          dwStyle |= ES_READONLY;
        if (pField->nFlags & FLAG_VSCROLL)
          dwStyle |= WS_VSCROLL;
        if (pField->nFlags & FLAG_MULTILINE)
        {
          dwStyle |= ES_MULTILINE | ES_AUTOVSCROLL;

          // Enable word-wrap unless we have a horizontal scroll bar
          // or it has been explicitly disallowed
          if (!(pField->nFlags & (FLAG_HSCROLL | FLAG_NOWORDWRAP)))
            dwStyle &= ~ES_AUTOHSCROLL;

          ConvertNewLines(pField->pszState);

          // If multiline-readonly then hold the text back until after the
          // initial focus has been set. This is so the text is not initially
          // selected - useful for License Page look-a-likes.
          if (pField->nFlags & FLAG_READONLY)
            title = NULL;
        }
        break;
      case FIELD_COMBOBOX:
        dwStyle |= (pField->nFlags & FLAG_DROPLIST) ? CBS_DROPDOWNLIST : CBS_DROPDOWN;
        if (pField->nFlags & FLAG_VSCROLL)
          dwStyle |= CBS_DISABLENOSCROLL;
        title = pField->pszState;
        if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
          dwStyle &= ~CBS_OWNERDRAWFIXED;
        break;
      case FIELD_LISTBOX:
        if (pField->nFlags & FLAG_MULTISELECT)
          dwStyle |= LBS_MULTIPLESEL;
        if (pField->nFlags & FLAG_EXTENDEDSELECT)
          dwStyle |= LBS_EXTENDEDSEL;
        if (pField->nFlags & FLAG_VSCROLL)
          dwStyle |= LBS_DISABLENOSCROLL;
        if (pField->pszText)
          if(myatoi(pField->pszText) > 0)
            dwStyle |= LBS_MULTICOLUMN;
        if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
          dwStyle &= ~LBS_OWNERDRAWFIXED;
        break;
      case FIELD_TREEVIEW:
        if (pField->nFlags & FLAG_CHECKBOXES && FileExists(pField->pszStateImageList))
          dwStyle |= TVS_CHECKBOXES;
        else
          pField->nFlags &= ~FLAG_CHECKBOXES;
        if (pField->nFlags & FLAG_EDITLABELS)
          dwStyle |= TVS_EDITLABELS;
        break;
      case FIELD_LISTVIEW:
        if (pField->nFlags & FLAG_EDITLABELS)
          dwStyle |= LVS_EDITLABELS;
        if (pField->nFlags & FLAG_ICON_VIEW)
          dwStyle |= LVS_ICON;
        if (pField->nFlags & FLAG_LIST_VIEW)
          dwStyle |= LVS_LIST;
        if (pField->nFlags & FLAG_SMALLICON_VIEW)
          dwStyle |= LVS_SMALLICON;
        if (pField->nFlags & FLAG_REPORT_VIEW)
          dwStyle |= LVS_REPORT;
        if (pField->nFlags & FLAG_MULTISELECT)
          dwStyle &= ~LVS_SINGLESEL;
        break;
      case FIELD_BUTTON:
        if (pField->nFlags & FLAG_BITMAP)
          dwStyle |= BS_BITMAP;
        if (pField->nFlags & FLAG_ICON)
          dwStyle |= BS_ICON;
        break;
      case FIELD_PROGRESSBAR:
        if (pField->nFlags & FLAG_SMOOTH)
          dwStyle |= PBS_SMOOTH;
        if (pField->nFlags & FLAG_VSCROLL)
          dwStyle |= PBS_VERTICAL;
        break;
      case FIELD_TRACKBAR:
        if (pField->nFlags & FLAG_VSCROLL)
          dwStyle |= TBS_VERT;
        if (pField->nFlags & FLAG_NO_TICKS)
          dwStyle |= TBS_NOTICKS;
        break;
      case FIELD_DATETIME:
        if (pField->nFlags & FLAG_UPDOWN)
          dwStyle |= DTS_TIMEFORMAT;
        break;
      case FIELD_MONTHCALENDAR:
        if (pField->nFlags & FLAG_NOTODAY)
          dwStyle |= MCS_NOTODAY | MCS_NOTODAYCIRCLE;
        if (pField->nFlags & FLAG_WEEKNUMBERS)
          dwStyle |= MCS_WEEKNUMBERS;
        break;
      case FIELD_UPDOWN:
        if (pField->nFlags & FLAG_HSCROLL)
          dwStyle |= UDS_HORZ;
        if (pField->nFlags & FLAG_WRAP)
          dwStyle |= UDS_WRAP;
        break;
    }
    if (pField->nFlags & FLAG_GROUP) dwStyle |= WS_GROUP;
    if (pField->nFlags & FLAG_HSCROLL && (pField->nType == FIELD_TEXT || pField->nType == FIELD_RICHTEXT)) dwStyle |= WS_HSCROLL;
    if (pField->nFlags & FLAG_VSCROLL && (pField->nType == FIELD_TEXT || pField->nType == FIELD_LISTBOX || pField->nType == FIELD_COMBOBOX)) dwStyle |= WS_VSCROLL;
    if (pField->nFlags & FLAG_DISABLED) dwStyle |= WS_DISABLED;
    if (pField->nFlags & FLAG_NOTABSTOP) dwStyle &= ~WS_TABSTOP;

// Assign To Each Control Type An Optional Align Flag
//================================================================

    switch (pField->nType) {
      case FIELD_UPDOWN:
        if (pField->nAlign == ALIGN_LEFT)
        {
          dwStyle &= ~UDS_ALIGNRIGHT;
          dwStyle |= UDS_ALIGNLEFT;
        }
        else if (pField->nAlign == ALIGN_RIGHT)
        {
          dwStyle &= ~UDS_ALIGNLEFT;
          dwStyle |= UDS_ALIGNRIGHT;
        }
        break;
      case FIELD_CHECKBOX:
        if (pField->nAlign == ALIGN_LEFT)
          dwStyle &= ~BS_LEFTTEXT;
        else if (pField->nAlign == ALIGN_RIGHT)
          dwStyle |= BS_LEFTTEXT;
        break;
      case FIELD_DATETIME:
        if (pField->nAlign == ALIGN_LEFT)
          dwStyle &= ~DTS_RIGHTALIGN;
        else if (pField->nAlign == ALIGN_RIGHT)
          dwStyle |= DTS_RIGHTALIGN;
        break;
      case FIELD_TRACKBAR:
        if (pField->nFlags & FLAG_VSCROLL)
        {
          if (pField->nAlign == ALIGN_LEFT)
            dwStyle |= TBS_LEFT;
          else if (pField->nAlign == ALIGN_CENTER)
            dwStyle |= TBS_BOTH;
          else if (pField->nAlign == ALIGN_RIGHT)
            dwStyle |= TBS_RIGHT;
        }
        else
        {
          if (pField->nVAlign == VALIGN_TOP)
            dwStyle |= TBS_TOP;
          else if (pField->nVAlign == VALIGN_CENTER)
            dwStyle |= TBS_BOTH;
          else if (pField->nVAlign == VALIGN_BOTTOM)
            dwStyle |= TBS_BOTTOM;
        }
        break;
    }

    switch (pField->nType) {
      case FIELD_LABEL:
        if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        {
          if (pField->nTxtAlign == ALIGN_TEXT_LEFT)
            dwStyle |= SS_LEFT;
          else if (pField->nTxtAlign == ALIGN_TEXT_CENTER)
            dwStyle |= SS_CENTER;
          else if (pField->nTxtAlign == ALIGN_TEXT_RIGHT)
            dwStyle |= SS_RIGHT;
        }
      case FIELD_BUTTON:
        if (pField->nTxtVAlign == VALIGN_TEXT_TOP)
          dwStyle |= BS_TOP;
        else if (pField->nTxtVAlign == VALIGN_TEXT_CENTER)
          dwStyle |= BS_VCENTER;
        else if (pField->nTxtVAlign == VALIGN_TEXT_BOTTOM)
          dwStyle |= BS_BOTTOM;
      case FIELD_CHECKBOX:
      case FIELD_RADIOBUTTON:
      case FIELD_GROUPBOX:
        if (pField->nTxtAlign == ALIGN_TEXT_LEFT)
        {
          dwStyle &= ~BS_CENTER;
          dwStyle &= ~BS_RIGHT;
          dwStyle |= BS_LEFT;
        }
        else if (pField->nTxtAlign == ALIGN_TEXT_CENTER)
        {
          dwStyle &= ~BS_LEFT;
          dwStyle &= ~BS_RIGHT;
          dwStyle |= BS_CENTER;
        }
        else if (pField->nTxtAlign == ALIGN_TEXT_RIGHT)
        {
          dwStyle &= ~BS_LEFT;
          dwStyle &= ~BS_CENTER;
          dwStyle |= BS_RIGHT;
        }
        break;
      case FIELD_TEXT:
        if (pField->nTxtAlign == ALIGN_LEFT)
        {
          dwStyle &= ~ES_CENTER;
          dwStyle &= ~ES_RIGHT;
          dwStyle |= ES_LEFT;
        }
        else if (pField->nTxtAlign == ALIGN_CENTER)
        {
          dwStyle &= ~ES_LEFT;
          dwStyle &= ~ES_RIGHT;
          dwStyle |= ES_CENTER;
        }
        else if (pField->nTxtAlign == ALIGN_RIGHT)
        {
          dwStyle &= ~ES_LEFT;
          dwStyle &= ~ES_CENTER;
          dwStyle |= ES_RIGHT;
        }
        break;
    }

// Create Control
//================================================================
    HWND hwCtrl = pField->hwnd = CreateWindowEx(
      dwExStyle,
      pszClass,
      title,
      dwStyle,
      pField->RectPx.left,
      pField->RectPx.top,
      pField->RectPx.right - pField->RectPx.left,
      pField->RectPx.bottom - pField->RectPx.top,
      hConfigWindow,
      (HMENU)pField->nControlID,
      m_hInstance,
      NULL
    );

    FREE(pszClass);

// Create ToolTip
//================================================================
    if(pField->pszToolTipText) {

      if(!bRTL)
        dwExStyle = WS_EX_TOPMOST;
      else
        dwExStyle = WS_EX_TOPMOST | RTL_EX_STYLES;

      pField->hwToolTip = CreateWindowEx(
        dwExStyle,
        TOOLTIPS_CLASS,
        NULL,
        pField->nToolTipFlags,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        pField->hwnd,
        NULL,
        m_hInstance,
        NULL
        );

      SetWindowPos(pField->hwToolTip,
        HWND_TOPMOST,
        0,
        0,
        0,
        0,
        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

      TOOLINFO ti;

      ti.cbSize = sizeof(TOOLINFO);
      if(!bRTL)
        ti.uFlags = TTF_TRANSPARENT | TTF_SUBCLASS | TTF_IDISHWND;
      else
        ti.uFlags = TTF_TRANSPARENT | TTF_SUBCLASS | TTF_IDISHWND | TTF_RTLREADING;
      ti.uId = (int)pField->hwnd;
      ti.lpszText = pField->pszToolTipText;

      mySendMessage(pField->hwToolTip, TTM_ADDTOOL, 0, (LPARAM) (LPTOOLINFO) &ti);

      mySendMessage(pField->hwToolTip, TTM_SETMAXTIPWIDTH, 0, (LPARAM) (INT) pField->nToolTipMaxWidth);

      if(pField->crToolTipTxtColor != 0xFFFFFFFF)
        mySendMessage(pField->hwToolTip,TTM_SETTIPTEXTCOLOR, (WPARAM) (COLORREF) pField->crToolTipTxtColor, 0);
      if(pField->crToolTipBgColor != 0xFFFFFFFF)
        mySendMessage(pField->hwToolTip,TTM_SETTIPBKCOLOR, (WPARAM) (COLORREF) pField->crToolTipBgColor, 0);

      if(pField->nToolTipFlags & TTS_BALLOON)
        mySendMessage(pField->hwToolTip, TTM_SETTITLE, pField->nToolTipIcon, (LPARAM) (LPCTSTR) pField->pszToolTipTitle);
    }

    {
      TCHAR szField[64];
      TCHAR szHwnd[64];
      wsprintf(szField, _T("Field %d"), pField->nField);
      wsprintf(szHwnd, _T("%d"), hwCtrl);
      WritePrivateProfileString(szField, _T("HWND"), szHwnd, pszFilename);
    }

// Set Configurations For Each Control Type
//================================================================
    if (hwCtrl) {

  // Font
  //--------------------------------------------------------------
      // Sets the font of IO window to be the same as the main window by default
      mySendMessage(hwCtrl, WM_SETFONT, (WPARAM)hFont, TRUE);

      // Set new font based on control settings
      {
        LOGFONT lf;
        HDC hDC = GetDC(hwCtrl);
        HFONT OldFont;

        // CreateFont now

        GetObject(hFont, sizeof(lf), (LPVOID)&lf);

        if(pField->pszFontName)
          strncpy(lf.lfFaceName,pField->pszFontName, 32);
        if(pField->nFontHeight)
          lf.lfHeight = -MulDiv(pField->nFontHeight, GetDeviceCaps(hDC, LOGPIXELSY), 72);
        if(pField->nFontWidth)
          lf.lfWidth = pField->nFontWidth;
        if(pField->nFontBold)
          lf.lfWeight = 700;
        if(pField->nFontItalic)
          lf.lfItalic = TRUE;
        if(pField->nFontUnderline)
          lf.lfUnderline = TRUE;
        if(pField->nFontStrikeOut)
          lf.lfStrikeOut = TRUE;

        OldFont = (HFONT)SelectObject(hDC, CreateFontIndirect(&lf));

        mySendMessage(hwCtrl, WM_SETFONT, (WPARAM) CreateFontIndirect(&lf), TRUE);

        DeleteObject(SelectObject(hDC, OldFont));
        ReleaseDC(hwCtrl, hDC);
      }

  // Configurations for each control type
  //--------------------------------------------------------------

      // make sure we created the window, then set additional attributes
      switch (pField->nType) {
        case FIELD_IMAGE:
        {
          switch(pField->nDataType)
          {
            case IMAGE_TYPE_BITMAP:
            case IMAGE_TYPE_OLE:
            case IMAGE_TYPE_GDIPLUS:
            {
              if (pField->nFlags & FLAG_TRANSPARENT)
              {
                MakeTransparentAreaFromBitmap(hwCtrl, (HBITMAP) pField->hImage);
              }
 
              mySendMessage(hwCtrl,STM_SETIMAGE,IMAGE_BITMAP,(LPARAM)pField->hImage);

              // Center the image in the requested space.
              // Cannot use SS_CENTERIMAGE because it behaves differently on XP to
              // everything else.  (Thank you Microsoft.)
              RECT  bmp_rect;
              GetClientRect(hwCtrl, &bmp_rect);
              bmp_rect.left = (pField->RectPx.left + pField->RectPx.right - bmp_rect.right) / 2;
              bmp_rect.top = (pField->RectPx.top + pField->RectPx.bottom - bmp_rect.bottom) / 2;
              SetWindowPos(hwCtrl, NULL, bmp_rect.left, bmp_rect.top, 0, 0,SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOZORDER);
              break;
            }
            case IMAGE_TYPE_ICON:
            {
              if (pField->nFlags & FLAG_TRANSPARENT)
              {
                ICONINFO csII;
                if(GetIconInfo((HICON)pField->hImage, &csII))
                {
                  MakeTransparentAreaFromBitmap(hwCtrl, csII.hbmColor);
                }
              }
              mySendMessage(hwCtrl,STM_SETIMAGE,IMAGE_ICON,(LPARAM)pField->hImage);
              break;
            }
            case IMAGE_TYPE_CURSOR:
            {
              mySendMessage(hwCtrl,STM_SETIMAGE,IMAGE_CURSOR,(LPARAM)pField->hImage);
              break;
            }
            case IMAGE_TYPE_ANIMATION:
            {
              if (pField->pszText) {
                mySendMessage(hwCtrl, ACM_OPEN, 0, (LPARAM) pField->pszText);
                if(lstrcmp(pField->pszState,_T(""))==0)
                  mySendMessage(hwCtrl, ACM_PLAY, -1, MAKELONG(0, -1));
                else
                {
                  UINT nTimes = myatoi(pField->pszState);

                  if (nTimes != 0) {
                    if((pField->nMaxLength == NULL)&&(pField->nMinLength == NULL))
                      mySendMessage(hwCtrl, ACM_PLAY, nTimes, MAKELONG(0, -1));
                    else if((pField->nMaxLength != NULL)&&(pField->nMinLength == NULL)&&(0 <= pField->nMaxLength))
                      mySendMessage(hwCtrl, ACM_PLAY, nTimes, MAKELONG(0, pField->nMaxLength));
                    else if((pField->nMaxLength == NULL)&&(pField->nMinLength != NULL)&&(pField->nMinLength <= 65536))
                      mySendMessage(hwCtrl, ACM_PLAY, nTimes, MAKELONG(pField->nMinLength, -1));
                    else if((pField->nMaxLength != NULL)&&(pField->nMinLength != NULL)&&(pField->nMinLength <= pField->nMaxLength))
                      mySendMessage(hwCtrl, ACM_PLAY, nTimes, MAKELONG(pField->nMinLength, pField->nMaxLength));
                    else
                      pField->nMinLength = pField->nMaxLength = NULL;
                  }
                }
              }
              break;
            }
          }
          break;
        }
        case FIELD_TEXT:
          mySendMessage(hwCtrl, EM_LIMITTEXT, (WPARAM)pField->nMaxLength, (LPARAM)0);

          if(pField->nFlags & FLAG_PASSWORD)
          {
            if(pField->pszText)
            {
              TCHAR* pszChar = (TCHAR*)MALLOC((1+1)*sizeof(TCHAR));
              strncpy(pszChar, pField->pszText, 1+1);
              mySendMessage(hwCtrl, EM_SETPASSWORDCHAR, (WPARAM) *pszChar, 0);
            }
            else
              mySendMessage(hwCtrl, EM_SETPASSWORDCHAR, (WPARAM) '*', 0);
          }
          break;

        case FIELD_UPDOWN:
        {
          if((pField->nMaxLength == NULL)&&(pField->nMinLength == NULL))
            mySendMessage(hwCtrl, UDM_SETRANGE32, -0x7FFFFFFE, 0x7FFFFFFE);
          else if((pField->nMaxLength != NULL)&&(pField->nMinLength == NULL)&&(-2147483646 <= pField->nMaxLength))
            mySendMessage(hwCtrl, UDM_SETRANGE32, -0x7FFFFFFE, pField->nMaxLength);
          else if((pField->nMaxLength == NULL)&&(pField->nMinLength != NULL)&&(pField->nMinLength <= 2147483646))
            mySendMessage(hwCtrl, UDM_SETRANGE32, pField->nMinLength,0x7FFFFFFE);
          else if((pField->nMaxLength != NULL)&&(pField->nMinLength != NULL)&&(pField->nMinLength <= pField->nMaxLength))
             mySendMessage(hwCtrl, UDM_SETRANGE32, pField->nMinLength, pField->nMaxLength);
          else
            pField->nMinLength = pField->nMaxLength = NULL;

          if (pField->pszState)
            mySendMessage(hwCtrl, UDM_SETPOS32, 0, myatoi(pField->pszState));
          break;
        }
        case FIELD_CHECKBOX:
        case FIELD_RADIOBUTTON:
          switch (pField->pszState[0])
          {
            case _T('1'):
            {
              mySendMessage(hwCtrl, BM_SETCHECK, (WPARAM)BST_CHECKED, 0);
              break;
            }
            case _T('2'):
            {
              if(pField->nFlags & FLAG_3STATE)
                mySendMessage(hwCtrl, BM_SETCHECK, (WPARAM)BST_INDETERMINATE, 0);
              break;
            }
          }
          break;

        case FIELD_COMBOBOX:
        case FIELD_LISTBOX:
          // if this is a listbox or combobox, we need to add the list items.
          if (pField->pszListItems)
          {
            UINT nAddMsg, nFindMsg, nSetSelMsg;

            switch(pField->nType)
            {
              case FIELD_COMBOBOX:
              {
                nAddMsg = CB_ADDSTRING;
                nFindMsg = CB_FINDSTRINGEXACT;
                nSetSelMsg = CB_SETCURSEL;

                // Using the same limit for text controls because
                // nobody will write 32KB of text in a single line.
                mySendMessage(hwCtrl, CB_LIMITTEXT, 32766, 0);

                break;
              }
              case FIELD_LISTBOX:
              {
                nAddMsg = LB_ADDSTRING;
                nFindMsg = LB_FINDSTRINGEXACT;
                nSetSelMsg = LB_SETCURSEL;

                if(pField->pszText && myatoi(pField->pszText) > 0)
                  mySendMessage(hwCtrl, LB_SETCOLUMNWIDTH, (WPARAM) myatoi(pField->pszText), 0);
              }
            }

            int nResult = 0;

            LPTSTR pszStart, pszEnd;
            pszStart = pszEnd = pField->pszListItems;

            while(nResult = IOLI_NextItem(pField->pszListItems, &pszStart, &pszEnd, 1))
            {
              if((nResult == 5 && lstrcmp(pszStart,_T("")) == 0) || nResult == 6)
                break;

              mySendMessage(hwCtrl, nAddMsg, 0, (LPARAM) pszStart);

              IOLI_RestoreItemStructure(pField->pszListItems, &pszStart, &pszEnd, nResult);
            }
            
            if(pField->pszState)
            {
              if(pField->nType == FIELD_LISTBOX && pField->nFlags & (FLAG_MULTISELECT|FLAG_EXTENDEDSELECT))
              {
                mySendMessage(hwCtrl, LB_SETSEL, FALSE, (LPARAM)-1);

                pszStart = pszEnd = pField->pszState;

                while(nResult = IOLI_NextItem(pField->pszState, &pszStart, &pszEnd, 1))
                {
                  if((nResult == 5 && lstrcmp(pszStart,_T("")) == 0) || nResult == 6)
                    break;

                  INT_PTR nItem = mySendMessage(hwCtrl, LB_FINDSTRINGEXACT, (WPARAM)-1, (LPARAM)pszStart);
                  if (nItem != LB_ERR)
                    mySendMessage(hwCtrl, LB_SETSEL, TRUE, nItem);

                  IOLI_RestoreItemStructure(pField->pszState, &pszStart, &pszEnd, nResult);
                }
              }
              else
              {
                INT_PTR nItem = mySendMessage(hwCtrl, nFindMsg, (WPARAM)-1, (LPARAM)pField->pszState);

                if (nItem != CB_ERR) // CB_ERR == LB_ERR == -1
                  mySendMessage(hwCtrl, nSetSelMsg, nItem, 0);
                else
                if(pField->nType == FIELD_COMBOBOX && !(pField->nFlags & FLAG_DROPLIST))
                  mySendMessage(hwCtrl, WM_SETTEXT, 0, (LPARAM)pField->pszState);
              }
            }
          }
          break;

        case FIELD_TREEVIEW:
        {
          // "ListItems" has to exist in order to continue
          if (pField->pszListItems && lstrlen(pField->pszListItems))
          {

            // Step 1: Implement image list feature.
            //----------------------------------------------------------------
            if(pField->pszStateImageList && pField->nFlags & FLAG_CHECKBOXES)
            {
              if(FileExists(pField->pszStateImageList))
              {
                // Set the image list to the TreeView control
                HBITMAP hBitmap = (HBITMAP)LoadImage(0, pField->pszStateImageList, IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
                pField->hStateImageList = (HIMAGELIST)ImageList_Create(16, 16, ILC_COLOR32|ILC_MASK, 6, 0);
                ImageList_AddMasked(pField->hStateImageList,hBitmap,RGB(255,0,255));
                TreeView_SetImageList(hwCtrl, pField->hStateImageList, TVSIL_STATE);

                DeleteObject((HBITMAP)hBitmap);
              }
            }

            // Step 2: Include items to TreeView.
            //----------------------------------------------------------------
            HTREEITEM hItem = NULL;
            HTREEITEM hParentItem = NULL;

            int nResult = 0;

            LPTSTR pszStart, pszEnd;

            pszStart = pszEnd = pField->pszListItems;
            while(nResult = IOLI_NextItem(pField->pszListItems, &pszStart, &pszEnd, 0))
            {
              if((nResult == 5 && lstrlen(pszStart)==0 )||nResult == 6)
                break;

              // When two sublevel are closed, it return an empty string... Then we go to the next item
              if(lstrlen(pszStart)==0)
              {
                if(nResult == 2) hParentItem = hItem;
                if(nResult == 3) hParentItem = TreeView_GetParent(hwCtrl, hParentItem);

                IOLI_RestoreItemStructure(pField->pszListItems, &pszStart, &pszEnd, nResult);
                continue;
              }

              // Provide information about the item
              TVITEM tvi;

              tvi.mask = TVIF_TEXT | TVIF_CHILDREN;
              tvi.pszText = pszStart;
              tvi.cchTextMax = lstrlen(pszStart);
              tvi.cChildren = nResult == 2 ? 1 : 0;

              // Insert the item to the TreeView control
              TVINSERTSTRUCT tvins;
              tvins.hParent = hParentItem;
              tvins.item = tvi;

              tvins.hInsertAfter = TVI_LAST;

              tvi.hItem = hItem = TreeView_InsertItem(hwCtrl, &tvins);
              TreeView_SetItem(hwCtrl, &tvi);
              if(nResult == 2) hParentItem = hItem;
              if(nResult == 3) hParentItem = TreeView_GetParent(hwCtrl, hParentItem);

              IOLI_RestoreItemStructure(pField->pszListItems, &pszStart, &pszEnd, nResult);
            }

            hParentItem = NULL;

            // Step 3: Set the state of each item.
            //----------------------------------------------------------------
            if(pField->pszState)
            {
              int nPrevResult = 0;
              hItem = TreeView_GetRoot(hwCtrl);

              pszStart = pszEnd = pField->pszState;

              while(nResult = IOLI_NextItem(pField->pszState, &pszStart, &pszEnd, 0))
              {
                if((nResult == 5 && lstrcmp(pszStart,_T(""))==0)||nResult == 6)
                  break;

                if(pField->pszStateImageList && pField->nFlags & FLAG_CHECKBOXES)
                {
                  if(*pszStart)
                    FIELD_TREEVIEW_Check(hwCtrl, hItem, TREEVIEW_AUTOCHECK, myatoi(pszStart), FALSE);

                  if(nResult == 2)
                    hItem = TreeView_GetChild(hwCtrl, hItem);
                  if(nResult == 1)
                    hItem = TreeView_GetNextSibling(hwCtrl, hItem);
                  if(nResult == 3)
                  {
                    hItem = TreeView_GetParent(hwCtrl, hItem);
                    if(TreeView_GetNextSibling(hwCtrl, hItem) != NULL)
                      hItem = TreeView_GetNextSibling(hwCtrl, hItem);
                  }
                }
                else
                {
                  LPTSTR pszText = (LPTSTR)MALLOC(260+1);

                  for(;;)
                  {
                    TVITEM tvi;
                    tvi.mask = TVIF_TEXT | TVIF_CHILDREN;
                    tvi.pszText = pszText;
                    tvi.cchTextMax = 260;
                    tvi.hItem = hItem;
                    TreeView_GetItem(hwCtrl, &tvi);

                    pszText = tvi.pszText;

                    if(!hItem) goto AfterState;

                    if(lstrcmp(pszText, pszStart) == 0)
                    {
                      if(nResult == 2)
                      {
                        hItem = TreeView_GetChild(hwCtrl, hItem);
                        break;
                      }
                      else
                      {
                        TreeView_SelectItem(hwCtrl, hItem);
                        goto AfterState;
                      }
                    }
                    hItem = TreeView_GetNextSibling(hwCtrl, hItem);
                  }

                  FREE(pszText);
                }

                nPrevResult = nResult;

                IOLI_RestoreItemStructure(pField->pszState, &pszStart, &pszEnd, nResult);
              }
              AfterState:;
            }

            if(pField->pszStateImageList && pField->nFlags & FLAG_CHECKBOXES)
              SetWindowLong(hwCtrl, GWL_STYLE ,dwStyle | TVS_CHECKBOXES);
          }

          TreeView_SetTextColor(hwCtrl, pField->crTxtColor);
          TreeView_SetBkColor(hwCtrl, pField->crBgColor);

          TreeView_SetItemHeight(hwCtrl, pField->nListItemsHeight);

          break;
        }

        case FIELD_LISTVIEW:
        {
          if (pField->nFlags & FLAG_CHECKBOXES && FileExists(pField->pszStateImageList))
            ListView_SetExtendedListViewStyleEx(hwCtrl, LVS_EX_CHECKBOXES, LVS_EX_CHECKBOXES); 
          else
            pField->nFlags &= ~FLAG_CHECKBOXES;

          // "ListItems" has to exist in order to continue
          if (pField->pszListItems)
          {
            // Step 1: Implement image list feature.
            //----------------------------------------------------------------
            
            // Step 1.1: Detect number of items.
            INT_PTR nItems = FIELD_LISTVIEW_IOLI_CountItems(pField->pszListItems, 0);

            // Step 1.2: State image list.
            if(pField->nFlags & FLAG_CHECKBOXES)
            {
              if(FileExists(pField->pszStateImageList))
              {
                // Set the image list to the ListView control
                HBITMAP hBitmap = (HBITMAP)LoadImage(0, pField->pszStateImageList, IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
                HIMAGELIST hImageList = (HIMAGELIST)ImageList_Create(16, 16, ILC_COLOR32|ILC_MASK, 6, 0);
                ImageList_AddMasked(hImageList,hBitmap,RGB(255,0,255));
                ListView_SetImageList(hwCtrl, hImageList, LVSIL_STATE);

                DeleteObject((HBITMAP)hBitmap);
              }
            }

            // Step 1.3: Small items image list.
            if(pField->pszSmallImageList)
            {
              if(FileExists(pField->pszSmallImageList))
              {
                // Set the image list to the ListView control
                HBITMAP hBitmap = (HBITMAP)LoadImage(0, pField->pszSmallImageList, IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
                HIMAGELIST hImageList = (HIMAGELIST)ImageList_Create(16, 16, ILC_COLOR32|ILC_MASK, nItems, 0);
                ImageList_AddMasked(hImageList,hBitmap,RGB(255,0,255));
                ListView_SetImageList(hwCtrl, hImageList, LVSIL_SMALL);

                DeleteObject((HBITMAP)hBitmap);
              }
            }

            // Step 1.4: Large items image list.
            if(pField->pszLargeImageList)
            {
              if(FileExists(pField->pszLargeImageList))
              {
                // Set the image list to the ListView control
                HBITMAP hBitmap = (HBITMAP)LoadImage(0, pField->pszLargeImageList, IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
                HIMAGELIST hImageList = (HIMAGELIST)ImageList_Create(16, 16, ILC_COLOR32|ILC_MASK, nItems, 0);
                ImageList_AddMasked(hImageList,hBitmap,RGB(255,0,255));
                ListView_SetImageList(hwCtrl, hImageList, LVSIL_NORMAL);

                DeleteObject((HBITMAP)hBitmap);
              }
            }

            // Step 2: Create columns.
            //----------------------------------------------------------------

            // Step 2.1: Detect number of colums to add.
            int nColumns = FIELD_LISTVIEW_IOLI_CountSubItems(pField->pszListItems, pField->pszHeaderItems) + 1; // Plus an item column.

            // Step 2.2: Insert a fake column.
            LVCOLUMN lvc;

            int nSubItem = 0;

            lvc.mask = LVCF_WIDTH;
            lvc.cx = 0;

            ListView_InsertColumn(hwCtrl, 0, &lvc);
            
            // Step 2.3: Create custom columns.
            LPTSTR pszHeaderStart, pszHeaderEnd;
            pszHeaderStart = pszHeaderEnd = pField->pszHeaderItems;

            LPTSTR pszHeaderItemsWidthStart, pszHeaderItemsWidthEnd;
            pszHeaderItemsWidthStart = pszHeaderItemsWidthEnd = pField->pszHeaderItemsWidth;

            LPTSTR pszHeaderItemsAlignStart, pszHeaderItemsAlignEnd;
            pszHeaderItemsAlignStart = pszHeaderItemsAlignEnd = pField->pszHeaderItemsAlign;

            int nHeaderResult = 0;
            int nHeaderItemsWidthResult = 0;
            int nHeaderItemsAlignResult = 0;

            // pField->pszHeaderItems has a trailing pipe
            if(pField->pszHeaderItems)
            {
              pszHeaderStart = pszHeaderEnd = pField->pszHeaderItems;

              while((nHeaderResult = IOLI_NextItem(pField->pszHeaderItems, &pszHeaderStart, &pszHeaderEnd, 1)) && nSubItem < nColumns)
              {
                lvc.mask = LVCF_FMT | LVCF_WIDTH | LVCF_TEXT | LVCF_SUBITEM;

                lvc.cx = 100;
                if(pField->pszHeaderItemsWidth)
                {
                  nHeaderItemsWidthResult = IOLI_NextItem(pField->pszHeaderItemsWidth, &pszHeaderItemsWidthStart, &pszHeaderItemsWidthEnd, 1);

                  if (*pszHeaderItemsWidthStart)
                    lvc.cx = myatoi(pszHeaderItemsWidthStart);

                  IOLI_RestoreItemStructure(pField->pszHeaderItemsWidth, &pszHeaderItemsWidthStart, &pszHeaderItemsWidthEnd, nHeaderItemsWidthResult);
                }

                lvc.fmt = (bRTL ? LVCFMT_RIGHT : LVCFMT_LEFT);
                // pField->pszHeaderItemsAlign has a trailing pipe
                if(pField->pszHeaderItemsAlign)
                {
                  nHeaderItemsAlignResult = IOLI_NextItem(pField->pszHeaderItemsAlign, &pszHeaderItemsAlignStart, &pszHeaderItemsAlignEnd, 1);

                  if (*pszHeaderItemsAlignStart)
                  {
                    if(lstrcmp(pszHeaderItemsAlignStart,_T("LEFT"))==0)
                      lvc.fmt = (bRTL ? LVCFMT_RIGHT : LVCFMT_LEFT);
                    else if(lstrcmp(pszHeaderItemsAlignStart,_T("CENTER"))==0)
                      lvc.fmt = LVCFMT_CENTER;
                    else if(lstrcmp(pszHeaderItemsAlignStart,_T("RIGHT"))==0)
                      lvc.fmt = (bRTL ? LVCFMT_LEFT : LVCFMT_RIGHT);
                  }

                  IOLI_RestoreItemStructure(pField->pszHeaderItemsAlign, &pszHeaderItemsAlignStart, &pszHeaderItemsAlignEnd, nHeaderItemsAlignResult);
                }

                lvc.pszText = pszHeaderStart;
                lvc.iSubItem = nSubItem;
                nSubItem++;

                ListView_InsertColumn(hwCtrl, nSubItem, &lvc);

                IOLI_RestoreItemStructure(pField->pszHeaderItems, &pszHeaderStart, &pszHeaderEnd, nHeaderResult);
              }
            }

            ListView_DeleteColumn(hwCtrl, 0);

            // Step 3: Include items to ListView.
            //----------------------------------------------------------------

            int nResult = 0;

            if(pField->pszListItems)
            {
              LPTSTR pszStart, pszEnd;
              pszStart = pszEnd = pField->pszListItems;

              int nResult = 0;

              int nLevel = 0;
              INT_PTR nItem = -1;
              nSubItem = 0;

              while(nResult = IOLI_NextItem(pField->pszListItems, &pszStart, &pszEnd, 0))
              {
                if((nResult == 5 && lstrcmp(pszStart,_T(""))==0)||nResult == 6)
                  break;

                nItem++;
                // Provide information about the item
                LVITEM lvi;

                lvi.mask = LVIF_TEXT | LVIF_IMAGE;
                lvi.pszText = pszStart;
                lvi.cchTextMax = strlen(pszStart);
                lvi.iItem = nItem;

                if(nLevel <= 0)
                {
                  lvi.mask |= LVIF_STATE;
                  lvi.iImage = nItem;
                  lvi.state = 0;
                  lvi.stateMask = 0;

                  lvi.iSubItem = 0;
                  // Insert the item to the ListView control
                  ListView_InsertItem(hwCtrl, &lvi);
                }
                else
                {
                  nItem--;
                  nSubItem++;

                  lvi.iItem = nItem;
                  lvi.iSubItem = nSubItem;
                  // Set the information to the respective item
                  ListView_SetItem(hwCtrl, &lvi);
                }

                switch (nResult)
                {
                  case 2: {nLevel++; break;}
                  case 1: break;
                  case 3: {nLevel--; nSubItem = 0; break;}
                }

                IOLI_RestoreItemStructure(pField->pszListItems, &pszStart, &pszEnd, nResult);
              }
            }

            // Step 4: Select items.
            //----------------------------------------------------------------

            if(pField->pszState)
            {
              INT_PTR nItem = -1;

              // Search the "State" string given by the user for ListView items

              LPTSTR pszStart, pszEnd;
              pszStart = pszEnd = pField->pszState;
              while(nResult = IOLI_NextItem(pField->pszState, &pszStart, &pszEnd, 1))
              {
                if((nResult == 5 && lstrcmp(pszStart,_T(""))==0)||nResult == 6)
                  break;

                if(pField->nFlags & FLAG_CHECKBOXES)
                {
                  nItem++;
                  LVITEM lvi;

                  lvi.mask = LVIF_STATE;
                  lvi.iItem = nItem;
                  lvi.iSubItem = 0;

                  lvi.state = INDEXTOSTATEIMAGEMASK(2);
                  lvi.stateMask = LVIS_STATEIMAGEMASK;

                  int nState = myatoi(pszStart);
                  int iState = INDEXTOSTATEIMAGEMASK(2);

                  // No checkboxes (TREEVIEW_NOCHECKBOX)
                  //------------------------------------
                  if(nState & TREEVIEW_NOCHECKBOX)
                    iState = INDEXTOSTATEIMAGEMASK(0);

                  // Read-only checkboxes (TREEVIEW_READONLY)
                  //-----------------------------------------
                  else if(nState & TREEVIEW_READONLY)
                  {
                    if(nState & TREEVIEW_CHECKED)
                      // Read-only checked checkboxes (TREEVIEW_READONLY | TREEVIEW_CHECKED)
                      iState = INDEXTOSTATEIMAGEMASK(6);
                    else
                      // Read-only unchecked checkboxes (TREEVIEW_READONLY)
                      iState = INDEXTOSTATEIMAGEMASK(5);
                  }
                 
                  // Checked checkboxes (TREEVIEW_CHECKED)
                  //-----------------------------------------
                  else if(nState & TREEVIEW_CHECKED)
                      iState = INDEXTOSTATEIMAGEMASK(3);

                  ListView_SetItemState(hwCtrl, nItem, iState, LVIS_STATEIMAGEMASK);
                }
                else
                {
                  LPTSTR pszText = (LPTSTR)MALLOC(260+1);
                  while(true)
                  {
                    nItem++;
                    // Provide information about the item
                    ListView_GetItemText(hwCtrl, nItem, 0, pszText, 260);
                    if(lstrcmp(pszStart, pszText)==0)
                    {
                      ListView_SetItemState(hwCtrl, nItem, LVIS_SELECTED, LVIS_SELECTED);
                      break;
                    }
                    if(ListView_GetNextItem(hwCtrl, nItem, 0) == -1)
                      break;
                  }
                  FREE(pszText);
                }

                IOLI_RestoreItemStructure(pField->pszState, &pszStart, &pszEnd, nResult);
              }
            }
          }

          ListView_SetTextColor(hwCtrl, pField->crTxtColor);
          ListView_SetBkColor(hwCtrl, pField->crBgColor);
          ListView_SetTextBkColor(hwCtrl, pField->crBgColor);
          break;
        }

        case FIELD_BUTTON:
        {
          if(FileExists(pField->pszText))
          {
            if (pField->nFlags & FLAG_BITMAP) {
              HANDLE hImageButton = LoadImage(m_hInstance,pField->pszText,IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
              mySendMessage(hwCtrl, BM_SETIMAGE, IMAGE_BITMAP, (LPARAM) hImageButton);
            }
            else if (pField->nFlags & FLAG_ICON) {
              HANDLE hImageButton = LoadImage(m_hInstance,pField->pszText,IMAGE_ICON, 0, 0, LR_LOADFROMFILE);
              mySendMessage(hwCtrl, BM_SETIMAGE, IMAGE_ICON, (LPARAM) hImageButton);
            }
          }
          break;
        }

        case FIELD_PROGRESSBAR:
        {
          if(pField->nMaxLength)
            mySendMessage(hwCtrl, PBM_SETRANGE32, 0, pField->nMaxLength);
          else
            mySendMessage(hwCtrl, PBM_SETRANGE32, 0, 100);

          int nState = myatoi(pField->pszState);

          mySendMessage(hwCtrl, PBM_SETPOS, nState, NULL);

          // Set Indicator and Background colors

          mySendMessage(hwCtrl, PBM_SETBARCOLOR, 0, pField->crTxtColor);
          if(pField->crBgColor != 0xFFFFFFFF)
            mySendMessage(hwCtrl, PBM_SETBKCOLOR, 0, pField->crBgColor);

          break;
        }
        case FIELD_TRACKBAR:
        {
          if(pField->nMaxLength)
            mySendMessage(hwCtrl, TBM_SETRANGEMAX, TRUE, pField->nMaxLength - 1);
          else
            mySendMessage(hwCtrl, TBM_SETRANGEMAX, TRUE, 100 - 1);

          mySendMessage(hwCtrl, TBM_SETTICFREQ, 1, NULL);
          mySendMessage(hwCtrl, TBM_SETPOS, TRUE, myatoi(pField->pszState));
          break;
        }
        case FIELD_IPADDRESS:
        {
          int nResult = strlen(pField->pszState);

          pField->pszState[nResult] = '.';
          pField->pszState[nResult + 1] = '\0';

          TCHAR *pszStart, *pszEnd, *pszList;
          pszStart = pszEnd = pszList = STRDUP(pField->pszState);

          int iItem = 0;
          BYTE b0 = 0;
          BYTE b1 = 0;
          BYTE b2 = 0;
          BYTE b3 = 0;

          while (*pszEnd) {
            if (*pszEnd == _T('.')) {
              *pszEnd = _T('\0');
              iItem++;
              if (*pszStart) {
                switch(iItem)
                {
                  case 1:
                    b0 = myatoi(pszStart);
                    break;
                  case 2:
                    b1 = myatoi(pszStart);
                    break;
                  case 3:
                    b2 = myatoi(pszStart);
                    break;
                  case 4:
                    b3 = myatoi(pszStart);
                    break;
                  default:
                    break;
                }
              }
              pszStart = ++pszEnd;
            }
            else
              pszEnd = CharNext(pszEnd);
          }

          LPARAM nIP = MAKEIPADDRESS(b0,b1,b2,b3);

          mySendMessage(hwCtrl, IPM_SETADDRESS, NULL, nIP);

          break;
        }
        case FIELD_DATETIME:
        {
          SYSTEMTIME SystemTime;

          if(pField->pszState != NULL) {

            int nResult = strlen(pField->pszState);

            pField->pszState[nResult] = ' ';
            pField->pszState[nResult + 1] = '\0';

            TCHAR *pszStart, *pszEnd, *pszList;
            pszStart = pszEnd = pszList = STRDUP(pField->pszState);

            int iItem = 0;

            while (*pszEnd) {
              if ((*pszEnd == '/')||(*pszEnd == '\\')||(*pszEnd == '-')||(*pszEnd == ':')||(*pszEnd == ' ')) {
                *pszEnd = '\0';
                iItem++;
                if (*pszStart) {
                  switch(iItem)
                  {
                    case 1:
                      SystemTime.wDay = myatoi(pszStart); //Day
                      break;
                    case 2:
                      SystemTime.wMonth = myatoi(pszStart); //Month
                      break;
                    case 3:
                      SystemTime.wYear = myatoi(pszStart); //Year
                      break;
                    case 4:
                      SystemTime.wHour = myatoi(pszStart); //Hour
                      break;
                    case 5:
                      SystemTime.wMinute = myatoi(pszStart); //Minute
                      break;
                    case 6:
                      SystemTime.wSecond = myatoi(pszStart); //Second
                      break;
                    case 7:
                      SystemTime.wDayOfWeek = myatoi(pszStart); //DayOfWeek
                      break;
                    default:
                      break;
                  }
                }
                pszStart = ++pszEnd;
              }
              else
                pszEnd = CharNext(pszEnd);
            }
          }
          else
          {
            GetSystemTime(&SystemTime);
          }
          mySendMessage(hwCtrl, DTM_SETSYSTEMTIME, GDT_VALID, (LPARAM) (LPSYSTEMTIME) &SystemTime);

          if(pField->pszText)
            mySendMessage(hwCtrl, DTM_SETFORMAT, 0, (LPARAM) (LPCTSTR) pField->pszText);

          // Set colors
          mySendMessage(hwCtrl, DTM_SETMCCOLOR, MCSC_TEXT, pField->crTxtColor);
          mySendMessage(hwCtrl, DTM_SETMCCOLOR, MCSC_MONTHBK, pField->crBgColor);
          mySendMessage(hwCtrl, DTM_SETMCCOLOR, MCSC_TITLETEXT, pField->crSelTxtColor);
          mySendMessage(hwCtrl, DTM_SETMCCOLOR, MCSC_TITLEBK, pField->crSelBgColor);
          mySendMessage(hwCtrl, DTM_SETMCCOLOR, MCSC_BACKGROUND, pField->crMonthOutColor);
          mySendMessage(hwCtrl, DTM_SETMCCOLOR, MCSC_TRAILINGTEXT, pField->crMonthTrailingTxtColor);

          break;
        }
        case FIELD_MONTHCALENDAR:
        {
          SYSTEMTIME SystemTime;

          if(pField->pszState != NULL) {

            int nResult = strlen(pField->pszState);

            pField->pszState[nResult] = ' ';
            pField->pszState[nResult + 1] = '\0';

            TCHAR *pszStart, *pszEnd, *pszList;
            pszStart = pszEnd = pszList = STRDUP(pField->pszState);

            int iItem = 0;

            while (*pszEnd) {
              if ((*pszEnd == '/')||(*pszEnd == '\\')||(*pszEnd == '-')||(*pszEnd == ' ')) {
                *pszEnd = '\0';
                iItem++;
                if (*pszStart) {
                  switch(iItem)
                  {
                    case 1:
                      SystemTime.wDay = myatoi(pszStart); //Day
                      break;
                    case 2:
                      SystemTime.wMonth = myatoi(pszStart); //Month
                      break;
                    case 3:
                      SystemTime.wYear = myatoi(pszStart); //Year
                      break;
                    default:
                      break;
                  }
                }
                pszStart = ++pszEnd;
              }
              else
                pszEnd = CharNext(pszEnd);
            }
            mySendMessage(hwCtrl, MCM_SETCURSEL, 0, (LPARAM) (LPSYSTEMTIME) &SystemTime);
          }
          else
          {
            GetSystemTime(&SystemTime);
            mySendMessage(hwCtrl, MCM_SETCURSEL, 0, (LPARAM) (LPSYSTEMTIME) &SystemTime);
          }

          // Set colors
          mySendMessage(hwCtrl, MCM_SETCOLOR, MCSC_TEXT, pField->crTxtColor);
          mySendMessage(hwCtrl, MCM_SETCOLOR, MCSC_MONTHBK, pField->crBgColor);
          mySendMessage(hwCtrl, MCM_SETCOLOR, MCSC_TITLETEXT, pField->crSelTxtColor);
          mySendMessage(hwCtrl, MCM_SETCOLOR, MCSC_TITLEBK, pField->crSelBgColor);
          mySendMessage(hwCtrl, MCM_SETCOLOR, MCSC_BACKGROUND, pField->crMonthOutColor);
          mySendMessage(hwCtrl, MCM_SETCOLOR, MCSC_TRAILINGTEXT, pField->crMonthTrailingTxtColor);

          break;
        }
        case FIELD_RICHTEXT:
        {
          // Step 1: Set settings
          mySendMessage(hwCtrl,EM_AUTOURLDETECT,TRUE,0);

          LPARAM nEvents = ENM_LINK|ENM_KEYEVENTS;
          if(pField->nNotify & NOTIFY_CONTROL_ONTEXTCHANGE)
            nEvents |= ENM_CHANGE;
          if(pField->nNotify & NOTIFY_CONTROL_ONTEXTUPDATE) //Ignored if RichEdit version => 2
            nEvents |= ENM_UPDATE;
          if(pField->nNotify & NOTIFY_CONTROL_ONTEXTSELCHANGE)
            nEvents |= ENM_SELCHANGE;

          mySendMessage(hwCtrl,EM_SETEVENTMASK,0,nEvents);
          if(pField->nMaxLength)
            mySendMessage(hwCtrl,EM_EXLIMITTEXT,0,(LPARAM)pField->nMaxLength);
          else
            mySendMessage(hwCtrl,EM_EXLIMITTEXT,0,0xFFFFFFFE);

          if(pField->nFlags & FLAG_PASSWORD)
          {
            if(pField->pszText)
            {
              TCHAR* pszChar = (TCHAR*)MALLOC((1+1)*sizeof(TCHAR));
              strncpy(pszChar, pField->pszText, 1+1);
              mySendMessage(hwCtrl, EM_SETPASSWORDCHAR, (WPARAM) *pszChar, 0);
            }
            else
              mySendMessage(hwCtrl, EM_SETPASSWORDCHAR, (WPARAM) '*', 0);
          }

          // Step 2: Open the user file
          // escape the path name:  \n => \\n but not \ to \\
          // I can't use IOtoa because "c:\\fichier.rtf" don't work on windows 9x
          pField->pszState = IOtoaFolder(pField->pszState);

          EDITSTREAM editStream = {0};
          FILE * hFile;
          if ( _tfopen_s(&hFile, pField->pszState, _T("rb")) != 0)
          {
            //TODO: Add error handling
            break;
          }
          fseek(hFile,0,SEEK_END);
          UINT nDataLen=ftell(hFile);
          if (nDataLen == -1)
          {
            //TODO: Add error handling
            break;
          }
          rewind(hFile);
          char *pszData = (char*)MALLOC(nDataLen+1);

          if (fread(pszData,1,nDataLen,hFile) != nDataLen)
          {
            //TODO: Add error handling
            fclose(hFile);
            FREE(pszData);
            break;
          }
          pszData[nDataLen]=0;
          if (!strncmp(pszData,"{\\rtf",sizeof("{\\rtf")-1))
          pField->nDataType = SF_RTF;
          else
          pField->nDataType = SF_TEXT;
          fclose(hFile);

          dwRead=0;
          editStream.pfnCallback = FIELD_RICHTEXT_StreamIn;
          editStream.dwCookie = (DWORD_PTR)pszData;
          mySendMessage(hwCtrl,EM_STREAMIN,(WPARAM)pField->nDataType,(LPARAM)&editStream);

          FREE(pszData);
          break;
        }
      }

     pField->nParentIdx = (INT_PTR) SetWindowLongPtr(hwCtrl, GWLP_WNDPROC, (LONG_PTR)ControlWindowProc);

      // Set initial focus to the first appropriate field ( with FOCUS flag)
      if (!fFocusedByFlag && (dwStyle & (WS_TABSTOP | WS_DISABLED)) == WS_TABSTOP && pField->nType >= FIELD_SETFOCUS) {
        if (pField->nFlags & FLAG_FOCUS) {
            fFocusedByFlag = TRUE;
        }
        if (fFocusedByFlag) {
            fFocused = TRUE;
            mySetFocus(hwCtrl);
        } else if (!fFocused) {
          // If multiline-readonly don't initially select RichText controls.
          // useful for License Page look-a-likes.
          if (! (pField->nType == FIELD_RICHTEXT && (pField->nFlags & (FLAG_MULTILINE | FLAG_READONLY)) == (FLAG_MULTILINE | FLAG_READONLY)) ) {
                fFocused = TRUE;
                mySetFocus(hwCtrl);
          }
        }
      }

      // If multiline-readonly then hold the text back until after the
      // initial focus has been set. This is so the text is not initially
      // selected - useful for License Page look-a-likes.
      if (pField->nType == FIELD_TEXT && (pField->nFlags & (FLAG_MULTILINE | FLAG_READONLY)) == (FLAG_MULTILINE | FLAG_READONLY))
        mySetWindowText(hwCtrl, pField->pszState);
    }
  }

// Special Control Configurations
//================================================================
  for (int nIdx2 = 0; nIdx2 < nNumFields; nIdx2++)
  {
    IOExControlStorage *pField = pFields + nIdx2;
    if(pField->nType == FIELD_UPDOWN && pField->nRefFields) {
      HWND hBuddy = GetDlgItem(hConfigWindow,1200+pField->nRefFields-1);
      mySendMessage(pField->hwnd, UDM_SETBUDDY, (WPARAM) (HWND) hBuddy, 0);
      myGetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), pField->pszState);
      mySendMessage(pField->hwnd, UDM_SETPOS32, 0, myatoi(pField->pszState));
    }
  }

// Set focus to the first control (depending on controls order)
//================================================================
  if (!fFocused)
    mySetFocus(hNextButton);

// "Title" ("Settings") Implementation
//================================================================
  mySetWindowText(mainwnd,pszTitle);

// Pop filename and push the hConfigWindow handle to NSIS Stack
//================================================================
  *g_stacktop = (*g_stacktop)->next;

  static TCHAR tmp[32];
  wsprintf(tmp,_T("%d"),hConfigWindow);
  pushstring(tmp);

// Return to original function
//================================================================
  return 0;
}














///----------------------------------------------------------------
// Show function part
//================================================================
void WINAPI showCfgDlg()
{
// Step 1: Show Page
//================================================================

  // Let the plugin handle dialog messages for now on
  //--------------------------------------------------------------
  lpWndProcOld = (WNDPROC) SetWindowLongPtr(hMainWindow,DWLP_DLGPROC,(LONG_PTR)ParentWndProc);

  // Enable keyboard and mouse input and show back window
  //--------------------------------------------------------------
  mySendMessage(hMainWindow, WM_NOTIFY_CUSTOM_READY, (WPARAM)hConfigWindow, 0);
  ShowWindow(hConfigWindow, SW_SHOWNA);

  // Provide a way to leave the "while" below
  //--------------------------------------------------------------
  g_done = 0;

  // Notification System Implementation
  //--------------------------------------------------------------
  g_aNotifyQueue = (NotifyQueue*)MALLOC(sizeof(NotifyQueue)*g_nNotifyQueueAmountMax);

  g_aNotifyQueueTemp = (NotifyQueue*)MALLOC(sizeof(NotifyQueue));
  g_aNotifyQueueTemp->iNotifyId = NOTIFY_NONE;
  g_aNotifyQueueTemp->iField = 0;
  g_aNotifyQueueTemp->bNotifyType = NOTIFY_TYPE_PAGE;

  // Set the page timer (TimeOut Implementation)
  //--------------------------------------------------------------
  if(nTimeOutTemp != 0)
  {
    g_timer_cur_time = GetTickCount();
    SetTimer(hConfigWindow,g_timer_id,nTimeOutTemp,NULL);
  }

  // Handle hConfigWindow messages (code execution pauses here)
  //--------------------------------------------------------------
  while (!g_done) {
    GetMessage(&msg, NULL, 0, 0);

    if (!IsDialogMessage(hConfigWindow,&msg) && !IsDialogMessage(hMainWindow,&msg))
    {
      TranslateMessage(&msg);
      DispatchMessage(&msg);
    }
  }

  //KillTimer(hConfigWindow, g_notification_timer_id);

// Step 2: Destroy Page
//================================================================

  // Notification System Implementation
  //--------------------------------------------------------------
  
  // we don't save settings on cancel since that means your installer will likely
  // quit soon, which means the ini might get flushed late and cause delete issues?
  if (!g_is_cancel) SaveSettings();

  // Free buffers (g_aNotifyQueue)
  FREE(g_aNotifyQueue);
  FREE(g_aNotifyQueueTemp);
  
  // Let NSIS handle dialog messages for now on
  //--------------------------------------------------------------
  SetWindowLongPtr(hMainWindow,DWLP_DLGPROC,(LONG_PTR)lpWndProcOld);

  // Destroy the window (w/ all the controls inside)
  //--------------------------------------------------------------
  DestroyWindow(hConfigWindow);

  // Show installer buttons as they were before using plugin
  //--------------------------------------------------------------
  if (bNextShow!=-1) ShowWindow(hNextButton,old_next_visible?SW_SHOWNA:SW_HIDE);
  if (bBackShow!=-1) ShowWindow(hBackButton,old_back_visible?SW_SHOWNA:SW_HIDE);
  if (bCancelShow!=-1) ShowWindow(hCancelButton,old_cancel_visible?SW_SHOWNA:SW_HIDE);

  if (bNextEnabled!=-1) EnableWindow(hNextButton,old_next_enabled);
  if (bBackEnabled!=-1) EnableWindow(hBackButton,old_back_enabled);
  if (bCancelEnabled!=-1) EnableWindow(hCancelButton,old_cancel_enabled);

  // Free buffers (global) and handles
  //--------------------------------------------------------------
  FREE(pszTitle);
  FREE(pszCancelButtonText);
  FREE(pszNextButtonText);
  FREE(pszBackButtonText);

  // Kill the page timer (TimeOut Implementation)
  //--------------------------------------------------------------
  if(g_timer_id != 0)
  {
    KillTimer(hConfigWindow, g_timer_id);
    g_timer_id = 0;
    nTimeOut = nTimeOutTemp = 0;
  }

  // Free buffers (pField) and handles
  //--------------------------------------------------------------
  int i = nNumFields;
  while (i--) {
    IOExControlStorage *pField = pFields + i;

    int j = FIELD_BUFFERS;
    while (j--)
      FREE(((TCHAR **) pField)[j]);

    if (pField->nType == FIELD_IMAGE) {
      switch(pField->nDataType)
      {
        case IMAGE_TYPE_BITMAP:
        case IMAGE_TYPE_OLE:
        case IMAGE_TYPE_GDIPLUS:
        {
          DeleteObject(pField->hImage);
          break;
        }
        case IMAGE_TYPE_ICON:
        {
          DestroyIcon((HICON)pField->hImage);
          break;
        }
        case IMAGE_TYPE_CURSOR:
        {
          DestroyCursor((HCURSOR)pField->hImage);
          break;
        }
        case IMAGE_TYPE_ANIMATION:
        {
          Animate_Close((HWND)pField->hImage);
          break;
        }
      }
    }
    if (pField->nType == FIELD_TREEVIEW && pField->pszStateImageList) {
      // ListView controls automatically destroy image lists.
      ImageList_Destroy((HIMAGELIST)pField->hStateImageList);
    }
  }
  FREE(pFields);

  // Push the page direction to NSIS Stack
  //--------------------------------------------------------------
  pushstring(g_is_cancel?_T("cancel"):g_is_back?_T("back"):_T("success"));
}







INT_PTR CALLBACK ParentWndProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
// Notification System Base Implementation (Depends on Conditions)
//================================================================
  INT_PTR bRes;
  BOOL g_timer_notify = false;

  if (message == WM_NOTIFY_OUTER_NEXT && wParam == 1)
  {
  // we don't save settings on cancel
  if (!g_is_cancel)
  {
      // Don't call leave function if fields aren't valid
      if (!g_aNotifyQueue[0].iField && !ValidateFields())
        return 0;
      // Get the settings ready for the leave function verification
    SaveSettings();
  }

    // Make default notification as "ONNEXT" so to confirm that "State"
    // and "Notify" INI value names were written under "Settings" INI
    // section when the page itself notifies (not controls).
    if (g_aNotifyQueue[0].bNotifyType == NOTIFY_TYPE_PAGE)
    {
      g_aNotifyQueue[0].iField = 0;
      if(g_timer_activated)
        g_aNotifyQueue[0].iNotifyId = NOTIFY_PAGE_ONTIMEOUT;
      else
        g_aNotifyQueue[0].iNotifyId = NOTIFY_PAGE_ONNEXT;

      WritePrivateProfileString(_T("Settings"), _T("State"), _T("0"), pszFilename);
      WritePrivateProfileString(_T("Settings"), _T("Notify"), LookupTokenName(PageNotifyTable, g_aNotifyQueue[0].iNotifyId), pszFilename);
    }

    // Timer: Allow SetTimer to reset the timer again.
    if(nTimeOut >= USER_TIMER_MINIMUM)
      g_timer_notify = true;

    // Reset the record of activated control and notification
    RemoveNotifyQueueItem();
  }

// Call lpWndProcOld (-> DialogProc NSIS function - this also calls the leave function if necessary)
//================================================================
  bRes = CallWindowProc(lpWndProcOld,hwnd,message,wParam,lParam);

// Start To Get Out Of This Page (Depends on Conditions)
//================================================================
  if (message == WM_NOTIFY_OUTER_NEXT && !bRes)
  {
    WritePrivateProfileString(_T("Settings"), _T("State"), _T("0"), pszFilename);
    // if leave function didn't abort (bRes != 0 in that case)
    if (wParam == (WPARAM)-1)
    {
      WritePrivateProfileString(_T("Settings"), _T("Notify"), LookupTokenName(PageNotifyTable, NOTIFY_PAGE_ONBACK), pszFilename);
      g_is_back = true;
    }
    else if (wParam == NOTIFY_BYE_BYE)
    {
      WritePrivateProfileString(_T("Settings"), _T("Notify"), LookupTokenName(PageNotifyTable, NOTIFY_PAGE_ONCANCEL), pszFilename);
      g_is_cancel = true;
    }
    else
    {
      if (g_timer_activated)
        WritePrivateProfileString(_T("Settings"), _T("Notify"), LookupTokenName(PageNotifyTable, NOTIFY_PAGE_ONTIMEOUT), pszFilename);
      else
        WritePrivateProfileString(_T("Settings"), _T("Notify"), LookupTokenName(PageNotifyTable, NOTIFY_PAGE_ONNEXT), pszFilename);
    }

    g_done++;
    PostMessage(hConfigWindow,WM_CLOSE,0,0);
  }
  else if (message == WM_NOTIFY_OUTER_NEXT && bRes && g_timer_notify)
  {
    // Set the page timer if notified (TimeOut Implementation)
    //--------------------------------------------------------------
    g_timer_cur_time = GetTickCount();
    SetTimer(hConfigWindow,g_timer_id,nTimeOut,NULL);
    nTimeOut = nTimeOutTemp;
  }

  g_timer_activated = false;
  return bRes;
}














//================================================================
// Dialog Configuration Proc
//================================================================
BOOL CALLBACK DialogWindowProc(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
// Handle Advanced Notification Messages
//================================================================

  switch (uMsg)
  {
    case WM_TIMER:
    {
      if (wParam == g_timer_id)
      {
        KillTimer(hConfigWindow,g_timer_id);
        g_timer_activated = true;
        mySendMessage(hMainWindow, WM_NOTIFY_OUTER_NEXT, 1, 0);
      }
    }
    case WM_COMMAND:
    {
      UINT nIdx = LOWORD((DWORD)wParam);
      UINT nNotifyCode = HIWORD((DWORD)wParam);
      HWND hCtrl = (HWND) lParam;

      if (nIdx < 0)
        break;
      IOExControlStorage *pField = pFields + nIdx;

      NotifyProc(hwndDlg, nIdx, hCtrl, nNotifyCode);
      break;
    }
    case WM_NOTIFY:
    {
      LPNMHDR hdr = (LPNMHDR)lParam;

      UINT nIdx = hdr->idFrom;
      UINT nNotifyCode = hdr->code;
      HWND hCtrl = GetDlgItem(hwndDlg, nIdx);

      if (nIdx < 0)
        break;
      IOExControlStorage *pField = pFields + FindControlIdx(nIdx);

      if(pField->nType == FIELD_TREEVIEW)
      {
        switch(nNotifyCode)
        {
          case NM_CLICK:
          {
            LPNMHDR hdr = (LPNMHDR)lParam;
            TVHITTESTINFO ht;
            GetCursorPos(&ht.pt);

            ScreenToClient(hCtrl, &ht.pt);

            HTREEITEM hItem = TreeView_HitTest(hCtrl, &ht);
            if(!(ht.flags & TVHT_ONITEMSTATEICON)) break;
            FIELD_TREEVIEW_Check(hCtrl, hItem, TREEVIEW_AUTOCHECK, 0xFFFFFFFF, FALSE);
          }
          case TVN_ITEMEXPANDED:
          {
            LPNMTREEVIEW pnmtv = (LPNMTREEVIEW)lParam;
            if (pnmtv->action == TVE_EXPAND)
              pnmtv->itemNew.lParam |= TREEVIEW_EXPANDED;
            else
              pnmtv->itemNew.lParam &= ~TREEVIEW_EXPANDED;
            TreeView_SetItem(hCtrl, &pnmtv->itemNew);
          }
          case TVN_BEGINLABELEDIT:
          {
            pField->hEditControl = TreeView_GetEditControl(hCtrl);
            return 0;
          }
          case TVN_ENDLABELEDIT:
          {
            NMTVDISPINFO* nmtvdi = (NMTVDISPINFO*)lParam;

            if((TCHAR)msg.wParam != VK_ESCAPE && nmtvdi->item.mask & LVIF_TEXT)
            {
              TVITEM tvItem;
              tvItem.mask = TVIF_TEXT;
              tvItem.pszText = nmtvdi->item.pszText;
              tvItem.hItem = nmtvdi->item.hItem;
              TreeView_SetItem(hCtrl, &tvItem);

              return 1;
            }
            else
              return 0;
          }
          case TVN_KEYDOWN:
          {
            LPNMTVKEYDOWN nmtvkd = (LPNMTVKEYDOWN)lParam;

            if(pField->nFlags & FLAG_EDITLABELS && nmtvkd->wVKey == VK_F2)
            {
              HTREEITEM iSelectedItem = TreeView_GetNextItem(hCtrl, -1, TVGN_CARET);

              if(!iSelectedItem)
                break;

              TreeView_EditLabel(hCtrl, iSelectedItem);
              break;
            }

            if(pField->nFlags & FLAG_CHECKBOXES && nmtvkd->wVKey == VK_SPACE)
            {
              HTREEITEM hItem = TreeView_GetSelection(hCtrl);
              FIELD_TREEVIEW_Check(hCtrl, hItem, TREEVIEW_AUTOCHECK, 0xFFFFFFFF, FALSE);
            }
            return 0;
          }
        }
      }

      else if(pField->nType == FIELD_LISTVIEW)
      {
        switch(nNotifyCode)
        {
          case NM_CLICK:
          case NM_DBLCLK:
          {
            LPNMHDR hdr = (LPNMHDR)lParam;
            LVHITTESTINFO ht;
            GetCursorPos(&ht.pt);

            ScreenToClient(hCtrl, &ht.pt);

            int iItem = ListView_HitTest(hCtrl, &ht);
            if(!(ht.flags & LVHT_ONITEMSTATEICON)) break;

            // Get Item Information
            //--------------------------------------------------------------
            int iState = ListView_GetItemState(hCtrl, iItem, LVIS_STATEIMAGEMASK) >> 12;

            // "Normal" checkboxes (Leaving from Checked state)
            //--------------------------------------------------------------
            if(iState == 3)
              iState-= 2;
            else if(iState >= 4 || iState == 1 || iState <= -1)
              iState--;

            ListView_SetItemState(hCtrl, iItem, INDEXTOSTATEIMAGEMASK(iState), LVIS_STATEIMAGEMASK);
          }
          case LVN_BEGINLABELEDIT:
          {
            pField->hEditControl = ListView_GetEditControl(hCtrl);
            return 0;
          }
          case LVN_ENDLABELEDIT:
          {
            NMLVDISPINFO* nmlvdi = (NMLVDISPINFO*)lParam;

            if((TCHAR)msg.wParam != VK_ESCAPE && nmlvdi->item.mask & LVIF_TEXT)
            {
              ListView_SetItemText(hCtrl, nmlvdi->item.iItem, 0, nmlvdi->item.pszText);
              return 1;
            }
            else
              return 0;
          }
          case LVN_KEYDOWN:
          {
            LPNMLVKEYDOWN nmlvkd = (LPNMLVKEYDOWN)lParam;

            if(pField->nFlags & FLAG_EDITLABELS && nmlvkd->wVKey == VK_F2)
            {
              int iSelectedItem = ListView_GetNextItem(hCtrl, -1, LVNI_FOCUSED);

              if(iSelectedItem == -1)
                break;

              ListView_EditLabel(hCtrl, iSelectedItem);
              break;
            }

            if(pField->nFlags & FLAG_CHECKBOXES && nmlvkd->wVKey == VK_SPACE)
            {
              int iSelectedItem = -1;

              while(true)
              {
                iSelectedItem = ListView_GetNextItem(hCtrl, iSelectedItem, LVNI_SELECTED);
                if(iSelectedItem == -1)
                  break;

                int iSelectionState = ListView_GetItemState(hCtrl, iSelectedItem, LVIS_STATEIMAGEMASK|LVNI_SELECTED|LVNI_FOCUSED);
                int iState = iSelectionState >> 12;

                // "Normal" checkboxes (Leaving from Checked state)
                //--------------------------------------------------------------
                if(iState == 3)
                {
                  if(iSelectionState & LVNI_SELECTED && iSelectionState & LVNI_FOCUSED)
                    iState -= 2;
                  else
                    iState--;
                }
                else if(iState == 2 && !(iSelectionState & LVNI_SELECTED && iSelectionState & LVNI_FOCUSED))
				{
					iState += 1;
				}
				else if(iState == 0 && (iSelectionState & LVNI_SELECTED && iSelectionState & LVNI_FOCUSED))
                {
                  iState;
                }
                else if((iState >= 4 || iState <= 1) && (iSelectionState & LVNI_SELECTED && iSelectionState & LVNI_FOCUSED))
                {
                  iState--;
                }

                ListView_SetItemState(hCtrl, iSelectedItem, INDEXTOSTATEIMAGEMASK(iState), LVIS_STATEIMAGEMASK);
              }
            }
            return 0;
          }
        }
      }

      else if(pField->nType == FIELD_RICHTEXT)
      {
        switch(nNotifyCode)
        {
          case EN_LINK:
          {
            ENLINK * enlink = (ENLINK *)lParam;
            LPTSTR ps_tmpbuf = (LPTSTR)MALLOC(g_nBufferSize);

            if(pField->nNotify == NOTIFY_CONTROL_ONCLICK)
            {
              if (enlink->msg==WM_LBUTTONDOWN) {
                TEXTRANGE tr = {
                  {
                    enlink->chrg.cpMin,
                    enlink->chrg.cpMax,
                  },
                  ps_tmpbuf
                };

                if (tr.chrg.cpMax-tr.chrg.cpMin < g_nBufferSize) {
                  SendMessage(hCtrl,EM_GETTEXTRANGE,0,(LPARAM)&tr);
                  SetCursor(LoadCursor(0,IDC_WAIT));
                  ShellExecute(hConfigWindow,_T("open"),tr.lpstrText,NULL,NULL,SW_SHOWNORMAL);
                  SetCursor(LoadCursor(0,IDC_ARROW));
                }

                // START: Parts of NotifyProc.
                g_aNotifyQueueTemp->iField = nIdx + 1;
                g_aNotifyQueueTemp->bNotifyType = NOTIFY_TYPE_CONTROL;

                AddNotifyQueueItem(g_aNotifyQueueTemp);

                // Kill the page timer if notified (TimeOut Implementation)
                //================================================================
                if(g_timer_id != 0)
                {
                  nTimeOut = nTimeOutTemp - (GetTickCount() - g_timer_cur_time);
                  if (nTimeOut < USER_TIMER_MINIMUM)
                    nTimeOut = 0;
                  KillTimer(hConfigWindow, g_timer_id);
                }

                // Simulate "Next" Click So NSIS Can Call Its PageAfter Function
                //================================================================
                mySendMessage(hMainWindow, WM_NOTIFY_OUTER_NEXT, 1, 0);
                // END: Part of NotifyProc.
              }

              return 0;
            }
          }
          case EN_MSGFILTER:
          {
            MSGFILTER * msgfilter = (MSGFILTER *)lParam;
            if (msgfilter->msg==WM_KEYDOWN)
            {
              if ((!IsWindowEnabled(hdr->hwndFrom) || pField->nFlags & FLAG_READONLY) && msgfilter->wParam==VK_RETURN)
                SendMessage(hMainWindow, WM_COMMAND, IDOK, 0);
              if (msgfilter->wParam==VK_ESCAPE)
                SendMessage(hMainWindow, WM_CLOSE, 0, 0);
              return 1;
            }
          }
        }
      }

      NotifyProc(hwndDlg, nIdx, hCtrl, nNotifyCode);
      break;
    }

    case WM_HSCROLL:
    case WM_VSCROLL:
    {
      int nTempIdx = FindControlIdx(GetDlgCtrlID((HWND)lParam));
      // Ignore if the dialog is in the process of being created
      if (g_done || nTempIdx < 0)
        break;
      IOExControlStorage *pField = pFields + nTempIdx;

      if(pField->nType == FIELD_TRACKBAR) {
        UINT nIdx = GetDlgCtrlID((HWND)lParam);
        UINT nNotifyCode = LOWORD(wParam);
        HWND hCtrl = (HWND)lParam;
        NotifyProc(hwndDlg, nIdx, hCtrl, nNotifyCode);
      }
      return 0;
    }

    case WM_MEASUREITEM:
    {
      LPMEASUREITEMSTRUCT lpmis = (LPMEASUREITEMSTRUCT) lParam;

      int nIdx = FindControlIdx(lpmis->CtlID);

      if (nIdx < 0)
        break;
      IOExControlStorage *pField = pFields + nIdx;

      switch(pField->nType)

      case FIELD_LISTBOX:
      case FIELD_COMBOBOX:

        // Set the height of the items
        lpmis->itemHeight = pField->nListItemsHeight;

      return TRUE;
    }

    case WM_DRAWITEM:
    {
      DRAWITEMSTRUCT* lpdis = (DRAWITEMSTRUCT*)lParam;
      int nIdx = FindControlIdx(lpdis->CtlID);

      if (nIdx < 0)
        break;
      IOExControlStorage *pField = pFields + nIdx;

      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP && pField->nType != FIELD_LINK)
        return 0; //Return false so the default handling is used

      // Set text and background colors now

      if(pField->nType == FIELD_LABEL || pField->nType == FIELD_LINK || pField->nType == FIELD_BUTTON) {

        // We need lpdis->rcItem later
        RECT rc = lpdis->rcItem;

        ++rc.left;
           --rc.right;

        if(pField->nType == FIELD_LINK)
        {
          ++rc.left;
             --rc.right;
        }

        // Move rect to right if in RTL mode
        if (bRTL)
        {
          rc.left += lpdis->rcItem.right - rc.right;
          rc.right += lpdis->rcItem.right - rc.right;
        }

        if (lpdis->itemAction & ODA_DRAWENTIRE)
        {
          COLORREF crBgColor;
          COLORREF crTxtColor;
          COLORREF crTxtShwColor;

          if(IsWindowEnabled(lpdis->hwndItem)) {
            if (pField->crBgColor == 0xFFFFFFFF && GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA))
              crBgColor = GetBkColor(lpdis->hDC);
            else
              crBgColor = pField->crBgColor;

            if (pField->crTxtColor == 0xFFFFFFFF)
            {
              if (GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA))
                crTxtColor = GetTextColor(lpdis->hDC);
              else
                crTxtColor = RGB(0,0,255);
            }
            else
            {
              crTxtColor = pField->crTxtColor;
            }

            if (pField->crTxtShwColor == 0xFFFFFFFF && GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA))
              crTxtShwColor = GetBkColor(lpdis->hDC);
            else
              crTxtShwColor = pField->crTxtShwColor;
          } else {
            if (pField->crDisBgColor == 0xFFFFFFFF && GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA))
              crBgColor = GetBkColor(lpdis->hDC);
            else
              crBgColor = pField->crDisBgColor;

            if (pField->crDisTxtColor == 0xFFFFFFFF)
            {
              if(GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA))
                crTxtColor = GetSysColor(COLOR_GRAYTEXT);
              else
                crTxtColor = RGB(0,0,100);
            }
            else
              crTxtColor = pField->crDisTxtColor;

            if (pField->crDisTxtShwColor == 0xFFFFFFFF)
            {
              if(GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA))
                crTxtColor = GetSysColor(COLOR_WINDOW);
              else
                crTxtColor = RGB(255, 255, 255);
            }
            else
              crTxtColor = pField->crDisTxtShwColor;
          }

          // Draw Background on the whole control
          if(crBgColor != 0xFFFFFFFF && GetBkMode(lpdis->hDC) != TRANSPARENT) {

            HBRUSH hBrush = CreateSolidBrush(crBgColor);

            HGDIOBJ hOldSelObj = SelectObject(lpdis->hDC, hBrush);
            if(GetDeviceCaps(lpdis->hDC, RASTERCAPS) & RC_BITBLT) {
              int rcWidth = lpdis->rcItem.right - lpdis->rcItem.left;
              int rcHeight = lpdis->rcItem.bottom - lpdis->rcItem.top;
              PatBlt(lpdis->hDC, lpdis->rcItem.left, lpdis->rcItem.top, rcWidth, rcHeight, PATCOPY);
            }
            SelectObject(lpdis->hDC, hOldSelObj);
          }

          int clrBackgroundMode = SetBkMode(lpdis->hDC, TRANSPARENT);

          if(crTxtShwColor != 0xFFFFFFFF && GetBkMode(lpdis->hDC) != TRANSPARENT)
          {
            // Draw Shadow Text
            ++rc.left;
            ++rc.right;
            ++rc.top;
            ++rc.bottom;
            SetTextColor(lpdis->hDC, crTxtShwColor);

            DrawText(lpdis->hDC, pField->pszText, -1, &rc, DT_EXPANDTABS | DT_WORDBREAK | (pField->nTxtAlign == ALIGN_TEXT_LEFT ? DT_LEFT : 0) | (pField->nTxtAlign == ALIGN_TEXT_CENTER ? DT_CENTER : 0) | (pField->nTxtAlign == ALIGN_TEXT_RIGHT ? DT_RIGHT : 0));

            // Draw Normal Text
            --rc.left;
            --rc.right;
            --rc.top;
            --rc.bottom;
          }

          // Set Text Color
          SetTextColor(lpdis->hDC, crTxtColor);

          DrawText(lpdis->hDC, pField->pszText, -1, &rc, DT_EXPANDTABS | DT_WORDBREAK | (pField->nTxtAlign == ALIGN_TEXT_LEFT ? DT_LEFT : 0) | (pField->nTxtAlign == ALIGN_TEXT_CENTER ? DT_CENTER : 0) | (pField->nTxtAlign == ALIGN_TEXT_RIGHT ? DT_RIGHT : 0));

          SetBkMode(lpdis->hDC, clrBackgroundMode);
        }

        if(pField->nType == FIELD_LINK) {
          // Draw the focus rect if needed
          if (lpdis->itemState & ODS_FOCUS)
          {
            // NB: when not in DRAWENTIRE mode, this will actually toggle the focus
            // rectangle since it's drawn in a XOR way
            DrawFocusRect(lpdis->hDC, &lpdis->rcItem);
          }
        }
        pField->RectPx = lpdis->rcItem;

        if(pField->nType == FIELD_LINK && pField->nFlags & FLAG_CUSTOMDRAW_TEMP) {
            return mySendMessage(hMainWindow, uMsg, wParam, lParam);
        }
      }
      else
      if(pField->nType == FIELD_LISTBOX || pField->nType == FIELD_COMBOBOX) {

        // If there are no list box items, skip this drawing part

        if (lpdis->itemID == -1)
          break;

        // We need lpdis->rcItem later
        RECT rc = lpdis->rcItem;

        switch (lpdis->itemAction)
        {
          case ODA_SELECT:
          case ODA_DRAWENTIRE:
          {

            COLORREF clrBackground;
            COLORREF clrForeground;

            // Set the item selected background color

            if(lpdis->itemState & ODS_SELECTED)
            {
              if(lpdis->itemState & ODS_DISABLED)
              {
                if (!GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA)) {
                  clrForeground = SetTextColor(lpdis->hDC, pField->crDisSelTxtColor);
                  if(pField->crBgColor != 0xFFFFFFFF)
                    clrBackground = SetBkColor(lpdis->hDC, pField->crDisSelBgColor);
                }
              }
              else
              {
                if (!GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA)) {
                  clrForeground = SetTextColor(lpdis->hDC, pField->crSelTxtColor);
                  if(pField->crBgColor != 0xFFFFFFFF)
                    clrBackground = SetBkColor(lpdis->hDC, pField->crSelBgColor);
                }
              }
            }
            else
            {
              if(lpdis->itemState & ODS_DISABLED)
              {
                if (!GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA)) {
                  clrForeground = SetTextColor(lpdis->hDC, pField->crDisTxtColor);
                  if(pField->crBgColor != 0xFFFFFFFF)
                    clrBackground = SetBkColor(lpdis->hDC, pField->crDisBgColor);
                }
              }
              else
              {
                if (!GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA)) {
                  clrForeground = SetTextColor(lpdis->hDC, pField->crTxtColor);
                  if(pField->crBgColor != 0xFFFFFFFF)
                    clrBackground = SetBkColor(lpdis->hDC, pField->crBgColor);
                }
              }
            }

            // Get text from the item specified by lpdis->itemID
            LPTSTR pszItemText = (LPTSTR)MALLOC(g_nBufferSize);

            if(pField->nType == FIELD_COMBOBOX)
              SendMessage(lpdis->hwndItem, CB_GETLBTEXT, lpdis->itemID, (LPARAM) pszItemText);
            else
              SendMessage(lpdis->hwndItem, LB_GETTEXT, lpdis->itemID, (LPARAM) pszItemText);

            if (!GetWindowLongPtr(lpdis->hwndItem, GWLP_USERDATA)) {

              COLORREF crBgColor;
              COLORREF crTxtColor;
              COLORREF crTxtShwColor;

              if(lpdis->itemState & ODS_SELECTED) {
                if(lpdis->itemState & ODS_DISABLED) {
                  crBgColor = pField->crDisSelBgColor;
                  crTxtColor = pField->crDisSelTxtColor;
                  crTxtShwColor = pField->crDisSelTxtShwColor;
                } else {
                  crBgColor = pField->crSelBgColor;
                  crTxtColor = pField->crSelTxtColor;
                  crTxtShwColor = pField->crSelTxtShwColor;
                }
              } else {
                if(lpdis->itemState & ODS_DISABLED) {
                  crBgColor = pField->crDisBgColor;
                  crTxtColor = pField->crDisTxtColor;
                  crTxtShwColor = pField->crDisTxtShwColor;
                } else {
                  crBgColor = pField->crBgColor;
                  crTxtColor = pField->crTxtColor;
                  crTxtShwColor = pField->crTxtShwColor;
                }
              }

              // Draw Background on the whole control
              if(crBgColor != 0xFFFFFFFF) {

                HBRUSH hBrush = CreateSolidBrush(crBgColor);

                HGDIOBJ hOldSelObj = SelectObject(lpdis->hDC, hBrush);
                if(GetDeviceCaps(lpdis->hDC, RASTERCAPS) & RC_BITBLT) {
                  int rcWidth = lpdis->rcItem.right - lpdis->rcItem.left;
                  int rcHeight = lpdis->rcItem.bottom - lpdis->rcItem.top;
                  PatBlt(lpdis->hDC, lpdis->rcItem.left, lpdis->rcItem.top, rcWidth, rcHeight, PATCOPY);
                }
                SelectObject(lpdis->hDC, hOldSelObj);
              }

              int clrBackgroundMode = SetBkMode(lpdis->hDC, TRANSPARENT);

              // Make some more room so the focus rect won't cut letters off
              rc.left += 2;
              rc.right -= 2;

              if(crTxtShwColor != 0xFFFFFFFF) {

                // Draw Shadow Text
                ++rc.left;
                   ++rc.right;
                ++rc.top;
                ++rc.bottom;
                SetTextColor(lpdis->hDC, crTxtShwColor);

                DrawText(lpdis->hDC, pszItemText, -1, &rc, DT_EXPANDTABS | (pField->nTxtAlign == ALIGN_TEXT_LEFT ? DT_LEFT : 0) | (pField->nTxtAlign == ALIGN_TEXT_CENTER ? DT_CENTER : 0) | (pField->nTxtAlign == ALIGN_TEXT_RIGHT ? DT_RIGHT : 0));

                // Draw Normal Text
                --rc.left;
                --rc.right;
                --rc.top;
                --rc.bottom;
              }

              // Set Text Color
              SetTextColor(lpdis->hDC, crTxtColor);

              DrawText(lpdis->hDC, pszItemText, -1, &rc, DT_EXPANDTABS | (pField->nTxtAlign == ALIGN_TEXT_LEFT ? DT_LEFT : 0) | (pField->nTxtAlign == ALIGN_TEXT_CENTER ? DT_CENTER : 0) | (pField->nTxtAlign == ALIGN_TEXT_RIGHT ? DT_RIGHT : 0));

              SetBkMode(lpdis->hDC, clrBackgroundMode);

              if(pField->nType == FIELD_LISTBOX) {
                // Draw the focus rect if needed
                if (lpdis->itemAction & ODA_FOCUS)
                {
                  // Return to the default rect size
                  rc.left -= 2;
                  rc.right += 2;
                  // NB: when not in DRAWENTIRE mode, this will actually toggle the focus
                  // rectangle since it's drawn in a XOR way
                  DrawFocusRect(lpdis->hDC, &rc);
                }
              }

              if(pField->nType == FIELD_COMBOBOX) {
                // Draw the focus rect if needed
                if (lpdis->itemState & ODS_SELECTED)
                {
                  // Return to the default rect size
                  rc.left -= 2;
                  rc.right += 2;
                  // NB: when not in DRAWENTIRE mode, this will actually toggle the focus
                  // rectangle since it's drawn in a XOR way
                  DrawFocusRect(lpdis->hDC, &rc);
                }
              }
            }
          }
        }
      }
      return 0;
    }
    case WM_CTLCOLORDLG:
    {
      // let the NSIS window handle colors, it knows best
      return mySendMessage(hMainWindow, uMsg, wParam, lParam);
    }
    case WM_CTLCOLORSTATIC:
    case WM_CTLCOLOREDIT:
    case WM_CTLCOLORBTN:
    case WM_CTLCOLORLISTBOX:
    {
      int nIdx = FindControlIdx(GetDlgCtrlID((HWND)lParam));

      if (nIdx < 0)
        break;
      IOExControlStorage *pField = pFields + nIdx;

      if(pField->nType == FIELD_TREEVIEW || (pField->nType == FIELD_IMAGE && pField->nDataType & IMAGE_TYPE_ANIMATION)) break;

      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP || (
            pField->nType == FIELD_CHECKBOX ||
            pField->nType == FIELD_RADIOBUTTON ||
            pField->nType == FIELD_BUTTON ||
            pField->nType == FIELD_UPDOWN))
        // let the NSIS window handle colors, it knows best
        return mySendMessage(hMainWindow, uMsg, wParam, lParam);

      // Set text and background colors now

      if(pField->nType == FIELD_TEXT)
      {
        if(IsWindowEnabled((HWND)lParam) && !(GetWindowLong((HWND)lParam, GWL_STYLE) & ES_READONLY))
        {
          SetTextColor((HDC)wParam, pField->crTxtColor);
          if(pField->crBgColor != 0xFFFFFFFF) {
            SetBkColor((HDC)wParam, pField->crBgColor);

            LOGBRUSH lb;

            lb.lbStyle = BS_SOLID;
            lb.lbColor = pField->crBgColor;

            return (UINT)CreateBrushIndirect(&lb);
          }
          return 0;
        }
        else if(GetWindowLong((HWND)lParam, GWL_STYLE) & ES_READONLY)
        {
          SetTextColor((HDC)wParam, pField->crReadOnlyTxtColor);
          if(pField->crReadOnlyBgColor != 0xFFFFFFFF) {
            SetBkColor((HDC)wParam, pField->crReadOnlyBgColor);

            LOGBRUSH lb;

            lb.lbStyle = BS_SOLID;
            lb.lbColor = pField->crReadOnlyBgColor;

            return (UINT)CreateBrushIndirect(&lb);
          }
          return 0;
        }
      }
      else
      {
        if(pField->crTxtColor != 0xFFFFFFFF)
          SetTextColor((HDC)wParam, pField->crTxtColor);
        if(pField->crBgColor != 0xFFFFFFFF)
          SetBkColor((HDC)wParam, pField->crBgColor);
      }

      UpdateWindow(pField->hwnd);
      break;
    }
  }
  return 0;
}

int WINAPI ControlWindowProc(HWND hWin, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
  int iIdx = FindControlIdx(GetDlgCtrlID(hWin));
  if (iIdx < 0)
    return 0;
  IOExControlStorage *pField = pFields + iIdx;

  switch(uMsg)
  {
    //case WM_PAINT:
  /*case WM_ERASEBKGND:
  {
    if(pField->nType == FIELD_IMAGE)
    {
      SetBkMode((HDC)wParam,TRANSPARENT);
      Beep(1000,50);
        return 1;
    }
    break;
  }*/
    case WM_KEYDOWN:
    {
      //BUG: Caused by Windows bug: the "End" key used for the navigation
      //on ListView controls when the items have no icons and the view is LVS_ICON,
      //it selects the first item on the last row of items, which differs from
      //the usage everywhere else.

      //SOLUTION: Workaround below. This handles the "End" key in a
      //similar way than as done on the rest of ListView view modes,
      //except that this sends a lot of messages, and has lots of if's.
      
      switch(pField->nType)
      {
        case FIELD_LISTVIEW:
        {
          if(
             (pField->nFlags & FLAG_ICON_VIEW && !pField->pszLargeImageList) &&
             (
              (
               (wParam == VK_SHIFT || HIWORD(GetKeyState(VK_SHIFT))) &&
               (
                (wParam != VK_LEFT && !HIWORD(GetKeyState(VK_LEFT))) &&
                (wParam != VK_RIGHT && !HIWORD(GetKeyState(VK_RIGHT))) &&
                (wParam != VK_UP && !HIWORD(GetKeyState(VK_UP))) &&
                (wParam != VK_DOWN && !HIWORD(GetKeyState(VK_DOWN))) &&
                (wParam != VK_HOME && !HIWORD(GetKeyState(VK_HOME))) &&
                (wParam != VK_END && !HIWORD(GetKeyState(VK_END))) &&
                (wParam != VK_PRIOR && !HIWORD(GetKeyState(VK_PRIOR))) &&
                (wParam != VK_NEXT && !HIWORD(GetKeyState(VK_NEXT)))
               )
               ||
               (wParam != VK_SHIFT && !HIWORD(GetKeyState(VK_SHIFT)) &&
                (
                 (wParam == VK_LEFT || HIWORD(GetKeyState(VK_LEFT))) ||
                 (wParam == VK_RIGHT || HIWORD(GetKeyState(VK_RIGHT))) ||
                 (wParam == VK_UP || HIWORD(GetKeyState(VK_UP))) ||
                 (wParam == VK_DOWN || HIWORD(GetKeyState(VK_DOWN))) ||
                 (wParam == VK_HOME || HIWORD(GetKeyState(VK_HOME))) ||
                 (wParam == VK_END || HIWORD(GetKeyState(VK_END))) ||
                 (wParam == VK_PRIOR || HIWORD(GetKeyState(VK_PRIOR))) ||
                 (wParam == VK_NEXT || HIWORD(GetKeyState(VK_NEXT)))
                )
               )
              )
              &&
              ListView_GetItemState(hWin, ListView_GetNextItem(hWin, -1, LVNI_FOCUSED), LVIS_SELECTED) & LVIS_SELECTED
             )
            )
          {
            iItemPlaceholder = ListView_GetNextItem(hWin, -1, LVNI_FOCUSED);
          }

          if(wParam == VK_END || HIWORD(GetKeyState(VK_END)))
          {
            if(HIWORD(GetKeyState(VK_SHIFT)))
            {
              int iItem = 0;

              while(iItem < iItemPlaceholder && iItem != -1)
              {
                if(ListView_GetItemState(hWin, iItem, LVIS_SELECTED) & LVIS_SELECTED)
                  ListView_SetItemState(hWin, iItem, 0, LVIS_FOCUSED|LVIS_SELECTED);
                iItem = ListView_GetNextItem(hWin, iItem, LVNI_ALL);
              }

              while(iItem >= iItemPlaceholder)
              {
                if(ListView_GetNextItem(hWin, iItem, LVNI_ALL) == -1)
                {
                  if(ListView_GetItemState(hWin, iItem, LVIS_FOCUSED) | ~LVIS_FOCUSED)
                    ListView_SetItemState(hWin, iItem, LVIS_FOCUSED, LVIS_FOCUSED);

                  if(ListView_GetItemState(hWin, iItem, LVIS_SELECTED) | ~LVIS_SELECTED)
                    ListView_SetItemState(hWin, iItem, LVIS_SELECTED, LVIS_SELECTED);

                  POINT ptlvItem;
                  ListView_GetItemPosition(hWin, iItem, &ptlvItem);
                  ListView_Scroll(hWin, 0, ptlvItem.y);
                  break;
                }

                if(ListView_GetItemState(hWin, iItem, LVIS_SELECTED) | ~LVIS_SELECTED)
                  ListView_SetItemState(hWin, iItem, LVIS_SELECTED, LVIS_FOCUSED|LVIS_SELECTED);

                iItem = ListView_GetNextItem(hWin, iItem, LVNI_ALL);
              }
            }
            else
            {
              int iItem = -1;

              while(true)
              {
                iItem = ListView_GetNextItem(hWin, iItem, LVNI_SELECTED);
                if(iItem == -1)
                  break;

                ListView_SetItemState(hWin, iItem, 0, LVIS_FOCUSED|LVIS_SELECTED);
              }

              iItem = -1;

              while(ListView_GetNextItem(hWin, iItem, 0) != -1)
                iItem = ListView_GetNextItem(hWin, iItem, LVNI_ALL);

              ListView_SetItemState(hWin, iItem, LVIS_FOCUSED|LVIS_SELECTED, LVIS_FOCUSED|LVIS_SELECTED);
              POINT ptlvItem;
              ListView_GetItemPosition(hWin, iItem, &ptlvItem);
              ListView_Scroll(hWin, 0, ptlvItem.y);
            }
            return 0;
          }
          break;
        }
      }
      break;
    }
    case WM_GETDLGCODE:
    {
      if(pField->nType == FIELD_LINK)
        // Pretend we are a normal button/default button as appropriate
        return DLGC_BUTTON | ((pField->nFlags & FLAG_DRAW_TEMP) ? DLGC_DEFPUSHBUTTON : DLGC_UNDEFPUSHBUTTON);
      break;
    }
    case BM_SETSTYLE:
    {
      if(pField->nType == FIELD_LINK)
      {
        // Detect when we are becoming the default button but don't lose the owner-draw style

        // Microsoft implicitly says that BS_TYPEMASK was introduced
        // on Win 2000. But really (maybe), it was introduced w/ Win 95.
        // (Thank you Microsoft.)
        if ((wParam & BS_TYPEMASK) == BS_DEFPUSHBUTTON)
          pField->nFlags |= FLAG_DRAW_TEMP;  // Hijack this flag to indicate default button status
        else
          pField->nFlags &= ~FLAG_DRAW_TEMP;
        wParam = (wParam & ~BS_TYPEMASK) | BS_OWNERDRAW;
      }
      break;
    }
    case WM_SETCURSOR:
    {
      if((HWND)wParam == hWin &&
         ((pField->nType != FIELD_LINK && pField->nType != FIELD_RICHTEXT && pField->nNotify & NOTIFY_CONTROL_ONCLICK) ||
          pField->nType == FIELD_LINK) &&
         LOWORD(lParam) == HTCLIENT)
      {
        HCURSOR hCur = LoadCursor(NULL, MAKEINTRESOURCE(pField->nNotifyCursor));
        if (hCur)
        {
          SetCursor(hCur);
          return 1; // halt further processing
        }
      }
      break;
    }
    case EM_SETPASSWORDCHAR:
    {
      if(pField->nType == FIELD_TEXT)
      {
        TCHAR* pszChar = (TCHAR*)wParam;
        if(pField->nFlags == FLAG_PASSWORD)
        {
          if(lstrcmp(pszChar,_T(""))==0)
            pszChar = _T("*");
        }
        else
        {
          pszChar = _T("");
        }
        return 0;
      }
      break;
    }
    case EM_SETREADONLY:
    {
      if(wParam)
        mySendMessage(pField->hwnd,EM_SETBKGNDCOLOR,0,pField->crReadOnlyBgColor);
      else
        mySendMessage(pField->hwnd,EM_SETBKGNDCOLOR,0,pField->crBgColor);
    }
  }
  return CallWindowProc((WNDPROC)pField->nParentIdx, hWin, uMsg, wParam, lParam);
}

LRESULT WINAPI NotifyProc(HWND hWnd, UINT id, HWND hwndCtl, UINT codeNotify) {

// Initialize Variables
//================================================================
  TCHAR szBrowsePath[MAX_PATH];

// Initialize pField
//================================================================
  int nIdx = FindControlIdx(id);
  // Ignore if the dialog is in the process of being created
  if (g_done || nIdx < 0)
    return 0;
  IOExControlStorage *pField = pFields + nIdx;

// Detect Notifications
//================================================================
  bool isNotified = false;
  bool dontSimulateNext = false;

  switch(pField->nType)
  {
    case FIELD_RICHTEXT:
	  {
		Notification(NOTIFY_CONTROL_ONTEXTSELCHANGE, EN_SELCHANGE);
	  }
    case FIELD_TEXT:
	  {
		Notification(NOTIFY_CONTROL_ONTEXTTRUNCATE, EN_MAXTEXT);
		Notification(NOTIFY_CONTROL_ONTEXTVSCROLL, EN_VSCROLL);
		Notification(NOTIFY_CONTROL_ONTEXTCHANGE, EN_CHANGE);
		Notification(NOTIFY_CONTROL_ONTEXTUPDATE, EN_UPDATE);
		Notification(NOTIFY_CONTROL_ONSETFOCUS, EN_SETFOCUS);
		Notification(NOTIFY_CONTROL_ONKILLFOCUS, EN_KILLFOCUS);
		break;
	  }

    case FIELD_IPADDRESS:
	  {
		Notification(NOTIFY_CONTROL_ONTEXTCHANGE, EN_CHANGE);
		Notification(NOTIFY_CONTROL_ONSELCHANGE, IPN_FIELDCHANGED);
		Notification(NOTIFY_CONTROL_ONSETFOCUS, EN_SETFOCUS);
		Notification(NOTIFY_CONTROL_ONKILLFOCUS, EN_KILLFOCUS);
		break;
	  }

    case FIELD_COMBOBOX:
	  {
		Notification(NOTIFY_CONTROL_ONTEXTCHANGE, CBN_EDITCHANGE);
		Notification(NOTIFY_CONTROL_ONTEXTUPDATE, CBN_EDITUPDATE);
		Notification(NOTIFY_CONTROL_ONLISTOPEN, CBN_DROPDOWN);
		Notification(NOTIFY_CONTROL_ONLISTCLOSE, CBN_CLOSEUP);
	  }
    case FIELD_LISTBOX:
	  {
		Notification(NOTIFY_CONTROL_ONSELCHANGE, CBN_SELCHANGE);
		Notification(NOTIFY_CONTROL_ONDBLCLICK, CBN_DBLCLK);
		Notification(NOTIFY_CONTROL_ONSETFOCUS, CBN_SETFOCUS);
		Notification(NOTIFY_CONTROL_ONKILLFOCUS, CBN_KILLFOCUS);
		break;
	  }

    case FIELD_DATETIME:
	  {
		Notification(NOTIFY_CONTROL_ONSELCHANGE, DTN_DATETIMECHANGE);
		Notification(NOTIFY_CONTROL_ONLISTOPEN, DTN_DROPDOWN);
		Notification(NOTIFY_CONTROL_ONLISTCLOSE, NOTIFY_CONTROL_ONLISTCLOSE);
		Notification(NOTIFY_CONTROL_ONSETFOCUS, NM_SETFOCUS);
		Notification(NOTIFY_CONTROL_ONKILLFOCUS, NM_KILLFOCUS);
		break;
	  }

    case FIELD_MONTHCALENDAR:
	  {
		Notification(NOTIFY_CONTROL_ONSELCHANGE, MCN_SELCHANGE);
		break;
	  }

    case FIELD_UPDOWN:
	  {
		Notification(NOTIFY_CONTROL_ONSELCHANGE, UDN_DELTAPOS);
		break;
	  }

    case FIELD_TRACKBAR:
	  {
		Notification(NOTIFY_CONTROL_ONSELCHANGE, TB_BOTTOM ||
			   codeNotify == TB_LINEDOWN ||
			   codeNotify == TB_LINEUP ||
			   codeNotify == TB_PAGEDOWN ||
			   codeNotify == TB_PAGEUP ||
			   codeNotify == TB_THUMBTRACK ||
			   codeNotify == TB_TOP);
		break;
	  }

    case FIELD_LABEL:
	  {
		Notification(NOTIFY_CONTROL_ONCLICK, STN_CLICKED);
		Notification(NOTIFY_CONTROL_ONDBLCLICK, STN_DBLCLK);
		break;
	  }

    case FIELD_LINK:
    case FIELD_BUTTON:
	  {
		if (codeNotify == BN_CLICKED || codeNotify == BN_DBLCLK)
		{
			  // If button or link with OPEN_FILEREQUEST, SAVE_FILEREQUEST, DIRREQUEST... flags
			  if ( (pField->nFlags & FLAG_OPEN_FILEREQUEST) || (pField->nFlags & FLAG_SAVE_FILEREQUEST) || (pField->nFlags & FLAG_DIRREQUEST) || (pField->nFlags & FLAG_COLORREQUEST) || (pField->nFlags & FLAG_FONTREQUEST) )
			  {
				dontSimulateNext = true;
				isNotified = true;
			  }
			  // If button or link with a not empty state link
			  else if (strlen(pField->pszState) > 0)
			  {
				dontSimulateNext = true;
				isNotified = true;
			  }
		}
	  }
    case FIELD_CHECKBOX:
    case FIELD_RADIOBUTTON:
	  {
		Notification(NOTIFY_CONTROL_ONCLICK, BN_CLICKED);
		Notification(NOTIFY_CONTROL_ONDBLCLICK, BN_DBLCLK);

		break;
	  }

    case FIELD_TREEVIEW:
	  {
		Notification(NOTIFY_CONTROL_ONSELCHANGE, TVN_SELCHANGED);
		Notification(NOTIFY_CONTROL_ONCLICK, NM_CLICK);
		Notification(NOTIFY_CONTROL_ONDBLCLICK, NM_DBLCLK);
		Notification(NOTIFY_CONTROL_ONRCLICK, NM_RCLICK);
		Notification(NOTIFY_CONTROL_ONSETFOCUS, NM_SETFOCUS);
		Notification(NOTIFY_CONTROL_ONKILLFOCUS, NM_KILLFOCUS);
		break;
	  }

    case FIELD_LISTVIEW:
	  {
		Notification(NOTIFY_CONTROL_ONSELCHANGE, LVN_ITEMCHANGED);
		Notification(NOTIFY_CONTROL_ONCLICK, NM_CLICK);
		Notification(NOTIFY_CONTROL_ONDBLCLICK, NM_DBLCLK);
		Notification(NOTIFY_CONTROL_ONRCLICK, NM_RCLICK);
		Notification(NOTIFY_CONTROL_ONRDBLCLICK, NM_RDBLCLK);
		Notification(NOTIFY_CONTROL_ONSETFOCUS, NM_SETFOCUS);
		Notification(NOTIFY_CONTROL_ONKILLFOCUS, NM_KILLFOCUS);
		break;
	  }

    case FIELD_IMAGE:
	  {
		if(pField->nDataType == IMAGE_TYPE_ANIMATION)
		{
		  Notification(NOTIFY_CONTROL_ONSTART, ACN_START);
		  Notification(NOTIFY_CONTROL_ONSTOP, ACN_STOP);
		}
		else
		{
		  Notification(NOTIFY_CONTROL_ONCLICK, STN_CLICKED);
		  Notification(NOTIFY_CONTROL_ONDBLCLICK, STN_DBLCLK);
		}
		break;
	  }
    default:
      return 0;
  }

  if (!isNotified) 
  {
    return 0;
  }

  g_aNotifyQueueTemp->iField = nIdx + 1;
  g_aNotifyQueueTemp->bNotifyType = NOTIFY_TYPE_CONTROL;

  AddNotifyQueueItem(g_aNotifyQueueTemp);


// Associate Notifications With Actions (By Control Types)
//================================================================

  switch (pField->nType)
  {

  // Link, Button
  //--------------------------------------------------------------
    case FIELD_LINK:
    case FIELD_BUTTON:
    {
    // State ShellExecute Implementation
    //----------------------------------

      // Allow the state to be empty - this might be useful in conjunction
      // with the NOTIFY flag
      if (lstrcmp(pField->pszState,_T(""))!=0 && !(pField->nFlags & FLAG_OPEN_FILEREQUEST) && !(pField->nFlags & FLAG_SAVE_FILEREQUEST) && !(pField->nFlags & FLAG_DIRREQUEST) && !(pField->nFlags & FLAG_COLORREQUEST) && !(pField->nFlags & FLAG_FONTREQUEST))
      {
        ShellExecute(hMainWindow, NULL, pField->pszState, NULL, NULL, SW_SHOWDEFAULT);
      }

    // FileRequest Dialog Implementation
    //----------------------------------
      else if(pField->nFlags & FLAG_OPEN_FILEREQUEST || pField->nFlags & FLAG_SAVE_FILEREQUEST) {
        OPENFILENAME ofn={0,};

        ofn.lStructSize = sizeof(ofn);
        ofn.hwndOwner = hConfigWindow;
        ofn.lpstrFilter = pField->pszFilter;
        ofn.lpstrFile = szBrowsePath;
        ofn.nMaxFile  = _countof(szBrowsePath);

        if(pField->nFlags & FLAG_WARN_IF_EXIST)
          ofn.Flags |= OFN_OVERWRITEPROMPT;
        if(pField->nFlags & FLAG_FILE_HIDEREADONLY)
          ofn.Flags |= OFN_HIDEREADONLY;
        if(pField->nFlags & FLAG_PATH_MUST_EXIST)
          ofn.Flags |= OFN_PATHMUSTEXIST;
        if(pField->nFlags & FLAG_FILE_MUST_EXIST)
          ofn.Flags |= OFN_FILEMUSTEXIST;
        if(pField->nFlags & FLAG_PROMPT_CREATE)
          ofn.Flags |= OFN_CREATEPROMPT;
        if(pField->nFlags & FLAG_FILE_EXPLORER)
          ofn.Flags |= OFN_EXPLORER;
        if(pField->nFlags & FLAG_MULTISELECT) {
          ofn.Flags |= OFN_ALLOWMULTISELECT;
          ofn.Flags |= OFN_EXPLORER;
        }

        if(pField->nRefFields)
          GetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), szBrowsePath, _countof(szBrowsePath));
        else
          strcpy(szBrowsePath, pField->pszState);

        tryagain:
        GetCurrentDirectory(BUFFER_SIZE, szResult); // save working dir
        if ((pField->nFlags & FLAG_SAVE_FILEREQUEST) ? GetSaveFileName(&ofn) : GetOpenFileName(&ofn)) {
           lstrcpyn(pField->pszState,szBrowsePath,g_nBufferSize);
          if(pField->nRefFields)
            mySetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), szBrowsePath);

          SetCurrentDirectory(szResult); // restore working dir
                                           // OFN_NOCHANGEDIR doesn't always work (see MSDN)
          break;
        }
        else if (szBrowsePath[0] && CommDlgExtendedError() == FNERR_INVALIDFILENAME) {
          szBrowsePath[0] = _T('\0');
          goto tryagain;
        }
        break;
      }

    // DirRequest Dialog Implementation
    //---------------------------------
      else if(pField->nFlags & FLAG_DIRREQUEST) {

        if(codeNotify == EN_CHANGE)
          break;
        BROWSEINFO bi;

        LPTSTR szDisplayNameFolder = (LPTSTR)MALLOC(MAX_PATH);

        bi.hwndOwner = hConfigWindow;
        bi.pidlRoot = NULL;
        bi.pszDisplayName = szDisplayNameFolder;
        ConvertNewLines(pField->pszListItems);
        bi.lpszTitle = pField->pszListItems;
#ifndef BIF_NEWDIALOGSTYLE
  #define BIF_NEWDIALOGSTYLE 0x0040
#endif
        bi.ulFlags = BIF_STATUSTEXT | BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE;
        bi.lpfn = BrowseCallbackProc;
        bi.lParam = (LPARAM)szBrowsePath;
        bi.iImage = 0;

        if(pField->nRefFields)
          GetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), szBrowsePath, _countof(szBrowsePath));
        else
          strcpy(szBrowsePath, pField->pszState);

        lstrcpyn(szDisplayNameFolder, pField->pszState, g_nBufferSize > MAX_PATH ? MAX_PATH : g_nBufferSize);

        if (pField->pszRoot) {
          LPSHELLFOLDER sf;
          ULONG eaten;
          LPITEMIDLIST root;
          SHGetDesktopFolder(&sf);
#ifdef _UNICODE
          sf->ParseDisplayName(hConfigWindow, NULL, pField->pszRoot, &eaten, &root, NULL);
#else
          int ccRoot = (lstrlen(pField->pszRoot) * 2) + 2;
          LPWSTR pwszRoot = (LPWSTR) MALLOC(ccRoot);
          MultiByteToWideChar(CP_ACP, 0, pField->pszRoot, -1, pwszRoot, ccRoot);
          sf->ParseDisplayName(hConfigWindow, NULL, pwszRoot, &eaten, &root, NULL);
          FREE(pwszRoot);
#endif
          bi.pidlRoot = root;
          sf->Release();
        }
        
        LPITEMIDLIST pResult = SHBrowseForFolder(&bi);
        if (pResult)
        {
          if (SHGetPathFromIDList(pResult, szBrowsePath)) 
          {
            lstrcpyn(pField->pszState,szBrowsePath,g_nBufferSize);
            if(pField->nRefFields)
              mySetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), szBrowsePath);
          }
        }

        FREE(szDisplayNameFolder);

        break;
      }

    // ColorRequest Dialog Implementation
    //---------------------------------
      else if (pField->nFlags & FLAG_COLORREQUEST)
      {
        CHOOSECOLOR cc;

        cc.lStructSize = sizeof(cc);
        cc.hwndOwner = pField->hwnd;
        cc.Flags = CC_RGBINIT;

        int i = 0;
        COLORREF acrCustClr[16];

        if(pField->pszListItems)
        {
          int nResult = 0;

          LPTSTR pszStart, pszEnd;
          pszStart = pszEnd = pField->pszListItems;

          while(nResult = IOLI_NextItem(pField->pszListItems, &pszStart, &pszEnd, 0))
          {
            if((nResult == 5 && lstrcmp(pszStart,_T("")) == 0) || nResult == 6)
              break;

            if(lstrcmp(pszStart,_T(""))!=0)
              acrCustClr[i] = (COLORREF)myatoi(pszStart);
            else
              acrCustClr[i] = (COLORREF)0xFFFFFF;

            i++;

            IOLI_RestoreItemStructure(pField->pszListItems, &pszStart, &pszEnd, nResult);
          }
        }
        
        for(;i < 16;i++)
          acrCustClr[i] = RGB(255,255,255);

        cc.lpCustColors = (LPDWORD)acrCustClr;

        if(pField->nRefFields)
          myGetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), pField->pszState);
        
        cc.rgbResult = (COLORREF)myatoi(pField->pszState);

        if(ChooseColor(&cc))
        {
          mycrxtoa(pField->pszState, (int)cc.rgbResult);

          if(pField->nRefFields)
            mySetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), pField->pszState);

          LPTSTR pszColorTemp = (LPTSTR)MALLOC(sizeof(COLORREF)+1);
          LPTSTR pszResultTemp = (LPTSTR)MALLOC(g_nBufferSize);
          LPTSTR pszResultTemp2 = (LPTSTR)MALLOC(g_nBufferSize);
          for(i = 0;i < 16;i++)
          {
            mycrxtoa(pszColorTemp, acrCustClr[i]);
            if(i==15)
               wsprintf(pszResultTemp2,_T("%s%s"),pszResultTemp,pszColorTemp);
            else
               wsprintf(pszResultTemp2,_T("%s%s\x01"),pszResultTemp,pszColorTemp);
            strcpy(pszResultTemp, pszResultTemp2);
          }

          lstrcpy(pField->pszListItems, pszResultTemp);

          FREE(pszColorTemp);
          FREE(pszResultTemp);
          FREE(pszResultTemp2);
        }
        break;
      }

    // FontRequest Dialog Implementation
    //---------------------------------
      else if (pField->nFlags & FLAG_FONTREQUEST)
      {
        CHOOSEFONT cf;
        LOGFONT lf;
        LPTSTR pszFontRequestList = (LPTSTR)MALLOC(96);

        GetObject((HFONT)mySendMessage(hConfigWindow, WM_GETFONT, 0, 0), sizeof(lf), (LPVOID)&lf);

        if(pField->nRefFields)
        {
          GetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), pField->pszState, 96);
          pField->pszState[lstrlen(pField->pszState)-1] = '\0';
          LItoIOLI(pField->pszState, 96, 2, pField->nType);
        }

        lstrcpyn(pszFontRequestList, pField->pszState, 96);
        
        if(pszFontRequestList)
        {
          int i = 0;

          int nResult = 0;

          LPTSTR pszStart, pszEnd;
          pszStart = pszEnd = pszFontRequestList;

          while(nResult = IOLI_NextItem(pszFontRequestList, &pszStart, &pszEnd, 1))
          {
            if((nResult == 5 && lstrcmp(pszStart,_T("")) == 0) || nResult == 6)
              break;

            switch(i)
            {
              case 0:
              {
                if(pszStart)
                  strncpy(lf.lfFaceName,pszStart, 32);
                break;
              }
              case 1:
              {
                if(pszStart)
                {
                  HDC hDC = GetDC(pField->hwnd);
                  lf.lfHeight = -MulDiv(myatoi(pszStart), GetDeviceCaps(hDC, LOGPIXELSY), 72);
                  ReleaseDC(pField->hwnd, hDC);
                }
                break;
              }
              case 2:
              {
                if(myatoi(pszStart))
                  lf.lfWeight = 700;
                break;
              }
              case 3:
              {
                if(myatoi(pszStart))
                  lf.lfItalic = TRUE;
                break;
              }
              case 4:
              {
                if(myatoi(pszStart))
                  lf.lfUnderline = TRUE;
                break;
              }
              case 5:
              {
                if(myatoi(pszStart))
                  lf.lfStrikeOut = TRUE;
                break;
              }
              case 6:
              {
                cf.rgbColors = (COLORREF)myatoi(pszStart);
                break;
              }
              default:
              { 
                break;
              }
            }
            ++i;

            IOLI_RestoreItemStructure(pszFontRequestList, &pszStart, &pszEnd, nResult);
          }
        }

        cf.lStructSize = sizeof(cf);
        cf.hwndOwner = pField->hwnd;
        cf.Flags = CF_SCREENFONTS | CF_EFFECTS | CF_INITTOLOGFONTSTRUCT | CF_NOSCRIPTSEL;
        cf.lpLogFont = &lf;

        if(ChooseFont(&cf))
        {
          lstrcpy(pszFontRequestList, _T(""));
          LPTSTR pszStrTemp = (LPTSTR)MALLOC(32);
          for(int i = 0;i < 7;i++)
          {
            switch(i)
            {
              case 0:
              {
                strcpy(pszStrTemp,lf.lfFaceName);
                break;
              }
              case 1:
              {
                HDC hDC = GetDC(pField->hwnd);
                myitoa(pszStrTemp, -MulDiv(lf.lfHeight, 72, GetDeviceCaps(hDC, LOGPIXELSY)));
                ReleaseDC(pField->hwnd, hDC);
                break;
              }
              case 2:
              {
                if(lf.lfWeight == 700)
                  myitoa(pszStrTemp,1);
                else
                  myitoa(pszStrTemp,0);
                break;
              }
              case 3:
              {
                if(lf.lfItalic)
                  myitoa(pszStrTemp,1);
                else
                  myitoa(pszStrTemp,0);
                break;
              }
              case 4:
              {
                if(lf.lfUnderline)
                  myitoa(pszStrTemp,1);
                else
                  myitoa(pszStrTemp,0);
                break;
              }
              case 5:
              {
                if(lf.lfStrikeOut)
                  myitoa(pszStrTemp,1);
                else
                  myitoa(pszStrTemp,0);
                break;
              }
              case 6:
              {
                mycrxtoa(pszStrTemp, cf.rgbColors);
                break;
              }
              default:
                break;
            }

            if(i==6)
              lstrcat(pszFontRequestList, pszStrTemp);
            else
            {
              lstrcat(pszFontRequestList, pszStrTemp);
              lstrcat(pszFontRequestList, _T("\x01"));
            }
          }

          lstrcpyn(pField->pszState,pszFontRequestList, 96);

          if(pField->nRefFields)
          {
            pszFontRequestList = IOLItoLI(pszFontRequestList, 96, pField->nType);
            SetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), pszFontRequestList);
          }
          FREE(pszStrTemp);
        }
        //else
        //if(!pField->nRefFields)
        //  pField->pszState[lstrlen(pField->pszState)-1] = '\0';
        FREE(pszFontRequestList);
      }
      break;
    }
  }

// Kill the page timer if notified (TimeOut Implementation)
//================================================================
  if(g_timer_id != 0)
  {
    nTimeOut = nTimeOutTemp - (GetTickCount() - g_timer_cur_time);
    if (nTimeOut < USER_TIMER_MINIMUM)
      nTimeOut = 0;
    KillTimer(hConfigWindow, g_timer_id);
  }

// Simulate "Next" Click So NSIS Can Call Its PageAfter Function
//================================================================
  if (dontSimulateNext != true)
    mySendMessage(hMainWindow, WM_NOTIFY_OUTER_NEXT, 1, 0);

  return 0;
}














int WINAPI ReadSettings() {

// Initial Variables Definitions
//================================================================
  static TCHAR szField[25];
  int nIdx, nCtrlIdx;

// Get Window Settings
//================================================================
  pszAppName = _T("Settings");

  // Assign default values for buffer sizes:
  g_nBufferSize = BUFFER_SIZE;
  g_nNotifyQueueAmountMax = NOTIFY_QUEUE_AMOUNT_MAX;

  // BufferSize
  //--------------------------------------------------------------
  LPTSTR pszTemp = (LPTSTR)MALLOC(16 * sizeof(TCHAR));

  GetPrivateProfileString(pszAppName, _T("BufferSize"), _T(""), pszTemp, 16, pszFilename);
  g_nBufferSize = myatoi(pszTemp);
  FREE(pszTemp);

  if(g_nBufferSize < 1)
    g_nBufferSize = BUFFER_SIZE;

  // Use this for normal "szResult" uses
  szResult = (TCHAR*)MALLOC(g_nBufferSize);

  // NotifyFlagsMax
  //--------------------------------------------------------------
  g_nNotifyQueueAmountMax = myGetProfileInt(_T("NotifyFlagsMax"), NOTIFY_QUEUE_AMOUNT_MAX);

  if(g_nNotifyQueueAmountMax <= 0)
    g_nNotifyQueueAmountMax = NOTIFY_QUEUE_AMOUNT_MAX;

  // Title
  //--------------------------------------------------------------
  pszTitle = myGetProfileStringDup(_T("Title"));

  // CancelButtonText
  //--------------------------------------------------------------
  pszCancelButtonText = myGetProfileStringDup(_T("CancelButtonText"));

  // NextButtonText
  //--------------------------------------------------------------
  pszNextButtonText = myGetProfileStringDup(_T("NextButtonText"));

  // BackButtonText
  //--------------------------------------------------------------
  pszBackButtonText = myGetProfileStringDup(_T("BackButtonText"));

  // Rect
  //--------------------------------------------------------------
  nRectId = myGetProfileInt(_T("Rect"), DEFAULT_RECT);

  // NextEnabled
  //--------------------------------------------------------------
  bNextEnabled = myGetProfileInt(_T("NextEnabled"), -1);

  // NextShow
  //--------------------------------------------------------------
  bNextShow = myGetProfileInt(_T("NextShow"), -1);

  // BackEnabled
  //--------------------------------------------------------------
  bBackEnabled = myGetProfileInt(_T("BackEnabled"), -1);

  // BackShow
  //--------------------------------------------------------------
  bBackShow = myGetProfileInt(_T("BackShow"), -1);

  // CancelEnabled
  //--------------------------------------------------------------
  bCancelEnabled = myGetProfileInt(_T("CancelEnabled"), -1);

  // CancelShow
  //--------------------------------------------------------------
  bCancelShow = myGetProfileInt(_T("CancelShow"), -1);

  // RTL
  //--------------------------------------------------------------
  bRTL = myGetProfileInt(_T("RTL"), 0);

  // MUnit
  //--------------------------------------------------------------
  bMUnit = myGetProfileInt(_T("MUnit"), 0);

  // TimeOut
  //--------------------------------------------------------------
  nTimeOut = myGetProfileInt(_T("TimeOut"), 0);
  if (nTimeOut < USER_TIMER_MINIMUM || nTimeOut > USER_TIMER_MAXIMUM)
    nTimeOut = 0;
  nTimeOutTemp = nTimeOut;

  // NumFields
  //--------------------------------------------------------------
  nNumFields = myGetProfileInt(_T("NumFields"), 0);
  if(nNumFields <= 0)
  {
    nNumFields = 0;
    while (nNumFields > -1) {
      wsprintf(szField, _T("Field %d"), nNumFields + 1);
      pszAppName = szField;
      if(myGetProfileString(_T("Type")))
        ++nNumFields;
      else
        break;
    }
  }

  if (nNumFields > 0)
    // the structure is small enough that this won't waste much memory.
    // if the structure gets much larger, we should switch to a linked list.
    pFields = (IOExControlStorage *)MALLOC(sizeof(IOExControlStorage)*nNumFields);

// Detect Field Settings
//================================================================
  for (nIdx = 0, nCtrlIdx = 0; nCtrlIdx < nNumFields; nCtrlIdx++, nIdx++) {

  // Type Flags
  //-----------
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
      { NULL,            0                   }
    };

  // Control Flags
  //--------------
    static TableEntry FlagTable[] = {
      // All
      { _T("DISABLED"),          FLAG_DISABLED          },
      { _T("GROUP"),             FLAG_GROUP             },
      { _T("NOTABSTOP"),         FLAG_NOTABSTOP         },

      // Image
      { _T("RESIZETOFIT"),       FLAG_RESIZETOFIT       },
      { _T("TRANSPARENT"),       FLAG_TRANSPARENT       },

      // ProgressBar
      { _T("SMOOTH"),             FLAG_SMOOTH            },
      { _T("VSCROLL"),           FLAG_VSCROLL           },

      // CheckBox/RadioButton
      { _T("READONLY"),          FLAG_READONLY          },
      { _T("3STATE"),             FLAG_3STATE            }, // Except CheckBox

      // Button
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

      // UpDown
      { _T("HSCROLL"),           FLAG_HSCROLL           },
      { _T("WRAP"),              FLAG_WRAP              },

      // Text/Password/RichText
      { _T("ONLY_NUMBERS"),      FLAG_ONLYNUMBERS       },
      { _T("MULTILINE"),         FLAG_MULTILINE         },
      { _T("WANTRETURN"),        FLAG_WANTRETURN        },
      { _T("NOWORDWRAP"),        FLAG_NOWORDWRAP        },
//    { _T("HSCROLL"),           FLAG_HSCROLL           },
//    { _T("VSCROLL"),           FLAG_VSCROLL           },
//    { _T("READONLY"),          FLAG_READONLY          },
      { _T("PASSWORD"),          FLAG_PASSWORD          }, //Except Password
      { _T("FOCUS"),             FLAG_FOCUS             },

      // DropList/ComboBox
//    { _T("VSCROLL"),           FLAG_VSCROLL           },
      { _T("DROPLIST"),          FLAG_DROPLIST          }, //Except DropList

      // DateTime
      { _T("UPDOWN"),            FLAG_UPDOWN            },

      // ListBox
      { _T("MULTISELECT"),       FLAG_MULTISELECT       },
      { _T("EXTENDEDSELECT"),    FLAG_EXTENDEDSELECT    },
      { _T("EXTENDEDSELCT"),     FLAG_EXTENDEDSELECT    },

//    { _T("VSCROLL"),           FLAG_VSCROLL           },

      // ListView/TreeView
      { _T("CHECKBOXES"),        FLAG_CHECKBOXES        },
      { _T("EDITLABELS"),        FLAG_EDITLABELS        },
//    { _T("MULTISELECT"),       FLAG_MULTISELECT       },

      { _T("LIST_VIEW"),         FLAG_LIST_VIEW         },
      { _T("ICON_VIEW"),         FLAG_ICON_VIEW         },
      { _T("SMALLICON_VIEW"),    FLAG_SMALLICON_VIEW    },
      { _T("REPORT_VIEW"),       FLAG_REPORT_VIEW       },

      // TrackBar
      { _T("NO_TICKS"),          FLAG_NO_TICKS          },
//    { _T("VSCROLL"),           FLAG_VSCROLL           },

      // MonthCalendar
      { _T("NOTODAY"),           FLAG_NOTODAY           },
      { _T("WEEKNUMBERS"),       FLAG_WEEKNUMBERS       },

      // Null
      { NULL,                0                      }
    };

  // Control Alignation Flags
  //-------------------------
    static TableEntry AlignTable[] = {
      { _T("LEFT"),   ALIGN_LEFT    },
      { _T("CENTER"), ALIGN_CENTER  },
      { _T("RIGHT"),  ALIGN_RIGHT   },
      { NULL,     0             }
    };

    static TableEntry VAlignTable[] = {
      { _T("TOP"),    VALIGN_TOP    },
      { _T("CENTER"), VALIGN_CENTER },
      { _T("BOTTOM"), VALIGN_BOTTOM },
      { NULL,     0             }
    };

  // Text Alignation Flags
  //-------------------------
    static TableEntry TxtAlignTable[] = {
      { _T("LEFT"),    ALIGN_TEXT_LEFT    },
      { _T("CENTER"),  ALIGN_TEXT_CENTER  },
      { _T("RIGHT"),   ALIGN_TEXT_RIGHT   },
      { _T("JUSTIFY"), ALIGN_TEXT_JUSTIFY },
      { NULL,      0                  }
    };

    static TableEntry TxtVAlignTable[] = {
      { _T("TOP"),     VALIGN_TEXT_TOP     },
      { _T("CENTER"),  VALIGN_TEXT_CENTER  },
      { _T("BOTTOM"),  VALIGN_TEXT_BOTTOM  },
      { _T("JUSTIFY"), VALIGN_TEXT_JUSTIFY },
      { NULL,      0                   }
    };

  // ToolTip Flags
  //--------------
    static TableEntry ToolTipFlagTable[] = {
      { _T("NOALWAYSTIP"), TTS_ALWAYSTIP },
      { _T("BALLOON"),     TTS_BALLOON   },
      { _T("NOANIMATE"),   TTS_NOANIMATE },
      { _T("NOFADE"),      TTS_NOFADE    },
      { _T("NOPREFIX"),    TTS_NOPREFIX  },
      { NULL,          0             }
    };

  // ToolTip Icon Flags
  //-------------------
    static TableEntry ToolTipIconTable[] = {
      { _T("INFORMATION"), 1 },
      { _T("EXCLAMATION"), 2 },
      { _T("STOP"),        3 },
      { NULL,          0 }
    };

  // Notification Flag Cursor Flags
  //-------------------------------
  // These below are resource numbers. Needs to use MAKEINTRESOURCE later.
    static TableEntry NotifyCursorTable[] = {
      { _T("APPSTARTING"), 32650 },
      { _T("ARROW"),       32512 },
      { _T("CROSS"),       32515 },
      { _T("HAND"),        32649 },
      { _T("HELP"),        32651 },
      { _T("IBEAM"),       32513 },
      { _T("NO"),          32648 },
      { _T("SIZEALL"),     32646 },
      { _T("SIZENESW"),    32643 },
      { _T("SIZENS"),      32645 },
      { _T("SIZENWSE"),    32642 },
      { _T("SIZEWE"),      32644 },
      { _T("UPARROW"),     32516 },
      { _T("WAIT"),        32514 },
      { NULL,          0     }
    };

  // Initialize pField
  //--------------------------------------------------------------
    IOExControlStorage *pField = pFields + nIdx;

    pField->nControlID = 1200 + nIdx;

  // Initialize Field Settings
  //--------------------------------------------------------------
    pField->nField = nCtrlIdx + 1;
    wsprintf(szField, _T("Field %d"), nCtrlIdx + 1);
    pszAppName = szField;

  // Type
  //--------------------------------------------------------------
    myGetProfileString(_T("TYPE"));
    pField->nType = LookupToken(TypeTable, szResult);

    if (pField->nType == FIELD_INVALID)
      continue;

  // Flags
  //--------------------------------------------------------------
    // This transforms the control type name to a flag, if the
    // flag exists.
    pField->nFlags = LookupToken(FlagTable, szResult);

    myGetProfileString(_T("Flags"));
    pField->nFlags |= LookupTokens(FlagTable, szResult);

    // Remove FLAG_CUSTOMDRAW_TEMP flag only when the user
    // specifies any of the color commands.
    switch(pField->nType)
    {
      case FIELD_LABEL:
      case FIELD_LINK:
      case FIELD_TEXT:
      case FIELD_LISTBOX:
      case FIELD_COMBOBOX:
    case FIELD_GROUPBOX:
      {
        pField->nFlags |= FLAG_CUSTOMDRAW_TEMP;
        break;
      }
    }

  // Notify
  //--------------------------------------------------------------
    myGetProfileString(_T("Notify"));
    pField->nNotify |= LookupTokens(ControlNotifyTable, szResult);

  // NotifyCursor
  //--------------------------------------------------------------
    myGetProfileString(_T("NotifyCursor"));
    pField->nNotifyCursor = LookupToken(NotifyCursorTable, szResult);
    if(!pField->nNotifyCursor || pField->nNotifyCursor == 0)
      if(pField->nType == FIELD_LINK)
        pField->nNotifyCursor = 32649; //HAND
      else
        pField->nNotifyCursor = 32512; //ARROW

  // Align
  //--------------------------------------------------------------
    myGetProfileString(_T("Align"));
    pField->nAlign = LookupToken(AlignTable, szResult);
    if(bRTL)
    {
      if(pField->nAlign == ALIGN_LEFT)
        pField->nAlign = ALIGN_RIGHT;
      else
      if(pField->nAlign == ALIGN_RIGHT)
        pField->nAlign = ALIGN_LEFT;
    }

  // VAlign
  //--------------------------------------------------------------
    myGetProfileString(_T("VAlign"));
    pField->nVAlign = LookupToken(VAlignTable, szResult);

  // TxtAlign
  //--------------------------------------------------------------
    myGetProfileString(_T("TxtAlign"));
    pField->nTxtAlign = LookupToken(TxtAlignTable, szResult);
    if(bRTL)
    {
      if(pField->nTxtAlign == ALIGN_TEXT_LEFT)
        pField->nTxtAlign = ALIGN_TEXT_RIGHT;
      else
      if(pField->nTxtAlign == ALIGN_TEXT_RIGHT)
        pField->nTxtAlign = ALIGN_TEXT_LEFT;
    }

  // TxtVAlign
  //--------------------------------------------------------------
    myGetProfileString(_T("TxtVAlign"));
    pField->nTxtVAlign = LookupToken(TxtVAlignTable, szResult);

  // RefFields
  //--------------------------------------------------------------
    pField->nRefFields = myGetProfileInt(_T("RefFields"), 0);

  // State
  //--------------------------------------------------------------
    // pszState must not be NULL!
    if (pField->nType == FIELD_TREEVIEW || pField->nType == FIELD_LISTVIEW || (pField->nType == FIELD_BUTTON && pField->nFlags & FLAG_FONTREQUEST && !pField->nRefFields))
    {
      pField->pszState = myGetProfileListItemsDup(_T("State"), 0, pField->nType);
    }
    else
    {
      myGetProfileString(_T("State"));
      if(!szResult && (pField->nType == FIELD_BUTTON || pField->nType == FIELD_LINK) && pField->nFlags & FLAG_COLORREQUEST)
        myitoa(szResult, GetSysColor(COLOR_WINDOW));
      pField->pszState = strdup(szResult); // uses STRDUP
    }

  // ListItems
  //--------------------------------------------------------------
    {
      if ((pField->nType == FIELD_BUTTON || pField->nType == FIELD_LINK) && pField->nFlags & FLAG_DIRREQUEST)
        pField->pszListItems = myGetProfileStringDup(_T("ListItems"));
      else
      {
        pField->pszListItems = myGetProfileListItemsDup(_T("ListItems"), 0, pField->nType);
      }
    }

  // Text
  //--------------------------------------------------------------
    // Label Text - convert newline
    pField->pszText = myGetProfileStringDup(_T("TEXT"));
    if (pField->nType == FIELD_LABEL || pField->nType == FIELD_LINK)
      ConvertNewLines(pField->pszText);

  // Root
  //--------------------------------------------------------------
    // Dir request - root folder
    pField->pszRoot = myGetProfileStringDup(_T("ROOT"));

  // ValidateText
  //--------------------------------------------------------------
    // ValidateText - convert newline
    pField->pszValidateText = myGetProfileStringDup(_T("ValidateText"));
    ConvertNewLines(pField->pszValidateText);

  // Filter
  //--------------------------------------------------------------
    pField->pszFilter = myGetProfileFilterItemsDup(_T("Filter"));

  // HeaderItems
  //--------------------------------------------------------------
    pField->pszHeaderItems = myGetProfileListItemsDup(_T("HeaderItems"), 2, pField->nType);

  // HeaderItemsWidth
  //--------------------------------------------------------------
    pField->pszHeaderItemsWidth = myGetProfileListItemsDup(_T("HeaderItemsWidth"), 2, pField->nType);

  // HeaderItemsAlign
  //--------------------------------------------------------------
    pField->pszHeaderItemsAlign = myGetProfileListItemsDup(_T("HeaderItemsAlign"), 2, pField->nType);

  // StateImageList
  //--------------------------------------------------------------
    pField->pszStateImageList = myGetProfileStringDup(_T("StateImageList"));

  // SmallImageList
  //--------------------------------------------------------------
    pField->pszSmallImageList = myGetProfileStringDup(_T("SmallImageList"));

  // LargeImageList
  //--------------------------------------------------------------
    pField->pszLargeImageList = myGetProfileStringDup(_T("LargeImageList"));

  // Left
  //--------------------------------------------------------------
    pField->RectUDU.left = myGetProfileInt(_T("Left"), 0);

  // Top
  //--------------------------------------------------------------
    pField->RectUDU.top = myGetProfileInt(_T("Top"), 0);

  // Width
  //--------------------------------------------------------------
    if(myGetProfileInt(_T("Width"), 0))
      pField->RectUDU.right = myGetProfileInt(_T("Width"), 0) + pField->RectUDU.left;

  // Height
  //--------------------------------------------------------------
    if(myGetProfileInt(_T("Height"), 0))
      pField->RectUDU.bottom = myGetProfileInt(_T("Height"), 0) + pField->RectUDU.top;

  // Right
  //--------------------------------------------------------------
    pField->RectUDU.right = myGetProfileInt(_T("Right"), 0);

  // Bottom
  //--------------------------------------------------------------
    pField->RectUDU.bottom = myGetProfileInt(_T("Bottom"), 0);

  // MinLen
  //--------------------------------------------------------------
    pField->nMinLength = myGetProfileInt(_T("MinLen"), 0);

  // MaxLen
  //--------------------------------------------------------------
    pField->nMaxLength = myGetProfileInt(_T("MaxLen"), 0);

  // FontName
  //--------------------------------------------------------------
    if(myGetProfileString(_T("FontName")))
      pField->pszFontName = myGetProfileStringDup(_T("FontName"));

  // FontHeight
  //--------------------------------------------------------------
    pField->nFontHeight = myGetProfileInt(_T("FontHeight"), 0);

  // FontWidth
  //--------------------------------------------------------------
    pField->nFontWidth = myGetProfileInt(_T("FontWidth"), 0);

  // FontBold
  //--------------------------------------------------------------
    if(myGetProfileInt(_T("FontBold"), 0))
      pField->nFontBold = TRUE;
    else
      pField->nFontBold = FALSE;

  // FontItalic
  //--------------------------------------------------------------
    if(myGetProfileInt(_T("FontItalic"), 0))
      pField->nFontItalic = TRUE;
    else
      pField->nFontItalic = FALSE;

  // FontUnderline
  //--------------------------------------------------------------
    if(myGetProfileInt(_T("FontUnderline"), 0))
      pField->nFontUnderline = TRUE;
    else
      pField->nFontUnderline = FALSE;

  // FontStrikeOut
  //--------------------------------------------------------------
    if(myGetProfileInt(_T("FontStrikeOut"), 0))
      pField->nFontStrikeOut = TRUE;
    else
      pField->nFontStrikeOut = FALSE;

  // ListItemsHeight
  //--------------------------------------------------------------
    pField->nListItemsHeight = myGetProfileInt(_T("ListItemsHeight"), 15);

  // TxtColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("TxtColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_LABEL:
        case FIELD_TEXT:
        case FIELD_LISTBOX:
        case FIELD_COMBOBOX:
        case FIELD_TREEVIEW:
        case FIELD_LISTVIEW:
          pField->crTxtColor = GetSysColor(COLOR_WINDOWTEXT);
          break;
        case FIELD_MONTHCALENDAR:
        case FIELD_DATETIME:
          pField->crTxtColor = GetSysColor(COLOR_INFOTEXT);
          break;
//Retain 0xFFFFFFFF for later:
//        case FIELD_LINK:
//          pField->crTxtColor = RGB(0,0,255);
//        break;
        case FIELD_PROGRESSBAR:
          pField->crTxtColor = GetSysColor(COLOR_MENUHILIGHT);
          break;
        default:
          pField->crTxtColor = myGetProfileInt(_T("TxtColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crTxtColor = myGetProfileInt(_T("TxtColor"), 0xFFFFFFFF);
    }

  // BgColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("BgColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_TEXT:
        case FIELD_RICHTEXT:
        case FIELD_LISTBOX:
        case FIELD_COMBOBOX:
        case FIELD_TREEVIEW:
        case FIELD_LISTVIEW:
          pField->crBgColor = GetSysColor(COLOR_WINDOW);
          break;
        case FIELD_DATETIME:
        case FIELD_MONTHCALENDAR:
          pField->crBgColor = GetSysColor(COLOR_INFOBK);
          break;
        default:
          pField->crBgColor = myGetProfileInt(_T("BgColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crBgColor = myGetProfileInt(_T("BgColor"), 0xFFFFFFFF);
    }

  // ReadOnlyTxtColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("ReadOnlyTxtColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_TEXT:
          pField->crReadOnlyTxtColor = GetSysColor(COLOR_WINDOWTEXT);
          break;
        default:
          pField->crReadOnlyTxtColor = myGetProfileInt(_T("ReadOnlyTxtColor"), 0xFFFFFFFF);
          break;
      }
    else
      pField->crReadOnlyTxtColor = myGetProfileInt(_T("ReadOnlyTxtColor"), 0xFFFFFFFF);

  // ReadOnlyBgColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("ReadOnlyBgColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_TEXT:
        case FIELD_RICHTEXT:
          pField->crReadOnlyBgColor = GetSysColor(COLOR_3DFACE);
          break;
        default:
          pField->crReadOnlyBgColor = myGetProfileInt(_T("ReadOnlyBgColor"), 0xFFFFFFFF);
          break;
      }
    else
      pField->crReadOnlyBgColor = myGetProfileInt(_T("ReadOnlyBgColor"), 0xFFFFFFFF);

  // SelTxtColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("SelTxtColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_LISTBOX:
        case FIELD_COMBOBOX:
        case FIELD_MONTHCALENDAR:
        case FIELD_DATETIME:
          pField->crSelTxtColor = GetSysColor(COLOR_HIGHLIGHTTEXT);
          break;
        default:
          pField->crSelTxtColor = myGetProfileInt(_T("SelTxtColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crSelTxtColor = myGetProfileInt(_T("SelTxtColor"), 0xFFFFFFFF);
    }

  // SelBgColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("SelBgColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_LISTBOX:
        case FIELD_COMBOBOX:
        case FIELD_MONTHCALENDAR:
        case FIELD_DATETIME:
          pField->crSelBgColor = GetSysColor(COLOR_MENUHILIGHT);
          break;
        default:
          pField->crSelBgColor = myGetProfileInt(_T("SelBgColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crSelBgColor = myGetProfileInt(_T("SelBgColor"), 0xFFFFFFFF);
    }

  // DisTxtColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("DisTxtColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_LABEL:
        case FIELD_LISTBOX:
        case FIELD_COMBOBOX:
          pField->crDisTxtColor = GetSysColor(COLOR_GRAYTEXT);
          break;
//Retain 0xFFFFFFFF for later:
//        case FIELD_LINK:
//          pField->crDisTxtColor = RGB(0,0,100);
//          break;
        default:
          pField->crDisTxtColor = myGetProfileInt(_T("DisTxtColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crDisTxtColor = myGetProfileInt(_T("DisTxtColor"), 0xFFFFFFFF);
    }

  // DisBgColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("DisBgColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_LISTBOX:
          pField->crDisBgColor = GetSysColor(COLOR_WINDOW);
          break;
        default:
          pField->crDisBgColor = myGetProfileInt(_T("DisBgColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crDisBgColor = myGetProfileInt(_T("DisBgColor"), 0xFFFFFFFF);
    }

  // DisSelTxtColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("DisSelTxtColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_LISTBOX:
          pField->crDisSelTxtColor = RGB(255, 255, 255);
          break;
        default:
          pField->crDisSelTxtColor = myGetProfileInt(_T("DisSelTxtColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crDisSelTxtColor = myGetProfileInt(_T("DisSelTxtColor"), 0xFFFFFFFF);
    }

  // DisSelBgColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("DisSelBgColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_LISTBOX:
          pField->crDisSelBgColor = RGB(170, 170, 170);
          break;
        default:
             pField->crDisSelBgColor = myGetProfileInt(_T("DisSelBgColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crDisSelBgColor = myGetProfileInt(_T("DisSelBgColor"), 0xFFFFFFFF);
    }

  // TxtShwColor
  //--------------------------------------------------------------
    pField->crTxtShwColor = (COLORREF)myGetProfileInt(_T("TxtShwColor"), 0xFFFFFFFF);
    if (pField->crTxtShwColor != 0xFFFFFFFF)
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;


  // SelTxtShwColor
  //--------------------------------------------------------------
    pField->crSelTxtShwColor = (COLORREF)myGetProfileInt(_T("SelTxtShwColor"), 0xFFFFFFFF);
    if (pField->crSelTxtShwColor != 0xFFFFFFFF)
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;

  // DisTxtShwColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("DisTxtShwColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_LABEL:
          pField->crDisTxtShwColor = GetSysColor(COLOR_WINDOW);
          break;
//Retain 0xFFFFFFFF for later:
//        case FIELD_LINK:
//          pField->crDisTxtShwColor = RGB(255, 255, 255);
//          break;
        default:
          pField->crDisTxtShwColor = myGetProfileInt(_T("DisTxtShwColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crDisTxtShwColor = myGetProfileInt(_T("DisTxtShwColor"), 0xFFFFFFFF);
    }

  // DisSelTxtShwColor
  //--------------------------------------------------------------
    pField->crDisSelTxtShwColor = (COLORREF)myGetProfileInt(_T("DisSelTxtShwColor"), 0xFFFFFFFF);
    if (pField->crDisSelTxtShwColor != 0xFFFFFFFF)
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;

  // MonthOutColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("MonthOutColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_MONTHCALENDAR:
          pField->crMonthOutColor = GetSysColor(COLOR_WINDOW);
          break;
        default:
             pField->crMonthOutColor = (COLORREF)myGetProfileInt(_T("MonthOutColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crMonthOutColor = (COLORREF)myGetProfileInt(_T("MonthOutColor"), 0xFFFFFFFF);
    }

  // MonthTrailingTxtColor
  //--------------------------------------------------------------
    if((COLORREF)myGetProfileInt(_T("MonthTrailingTxtColor"), 0xFFFFFFFF) == 0xFFFFFFFF)
      switch(pField->nType) {
        case FIELD_MONTHCALENDAR:
          pField->crMonthTrailingTxtColor = GetSysColor(COLOR_GRAYTEXT);
          break;
        default:
             pField->crMonthTrailingTxtColor = (COLORREF)myGetProfileInt(_T("MonthTrailingTxtColor"), 0xFFFFFFFF);
          break;
      }
    else
    {
      if (pField->nFlags & FLAG_CUSTOMDRAW_TEMP)
        pField->nFlags &= ~FLAG_CUSTOMDRAW_TEMP;
      pField->crMonthTrailingTxtColor = (COLORREF)myGetProfileInt(_T("MonthTrailingTxtColor"), 0xFFFFFFFF);
    }

  // ToolTipText
  //--------------------------------------------------------------
    pField->pszToolTipText = myGetProfileStringDup(_T("ToolTipText"));

    // "ToolTipText" has to exist in order to have a ToolTip
    if(pField->pszToolTipText) {

    // ToolTipFlags
    //--------------------------------------------------------------
      pField->nToolTipFlags = LookupToken(ToolTipFlagTable, szResult);
      myGetProfileString(_T("ToolTipFlags"));
      pField->nToolTipFlags |= LookupTokens(ToolTipFlagTable, szResult);

      if(pField->nToolTipFlags & TTS_BALLOON) {
      // ToolTipIcon
      //--------------------------------------------------------------
        myGetProfileString(_T("ToolTipIcon"));
        pField->nToolTipIcon = LookupToken(ToolTipIconTable, szResult);

      // ToolTipTitle
      //--------------------------------------------------------------
        pField->pszToolTipTitle = myGetProfileStringDup(_T("ToolTipTitle"));
      }
    // ToolTipTxtColor
    //--------------------------------------------------------------
      pField->crToolTipTxtColor = (COLORREF)myGetProfileInt(_T("ToolTipTxtColor"), GetSysColor(COLOR_INFOTEXT));

    // ToolTipBgColor
    //--------------------------------------------------------------
      pField->crToolTipBgColor = (COLORREF)myGetProfileInt(_T("ToolTipBgColor"), GetSysColor(COLOR_INFOBK));

    // ToolTipMaxWidth
    //--------------------------------------------------------------
      pField->nToolTipMaxWidth = myGetProfileInt(_T("ToolTipMaxWidth"), 300);
    }
  }

  return nNumFields;
}











bool WINAPI SaveSettings(void) {

// Initialize Variables
//================================================================
  static TCHAR szField[25];
  int nBufLen = g_nBufferSize;
  TCHAR *pszBuffer = (TCHAR*)MALLOC(nBufLen*sizeof(TCHAR));
  if (!pszBuffer) return false;

  int nIdx;
  int CurrField;

// Save Settings For Each Field Existant
//================================================================
  for (nIdx = 0, CurrField = 1; nIdx < nNumFields; nIdx++, CurrField++) {

  // Define pField
  //--------------------------------------------------------------
    IOExControlStorage *pField = pFields + nIdx;
    HWND hwnd = pField->hwnd;

  // Save Settings (By Control Type)
  //--------------------------------------------------------------
    switch (pField->nType) {

    // Invalid
    //--------
      default:
        continue;

    // CheckBox, RadioButton
    //----------------------
      case FIELD_CHECKBOX:
      case FIELD_RADIOBUTTON:
      {
        myltoa(pszBuffer, mySendMessage(hwnd, BM_GETCHECK, 0, 0));
        break;
      }

    // ListBox
    //--------
      case FIELD_LISTBOX:
      {
        // Ok, this one requires a bit of work.
        // First, we allocate a buffer long enough to hold every item.
        // Then, we loop through every item and if it's selected we add it to our buffer.
        // If there is already an item in the list, then we prepend a | character before the new item.
        // We could simplify for single-select boxes, but using one piece of code saves some space.
        int nLength = strlen(pField->pszListItems) + 10;
        if (nLength > nBufLen) {
          FREE(pszBuffer);
          nBufLen = nLength;
          pszBuffer = (TCHAR*)MALLOC(nBufLen*sizeof(TCHAR));
          if (!pszBuffer) return false;
        }
        TCHAR *pszItem = (TCHAR*)MALLOC(nBufLen*sizeof(TCHAR));
        if (!pszItem) return false;

        *pszBuffer = _T('\0');
        INT_PTR nNumItems = mySendMessage(hwnd, LB_GETCOUNT, 0, 0);
        for (int nIdx2 = 0; nIdx2 < nNumItems; nIdx2++) {
          if (mySendMessage(hwnd, LB_GETSEL, nIdx2, 0) > 0) {
            if (*pszBuffer) lstrcat(pszBuffer, _T("|"));
            mySendMessage(hwnd, LB_GETTEXT, (WPARAM)nIdx2, (LPARAM)pszItem);
            lstrcat(pszBuffer, pszItem);
          }
        }

        FREE(pszItem);
        break;
      }

    // Text, ComboBox
    //---------------
      case FIELD_TEXT:
      case FIELD_COMBOBOX:
      {
        //we should only add the final '\"' after IOtoa is called.
        GetWindowText(hwnd, pszBuffer, nBufLen);

        //Re-allocate, truncate, and add '\"' to the string.
        TCHAR* pszBuf2 = (TCHAR*)MALLOC((nBufLen+2)*sizeof(TCHAR));

        *pszBuf2='\"';

        if (pField->nType == FIELD_TEXT && (pField->nFlags & FLAG_MULTILINE))
          strncpy(pszBuf2+1, IOtoa(pszBuffer), nBufLen);
        else
          strcpy(pszBuf2+1, pszBuffer);

        FREE(pszBuffer);

        int nLength = strlen(pszBuf2);
        pszBuf2[nLength]=_T('"');
        pszBuf2[nLength+1]=_T('\0');

        pszBuffer=pszBuf2;
        break;
      }

    // RichText
    //---------
      case FIELD_RICHTEXT:
      {
        // Only if not read only
        if ( (pField->nFlags & FLAG_READONLY) == 0)
        {
            strcpy(pszBuffer, pField->pszState);

            // Step 1: Open the user file
            EDITSTREAM editStream;
            TCHAR *pszData = (TCHAR*)MALLOC(g_nBufferSize);
            if (!pszData)
            {
              //TODO: Add error handling
              break;
            }

            dwRead=0;

            editStream.pfnCallback = FIELD_RICHTEXT_StreamOut;
            editStream.dwCookie = (DWORD)pszData;
            mySendMessage(hwnd,EM_STREAMOUT,(WPARAM)pField->nDataType,(LPARAM)&editStream);

            FILE * hFile;
            if ( _tfopen_s(&hFile, pField->pszState, _T("wb")) != 0)
            {
              //TODO: Add error handling
              FREE(pszData);
              break;
            }

            UINT nDataLen = (UINT)strlen(pszData);
            fwrite(pszData,1,nDataLen,hFile);
            fclose(hFile);
            FREE(pszData);
        }
        break;
      }

    // TreeView
    //---------
      case FIELD_TREEVIEW:
      {
        LPTSTR pszBuf2 = (LPTSTR)MALLOC(260*2+1);
        LPTSTR pszBuf3 = (LPTSTR)MALLOC(260*2+1);

        if(pField->nFlags & FLAG_CHECKBOXES)
        {
          bool bFinishing = FALSE;
          TVITEM tvItem;
          HTREEITEM hItem = TreeView_GetRoot(hwnd);
          while(hItem)
          {
            tvItem.mask = TVIF_PARAM;
            tvItem.hItem = hItem;
            TreeView_GetItem(hwnd, &tvItem);

            if(!bFinishing)
            {
              myitoa(pszBuf2, tvItem.lParam);

              if(TreeView_GetChild(hwnd, hItem))
                lstrcat(pszBuf2, _T("{"));
              else if(TreeView_GetNextSibling(hwnd, hItem))
                lstrcat(pszBuf2, _T("|"));
              else if(TreeView_GetParent(hwnd, hItem))
                lstrcat(pszBuf2, _T("}"));
            }
            else
            {
              if(TreeView_GetParent(hwnd, hItem))
                strcpy(pszBuf2,_T("}"));
              else
                strcpy(pszBuf2,_T(""));
            }

            lstrcat(pszBuffer, pszBuf2);

            if(TreeView_GetChild(hwnd, hItem) && bFinishing == FALSE)
              hItem = TreeView_GetChild(hwnd, hItem);
            else if(TreeView_GetNextSibling(hwnd, hItem) && bFinishing == FALSE)
              hItem = TreeView_GetNextSibling(hwnd, hItem);
            else if(TreeView_GetParent(hwnd, hItem))
            {
              bFinishing = FALSE;
              hItem = TreeView_GetParent(hwnd, hItem);
              if(TreeView_GetNextSibling(hwnd, hItem))
                hItem = TreeView_GetNextSibling(hwnd, hItem);
              else
              {
                bFinishing = TRUE;
              }
            }
            else
            {
              bFinishing = FALSE;
              break;
            }
          }
        }
        // No CHECKBOXES Flag
        else
        {
          HTREEITEM hItem = TreeView_GetSelection(hwnd);
          int i = 0;

          TVITEM tvi;
          tvi.mask = TVIF_TEXT;
          tvi.pszText = pszBuf2;
          tvi.cchTextMax = 260;

          while(hItem)
          {
            ++i;
            tvi.hItem = hItem;

            TreeView_GetItem(hwnd, &tvi);

            pszBuf2 = tvi.pszText;

            pszBuf2 = IOLItoLI(pszBuf2, 260*2+1, pField->nType);
            strcpy(pszBuf3, pszBuf2);

            if(i!=1)
            {
              lstrcat(pszBuf3, _T("{"));
              lstrcat(pszBuf3, pszBuffer);
              lstrcat(pszBuf3, _T("}"));
            }

            strcpy(pszBuffer, pszBuf3);

            hItem = TreeView_GetParent(hwnd, hItem);
          }
        }

        // Return "ListItems"

        if(pField->nFlags & FLAG_EDITLABELS)
        {
          strcpy(pszBuf3, _T(""));
          strcpy(pszBuf2, _T(""));
          strcpy(pField->pszListItems, _T(""));
          bool bFinishing = FALSE;
          TVITEM tvItem;
          HTREEITEM hItem = TreeView_GetRoot(hwnd);

          while(hItem)
          {
            tvItem.mask = TVIF_TEXT;
            tvItem.hItem = hItem;
            tvItem.pszText = pszBuf2;
            tvItem.cchTextMax = 260;
            TreeView_GetItem(hwnd, &tvItem);

            pszBuf3 = tvItem.pszText;

            pszBuf2 = IOLItoLI(pszBuf2, 260*2+1, pField->nType);
            strcpy(pszBuf3, pszBuf2);

            if(!bFinishing)
            {
              if(TreeView_GetChild(hwnd, hItem))
                lstrcat(pszBuf3, _T("{"));
              else if(TreeView_GetNextSibling(hwnd, hItem))
                lstrcat(pszBuf3, _T("|"));
              else if(TreeView_GetParent(hwnd, hItem))
                lstrcat(pszBuf3, _T("}"));
            }
            else
            {
              if(TreeView_GetParent(hwnd, hItem))
                strcpy(pszBuf3,_T("}"));
              else
                strcpy(pszBuf3,_T(""));
            }

            lstrcat(pField->pszListItems, pszBuf2);

            if(TreeView_GetChild(hwnd, hItem) && bFinishing == FALSE)
              hItem = TreeView_GetChild(hwnd, hItem);
            else if(TreeView_GetNextSibling(hwnd, hItem) && bFinishing == FALSE)
              hItem = TreeView_GetNextSibling(hwnd, hItem);
            else if(TreeView_GetParent(hwnd, hItem))
            {
              bFinishing = FALSE;
              hItem = TreeView_GetParent(hwnd, hItem);
              if(TreeView_GetNextSibling(hwnd, hItem))
                hItem = TreeView_GetNextSibling(hwnd, hItem);
              else
              {
                bFinishing = TRUE;
              }
            }
            else
            {
              bFinishing = FALSE;
              break;
            }
          }

          wsprintf(szField, _T("Field %d"), CurrField);
          WritePrivateProfileString(szField, _T("ListItems"), pField->pszListItems, pszFilename);
        }
        FREE(pszBuf3);
        FREE(pszBuf2);

        break;
      }

    // ListView
    //---------
      case FIELD_LISTVIEW:
      { 
        LPTSTR pszBuf2 = (LPTSTR)MALLOC(260*2+1);
        LPTSTR pszBuf3 = (LPTSTR)MALLOC(260*2+1);

        // Step 1: Detect number of subitems per item to retrieve.
        int nColumns = FIELD_LISTVIEW_IOLI_CountSubItems(pField->pszListItems, pField->pszHeaderItems) + 1; // Plus an item column.

        if(pField->nFlags & FLAG_CHECKBOXES)
        {
          int iItem = -1;
          bool bFinishing = FALSE;

          // Step 2: Create output data.
          while(true)
          {
            iItem++;
            // It's the same as:
            // iItem = ListView_GetNextItem(hwnd, iItem, 0);

            // Step 2.1: Return "State"
            //-----------------------
            int iState = (ListView_GetItemState(hwnd, iItem, LVIS_STATEIMAGEMASK)>>12);

            myitoa(pszBuf2, TREEVIEW_UNCHECKED);

            // No checkboxes (TREEVIEW_NOCHECKBOX)
            //------------------------------------
            if(iState == 0)
              myitoa(pszBuf2, TREEVIEW_NOCHECKBOX);

            // Read-only checkboxes (TREEVIEW_READONLY)
            //-----------------------------------------
            else if(iState == 6)
              myitoa(pszBuf2, TREEVIEW_READONLY | TREEVIEW_CHECKED);
            else if(iState == 5)
              myitoa(pszBuf2, TREEVIEW_READONLY | TREEVIEW_UNCHECKED);
            // Checked checkboxes (TREEVIEW_CHECKED)
            //-----------------------------------------
            else if(iState == 3)
              myitoa(pszBuf2, TREEVIEW_CHECKED);
            else if(iState == 2)
              myitoa(pszBuf2, TREEVIEW_UNCHECKED);

            if(ListView_GetNextItem(hwnd, iItem, 0)!=-1)
              lstrcat(pszBuf2, _T("|"));

            lstrcat(pszBuffer, pszBuf2);

            if(ListView_GetNextItem(hwnd, iItem, 0)==-1)
              break;
          }
        }
        else
        {
          int iSelectedItem = -1;
          int i = 0;

          while((iSelectedItem = ListView_GetNextItem(hwnd, iSelectedItem, LVNI_ALL | LVNI_SELECTED)) > -1)
          {
            int nSelected = ListView_GetItemState(hwnd, iSelectedItem, LVIS_SELECTED);
            if (nSelected == LVIS_SELECTED)
            {
              ListView_GetItemText(hwnd, iSelectedItem, 0, pszBuf2, 260);
              pszBuf2 = IOLItoLI(pszBuf2, 260*2+1, pField->nType);

              if(i!=0)
                lstrcat(pszBuffer, _T("|"));
              lstrcat(pszBuffer, pszBuf2);
            }
            i++;
          }
        }

        // Return "ListItems"

        if(pField->nFlags & FLAG_EDITLABELS)
        {
          strcpy(pszBuf2,_T(""));
          strcpy(pszBuf3,_T(""));
          strcpy(pField->pszListItems, _T(""));

          int iItem = -1;
          while(true)
          {
            ++iItem;
            ListView_GetItemText(hwnd, iItem, 0, pszBuf2, 260);

            pszBuf2 = IOLItoLI(pszBuf2, 260*2+1, pField->nType);

            lstrcat(pField->pszListItems, pszBuf2);

            int iSubItem = 1;
            int nStWritten = 0;
            if(nColumns-1 > 0)
            {
              strcpy(pszBuf2, _T(""));
              strcpy(pszBuf3, _T("{"));

              while(true)
              {
                ListView_GetItemText(hwnd, iItem, iSubItem, pszBuf2, 260)
                pszBuf2 = IOLItoLI(pszBuf2, 260*2+1, pField->nType);

                if(lstrcmp(pszBuf2,_T(""))==0)
                {
                  if(iSubItem > 1)
                    lstrcat(pszBuf3, _T("|"));
                }
                else
                {
                  nStWritten = 1;
                  lstrcat(pField->pszListItems, pszBuf3);
                  if(iSubItem > 1)
                    lstrcat(pField->pszListItems, _T("|"));
                  lstrcat(pField->pszListItems, pszBuf2);
                  strcpy(pszBuf3, _T(""));
                }

                ++iSubItem;
                if(iSubItem > nColumns)
                {
                  strcpy(pszBuf2, _T(""));
                  strcpy(pszBuf3, _T(""));
                  if(nStWritten)
                    lstrcat(pField->pszListItems, _T("}"));
                  break;
                }
              }
            }
            if(ListView_GetNextItem(hwnd, iItem, 0)!=-1 && !nStWritten)
              lstrcat(pField->pszListItems, _T("|"));

            if(ListView_GetNextItem(hwnd, iItem, 0)==-1)
              break;
          }
          wsprintf(szField, _T("Field %d"), CurrField);
          WritePrivateProfileString(szField, _T("ListItems"), pField->pszListItems, pszFilename);
        }

        FREE(pszBuf2);
        FREE(pszBuf3);

        break;
      }

    // ProgressBar
    //------------
      case FIELD_PROGRESSBAR:
        {
          int nProgressState = mySendMessage(hwnd, PBM_GETPOS, 0, 0);
          myitoa(pszBuffer, nProgressState);
          break;
        }

    // TrackBar
    //---------
      case FIELD_TRACKBAR:
        {
          int nTrackState = mySendMessage(hwnd, TBM_GETPOS, 0, 0);
          myitoa(pszBuffer, nTrackState);
          break;
        }

    // IPAddress
    //----------
      case FIELD_IPADDRESS:
        {
          DWORD nIPAddressState;

          mySendMessage(hwnd, IPM_GETADDRESS, 0, (LPARAM) &nIPAddressState);
          BYTE nField0 = (BYTE) FIRST_IPADDRESS(nIPAddressState);
          BYTE nField1 = (BYTE) SECOND_IPADDRESS(nIPAddressState);
          BYTE nField2 = (BYTE) THIRD_IPADDRESS(nIPAddressState);
          BYTE nField3 = (BYTE) FOURTH_IPADDRESS(nIPAddressState);

          wsprintf(pszBuffer, _T("%d.%d.%d.%d"), nField0, nField1, nField2, nField3);
          break;
        }

    // DateTime
    //---------
      case FIELD_DATETIME:
        {
          SYSTEMTIME lpSysTime;

          mySendMessage(hwnd, DTM_GETSYSTEMTIME, 0, (LPARAM) &lpSysTime);
          const SYSTEMTIME lpSysTime2 = lpSysTime;

          TCHAR* pszDate = (TCHAR*)MALLOC((10+1)*sizeof(TCHAR));
          TCHAR* pszDayOfWeek = (TCHAR*)MALLOC((3+1)*sizeof(TCHAR));
          TCHAR* pszTime = (TCHAR*)MALLOC((8+1)*sizeof(TCHAR));

          GetDateFormat(MAKELCID(MAKELANGID(LANG_ENGLISH,SUBLANG_ENGLISH_US),SORT_DEFAULT), NULL, &lpSysTime2, _T("dd/MM/yyyy"), pszDate, 11);
          GetDateFormat(MAKELCID(MAKELANGID(LANG_ENGLISH,SUBLANG_ENGLISH_US),SORT_DEFAULT), NULL, &lpSysTime2, _T("ddd"), pszDayOfWeek, 4);
          GetTimeFormat(MAKELCID(MAKELANGID(LANG_ENGLISH,SUBLANG_ENGLISH_US),SORT_DEFAULT), NULL, &lpSysTime2, _T("HH:mm:ss"), pszTime, 9);

          int nDayOfWeek = 6;

          if(pszDayOfWeek == _T("Sun"))
            nDayOfWeek = 0;
          else if(pszDayOfWeek == _T("Mon"))
            nDayOfWeek = 1;
          else if(pszDayOfWeek == _T("Tue"))
            nDayOfWeek = 2;
          else if(pszDayOfWeek == _T("Wed"))
            nDayOfWeek = 3;
          else if(pszDayOfWeek == _T("Thu"))
            nDayOfWeek = 4;
          else if(pszDayOfWeek == _T("Fri"))
            nDayOfWeek = 5;
          else if(pszDayOfWeek == _T("Sat"))
            nDayOfWeek = 6;

          wsprintf(pszBuffer, _T("%hs %hs %d"), pszDate, pszTime, nDayOfWeek);

          FREE(pszDate);
          FREE(pszDayOfWeek);
          FREE(pszTime);
          break;
        }

    // MonthCalendar
    //--------------
      case FIELD_MONTHCALENDAR:
        {
          SYSTEMTIME lpSysTime;
          mySendMessage(hwnd, MCM_GETCURSEL, 0, (LPARAM) &lpSysTime);
          const SYSTEMTIME lpSysTime2 = lpSysTime;
          GetDateFormat(LOCALE_USER_DEFAULT, NULL,&lpSysTime2, _T("dd/MM/yyyy"), pszBuffer, 11);
          break;
        }

    // UpDown
    //-------
      case FIELD_UPDOWN:
        {
          if(!pField->nRefFields)
          {
            int nUpDownState = mySendMessage(hwnd, UDM_GETPOS32, 0, 0);
            wsprintf(pszBuffer, _T("%d"), nUpDownState);
          }
          else
            continue;
          break;
        }

    // Link, Button
    //-------------
      case FIELD_LINK:
      case FIELD_BUTTON:
        {
          // Copy the State of the refFields in the its State
          if (lstrcmp(pField->pszState,_T(""))==0 && pField->nRefFields != 0)
          {
            GetWindowText(GetDlgItem(hConfigWindow,1200+pField->nRefFields-1), pszBuffer, g_nBufferSize);
          }
          else if(lstrcmp(pField->pszState,_T(""))!=0 && (pField->nFlags & FLAG_OPEN_FILEREQUEST || pField->nFlags & FLAG_SAVE_FILEREQUEST || pField->nFlags & FLAG_DIRREQUEST))
          {
            lstrcpyn(pszBuffer, (LPTSTR)pField->pszState, g_nBufferSize);
          }
          else if(lstrcmp(pField->pszState,_T(""))!=0 && pField->nFlags & FLAG_FONTREQUEST)
          {
            lstrcpyn(pszBuffer, (LPTSTR)pField->pszState, g_nBufferSize);
            pszBuffer = IOLItoLI(pszBuffer, lstrlen(pszBuffer)+1, pField->nType);
          }
          else if(pField->nFlags & FLAG_COLORREQUEST)
          {
            lstrcpyn(pszBuffer, (LPTSTR)pField->pszListItems, g_nBufferSize);
            pszBuffer = IOLItoLI(pszBuffer, lstrlen(pszBuffer)+1, pField->nType);
            wsprintf(szField, _T("Field %d"), CurrField);
            WritePrivateProfileString(szField, _T("ListItems"), pszBuffer, pszFilename);

            lstrcpyn(pszBuffer, (LPTSTR)pField->pszState, g_nBufferSize);
          }
          else
            continue;
          break;
        }
    }
  // Write Back To INI File The Control State
  //--------------------------------------------------------------
    wsprintf(szField, _T("Field %d"), CurrField);
    WritePrivateProfileString(szField, _T("State"), pszBuffer, pszFilename);
  }

// Write To INI File What Field and Notification Were Activated
//================================================================
  if(g_aNotifyQueue[0].iField > 0)
  {
    myitoa(pszBuffer, g_aNotifyQueue[0].iField);
    WritePrivateProfileString(_T("Settings"), _T("State"), pszBuffer, pszFilename);
  }
  if(g_aNotifyQueue[0].iNotifyId != NOTIFY_NONE && g_aNotifyQueue[0].bNotifyType == NOTIFY_TYPE_CONTROL)
    WritePrivateProfileString(_T("Settings"), _T("Notify"), LookupTokenName(ControlNotifyTable, g_aNotifyQueue[0].iNotifyId), pszFilename);

  FREE(pszBuffer);
  return true;
}

//Other Functions
bool INLINE ValidateFields() {
  int nIdx;
  INT_PTR nLength;

// "MaxLen" and "MinLen" Implementations (Only For Text)
//================================================================
  // In the unlikely event we can't allocate memory, go ahead and return true so we can get out of here.
  // May cause problems for the install script, but no memory is problems for us.
  for (nIdx = 0; nIdx < nNumFields; nIdx++) {
    IOExControlStorage *pField = pFields + nIdx;

  // Only check controls that are after FIELD_CHECKLEN
  //--------------------------------------------------------------
    // this if statement prevents a stupid bug where a min/max length is assigned to a label control
    // where the user obviously has no way of changing what is displayed. (can you say, "infinite loop"?)
    if (pField->nType >= FIELD_CHECKLEN) {
      nLength = mySendMessage(pField->hwnd, WM_GETTEXTLENGTH, 0, 0);

      if (((pField->nMaxLength > 0) && (nLength > pField->nMaxLength)) ||
         ((pField->nMinLength > 0) && (nLength < pField->nMinLength))) {

        // "ValidateText" Implementation
        if (pField->pszValidateText) {
          TCHAR szTitle[1024];
          GetWindowText(hMainWindow, szTitle, _countof(szTitle));
          MessageBox(hConfigWindow, pField->pszValidateText, szTitle, MB_OK|MB_ICONWARNING);
        }
        mySetFocus(pField->hwnd);
        return false;
      }
    }
  }
  return true;
}
