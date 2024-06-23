/* wa_dlg.h

Modified for use in SkinnedControls by SuperPat
http://members.tripod.com/files_saivert/


Original version can be downloaded at http://www.winamp.com/nsdn
Following is original license (which still applies):
*/
/*
** Copyright (C) 2003 Nullsoft, Inc.
**
** This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held 
** liable for any damages arising from the use of this software. 
**
** Permission is granted to anyone to use this software for any purpose, including commercial applications, and to 
** alter it and redistribute it freely, subject to the following restrictions:
**
**   1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. 
**      If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
**
**   2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
**
**   3. This notice may not be removed or altered from any source distribution.
**
*/

#ifndef _WA_DLG_H_
#define _WA_DLG_H_

/*

dont know where to put this yet :)
genex.bmp has button and scrollbar images, as well as some individual
pixels that describe the colors for the dialog. The button and
scrollbar images should be self explanatory (note that the buttons
have 4 pixel sized edges that are not stretched, and the center is
stretched), and the scrollbars do something similar.
The colors start at (48,0) and run every other pixel. The meaning
of each pixel is:
x=74: scrollbar color
x=78: inverse scrollbar color
x=82: scrollbar dead area color
*/

enum
{
	WADLG_SCROLLBAR_COLOR,
	WADLG_SCROLLBAR_INV_COLOR,
	WADLG_SCROLLBAR_DEADAREA_COLOR,
	WADLG_NUM_COLORS
};

void WADlg_init(HBITMAP, HBITMAP, int, int, int); // call this on init, or on WM_DISPLAYCHANGE
void WADlg_close();
int WADlg_handleDialogMsgs(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam); 


int WADlg_getColor(int idx);

HBITMAP WADlg_getBitmap();

#ifdef WADLG_IMPLEMENT

static HBITMAP wadlg_bitmapb; // button
static HBITMAP wadlg_bitmapsb; // scrollbar

static int wadlg_text_color;
static int wadlg_selected_text_color;
static int wadlg_disabled_text_color;

static int wadlg_colors[WADLG_NUM_COLORS];
static int wadlg_defcolors[WADLG_NUM_COLORS]=
{
	RGB(36,36,60),
	RGB(78,88,110),
	RGB(36,36,60),
};

int WADlg_getColor(int idx)
{
	if (idx < 0 || idx >= WADLG_NUM_COLORS) return 0;
	return wadlg_colors[idx];
}

// Get ScrollBar Bitmap
HBITMAP WADlg_getBitmap()
{
	return wadlg_bitmapsb;
}

void WADlg_init(HBITMAP hbmb, HBITMAP hbma, int color, int selectedcolor, int disabledcolor) // call this on init, or on WM_DISPLAYCHANGE
{
	if (wadlg_bitmapsb) DeleteObject(wadlg_bitmapsb);
	wadlg_bitmapsb = hbmb;


	if (wadlg_bitmapsb)
	{
		HDC tmpDC=CreateCompatibleDC(NULL);
		HGDIOBJ o=SelectObject(tmpDC,(HGDIOBJ)wadlg_bitmapsb);
		int x;
		int defbgcol=GetPixel(tmpDC, 137, 33);
		for (x = 0; x < WADLG_NUM_COLORS; x ++)
		{
			int a=GetPixel(tmpDC, 137, 1+x*2);
			if (a == CLR_INVALID || a == RGB(0,198,255) || a == defbgcol) 
			{
				a=wadlg_defcolors[x];
			}
			wadlg_colors[x]=a;
		}

		SelectObject(tmpDC,o);
		DeleteDC(tmpDC);
	}

	if (wadlg_bitmapb) DeleteObject(wadlg_bitmapb);
	wadlg_bitmapb = hbma;  
	wadlg_text_color = color;
	wadlg_selected_text_color = selectedcolor;
	wadlg_disabled_text_color = disabledcolor;
}

void WADlg_close()
{
	if (wadlg_bitmapsb) DeleteObject(wadlg_bitmapsb);
	wadlg_bitmapsb=0;
	if (wadlg_bitmapb) DeleteObject(wadlg_bitmapb);
	wadlg_bitmapb=0;
}


