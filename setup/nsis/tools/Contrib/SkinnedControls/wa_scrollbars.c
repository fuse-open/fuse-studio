/*
wa_scrollbars.c
Function for skinning scrollbars and make them follow Winamp's theme.
Taken from Safai Ma's Album List plugin (UserInterface.*)
Separated out in it's own module by Saivert
*/
#include <windows.h>
#include <commctrl.h>
#include "coolsb/coolscroll.h"
#include "coolsb/coolsb_detours.h"

extern HINSTANCE g_hInstance;

#include "wa_dlg.h"

/* wascrollbars_initapp
Call on plug-in init
returns 1 if scrollbars can be skinned
*/
int wascrollbars_initapp(void) 
{
	// get windows version
	OSVERSIONINFO osi;
	BOOL bWinNT = TRUE;

	osi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
	GetVersionEx(&osi);

	//bWin9x = (osi.dwPlatformId == VER_PLATFORM_WIN32_WINDOWS);
	bWinNT = (osi.dwPlatformId == VER_PLATFORM_WIN32_NT);

	// initialize CoolSB
	if (bWinNT) {
		CoolSB_InitializeApp();
		return 1;
	}
	return 0;
}

void wascrollbars_uninitapp(void) 
{
	CoolSB_UninitializeApp();
}

/* wascrollbars_init
Call on window WM_CREATE or WM_INITDIALOG
returns 1 if scrollbars can be skinned
*/
int wascrollbars_init(HWND hwnd) 
{
	// get windows version
	OSVERSIONINFO osi;
	BOOL bWinNT = TRUE;

	osi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
	GetVersionEx(&osi);

	//bWin9x = (osi.dwPlatformId == VER_PLATFORM_WIN32_WINDOWS);
	bWinNT = (osi.dwPlatformId == VER_PLATFORM_WIN32_NT);

	// configure the custom scroll bars
	if (bWinNT)
	{
		InitializeCoolSB(hwnd);
		CoolSB_SetSize(hwnd, SB_BOTH, 17, 17);
		return 1;
	}
	return 0;
}

void wascrollbars_uninit(HWND hwnd) 
{
	UninitializeCoolSB(hwnd);
}

typedef struct 
{
	int x, y;
	int width, height;
} CustomDrawTable;

CustomDrawTable cdt_horz_normal[] = 
{
	{  0, 17, 17, 17 },	//left arrow  NORMAL
	{ 17, 17, 17, 17 }, //right arrow NORMAL
	{ WADLG_SCROLLBAR_COLOR, WADLG_SCROLLBAR_COLOR }, //page left   NORMAL
	{ WADLG_SCROLLBAR_COLOR, WADLG_SCROLLBAR_COLOR }, //page left   NORMAL

	{ -1, -1, -1, -1 },	//padding

	{ 102,  0, 4, 4 }, //horz thumb (top,left)
	{ 102,  4, 4, 9 }, //horz thumb (middle,left)
	{ 102, 13, 4, 4 }, //horz thumb (bottom,left)

	{ 106,  0, 26, 4 }, //horz thumb (top,middle)
	{ 106,  4,  9, 9 }, //horz thumb (middle,middle,left)
	{ 123,  4,  9, 9 }, //horz thumb (middle,middle,right)
	{ 106, 13, 26, 4 }, //horz thumb (bottom,middle)

	{ 132,  0, 4, 4 }, //horz thumb (top,right)
	{ 132,  4, 4, 9 }, //horz thumb (middle,right)
	{ 132, 13, 4, 4 }, //horz thumb (bottom,right)

	{ 115,  4, 8, 9 }, //horz thumb (middle,middle,middle)
};

