/***************************************************
* FILE NAME: execDos.c
*
* PURPOSE:
*   NSIS plugin for DOS (console) applications.
*   Creates child process with redirected I/O.
*   Catches application's output and can put it to the
*   log file. Returns process exit code or error code.
*
* CHANGE HISTORY
*
* $LOG$
*
* Author              Version  Date         Modifications
* Takhir Bedertdinov           May 31 2004  Original
*   --//--                     Oct 31 2004  Log output
*   --//--                     Feb 10 2005  /TIMEOUT option
*   --//--                     Jul 23 2005  /ASYNC option
*   --//--                     Jul 25 2005  multithreading
*   --//--                     Mar 10 2006  perf. improved
*   --//--                     Jul 25 2006  to stack option
*   --//--                     Dec 12 2006  detailed option
* Stuart Welch                 Jul 27 2007  func options
*   --//--                                  isdone func
* Takhir Bedertdinov           Jul 28 2007  to window option
* Stuart Welch                 Jan 18 2011  Unicode build
*   --//--                                  NSIS plugin API update (no /NOUNLOAD)
*   --//--                                  /DISABLEFSR option for WOW64
*   --//--            1.0.1.2  Apr 13 2014  x64 build
*   --//--                                  version info resource
*   --//--            1.0.1.3  Dec 11 2014  incorrect Unicode GlobalAlloc size
*                                           STDIN Unicode ANSI conversion
*
* Takhir Bedertdinov, Moscow, Russia, ineum@narod.ru
* Stuart 'Afrow UK' Welch, afrowuk@afrowsoft.co.uk
**************************************************/

#include <windows.h>
#include <fcntl.h>
#include <stdio.h>
#include <io.h>
#include <sys\stat.h>
#include "COMMCTRL.H"
#include "pluginapi.h"

#define NSISFUNC(name) void __declspec(dllexport) name(HWND hWndParent, int string_size, TCHAR* variables, stack_t** stacktop, extra_parameters* extra)

enum ERROR_CODES {
  ERR_DUPHANDLE = -16,
  ERR_CREATEPIPE,
  ERR_CREATEPROC,
  ERR_CLOSEHANDLE,
  ERR_WRITEPIPE,
  ERR_GETEXITCODE,
  ERR_LOGOPEN,
  ERR_LOGWRITE,
  ERR_TERMINATED,
  ERR_CREATETHREAD
};

enum OUT_OPTIONS {
  OUT_FILE,
  OUT_STACK,
  OUT_WINDOW,
  OUT_FUNCTION
};

#define SLEEP_MS 10
#define T_O        TEXT("/TIMEOUT")
#define N_W        TEXT("/ASYNC")
#define TOSTACK    TEXT("/TOSTACK")
#define DETAILED   TEXT("/DETAILED")
#define TOWINDOW   TEXT("/TOWINDOW")
#define TOFUNC     TEXT("/TOFUNC")
#define DISABLEFSR TEXT("/DISABLEFSR")
#define ENDFUNC    TEXT("/ENDFUNC")

typedef struct _threadParams {
  TCHAR *dosExec;
  TCHAR *pipeWrite;
  DWORD target;
  BOOL disableFsR;
  TCHAR *logFile;
  DWORD timeout;
  HWND hOut;
  LONG lFuncAddress;
  LONG lEndFuncAddress;
} threadParams, *pthreadParams;

typedef BOOL (WINAPI* PWow64EnableWow64FsRedirection)(BOOL);

extra_parameters* ep;
HANDLE g_hInstance;

/*****************************************************
 * FUNCTION NAME: my_strchr()
 * PURPOSE: 
 *   libc/msvcrt replacements
 *****************************************************/
TCHAR *my_strchr(TCHAR *s, TCHAR c)
{
  while(*s != 0)
  {
    if(*s == c)
      return s;
    s++;
  }
  return NULL;
}