int WADlg_handleDialogMsgs(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	// Skin a BUTTON
	if (uMsg == WM_DRAWITEM)
	{
		DRAWITEMSTRUCT *di = (DRAWITEMSTRUCT *)lParam;
		if (di->CtlType == ODT_BUTTON)
		{
			HDC hdc;
			int yoffs;
			int with;
			int height;
			BITMAP bm; 
			TCHAR wt[256];
			RECT r;

			GetDlgItemText(hwndDlg,(int)wParam,wt,sizeof(wt));

			// If window (control) has been subclassed, we skip it.
			// This was added to handle InstallOptions pages which have one or more Link controls.
			// InstallOptions.dll do it's own owner-drawing on such controls.
			if( GetWindowLongPtr(di->hwndItem, GWLP_WNDPROC) != GetClassLongPtr(di->hwndItem, GCLP_WNDPROC) )
			{
				// this adds in an additional handling on the controls
				// which allows some [...] buttons on InstallOptions
				// pages to be correctly skinned
				if (!GetProp(di->hwndItem, _T("SCBtn")) )
				{
					return FALSE;
				}
			}

			hdc = CreateCompatibleDC(di->hDC);
			SelectObject(hdc,wadlg_bitmapb);

			r=di->rcItem;
			SetStretchBltMode(di->hDC,COLORONCOLOR);

			GetObject(wadlg_bitmapb, sizeof(BITMAP), &bm);
			with = bm.bmWidth;
			height = bm.bmHeight /3;
			if (di->itemState & ODS_SELECTED)
				yoffs = height;
			else if (di->itemState & ODS_DISABLED)
				yoffs = height*2;
			else 
				yoffs = 0;

			// top left
			BitBlt(di->hDC,r.left,r.top,4,4,hdc,0,yoffs,SRCCOPY);
			// top center
			StretchBlt(di->hDC,r.left+4,r.top,r.right-r.left-4-4,4,hdc,4,yoffs,with-4-4,4,SRCCOPY); 
			// top right
			BitBlt(di->hDC,r.right-4,r.top,4,4,hdc,with-4,yoffs,SRCCOPY); 
			// left edge
			StretchBlt(di->hDC,r.left,r.top+4,4,r.bottom-r.top-4-4,hdc,0,4+yoffs,4,height-4-4,SRCCOPY);	
			// right edge
			StretchBlt(di->hDC,r.right-4,r.top+4,4,r.bottom-r.top-4-4,hdc,with-4,4+yoffs,4,height-4-4,SRCCOPY); 
			// center
			StretchBlt(di->hDC,r.left+4,r.top+4,r.right-r.left-4-4,r.bottom-r.top-4-4,hdc,4,4+yoffs,with-4-4,height-4-4,SRCCOPY);
			// bottom left
			BitBlt(di->hDC,r.left,r.bottom-4,4,4,hdc,0,height-4+yoffs,SRCCOPY); 
			// bottom center
			StretchBlt(di->hDC,r.left+4,r.bottom-4,r.right-r.left-4-4,4,hdc,4,height-4+yoffs,with-4-4,4,SRCCOPY); 
			// bottom right
			BitBlt(di->hDC,r.right-4,r.bottom-4,4,4,hdc,with-4,height-4+yoffs,SRCCOPY); 

			// draw text
			SetBkMode(di->hDC,TRANSPARENT);

			if (di->itemState & ODS_SELECTED)
				SetTextColor(di->hDC, wadlg_selected_text_color);
			else if (di->itemState & ODS_DISABLED)
				SetTextColor(di->hDC, wadlg_disabled_text_color);
			else
				SetTextColor(di->hDC, wadlg_text_color);

			if (di->itemState & ODS_SELECTED) {r.left+=2; r.top+=2;}
			DrawText(di->hDC,wt,-1,&r,DT_VCENTER|DT_SINGLELINE|DT_CENTER);

			DeleteDC(hdc);
		}
	}
	return 0;
}
#endif

#endif//_WA_DLG_H_