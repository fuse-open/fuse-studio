#define _WIN32_IE 0x0600
#define WINVER 0x0500
#define _WIN32_WINNT 0x0500
#include <windows.h>
#include <richedit.h>
#include <commctrl.h>

//Used in wa_dlg.h
HINSTANCE g_hInstance;

//Private #includes
#include "pluginapi.h"
#define WADLG_IMPLEMENT
#include "wa_dlg.h"
#include "wa_scrollbars.h"

//Variables
//================================================================
HWND g_hwndParent;
WNDPROC oldProc;
LONG g_oldnsiswndstyle;
HWND g_oldnsisparentwnd;
HBITMAP g_hbmb;
HBITMAP g_hbma;

//Functions
//================================================================
BOOL CALLBACK EnumChildProc(HWND, LPARAM);
BOOL CALLBACK EnumChildProc_ScrollBarUninit(HWND hwnd, LPARAM lParam);
void internal_skin(int);
void internal_unskin();
TCHAR* GetLastErrorStr(void);

unsigned int myhtoi(TCHAR*);
unsigned int rgbtobgr(unsigned int);
void FixMainControls();

#ifndef LEGACY_PLUGIN

// Plugin callback function
//================================================================
static UINT_PTR PluginCallback(enum NSPIM msg)
{ 
	if (msg == NSPIM_GUIUNLOAD)
	{
		internal_unskin();
	}
	return 0;
}

#endif

#pragma message(" ")
#pragma message("----------------------------------------------------")
#ifndef LEGACY_PLUGIN
	#pragma message(" Info: Compilation using the new NSIS plugin API")
#else
	#pragma message(" Info: Compilation using the legacy NSIS plugin API")
#endif
#pragma message("----------------------------------------------------")
#pragma message(" ")
