#include <windows.h>

#include "coolsb_detours.h"
#include "coolscroll.h"
#include "detours.h"

#pragma warning(disable:4100)   // Trampolines don't use formal parameters.

DETOUR_TRAMPOLINE(BOOL WINAPI Detour_EnableScrollBar(HWND hwnd, int wSBflags, UINT wArrows), EnableScrollBar);
DETOUR_TRAMPOLINE(BOOL WINAPI Detour_GetScrollInfo	(HWND hwnd, int fnBar, LPSCROLLINFO lpsi), GetScrollInfo);
DETOUR_TRAMPOLINE(int  WINAPI Detour_GetScrollPos	(HWND hwnd, int nBar), GetScrollPos);
DETOUR_TRAMPOLINE(BOOL WINAPI Detour_GetScrollRange	(HWND hwnd, int nBar, LPINT lpMinPos, LPINT lpMaxPos), GetScrollRange);
DETOUR_TRAMPOLINE(int  WINAPI Detour_SetScrollInfo	(HWND hwnd, int fnBar, LPSCROLLINFO lpsi, BOOL fRedraw), SetScrollInfo);
DETOUR_TRAMPOLINE(int  WINAPI Detour_SetScrollPos	(HWND hwnd, int nBar, int nPos, BOOL fRedraw), SetScrollPos);
DETOUR_TRAMPOLINE(int  WINAPI Detour_SetScrollRange	(HWND hwnd, int nBar, int nMinPos, int nMaxPos, BOOL fRedraw), SetScrollRange);
DETOUR_TRAMPOLINE(BOOL WINAPI Detour_ShowScrollBar	(HWND hwnd, int wBar, BOOL fShow), ShowScrollBar);

static BOOL WINAPI Tramp_EnableScrollBar(HWND hwnd, int wSBflags, UINT wArrows)
{
	if(CoolSB_IsCoolScrollEnabled(hwnd))	
		return CoolSB_EnableScrollBar(hwnd, wSBflags, wArrows);
	else
		return Detour_EnableScrollBar(hwnd, wSBflags, wArrows);
}

static BOOL WINAPI Tramp_GetScrollInfo(HWND hwnd, int fnBar, LPSCROLLINFO lpsi)
{
	if(CoolSB_IsCoolScrollEnabled(hwnd))	
		return CoolSB_GetScrollInfo(hwnd, fnBar, lpsi);
	else
		return Detour_GetScrollInfo(hwnd, fnBar, lpsi);
}

static int	 WINAPI Tramp_GetScrollPos(HWND hwnd, int nBar)
{
	if(CoolSB_IsCoolScrollEnabled(hwnd))	
		return CoolSB_GetScrollPos(hwnd, nBar);
	else
		return Detour_GetScrollPos(hwnd, nBar);
}

static BOOL WINAPI Tramp_GetScrollRange(HWND hwnd, int nBar, LPINT lpMinPos, LPINT lpMaxPos)
{
	if(CoolSB_IsCoolScrollEnabled(hwnd))	
		return CoolSB_GetScrollRange(hwnd, nBar, lpMinPos, lpMaxPos);
	else
		return Detour_GetScrollRange(hwnd, nBar, lpMinPos, lpMaxPos);
}

static int	 WINAPI Tramp_SetScrollInfo(HWND hwnd, int fnBar, LPSCROLLINFO lpsi, BOOL fRedraw)
{
	if(CoolSB_IsCoolScrollEnabled(hwnd))	
		return CoolSB_SetScrollInfo(hwnd, fnBar, lpsi, fRedraw);
	else
		return Detour_SetScrollInfo(hwnd, fnBar, lpsi, fRedraw);
}

static int  WINAPI Tramp_SetScrollPos(HWND hwnd, int nBar, int nPos, BOOL fRedraw)
{
	if(CoolSB_IsCoolScrollEnabled(hwnd))	
		return CoolSB_SetScrollPos(hwnd, nBar, nPos, fRedraw);
	else
		return Detour_SetScrollPos(hwnd, nBar, nPos, fRedraw);
}

