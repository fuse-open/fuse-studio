/*
o--------------------------------------------------------------------------o
|SkinnedControls 1.4                                                       |
|Based on wansis, a Plug-in written by Saivert that skins NSIS like Winamp.|
(--------------------------------------------------------------------------)
| Main source code.                           / A plug-in for NSIS 2 and 3 |
|                                            ------------------------------|
| By SuperPat                                                              |
o--------------------------------------------------------------------------o
*/

#include "SkinnedControls.h"

//Implementation
void main(){}
// makes a smaller DLL file
BOOL WINAPI _DllMainCRTStartup(HINSTANCE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	g_hInstance = hModule;
	return 1;
}

BOOL WINAPI DllMain(HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
	g_hInstance = hInst;
	return TRUE;
}

// This is really a window procedure
LRESULT CALLBACK ChildDlgProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	WNDPROC proc;
	LRESULT a, dlgresult;

	proc = (WNDPROC)(UIntToPtr(GetWindowLongPtr(hwnd, DWLP_USER)));
	dlgresult = CallWindowProc(proc, hwnd, uMsg, wParam, lParam);

	if (g_hbmb)
	{
		if (wascrollbars_handleMessages(hwnd, uMsg, wParam, lParam, &a))
			return a;
	}

	if (g_hbma)
	{
		if (a = WADlg_handleDialogMsgs(hwnd, uMsg, wParam, lParam))
		{
			SetWindowLong(hwnd, DWL_MSGRESULT, (LONG)a);
			return a;
		}

		/* fix for buttons unskinning itself after click under control */
		if (uMsg == WM_COMMAND) {
			FixMainControls();
		}
	}

	return dlgresult;
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	if (g_hbma)
	{
		LRESULT a = WADlg_handleDialogMsgs(hwnd, uMsg, wParam, lParam);
		if (a)
			return a;
	}
	// Little hack for proper redrawing buttons when MessageBox is shown
	// Use SendMessage $HWNDPARENT ${WM_MENUDRAG} 0 0 in your .nsi code to call this redraw
	if(uMsg == WM_MENUDRAG)
	{
		FixMainControls();

		InvalidateRect(hwnd, NULL, TRUE);
		{		
			HWND dlg = FindWindowEx(hwnd, 0, _T("#32770"), NULL);
			SetWindowLongPtr(dlg, GWLP_WNDPROC, GetWindowLongPtr(dlg, DWLP_USER));

			PostMessage(hwnd, WM_USER+0x9, 0, 0);
		}
	}
	else if (uMsg == WM_NOTIFY_OUTER_NEXT)
	{
		HWND dlg = FindWindowEx(hwnd, 0, _T("#32770"), NULL);
		SetWindowLongPtr(dlg, GWLP_WNDPROC, GetWindowLongPtr(dlg, DWLP_USER));

		PostMessage(hwnd, WM_USER+0x9, 0, 0);
	}
	else if (uMsg == WM_USER+0x9)
	{
		LONG oldChildDlgProc, tempChildDlgProc;
		EnumChildWindows(hwnd, EnumChildProc, 0);
		if (!wParam)
		{
			HWND dlg = FindWindowEx(hwnd, 0, _T("#32770"), NULL);
			if (dlg)
			{
				EnumChildWindows(dlg, EnumChildProc, 0);
				tempChildDlgProc=GetWindowLongPtr(dlg, GWLP_WNDPROC);
				if(PtrToInt(tempChildDlgProc)!=PtrToInt(ChildDlgProc))//don't replace DlgProc multiple times!
				{
					oldChildDlgProc = SetWindowLongPtr(dlg, GWLP_WNDPROC, PtrToInt(ChildDlgProc));
					SetWindowLongPtr(dlg, DWLP_USER, (LONG) oldChildDlgProc);
				}
			}
		}

		InvalidateRect(hwnd, NULL, TRUE);
		return 0;
	}

	return CallWindowProc(oldProc, hwnd, uMsg, wParam, lParam);
}

