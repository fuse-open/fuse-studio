/*
  nsArray NSIS plug-in by Stuart Welch <afrowuk@afrowsoft.co.uk>
  v1.1.1.7 - 2nd December 2014
*/

#include <windows.h>
#include "nsArray.h"
#include "Array.h"
#include "pluginapi.h"

HANDLE g_hInstance;
extern struct LIST g_arrays;

// Frees memory on plug-in unload.
static UINT_PTR PluginCallback(enum NSPIM msg)
{
  if (msg == NSPIM_UNLOAD)
  {
    UnInitArrays();
  }
  return 0;
}

int lstrcmpn(TCHAR* a, TCHAR* b, int m, BOOL bIgnoreCase)
{
  int i, j;
  for (i = 0, j = 0; j < m; i++, j++)
  {
    TCHAR ca = bIgnoreCase ? LOWORD(CharLower((LPTSTR)a[i])) : a[i];
    TCHAR cb = bIgnoreCase ? LOWORD(CharLower((LPTSTR)b[j])) : b[j];
    if (ca < cb)
      return -1;
    if (ca > cb)
      return 1;
  }
  return 0;
}

// Gets the number of elements in an array.
NSISFUNC(Length)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);

      if (pArray != NULL)
      {
        wsprintf(pArg, TEXT("%d"), pArray->nCount);
        pushstring(pArg);
        bOK = TRUE;
      }
    }
    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}

// Gets a single element in an array.
NSISFUNC(Get)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;
    ALLOC_C(TCHAR, pArg, string_size);

    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);

      if (popstring(pArg) == 0 && pArray != NULL)
      {
        struct ELEMENT* pElement;
        BOOL bDataOnly = FALSE;

        if (lstrcmpn(pArg, TEXT("/at="), 4, TRUE) == 0)
        {
          pElement = GetElementAt(pArray, myatoi(pArg + 4));
        }
        else
        {
          pElement = GetElement(pArray, pArg);
          bDataOnly = TRUE;
        }

        if (pElement != NULL)
        {
          pushstring(pElement->pData);
          if (!bDataOnly)
            pushstring(pElement->pKey);
          bOK = TRUE;
        }
      }
    }
    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}

// Adds a single value to an array.
NSISFUNC(Set)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);
      ALLOC_C(TCHAR, pKey, string_size);

      if (pArray == NULL)
        pArray = NewArray(pArg);
	  
      if (popstring(pArg) == 0)
      {
        if (lstrcmpn(pArg, TEXT("/at="), 4, TRUE) == 0)
        {
          struct ELEMENT* pElement = GetElementAt(pArray, myatoi(pArg + 4));

          if (popstring(pArg) == 0 && pElement != NULL)
          {
            SetElementData(pElement, pArg);
            bOK = TRUE;
          }
        }
        else if (lstrcmpn(pArg, TEXT("/key="), 5, TRUE) == 0)
        {
          lstrcpy(pKey, pArg + 5);

          if (popstring(pArg) == 0 && NewElement(pArray, pKey, pArg) != NULL)
            bOK = TRUE;
        }
        else
        {
          GetNextIndex(pArray, pKey);

          if (NewElement(pArray, pKey, pArg) != NULL)
            bOK = TRUE;
        }
      }
      FREE(pKey);
    }
    FREE(pArg);

    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}

// Adds one or more values to an array.
NSISFUNC(SetList)
{
  DLL_INIT();
  {
    int bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);
      ALLOC_C(TCHAR, pKey, string_size);

      if (pArray == NULL)
        pArray = NewArray(pArg);

      bOK = TRUE;

      while (popstring(pArg) == 0)
      {
        if (lstrcmpi(pArg, TEXT("/end")) == 0)
          break;

        if (lstrcmpn(pArg, TEXT("/at="), 4, TRUE) == 0)
        {
          struct ELEMENT* pElement = GetElementAt(pArray, myatoi(pArg + 4));

          if (popstring(pArg) == 0 && pElement != NULL)
            SetElementData(pElement, pArg);
          else
            bOK = FALSE;
        }
        else if (lstrcmpn(pArg, TEXT("/key="), 5, TRUE) == 0)
        {
          lstrcpy(pKey, pArg + 5);

          if (popstring(pArg) != 0 || NewElement(pArray, pKey, pArg) == NULL)
            bOK = FALSE;
        }
        else
        {
          GetNextIndex(pArray, pKey);

          if (NewElement(pArray, pKey, pArg) == NULL)
            bOK = FALSE;
        }
      }
      FREE(pKey);
    }
    FREE(pArg);

    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}