CustomDrawTable cdt_horz_active[] = 
{
	{ 34, 17, 17, 17 }, //left arrow  ACTIVE
	{ 51, 17, 17, 17 }, //right arrow ACTIVE
	{ WADLG_SCROLLBAR_INV_COLOR, WADLG_SCROLLBAR_INV_COLOR }, //page left   NORMAL
	{ WADLG_SCROLLBAR_INV_COLOR, WADLG_SCROLLBAR_INV_COLOR }, //page left   NORMAL

	{ -1, -1, -1, -1 },	//padding

	{ 102, 17, 4, 4 }, //horz thumb (top,left)
	{ 102, 21, 4, 9 }, //horz thumb (middle,left)
	{ 102, 30, 4, 4 }, //horz thumb (bottom,left)

	{ 106, 17, 26, 4 }, //horz thumb (top,middle)
	{ 106, 21,  9, 9 }, //horz thumb (middle,middle,left)
	{ 123, 21,  9, 9 }, //horz thumb (middle,middle,right)
	{ 106, 30, 26, 4 }, //horz thumb (bottom,middle)

	{ 132, 17, 4, 4 }, //horz thumb (top,right)
	{ 132, 21, 4, 9 }, //horz thumb (middle,right)
	{ 132, 30, 4, 4 }, //horz thumb (bottom,right)

	{ 115, 21, 8, 9 }, //horz thumb (middle,middle,middle)
};

CustomDrawTable cdt_vert_normal[] = 
{
	{ 0,  0, 17, 17 }, //up arrow   NORMAL
	{ 17, 0, 17, 17 }, //down arrow NORMAL
	{ WADLG_SCROLLBAR_COLOR, WADLG_SCROLLBAR_COLOR }, //page left   NORMAL
	{ WADLG_SCROLLBAR_COLOR, WADLG_SCROLLBAR_COLOR }, //page left   NORMAL

	{ -1, -1, -1, -1 },	//padding

	{ 68, 0, 4, 4 }, //vert thumb (left)
	{ 72, 0, 9, 4 }, //vert thumb (middle)
	{ 81, 0, 4, 4 }, //vert thumb (right)

	{ 68,  4,  4, 26 }, //vert thumb (middle,left)
	{ 72,  4, 11,  9 }, //vert thumb (middle,middle,top)
	{ 72, 21, 11,  9 }, //vert thumb (middle,middle,bottom)
	{ 81,  4,  4, 26 }, //vert thumb (middle,right)

	{ 68, 30, 4, 4 }, //vert thumb (bottom,left)
	{ 72, 30, 9, 4 }, //vert thumb (bottom,middle)
	{ 81, 30, 4, 4 }, //vert thumb (bottom,right)

	{ 72, 13, 9, 8 }, //vert thumb (middle,middle,middle)
};

CustomDrawTable cdt_vert_active[] = 
{
	{ 34, 0, 17, 17 }, //up arrow   ACTIVE
	{ 51, 0, 17, 17 }, //down arrow ACTIVE
	{ WADLG_SCROLLBAR_INV_COLOR, WADLG_SCROLLBAR_INV_COLOR }, //page left   NORMAL
	{ WADLG_SCROLLBAR_INV_COLOR, WADLG_SCROLLBAR_INV_COLOR }, //page left   NORMAL

	{ -1, -1, -1, -1 },	//padding

	{ 85, 0, 4, 4 }, //vert thumb (left)
	{ 89, 0, 9, 4 }, //vert thumb (middle)
	{ 98, 0, 4, 4 }, //vert thumb (right)

	{ 85,  4,  4, 26 }, //vert thumb (middle,left)
	{ 89,  4, 11,  9 }, //vert thumb (middle,middle,top)
	{ 89, 21, 11,  9 }, //vert thumb (middle,middle,bottom)
	{ 98,  4,  4, 26 }, //vert thumb (middle,right)

	{ 85, 30, 4, 4 }, //vert thumb (bottom,left)
	{ 89, 30, 9, 4 }, //vert thumb (bottom,middle)
	{ 98, 30, 4, 4 }, //vert thumb (bottom,right)

	{ 89, 13, 9, 8 }, //vert thumb (middle,middle,middle)
};