LRESULT CALLBACK FrameWndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
		case WM_DESTROY:
		{
			SetWindowLongPtr(g_hwndParent, GWLP_WNDPROC, PtrToInt(oldProc));
			SetWindowLong(g_hwndParent, GWL_STYLE, PtrToInt(g_oldnsiswndstyle));
			SetParent(g_hwndParent, g_oldnsisparentwnd);
			InvalidateRect(hwnd, NULL, TRUE);
		}
		break;

		case WM_CLOSE:
		{
			SetWindowLongPtr(g_hwndParent, GWLP_WNDPROC, PtrToInt(oldProc));
			SetWindowLong(g_hwndParent, GWL_STYLE, PtrToInt(g_oldnsiswndstyle));
			SetParent(g_hwndParent, g_oldnsisparentwnd);
			InvalidateRect(hwnd, NULL, TRUE);

			PostMessage(g_hwndParent, WM_SYSCOMMAND, SC_CLOSE, 0);
			return 0;
		}
		case WM_PAINT:
		{			
			PAINTSTRUCT ps;

			BeginPaint(hwnd, &ps);
			EndPaint(hwnd, &ps);

			return 0;
		}
		case WM_ERASEBKGND: // This didn't help my problem
		{
			return 1; // Return 1 if this has been handled 
		}
		case WM_CTLCOLORBTN:
		{
			int i; // plain C...
			// Test to see if this button is our subclassed button
			for (i = 0; i < 3; i++)
			{
				if ((HWND)lParam == GetDlgItem(g_hwndParent, 1 + i))
					// StockObject: doesn't have to be free'd, doesn't hurt if you do free
					return (LRESULT)GetStockObject(HOLLOW_BRUSH);
			}
		} 
	}
	return CallWindowProc(DefWindowProc, hwnd, uMsg, wParam, lParam);
}

// re-skin the button when we click on "Browse" in the directory page
void FixMainControls()
{
	LONG style;

#define MakeButtonOwnerdraw(hwnd) style = GetWindowLong(hwnd, GWL_STYLE);\
	SetWindowLong(hwnd, GWL_STYLE, (style & ~BS_PUSHBUTTON) | BS_OWNERDRAW); \
	SetProp(hwnd, _T("SCBtn"), (HANDLE)1);

	MakeButtonOwnerdraw(GetDlgItem(g_hwndParent, 3));		// Back button
	MakeButtonOwnerdraw(GetDlgItem(g_hwndParent, 2));		// Cancel button
	MakeButtonOwnerdraw(GetDlgItem(g_hwndParent, 1));		// Next button
	MakeButtonOwnerdraw(GetDlgItem(g_hwndParent, 1027));	// Show Detail button

#undef MakeButtonOwnerdraw
}

BOOL CALLBACK EnumChildProc(HWND hwnd, LPARAM lParam)
{
	TCHAR classname[256];

	GetClassName(hwnd, classname, 255);

	if (g_hbma)
	{
		if (!lstrcmpi(classname, _T("BUTTON")))
		{
			LONG style = GetWindowLong(hwnd, GWL_STYLE);

			if ( ((style & BS_GROUPBOX) == 0) || (GetParent(hwnd) == g_hwndParent))
			{
				if ((style & BS_TYPEMASK) != BS_OWNERDRAW &&
					(style & BS_TYPEMASK) != BS_GROUPBOX &&
					(style & BS_TYPEMASK) != BS_AUTOCHECKBOX &&
					(style & BS_TYPEMASK) != BS_AUTORADIOBUTTON ) {
						SetProp(hwnd, _T("SCBtn"), (HANDLE)1);
				}
				SetWindowLong(hwnd, GWL_STYLE, (style & ~BS_PUSHBUTTON) | BS_OWNERDRAW);
			}
		}
		FixMainControls();
	}
	if (g_hbmb)
	{
		// Special case for RichEdit controls
		if (!lstrcmpi(classname, _T("RICHEDIT20A")) 
			|| !lstrcmpi(classname, _T("RICHEDIT20W")) 
			|| !lstrcmpi(classname, _T("RICHEDIT32A"))
			|| !lstrcmpi(classname, _T("SYSLISTVIEW32"))
			|| !lstrcmpi(classname, _T("SYSTREEVIEW32"))
			|| !lstrcmpi(classname, _T("EDIT"))
			|| !lstrcmpi(classname, _T("LISTBOX")))
		{
			wascrollbars_init(hwnd);
		}
	}
	UpdateWindow(hwnd);
	return TRUE;
}

BOOL CALLBACK EnumChildProc_ScrollBarUninit(HWND hwnd, LPARAM lParam)
{
	TCHAR classname[256];
	if (g_hbmb)
	{
		GetClassName(hwnd, classname, 255);
		if (GetProp(hwnd, _T("SCBtn")))
			RemoveProp(hwnd, _T("SCBtn"));

		// Special case for RichEdit controls
		if (!lstrcmpi(classname, _T("RICHEDIT20A")) 
			|| !lstrcmpi(classname, _T("RICHEDIT20W")) 
			|| !lstrcmpi(classname, _T("RICHEDIT32A"))
			|| !lstrcmpi(classname, _T("SYSLISTVIEW32"))
			|| !lstrcmpi(classname, _T("SYSTREEVIEW32"))
			|| !lstrcmpi(classname, _T("EDIT"))
			|| !lstrcmpi(classname, _T("LISTBOX")))
		{
			wascrollbars_uninit(hwnd);
		}
		UpdateWindow(hwnd);
	}
	return TRUE;
}