// Removes a single value from an array.
NSISFUNC(Remove)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      BOOL fList = FALSE;
      struct ARRAY* pArray = GetArray(pArg);

      if (popstring(pArg) == 0)
      {
        struct ELEMENT* pElement;

        if (lstrcmpn(pArg, TEXT("/at="), 4, TRUE) == 0)
        {
          pElement = GetElementAt(pArray, myatoi(pArg + 4));
        }
        else if (lstrcmpn(pArg, TEXT("/val="), 5, TRUE) == 0)
        {
          pElement = GetElementByVal(pArray, pArg + 5);
        }
        else
        {
          pElement = GetElement(pArray, pArg);
        }

        if (pElement != NULL)
        {
          DeleteElement(pArray, pElement);
          bOK = TRUE;
        }
      }
    }
    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}

// Removes one or more values from an array.
NSISFUNC(RemoveList)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);
      
      bOK = TRUE;

      while (popstring(pArg) == 0)
      {
        if (lstrcmpi(pArg, TEXT("/end")) == 0)
          break;

        if (pArray != NULL)
        {
          struct ELEMENT* pElement;

          if (lstrcmpn(pArg, TEXT("/at="), 4, TRUE) == 0)
          {
            pElement = GetElementAt(pArray, myatoi(pArg + 4));
          }
          else if (lstrcmpn(pArg, TEXT("/val="), 5, TRUE) == 0)
          {
            pElement = GetElementByVal(pArray, pArg + 5);
          }
          else
          {
            pElement = GetElement(pArray, pArg);
          }

          if (pElement != NULL)
            DeleteElement(pArray, pElement);
          else
            bOK = FALSE;
        }
      }
    }
    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}

// Iterates through array elements.
NSISFUNC(Iterate)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;
    ALLOC_C(TCHAR, pArg, string_size);

    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);
      BOOL bNext = TRUE;
      struct ELEMENT* pElement = NULL;

      if (popstring(pArg) == 0)
      {
        if (lstrcmpi(pArg, TEXT("/reset")) == 0)
        {
          if (pArray != NULL)
          {
            pArray->pCurrentElement = NULL;
            bOK = TRUE;
          }

          bNext = FALSE;
        }
        else if (lstrcmpi(pArg, TEXT("/prev")) == 0)
        {
          if (pArray != NULL)
          {
            pElement = GetPrevElement(pArray);
          }

          bNext = FALSE;
        }
        else
        {
          pushstring(pArg);
        }
      }

      if (bNext && pArray != NULL)
      {
        pElement = GetNextElement(pArray);
      }

      if (pElement != NULL)
      {
        pushstring(pElement->pData);
        pushstring(pElement->pKey);
        bOK = TRUE;
      }
    }
    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}

#ifdef ARRAY_CLEAR
// Removes all elements from an array.
NSISFUNC(Clear)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);

      if (pArray != NULL)
      {
        ClearArray(pArray);
        bOK = TRUE;
      }
    }
    FREE(pArg);

    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}
#endif

#ifdef ARRAY_SORT
// Sorts the array.
NSISFUNC(Sort)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);
      if (popstring(pArg) == 0 && pArray != NULL)
      {
        SortArray(pArray, (enum SA_FLAGS)myatoi_or(pArg));
        bOK = TRUE;
      }
    }

    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}
#endif

#ifdef ARRAY_COPY
// Copies an array to a new array.
NSISFUNC(Copy)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);
        
      if (popstring(pArg) == 0 && pArray != NULL && CopyArray(pArray, pArg) != NULL)
        bOK = TRUE;
    }

    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}

// Copies an array's keys to a new array.
NSISFUNC(CopyKeys)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);
        
      if (popstring(pArg) == 0 && pArray != NULL && CopyArrayKeys(pArray, pArg) != NULL)
        bOK = TRUE;
    }

    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}
#endif

#ifdef ARRAY_JOIN
// Joins the elements of the array into a string.
NSISFUNC(Join)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);

      if (popstring(pArg) == 0 && pArray != NULL)
      {
        ALLOC_C(TCHAR, pJoined, string_size);

        struct ELEMENT* pElement = pArray->pFirstElement;
        int chars = 0, joinLen = lstrlen(pArg);
        BOOL bNoEmpty = FALSE;
        
        if (popstring(pJoined) == 0)
        {
          if (lstrcmpi(pJoined, TEXT("/noempty")) == 0)
            bNoEmpty = TRUE;
          else
            pushstring(pJoined);
        }
        
        bOK = TRUE;
        lstrcpy(pJoined, TEXT(""));
        while (pElement != NULL)
        {
          if (*pElement->pData || !bNoEmpty)
          {
            int length = lstrlen(pElement->pData);
            lstrcpyn(pJoined + chars, pElement->pData, string_size - chars - 1);
            chars += length;

            if (chars >= string_size)
            {
              bOK = FALSE;
              break;
            }

            if (pElement->pNext != NULL && (*pElement->pNext->pData || !bNoEmpty))
            {
              lstrcpyn(pJoined + chars, pArg, string_size - chars - 1);
              chars += joinLen;
            }

            if (chars >= string_size)
            {
              bOK = FALSE;
              break;
            }
          }
          pElement = pElement->pNext;
        }

        pushstring(pJoined);

        FREE(pJoined);
      }
    }
    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}
