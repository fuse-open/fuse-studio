#define _RICHEDIT_VER 0x0200
#include <richedit.h>
#undef _RICHEDIT_VER

int nRichTextVersion;
static DWORD dwRead;

DWORD CALLBACK FIELD_RICHTEXT_StreamIn(DWORD dwCookie, LPBYTE pbBuff, LONG cb, LONG *pcb)
{
  strncpy((TCHAR*)pbBuff,(TCHAR*)dwCookie+dwRead,cb/sizeof(TCHAR));
  *pcb=strlen((TCHAR*)pbBuff)*sizeof(TCHAR);
  dwRead+=strlen((TCHAR*)pbBuff);
  return 0;
}

DWORD CALLBACK FIELD_RICHTEXT_StreamOut(DWORD dwCookie, LPBYTE pbBuff, LONG cb, LONG *pcb)
{
  if(dwRead+1 > (UINT)g_nBufferSize)
    return 1;

  if(dwRead+cb+1 <= (UINT)g_nBufferSize)
    strcpy((TCHAR*)dwCookie+dwRead,(TCHAR*)pbBuff);
  else
    strncpy((TCHAR*)dwCookie+dwRead,(TCHAR*)pbBuff, (UINT)g_nBufferSize - dwRead+1);
  *pcb=strlen((TCHAR*)dwCookie);
  dwRead+=*pcb;
  return 0;
}