#ifndef LEGACY_PLUGIN
void __declspec(dllexport) skinit(HWND hwndParent, int string_size,
								  TCHAR *variables, stack_t **stacktop,
								  extra_parameters *extra)
#else
void __declspec(dllexport) skinit(HWND hwndParent, int string_size,
								  TCHAR *variables, stack_t **stacktop)
#endif
{
	EXDLL_INIT();

#ifndef LEGACY_PLUGIN
	extra->RegisterPluginCallback(g_hInstance, PluginCallback);
#endif

	g_hwndParent = hwndParent;
	internal_skin(0);
	wascrollbars_initapp();
}

#ifndef LEGACY_PLUGIN
void __declspec(dllexport) setskin(HWND hwndParent, int string_size,
								   TCHAR *variables, stack_t **stacktop,
								   extra_parameters *extra)
#else
void __declspec(dllexport) setskin(HWND hwndParent, int string_size,
								   TCHAR *variables, stack_t **stacktop)
#endif
{
	EXDLL_INIT();

#ifndef LEGACY_PLUGIN
	extra->RegisterPluginCallback(g_hInstance, PluginCallback);
#endif

	g_hwndParent = hwndParent;
	internal_skin(1);
}

void internal_skin(int isUpdating)
{
	TCHAR fnb[MAX_PATH];
	TCHAR fna[MAX_PATH];
	TCHAR temp[MAX_PATH];
	TCHAR compstring[30];
	TCHAR *szLastErr;

	int textcolor = 0x00000000;
	int textscolor = 0x00000000;
	int textdcolor = 0x00808080;
	int setReturn = 0;

	TCHAR tcp[] = _T("/textcolor=");
	TCHAR tscp[] = _T("/selectedtextcolor=");
	TCHAR tdcp[] = _T("/disabledtextcolor=");
	TCHAR bcp[] = _T("/button=");
	TCHAR sbcp[] = _T("/scrollbar=");
	TCHAR srcp[] = _T("/SetReturn");

	// get parmaters
	int valid = 1;

	lstrcpy(fnb, _T(""));
	lstrcpy(fna, _T(""));

	do
	{
		valid = 1;
		lstrcpy(temp, _T(""));
		popstring(temp);

		lstrcpyn(compstring, temp, lstrlen(tdcp)+1);
		if (lstrcmpi(compstring, tdcp) == 0)
		{
			textdcolor = myhtoi(&temp[lstrlen(tdcp)]);
		}
		else
		{
			lstrcpyn(compstring, temp, lstrlen(tscp)+1);
			if (lstrcmpi(compstring, tscp) == 0)
			{
				textscolor = myhtoi(&temp[lstrlen(tscp)]);
			}
			else
			{
				lstrcpyn(compstring, temp, lstrlen(tcp)+1);
				if (lstrcmpi(compstring, tcp) == 0)
				{
					textcolor = myhtoi(&temp[lstrlen(tcp)]);
				}
				else
				{
					lstrcpyn(compstring, temp, lstrlen(bcp)+1);
					if (lstrcmpi(compstring, bcp) == 0)
					{
						lstrcpy(fna, &temp[lstrlen(bcp)]);
					}
					else
					{
						lstrcpyn(compstring, temp, lstrlen(sbcp)+1);
						if (lstrcmpi(compstring, sbcp) == 0)
						{
							lstrcpy(fnb, &temp[lstrlen(sbcp)]);
						}
						else
						{
							lstrcpyn(compstring, temp, lstrlen(srcp)+1);
							if (lstrcmpi(compstring, srcp) == 0)
							{
								setReturn = 1;
							}
							else
							{
								valid = 0;
							}
						}
					}
				}
			}
		}
	}
	while (valid);

	lstrcpyn(compstring, temp, 2);
	if (lstrcmp(compstring, _T("/")) == 0)
	{
		TCHAR szErr[255];
		wsprintf(szErr, _T("SkinnedControls error: Bad parameter %s"), (LPSTR)temp);
		if (setReturn == 0)
			MessageBox(0,szErr,0,MB_ICONEXCLAMATION|MB_OK);
		else
			pushstring(szErr);
		return;
	}

	pushstring(temp);

	textcolor = rgbtobgr(textcolor);
	textscolor = rgbtobgr(textscolor);
	textdcolor = rgbtobgr(textdcolor);

	if (lstrcmp(fnb, _T("")) == 0 && lstrcmp(fna, _T("")) == 0)
	{
		if (setReturn == 0)
			MessageBox(0,_T("SkinnedControls error: Missing parameters /button and /scrollbar"),0,MB_ICONEXCLAMATION|MB_OK);
		else
			pushstring(_T("SkinnedControls error: Missing parameters /button and /scrollbar"));
		return;
	}

	if ((lstrcmp(fnb, _T("")) != 0 && GetFileAttributes(fnb) == 0xFFFFFFFF) || (lstrcmp(fna, _T("")) != 0 && GetFileAttributes(fna) == 0xFFFFFFFF))
	{
		TCHAR szErr[255];
		szLastErr = GetLastErrorStr();
		wsprintf(szErr, _T("SkinnedControls error: %s"), (LPSTR)szLastErr);
		LocalFree((HLOCAL)szLastErr);// Free the buffer.
		if (setReturn == 0)
			MessageBox(0,szErr,0,MB_ICONEXCLAMATION|MB_OK);
		else
			pushstring(szErr);
		return;
	}

	if (isUpdating)
	{
		internal_unskin();
	}

	g_hbma=LoadImage(NULL,fna,IMAGE_BITMAP,0,0,LR_CREATEDIBSECTION|LR_LOADFROMFILE);
	g_hbmb=LoadImage(NULL,fnb,IMAGE_BITMAP,0,0,LR_CREATEDIBSECTION|LR_LOADFROMFILE);

	if (!g_hbmb && !g_hbma) 
	{
		TCHAR szErr[255];
		szLastErr = GetLastErrorStr();
		wsprintf(szErr, _T("SkinnedControls error: %s"), (LPSTR)szLastErr);
		LocalFree((HLOCAL)szLastErr);// Free the buffer.
		if (setReturn == 0)
			MessageBox(0,szErr,0,MB_ICONEXCLAMATION|MB_OK);
		else
			pushstring(szErr);
		return;
	}


	WADlg_init(g_hbmb, g_hbma, textcolor, textscolor, textdcolor);

	if (!isUpdating)
	{
		SetWindowLong(g_hwndParent, GWL_EXSTYLE, WS_EX_CONTROLPARENT);

		oldProc = IntToPtr(SetWindowLongPtr(g_hwndParent, GWLP_WNDPROC, PtrToInt(WndProc)));
		PostMessage(g_hwndParent, WM_USER+0x9, 0, 0);
		ShowWindow(g_hwndParent, SW_SHOW);
	}

	if (setReturn == 1)
		pushstring(_T("success"));
}