static int  WINAPI Tramp_SetScrollRange(HWND hwnd, int nBar, int nMinPos, int nMaxPos, BOOL fRedraw)
{
	if(CoolSB_IsCoolScrollEnabled(hwnd))	
		return CoolSB_SetScrollRange(hwnd, nBar, nMinPos, nMaxPos, fRedraw);
	else
		return Detour_SetScrollRange(hwnd, nBar, nMinPos, nMaxPos, fRedraw);
}

static BOOL WINAPI Tramp_ShowScrollBar		(HWND hwnd, int wBar, BOOL fShow)
{
	if(CoolSB_IsCoolScrollEnabled(hwnd))	
		return CoolSB_ShowScrollBar(hwnd, wBar, fShow);
	else
		return Detour_ShowScrollBar(hwnd, wBar, fShow);
}

BOOL WINAPI CoolSB_InitializeApp(void)
{
	DWORD dwVersion = GetVersion();

	// Only available under Windows NT, 2000 and XP
	if(dwVersion < 0x80000000)
	{
		DetourFunctionWithTrampoline((PBYTE)Detour_EnableScrollBar, (PBYTE)Tramp_EnableScrollBar);
		DetourFunctionWithTrampoline((PBYTE)Detour_GetScrollInfo,   (PBYTE)Tramp_GetScrollInfo);
		DetourFunctionWithTrampoline((PBYTE)Detour_GetScrollPos,    (PBYTE)Tramp_GetScrollPos);
		DetourFunctionWithTrampoline((PBYTE)Detour_GetScrollRange,  (PBYTE)Tramp_GetScrollRange);
		DetourFunctionWithTrampoline((PBYTE)Detour_SetScrollInfo,   (PBYTE)Tramp_SetScrollInfo);
		DetourFunctionWithTrampoline((PBYTE)Detour_SetScrollPos,    (PBYTE)Tramp_SetScrollPos);
		DetourFunctionWithTrampoline((PBYTE)Detour_SetScrollRange,  (PBYTE)Tramp_SetScrollRange);
		DetourFunctionWithTrampoline((PBYTE)Detour_ShowScrollBar,   (PBYTE)Tramp_ShowScrollBar);

		// don't actually use this feature within coolsb yet, but we might need it
		CoolSB_SetESBProc(Detour_EnableScrollBar);

		return TRUE;
	}
	else
	{
		return FALSE;
	}	
}

BOOL WINAPI CoolSB_UninitializeApp(void)
{
	DWORD dwVersion = GetVersion();

	// Only available under Windows NT, 2000 and XP
	if(dwVersion < 0x80000000)
	{
		DetourRemove((PBYTE)Detour_EnableScrollBar, (PBYTE)Tramp_EnableScrollBar);
		DetourRemove((PBYTE)Detour_GetScrollInfo,   (PBYTE)Tramp_GetScrollInfo);
		DetourRemove((PBYTE)Detour_GetScrollPos,    (PBYTE)Tramp_GetScrollPos);
		DetourRemove((PBYTE)Detour_GetScrollRange,  (PBYTE)Tramp_GetScrollRange);
		DetourRemove((PBYTE)Detour_SetScrollInfo,   (PBYTE)Tramp_SetScrollInfo);
		DetourRemove((PBYTE)Detour_SetScrollPos,    (PBYTE)Tramp_SetScrollPos);
		DetourRemove((PBYTE)Detour_SetScrollRange,  (PBYTE)Tramp_SetScrollRange);
		DetourRemove((PBYTE)Detour_ShowScrollBar,   (PBYTE)Tramp_ShowScrollBar);

		// don't actually use this feature within coolsb yet, but we might need it
		CoolSB_SetESBProc(EnableScrollBar);

		return TRUE;
	}
	else
	{
		return FALSE;
	}
}