/*****************************************************
* FUNCTION NAME: redirect()
* PURPOSE: 
*   Creates child process with redirected IO
* SPECIAL CONSIDERATIONS:
*
*****************************************************/
DWORD __stdcall redirect(void *pp)
{
  HANDLE hOutputReadTmp,hOutputRead,hOutputWrite;
  HANDLE hInputWriteTmp,hInputRead,hInputWrite;
  HANDLE hErrorWrite;
  PROCESS_INFORMATION pi;
  STARTUPINFO *psi = (STARTUPINFO*)GlobalAlloc(GPTR, sizeof(STARTUPINFO));
  SECURITY_ATTRIBUTES sa;
  HANDLE f = INVALID_HANDLE_VALUE;
  DWORD rslt = 0,
    waitTime = 0,
    dwRead = 0;
  char b[256];
  TCHAR *szStack = NULL;
  unsigned int iStack = 0, iPipeWrite;
  pthreadParams ptp = (pthreadParams)pp;
  LVITEM lvItem;
  lvItem.mask = LVIF_TEXT;
  lvItem.iItem = 0;
  lvItem.iSubItem = 0;
  lvItem.pszText = NULL;
  lvItem.cchTextMax = 0;

	iPipeWrite = lstrlen(ptp->pipeWrite);
  
  /* creates child process with redirected IO */
  sa.nLength= sizeof(SECURITY_ATTRIBUTES);
  sa.lpSecurityDescriptor = NULL;
  sa.bInheritHandle = TRUE;
  if(ptp->target != OUT_FILE)
  {
    szStack = (TCHAR*)GlobalAlloc(GPTR, sizeof(TCHAR) * (g_stringsize + 2));
    lvItem.pszText = szStack;
  }

  /* disable WOW64 file system redirection */
  if (ptp->disableFsR)
  {
    HINSTANCE hDll = GetModuleHandle(TEXT("kernel32.dll"));
    PWow64EnableWow64FsRedirection Wow64EnableWow64FsRedirection = (PWow64EnableWow64FsRedirection)GetProcAddress(hDll, "Wow64EnableWow64FsRedirection");
    if (Wow64EnableWow64FsRedirection != NULL)
      Wow64EnableWow64FsRedirection(FALSE);
  }
  
  while(1)
  {
    /* creating pipes for child process with redirected IO */
    if(!CreatePipe(&hOutputReadTmp,&hOutputWrite,&sa,0) ||
      !CreatePipe(&hInputRead,&hInputWriteTmp,&sa,0))
    { rslt = ERR_CREATEPIPE; break; }
    if(!DuplicateHandle(GetCurrentProcess(), hOutputWrite, GetCurrentProcess(),
      
      /* duplicates handles and makes them inheritable */
      &hErrorWrite, 0, TRUE, DUPLICATE_SAME_ACCESS) ||
      !DuplicateHandle(GetCurrentProcess(), hOutputReadTmp, GetCurrentProcess(),
      &hOutputRead, 0, FALSE, DUPLICATE_SAME_ACCESS) ||
      !DuplicateHandle(GetCurrentProcess(), hInputWriteTmp, GetCurrentProcess(),
      &hInputWrite, 0, FALSE, DUPLICATE_SAME_ACCESS))
    { rslt = ERR_DUPHANDLE; break; }
    
    /* run process and close unnecessary handles */
    psi->cb = sizeof(STARTUPINFO);
    psi->dwFlags = STARTF_USESTDHANDLES|STARTF_USESHOWWINDOW;
    psi->hStdOutput = hOutputWrite;
    psi->hStdInput  = hInputRead;
    psi->hStdError  = hErrorWrite;
    psi->wShowWindow = SW_HIDE; /* SW_NORMAL - for tests */
    if(!CreateProcess(NULL, ptp->dosExec, NULL, NULL, TRUE, 0, NULL, NULL, psi, &pi))
    { rslt = ERR_CREATEPROC; break; }
    
    /* closes temporary and unnesessery handles */
    if(!CloseHandle(hOutputReadTmp) ||
      !CloseHandle(hInputWriteTmp) ||
      !CloseHandle(pi.hThread) ||
      !CloseHandle(hOutputWrite) ||
      !CloseHandle(hInputRead ) ||
      !CloseHandle(hErrorWrite))
    { rslt = ERR_CLOSEHANDLE; break; }
    
    /* write all to pipe - on tests it caches input strings correctly */
    if(iPipeWrite > 0)
    {
#ifdef UNICODE
			int cbConverted = WideCharToMultiByte(CP_ACP, WC_COMPOSITECHECK, ptp->pipeWrite, iPipeWrite, NULL, 0, NULL, NULL);
			rslt = -1;
			if (cbConverted > 0)
			{
				PCHAR pszConverted = (PCHAR)GlobalAlloc(GPTR, cbConverted + 1);
				if (pszConverted)
				{
					if (WideCharToMultiByte(CP_ACP, WC_COMPOSITECHECK, ptp->pipeWrite, iPipeWrite, pszConverted, cbConverted, NULL, NULL) > 0)
						WriteFile(hInputWrite, pszConverted, cbConverted, &rslt, NULL);
					GlobalFree(pszConverted);
				}
			}
      if(
#else
      if(WriteFile(hInputWrite, ptp->pipeWrite, iPipeWrite, &rslt, NULL) == 0 ||
#endif
        rslt != (unsigned)iPipeWrite)
      { rslt = ERR_WRITEPIPE; break; }
      FlushFileBuffers(hInputWrite);
    }
    
    /* open log file if name is correct */
    if(ptp->target == OUT_FILE &&
      *(ptp->logFile) != 0 &&
      (f = CreateFile(ptp->logFile, GENERIC_WRITE, FILE_SHARE_READ, NULL,
      CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL)) == INVALID_HANDLE_VALUE)
    { rslt = ERR_LOGOPEN; break; }
    
    /* OK, this is main execution loop */
    while(waitTime <= ptp->timeout &&
      rslt != ERR_LOGWRITE &&
      PeekNamedPipe(hOutputRead, 0, 0, 0, &dwRead, NULL) &&
      GetExitCodeProcess(pi.hProcess, &rslt))
    {
      if(dwRead > 0)
      {
        if(ReadFile(hOutputRead, b, dwRead > sizeof(b) ? sizeof(b) : dwRead, &rslt, NULL) &&
          rslt > 0)
        {
          if(ptp->target == OUT_FILE)
          {
            if(f != INVALID_HANDLE_VALUE &&
              (WriteFile(f, b, rslt, &dwRead, NULL) == 0 || dwRead != rslt))
            {
              rslt = ERR_LOGWRITE;
              break;
            }
          }
          else
          {
            dwRead = 0;
            while(dwRead < rslt)
            {
              if(b[dwRead] == TEXT('\n'))
              {
                /* supress empty lines */
                if(iStack > 0)
                {
                  szStack[iStack] = 0;
                  if(ptp->target == OUT_STACK)
                    pushstring(szStack);
                  else if(ptp->target == OUT_FUNCTION)
                  {
                    pushstring(szStack);
                    ep->ExecuteCodeSegment(ptp->lFuncAddress-1, 0);
                  }
                  else if(lstrcmpi(ptp->logFile, TEXT("SysListView32")) == 0)
                  {
                    lvItem.iItem = (int)SendMessage(ptp->hOut, LVM_GETITEMCOUNT, 0, 0);
                    lvItem.iItem = (int)SendMessage(ptp->hOut, LVM_INSERTITEM, 0,  (LPARAM)&lvItem);
                    SendMessage(ptp->hOut, LVM_ENSUREVISIBLE, lvItem.iItem,  0);
                  }
                  else if(lstrcmpi(ptp->logFile, TEXT("Edit")) == 0 || lstrcmpi(ptp->logFile, TEXT("RichEdit20A")) == 0)
                  {
                    lstrcat(szStack, TEXT("\r\n"));
                    lvItem.iItem = (int)SendMessage(ptp->hOut, WM_GETTEXTLENGTH, 0, 0);
                    SendMessage(ptp->hOut, EM_SETSEL, lvItem.iItem, lvItem.iItem);
                    SendMessage(ptp->hOut, EM_REPLACESEL, 0,  (LPARAM)szStack);
                  }
                  else if(lstrcmpi(ptp->logFile, TEXT("ListBox")) == 0)
                  {
                    lvItem.iItem = (int)SendMessage(ptp->hOut, LB_GETCOUNT, 0, 0);
                    lvItem.iItem = (int)SendMessage(ptp->hOut, LB_INSERTSTRING, lvItem.iItem,  (LPARAM)szStack);
                    //SendMessage(ptp->hOut, LVM_ENSUREVISIBLE, lvItem.iItem,  0);
                  }
                  iStack = 0;
                }
              }
              else if(b[dwRead] != TEXT('\r') && iStack < (g_stringsize-1))
              {
                szStack[iStack++] = b[dwRead];
              }
              dwRead++;
            }
          }
        }
        if(f != INVALID_HANDLE_VALUE)
          FlushFileBuffers(f);
      }
      else if(rslt == STILL_ACTIVE)
      {
        Sleep(SLEEP_MS);
        waitTime += SLEEP_MS;
      }
      else break;
    }
    if(iStack > 0)
    {
      szStack[iStack] = 0;
      pushstring(szStack);
    }
    if(f != INVALID_HANDLE_VALUE) CloseHandle(f);
    
    if(GetExitCodeProcess(pi.hProcess, &rslt))
    {
      if(rslt == STILL_ACTIVE)
      {
        TerminateProcess(pi.hProcess, -1);
        rslt = ERR_TERMINATED;
      }
    }
    else rslt = ERR_GETEXITCODE;
    CloseHandle(pi.hProcess);
    CloseHandle(hOutputRead);
    CloseHandle(hInputWrite);
    break;
  }
  
  GlobalFree(psi);
  GlobalFree(ptp->dosExec);
  GlobalFree(ptp->pipeWrite);
  GlobalFree(ptp->logFile);
  GlobalFree(ptp);
  if(szStack) GlobalFree(szStack);

  if (ptp->lEndFuncAddress != -1)
    ep->ExecuteCodeSegment(ptp->lEndFuncAddress - 1, 0);

  return (DWORD)rslt;
}

/*****************************************************
 * FUNCTION NAME: PluginCallback()
 * PURPOSE: 
 *   for NSIS plugin API
 *****************************************************/
static UINT_PTR PluginCallback(enum NSPIM msg)
{
  return 0;
}

/*****************************************************
 * FUNCTION NAME: wait()
 * PURPOSE: 
 *   waits for thread exit and closes handle
 * SPECIAL CONSIDERATIONS:
 *   tested with my consApp.exe
 *****************************************************/
NSISFUNC(wait)
{
  DWORD rslt = ERR_CREATETHREAD;
  TCHAR exitCode[16];
  HANDLE hThread;

  popstring(exitCode);
  hThread = (HANDLE)myatou(exitCode);
/* push to stack application' exit code or -1 or "still_running" */
  if(hThread != NULL)
  {
    WaitForSingleObject(hThread, INFINITE);
    GetExitCodeThread(hThread, &rslt);
    CloseHandle(hThread);
    hThread = NULL;
  }
  wsprintf(exitCode, TEXT("%d"), rslt);
  pushstring(exitCode);
}

/*****************************************************
 * FUNCTION NAME: isdone()
 * PURPOSE: 
 *   checks if the thread has completed
 * SPECIAL CONSIDERATIONS:
 *   
 *****************************************************/
NSISFUNC(isdone)
{
  TCHAR s[16];
  HANDLE hThread;

// get thread handle from stack
  popstring(s);
  hThread = (HANDLE)myatou(s);
// is it running? 1 == yes, 0 == exited, -1 == error 
  if(hThread != NULL)
    pushstring(WaitForSingleObject(hThread, 0) == WAIT_TIMEOUT ? TEXT("0") : TEXT("1"));
  else
    pushstring(TEXT("-1"));
}

/*****************************************************
 * FUNCTION NAME: exec()
 * PURPOSE: 
 *   C dll entry point for hidden DOS process execution
 * SPECIAL CONSIDERATIONS:
 *
 *****************************************************/
NSISFUNC(exec)
{
  DWORD dwThreadId;
  HANDLE hThread;
  BOOL fWait = TRUE;
  pthreadParams ptp = (pthreadParams)GlobalAlloc(GPTR, sizeof(threadParams));
  TCHAR s[16];
  TCHAR *p;
  HWND childwnd;

  ep = extra;
  EXDLL_INIT();

  ptp->dosExec = (TCHAR*)GlobalAlloc(GPTR, string_size * sizeof(TCHAR));
  ptp->pipeWrite = (TCHAR*)GlobalAlloc(GPTR, string_size * sizeof(TCHAR));
  ptp->logFile = (TCHAR*)GlobalAlloc(GPTR, string_size * sizeof(TCHAR));
  ptp->timeout = 0xffffff;
  ptp->lEndFuncAddress = -1;
  ptp->disableFsR = FALSE;

  while(!popstring(ptp->dosExec) && *(ptp->dosExec) == TEXT('/'))
  {
    if(lstrcmpi(ptp->dosExec, N_W) == 0)
    {
      fWait = FALSE;
    }
    else if(lstrcmpi(ptp->dosExec, DISABLEFSR) == 0)
    {
      ptp->disableFsR = TRUE;
    }
    else if(lstrcmpi(ptp->dosExec, TOSTACK) == 0)
    {
      ptp->target = OUT_STACK;
    }
    else if(lstrcmpi(ptp->dosExec, TOWINDOW) == 0)
    {
      ptp->target = OUT_WINDOW;
    }
    else if(lstrcmpi(ptp->dosExec, TOFUNC) == 0)
    {
      ptp->target = OUT_FUNCTION;
    }
    else if(lstrcmpi(ptp->dosExec, DETAILED) == 0)
    {
// convert 'detailed' to 'window' option
      ptp->target = OUT_WINDOW;
      if(hWndParent &&
        (childwnd = FindWindowEx(hWndParent, NULL, TEXT("#32770"), NULL)) != NULL)
        wsprintf(ptp->logFile, TEXT("%d"), (int)GetDlgItem(childwnd, 0x3f8));
    }
    else if((p = my_strchr(ptp->dosExec, TEXT('='))) != NULL)
    {
      *p++ = 0;
      if(lstrcmpi(ptp->dosExec, T_O) == 0)
        ptp->timeout = myatou(p);
      else if(lstrcmpi(ptp->dosExec, ENDFUNC) == 0)
        ptp->lEndFuncAddress = myatou(p);
    }
    *(ptp->dosExec) = 0;
  }

// if stack is not empty and no /tostack or /detailed output option
  if(popstring(ptp->pipeWrite) == 0 &&
    *ptp->logFile == 0 && // may be in use by DETAILED window
    ptp->target != OUT_STACK) // 2 param suposed only
    popstring(ptp->logFile);
// output window was kept in string. Let's handle it once
  if(ptp->target == OUT_WINDOW)
  {
    ptp->hOut = (HWND)myatou(ptp->logFile);
    GetClassName(ptp->hOut, ptp->logFile, string_size);
  }
  if(ptp->target == OUT_FUNCTION)
  {
    ptp->lFuncAddress = (LONG)myatou(ptp->logFile);
  }

  hThread = CreateThread(NULL, 0, redirect, ptp, 0, &dwThreadId);
  wsprintf(s, TEXT("%u"), hThread);
  pushstring(s);

  if(fWait)
    wait(hWndParent, string_size, variables, stacktop, extra);
  else
    extra->RegisterPluginCallback((HMODULE)g_hInstance, PluginCallback);
}

/*****************************************************
 * FUNCTION NAME: DllMain()
 * PURPOSE: 
 *   Dll main entry point
 * SPECIAL CONSIDERATIONS:
 *   
 *****************************************************/
BOOL WINAPI DllMain(HANDLE hInst,
              ULONG ul_reason_for_call,
              LPVOID lpReserved)
{
  g_hInstance = hInst;
  return TRUE;
}