#ifndef LEGACY_PLUGIN
void __declspec(dllexport) unskinit(HWND hwndParent, int string_size,
									TCHAR *variables, stack_t **stacktop,
									extra_parameters *extra)
#else
void __declspec(dllexport) unskinit(HWND hwndParent, int string_size,
									TCHAR *variables, stack_t **stacktop)
#endif
{
	internal_unskin();
	InvalidateRect(hwndParent, NULL, TRUE);
}

void internal_unskin()
{
	// Free graphics stuff
	WADlg_close();
	if (g_hbma)
	{
		DeleteObject(g_hbma);
		g_hbma=0;
	}
	if (g_hbmb)
	{
		DeleteObject(g_hbmb);
		g_hbmb=0;
		wascrollbars_uninitapp();
	}
}

// U T I L I T Y   F U N C T I O N S

TCHAR* GetLastErrorStr(void)
{
	LPVOID lpMsgBuf;

	FormatMessage( 
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		GetLastError(),
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
		(LPTSTR) &lpMsgBuf,
		0,
		NULL 
		);

	return (TCHAR*)lpMsgBuf;
}

unsigned int myhtoi(TCHAR *s)
{
	unsigned int v = 0;

	for (;;)
	{
		unsigned int c=*s++;
		if (c >= '0' && c <= '9')
		{
			c -= '0';
		}
		else if (c >= 'A' && c <= 'F')
		{
			c -= 'A';
			c += 10;
		}
		else if (c >= 'a' && c <= 'f')
		{
			c -= 'a';
			c += 10;
		}
		else break;
		v *= 16;
		v += c;
	}
	return v;
}

unsigned int rgbtobgr(unsigned int rgb)
{
	unsigned int bgr;
	unsigned int temp1, temp2;

	temp1 = rgb & 0x000000FF;
	temp2 = rgb & 0x00FF0000;

	temp1 = temp1 * 0x00010000;
	temp2 = temp2 / 0x00010000;

	bgr = rgb & 0x0000FF00;
	bgr += temp1;
	bgr += temp2;

	return bgr;
}