#endif

#ifdef ARRAY_SPLIT
// Splits a string into an array using a delimiter string.
NSISFUNC(Split)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size + 1);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);
      ALLOC_C(TCHAR, pDel, string_size);

      if (pArray == NULL)
        pArray = NewArray(pArg);

      if (popstring(pArg) == 0 && popstring(pDel) == 0 && pArray != NULL)
      {
        int i, j, nDel = lstrlen(pDel), nArg = lstrlen(pArg);
        ALLOC_C(TCHAR, pKey, string_size);
        ALLOC_C(TCHAR, pVal, string_size);
        BOOL bNoEmpty = FALSE;
        BOOL bIgnoreCase = FALSE;

        while (popstring(pKey) == 0)
        {
          if (lstrcmpi(pKey, TEXT("/noempty")) == 0)
          {
            bNoEmpty = TRUE;
          }
          else if (lstrcmpi(pKey, TEXT("/ignorecase")) == 0)
          {
            bIgnoreCase = TRUE;
          }
          else
          {
            pushstring(pKey);
            break;
          }
        }
          
        pVal[0] = (TCHAR)NULL;

        if (nArg == 0 && !bNoEmpty)
        {
          GetNextIndex(pArray, pKey);
          NewElement(pArray, pKey, TEXT(""));
        }

        for (i = 0, j = 0; i <= nArg; i++)
        {
          if (!*pDel)
          {
            GetNextIndex(pArray, pKey);
            pVal[0] = pArg[i];
            pVal[1] = (TCHAR)NULL;
            NewElement(pArray, pKey, pVal);
          }
          else
          {
            if (lstrcmpn(pArg + i, pDel, nDel, bIgnoreCase) == 0)
            {
              if (*pVal || !bNoEmpty)
              {
                GetNextIndex(pArray, pKey);
                NewElement(pArray, pKey, pVal);
              }
              i += nDel - 1;
              pVal[0] = (TCHAR)NULL;
              j = 0;
            }
            else
            {
              pVal[j] = pArg[i];
              pVal[j + 1] = (TCHAR)NULL;
              j++;
            }
          }
        }

        if (j > 0 && (*pVal || !bNoEmpty))
        {
          GetNextIndex(pArray, pKey);
          NewElement(pArray, pKey, pVal);
        }

        FREE(pKey);
        FREE(pVal);
      }
      FREE(pDel);
    }
    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}
#endif

#ifdef ARRAY_TOSTRING
// Returns a string representation of the array.
NSISFUNC(ToString)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);

      if (pArray != NULL)
      {
        ALLOC_C(TCHAR, pJoined, string_size);

        struct ELEMENT* pElement = pArray->pFirstElement;
        int chars = 0;
        
        bOK = TRUE;
        lstrcpy(pJoined, TEXT(""));
        while (pElement != NULL)
        {
          int length = lstrlen(pElement->pKey);
          lstrcpyn(pJoined + chars, pElement->pKey, string_size - chars - 1);
          chars += length;

          if (chars >= string_size)
          {
            bOK = FALSE;
            break;
          }

          lstrcpyn(pJoined + chars, TEXT(" => "), string_size - chars - 1);
          chars += 4;

          if (chars >= string_size)
          {
            bOK = FALSE;
            break;
          }

          length = lstrlen(pElement->pData);
          lstrcpyn(pJoined + chars, pElement->pData, string_size - chars - 1);
          chars += length;

          if (chars >= string_size)
          {
            bOK = FALSE;
            break;
          }

          if (pElement->pNext != NULL)
          {
            lstrcpyn(pJoined + chars, TEXT(", "), string_size - chars - 1);
            chars += 2;
          }

          if (chars >= string_size)
          {
            bOK = FALSE;
            break;
          }

          pElement = pElement->pNext;
        }

        pushstring(pJoined);

        FREE(pJoined);
      }
    }
    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}
#endif

#ifdef ARRAY_REVERSE
// Reverses the contents of an array.
NSISFUNC(Reverse)
{
  DLL_INIT();
  {
    BOOL bOK = FALSE;

    ALLOC_C(TCHAR, pArg, string_size);
    if (popstring(pArg) == 0)
    {
      struct ARRAY* pArray = GetArray(pArg);

      if (pArray != NULL)
      {
        ReverseArray(pArray);
        bOK = TRUE;
      }
    }
    FREE(pArg);
    
    if (!bOK)
      extra->exec_flags->exec_error = 1;
  }
}
#endif

BOOL WINAPI DllMain(HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
  g_hInstance = hInst;

  if (ul_reason_for_call == DLL_PROCESS_ATTACH)
  {
    InitArrays();
  }

  return TRUE;
}