BOOL wascrollbars_handleMessages(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam, LRESULT* pResult)
{
	NMHDR* pnmh;
	NMCSBCUSTOMDRAW *pCSCD;
	RECT *rc;
	CustomDrawTable *cdt;

	pnmh = (NMHDR*)lParam;
	pCSCD = (NMCSBCUSTOMDRAW *)pnmh;

	if (!(uMsg == WM_NOTIFY && pnmh->code == NM_COOLSB_CUSTOMDRAW)) return FALSE;

	// Take the default processing unless we set this to something else below.
	*pResult = 0;

	if (pCSCD->dwDrawStage == CDDS_PREPAINT)
	{
		*pResult = CDRF_SKIPDEFAULT;
		return TRUE;
	}

	//the sizing gripper in the bottom-right corner
	if (pCSCD->nBar == SB_BOTH)	
	{
		HBRUSH hbr = CreateSolidBrush(WADlg_getColor(WADLG_SCROLLBAR_DEADAREA_COLOR));
		FillRect(pCSCD->hdc, &pCSCD->rect, hbr);
		DeleteObject(hbr);

		*pResult = CDRF_SKIPDEFAULT;
		return TRUE;
	}
	else if (pCSCD->nBar == SB_HORZ)
	{
		rc = &pCSCD->rect;

		if (pCSCD->uState == CDIS_SELECTED) 
			cdt = &cdt_horz_active[pCSCD->uItem];
		else				   
			cdt = &cdt_horz_normal[pCSCD->uItem];

		if ((pCSCD->uItem == HTSCROLL_PAGELEFT) ||
			(pCSCD->uItem == HTSCROLL_PAGERIGHT))
		{
			HBRUSH hbr;
			COLORREF color;

			color = WADlg_getColor(cdt->x);

			hbr = CreateSolidBrush(color);
			FillRect(pCSCD->hdc, rc, hbr);
			DeleteObject(hbr);

			*pResult = CDRF_SKIPDEFAULT;
			return TRUE;
		}
		if (pCSCD->uItem == HTSCROLL_THUMB)
		{
			// get skin bitmap
			HDC hdcSkin;
			HBITMAP oBitmap;

			hdcSkin = CreateCompatibleDC(pCSCD->hdc);
			oBitmap = (HBITMAP)SelectObject(hdcSkin, WADlg_getBitmap());
			SetStretchBltMode(pCSCD->hdc, COLORONCOLOR);

			// top left
			StretchBlt(pCSCD->hdc, rc->left, rc->top, 4, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// middle left
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left, rc->top+4, 4, rc->bottom-rc->top-8, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// bottom left
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left, rc->bottom-4, 4, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);

			// top middle
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left+4, rc->top, rc->right-rc->left-8, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// middle middle left
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left+4, rc->top+4, (rc->right-rc->left-8)/2+2, rc->bottom-rc->top-7, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// middle middle right
			cdt++;
			StretchBlt(pCSCD->hdc, rc->right-(rc->right-rc->left-8)/2-3, rc->top+4, (rc->right-rc->left-8)/2, rc->bottom-rc->top-7, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// bottom middle
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left+4, rc->bottom-cdt->height, rc->right-rc->left-8, cdt->height, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);

			// top right
			cdt++;
			StretchBlt(pCSCD->hdc, rc->right-4, rc->top, 4, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// middle right
			cdt++;
			StretchBlt(pCSCD->hdc, rc->right-4, rc->top+4, 4, rc->bottom-rc->top-8, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// bottom right
			cdt++;
			StretchBlt(pCSCD->hdc, rc->right-4, rc->bottom-4, 4, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);

			// middle middle middle
			cdt++;
			if ((rc->right-rc->left-8) > cdt->width)
			{
				StretchBlt(pCSCD->hdc, rc->left+(rc->right-rc->left)/2-4, rc->top+4, cdt->width, cdt->height, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			}

			// free dc
			SelectObject(hdcSkin, oBitmap);
			DeleteDC(hdcSkin);

			*pResult = CDRF_SKIPDEFAULT;
			return TRUE;
		}
	}
	else if (pCSCD->nBar == SB_VERT)
	{
		rc = &pCSCD->rect;

		if (pCSCD->uState == CDIS_SELECTED)  
			cdt = &cdt_vert_active[pCSCD->uItem];
		else				    
			cdt = &cdt_vert_normal[pCSCD->uItem];

		if ((pCSCD->uItem == HTSCROLL_PAGEGUP) ||
			(pCSCD->uItem == HTSCROLL_PAGEGDOWN))
		{
			HBRUSH hbr;
			COLORREF color;

			color = WADlg_getColor(cdt->x);

			hbr = CreateSolidBrush(color);
			FillRect(pCSCD->hdc, rc, hbr);
			DeleteObject(hbr);

			*pResult = CDRF_SKIPDEFAULT;
			return TRUE;
		}
		if (pCSCD->uItem == HTSCROLL_THUMB)
		{
			// get skin bitmap
			HDC hdcSkin = CreateCompatibleDC(pCSCD->hdc);
			HBITMAP oBitmap = (HBITMAP)SelectObject(hdcSkin, WADlg_getBitmap());
			SetStretchBltMode(pCSCD->hdc, COLORONCOLOR);

			// top left
			StretchBlt(pCSCD->hdc, rc->left, rc->top, 4, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// top middle
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left+4, rc->top, rc->right-rc->left-8, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// top right
			cdt++;
			StretchBlt(pCSCD->hdc, rc->right-4, rc->top, 4, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);

			// middle left
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left, rc->top+4, 4, rc->bottom-rc->top-8, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// middle middle top
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left+4, rc->top+4, rc->right-rc->left-7, (rc->bottom-rc->top-8)/2+2, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// middle middle bottom
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left+4, rc->bottom-(rc->bottom-rc->top-8)/2-4, rc->right-rc->left-7, (rc->bottom-rc->top-8)/2, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// middle right
			cdt++;
			StretchBlt(pCSCD->hdc, rc->right-cdt->width, rc->top+4, cdt->width, rc->bottom-rc->top-8, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);

			// bottom left
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left, rc->bottom-4, 4, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// bottom middle
			cdt++;
			StretchBlt(pCSCD->hdc, rc->left+4, rc->bottom-4, rc->right-rc->left-8, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// bottom right
			cdt++;
			StretchBlt(pCSCD->hdc, rc->right-4, rc->bottom-4, 4, 4, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			// middle middle middle
			cdt++;
			if ((rc->bottom-rc->top-8) > cdt->height)
			{
				StretchBlt(pCSCD->hdc, rc->left+4, rc->top+(rc->bottom-rc->top)/2-4, cdt->width, cdt->height, hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);
			}

			// free dc
			SelectObject(hdcSkin, oBitmap);
			DeleteDC(hdcSkin);

			*pResult = CDRF_SKIPDEFAULT;
			return TRUE;
		}
	}
	else
	{
		*pResult = CDRF_DODEFAULT;
		return TRUE;
	}

	{
		HDC hdcSkin;
		HBITMAP oBitmap;

		// get skin bitmap
		hdcSkin = CreateCompatibleDC(pCSCD->hdc);
		oBitmap = (HBITMAP)SelectObject(hdcSkin, WADlg_getBitmap());
		SetStretchBltMode(pCSCD->hdc, COLORONCOLOR);

		//normal bitmaps, use same code for HORZ and VERT
		StretchBlt(pCSCD->hdc, rc->left, rc->top, rc->right-rc->left, rc->bottom-rc->top,
			hdcSkin, cdt->x, cdt->y, cdt->width, cdt->height, SRCCOPY);

		// free dc
		SelectObject(hdcSkin, oBitmap);
		DeleteDC(hdcSkin);
	}
	*pResult = CDRF_SKIPDEFAULT;
	return TRUE;
}