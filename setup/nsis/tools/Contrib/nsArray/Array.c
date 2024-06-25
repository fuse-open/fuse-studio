/*
  nsArray NSIS plug-in by Stuart Welch <afrowuk@afrowsoft.co.uk>
  v1.1.1.7 - 2nd December 2014
*/

#include <windows.h>
#include "Array.h"
#include "pluginapi.h"

// Initializes pointers to NULL and array count to 0.
void InitArrays()
{
  g_arrays.pFirst = NULL;
  g_arrays.pLast = NULL;
  g_arrays.nCount = 0;
}

// Deallocates all arrays and their data.
void UnInitArrays()
{
  struct ARRAY* pArray = g_arrays.pFirst;
  while (pArray != NULL)
  {
    struct ARRAY* pArrayNext = pArray->pNext;
    ClearArray(pArray);
    FREE(pArray);
    pArray = pArrayNext;
  }
}

// Adds a new array with the given name. If the array already exists then it is cleared.
struct ARRAY* NewArray(TCHAR* pName)
{
  struct ARRAY* pArrayExisting = GetArray(pName);
  if (pArrayExisting == NULL)
  {
    ALLOC(struct ARRAY, pArray);
    ALLOC_STRCPY(pArray->pName, pName);
    pArray->pFirstElement = NULL;
    pArray->pLastElement = NULL;
    pArray->pCurrentElement = NULL;
    pArray->nCount = 0;
    pArray->iNextIndex = 0;
    pArray->pNext = NULL;

    if (g_arrays.pFirst == NULL)
    {
      g_arrays.pFirst = pArray;
      g_arrays.pLast = pArray;
      g_arrays.nCount = 1;
      pArray->pPrev = NULL;
    }
    else
    {
      struct ARRAY* pLast = g_arrays.pLast;
      g_arrays.pLast->pNext = pArray;
      g_arrays.pLast = pArray;
      g_arrays.nCount++;
      pArray->pPrev = pLast;
    }

    return pArray;
  }

  ClearArray(pArrayExisting);
  return pArrayExisting;
}

// Deletes the given array.
void DeleteArray(struct ARRAY* pArray)
{
  ClearArray(pArray);

  if (g_arrays.nCount == 1)
  {
    g_arrays.pFirst = NULL;
    g_arrays.pLast = NULL;
  }
  else
  {
    if (g_arrays.pFirst == pArray)
      g_arrays.pFirst = pArray->pNext;
    if (g_arrays.pLast == pArray)
      g_arrays.pLast = pArray->pPrev;

    if (pArray->pNext != NULL)
      pArray->pNext->pPrev = pArray->pPrev;
    if (pArray->pPrev != NULL)
      pArray->pPrev->pNext = pArray->pNext;
  }

  FREE(pArray->pName);
  FREE(pArray);
  g_arrays.nCount--;
}

// Removes all elements from the given array.
void ClearArray(struct ARRAY* pArray)
{
  struct ELEMENT* pElement = pArray->pFirstElement;

  while (pElement != NULL)
  {
    struct ELEMENT* pElementNext = pElement->pNext;
    FREE(pElement->pKey);
    FREE(pElement->pData);
    FREE(pElement);
    pElement = pElementNext;
  }

  pArray->pFirstElement = NULL;
  pArray->pLastElement = NULL;
  pArray->pCurrentElement = NULL;
  pArray->nCount = 0;
  pArray->iNextIndex = 0;
}

// Gets the array with the given name.
struct ARRAY* GetArray(TCHAR* pName)
{
  struct ARRAY* pArray = g_arrays.pFirst;

  while (pArray != NULL)
  {
    if (lstrcmp(pArray->pName, pName) == 0)
      return pArray;

    pArray = pArray->pNext;
  }

  return NULL;
}

// Determines if the given string is an integer.
BOOL IsInt(TCHAR* pStr)
{
  int i = 0, l = lstrlen(pStr);

  if (l > 0)
  {
    if (pStr[0] == TEXT('-'))
      i++;
    for (; pStr[i] >= TEXT('0') && pStr[i] <= TEXT('9') && i < l; ++i);
    return i == l;
  }

  return FALSE;
}

// Fills the given buffer with the next element index for the given array.
void GetNextIndex(struct ARRAY* pArray, TCHAR* pKey)
{
  wsprintf(pKey, TEXT("%d"), pArray->iNextIndex);
}

// Adds a new element to the given array. Returns the new element.
struct ELEMENT* NewElement(struct ARRAY* pArray, TCHAR* pKey, TCHAR* pData)
{
  struct ELEMENT* pElement = GetElement(pArray, pKey);

  if (pElement != NULL)
  {
    SetElementData(pElement, pData);
  }
  else
  {
    ALLOC_P(struct ELEMENT, pElement);
    ALLOC_STRCPY(pElement->pKey, pKey);
    ALLOC_STRCPY(pElement->pData, pData);

    // If key is an integer, increase the last index.
    if (IsInt(pKey))
    {
      int i = myatoi(pKey);
      if (i >= pArray->iNextIndex)
        pArray->iNextIndex = i + 1;
    }

    // No elements.
    if (pArray->pFirstElement == NULL)
    {
      pArray->pFirstElement = pElement;
      pArray->pLastElement = pElement;
      pArray->nCount = 1;
      pElement->pNext = NULL;
      pElement->pPrev = NULL;
    }
    else
    {
      struct ELEMENT* pLast = pArray->pLastElement;
      pArray->pLastElement->pNext = pElement;
      pArray->pLastElement = pElement;
      pArray->nCount++;
      pElement->pNext = NULL;
      pElement->pPrev = pLast;
    }
  }

  return pElement;
}

// Deletes the given element from the given array.
void DeleteElement(struct ARRAY* pArray, struct ELEMENT* pElement)
{
  if (pArray->nCount == 1)
  {
    pArray->pFirstElement = NULL;
    pArray->pLastElement = NULL;
  }
  else
  {
    if (pArray->pFirstElement == pElement)
      pArray->pFirstElement = pElement->pNext;
    if (pArray->pLastElement == pElement)
      pArray->pLastElement = pElement->pPrev;

    if (pElement->pNext != NULL)
      pElement->pNext->pPrev = pElement->pPrev;
    if (pElement->pPrev != NULL)
      pElement->pPrev->pNext = pElement->pNext;
  }
  
  FREE(pElement->pKey);
  FREE(pElement->pData);
  FREE(pElement);
  pArray->nCount--;
}

// Gets the element in the given array with the given key.
struct ELEMENT* GetElement(struct ARRAY* pArray, TCHAR* pKey)
{
  struct ELEMENT* pElement = pArray->pFirstElement;

  while (pElement != NULL)
  {
    if (lstrcmp(pElement->pKey, pKey) == 0)
      return pElement;

    pElement = pElement->pNext;
  }

  return NULL;
}

// Gets the element in the given array at the given index.
struct ELEMENT* GetElementAt(struct ARRAY* pArray, int iIndex)
{
  struct ELEMENT* pElement;
  int i;

  if (iIndex < 0)
  {
    pElement = pArray->pLastElement;
    for (i = -1; i > iIndex && pElement != NULL; i--)
      pElement = pElement->pPrev;
  }
  else
  {
    pElement = pArray->pFirstElement;
    for (i = 0; i < iIndex && pElement != NULL; i++)
      pElement = pElement->pNext;
  }

  return pElement;
}

// Gets the element in the given array that matches the given value (case insensitive).
struct ELEMENT* GetElementByVal(struct ARRAY* pArray, TCHAR* pData)
{
  struct ELEMENT* pElement = pArray->pFirstElement;

  while (pElement != NULL)
  {
    if (lstrcmpi(pElement->pData, pData) == 0)
      return pElement;

    pElement = pElement->pNext;
  }

  return NULL;
}

// Gets the next element in the current array iteration.
struct ELEMENT* GetNextElement(struct ARRAY* pArray)
{
  pArray->pCurrentElement = pArray->pCurrentElement == NULL ? pArray->pFirstElement : pArray->pCurrentElement->pNext;
  return pArray->pCurrentElement;
}

// Gets the previous element in the current array iteration.
struct ELEMENT* GetPrevElement(struct ARRAY* pArray)
{
  pArray->pCurrentElement = pArray->pCurrentElement == NULL ? pArray->pLastElement : pArray->pCurrentElement->pPrev;
  return pArray->pCurrentElement;
}

// Sets the data for the given element.
void SetElementData(struct ELEMENT* pElement, TCHAR* pData)
{
  FREE(pElement->pData);
  ALLOC_STRCPY(pElement->pData, pData);
}

#ifdef ARRAY_SORT
// Sorts the given array.
void SortArray(struct ARRAY* pArray, enum SA_FLAGS flags)
{
  if (pArray->pFirstElement != NULL)
  {
    struct ELEMENT* pElementI = pArray->pFirstElement->pNext;
    struct ELEMENT* pElementJ;
    TCHAR* pKeyTemp;
    TCHAR* pDataTemp;

    while (pElementI != NULL)
    {
      pKeyTemp = pElementI->pKey;
      pDataTemp = pElementI->pData;
      pElementJ = pElementI->pPrev;

      while (pElementJ != NULL &&
            ((flags & SA_NUMERIC) ?
              ((flags & SA_DESCENDING) ? myatoi((flags & SA_BY_KEYS) ? pKeyTemp : pDataTemp) > myatoi((flags & SA_BY_KEYS) ? pElementJ->pKey : pElementJ->pData) : myatoi((flags & SA_BY_KEYS) ? pKeyTemp : pDataTemp) < myatoi((flags & SA_BY_KEYS) ? pElementJ->pKey : pElementJ->pData)) :
              ((flags & SA_DESCENDING) ?
                ((flags & SA_IGNORE_CASE) ? lstrcmpi((flags & SA_BY_KEYS) ? pKeyTemp : pDataTemp, (flags & SA_BY_KEYS) ? pElementJ->pKey : pElementJ->pData) > 0 : lstrcmp((flags & SA_BY_KEYS) ? pKeyTemp : pDataTemp, (flags & SA_BY_KEYS) ? pElementJ->pKey : pElementJ->pData) > 0) :
                ((flags & SA_IGNORE_CASE) ? lstrcmpi((flags & SA_BY_KEYS) ? pKeyTemp : pDataTemp, (flags & SA_BY_KEYS) ? pElementJ->pKey : pElementJ->pData) < 0 : lstrcmp((flags & SA_BY_KEYS) ? pKeyTemp : pDataTemp, (flags & SA_BY_KEYS) ? pElementJ->pKey : pElementJ->pData) < 0))))
      {
        if ((flags & SA_BY_KEYS) || !(flags & SA_VALUES_ONLY))
          pElementJ->pNext->pKey = pElementJ->pKey;
        pElementJ->pNext->pData = pElementJ->pData;
        pElementJ = pElementJ->pPrev;
      }

      if (pElementJ == NULL)
      {
        if ((flags & SA_BY_KEYS) || !(flags & SA_VALUES_ONLY))
          pArray->pFirstElement->pKey = pKeyTemp;
        pArray->pFirstElement->pData = pDataTemp;
      }
      else
      {
        if ((flags & SA_BY_KEYS) || !(flags & SA_VALUES_ONLY))
          pElementJ->pNext->pKey = pKeyTemp;
        pElementJ->pNext->pData = pDataTemp;
      }
      pElementI = pElementI->pNext;
    }
  }
}
#endif

#ifdef ARRAY_COPY
// Copies the given array to a new array with the given name.
struct ARRAY* CopyArray(struct ARRAY* pArraySrc, TCHAR* pNameDest)
{
  struct ARRAY* pArrayDest;
  struct ELEMENT* pElementSrc;

  if (lstrcmp(pArraySrc->pName, pNameDest) == 0)
    return pArraySrc;

  pArrayDest = NewArray(pNameDest);
  pElementSrc = pArraySrc->pFirstElement;

  while (pElementSrc != NULL)
  {
    NewElement(pArrayDest, pElementSrc->pKey, pElementSrc->pData);
    pElementSrc = pElementSrc->pNext;
  }

  return pArrayDest;
}

// Copies the keys from the given array to a new array with the given name.
struct ARRAY* CopyArrayKeys(struct ARRAY* pArraySrc, TCHAR* pNameDest)
{
  struct ARRAY* pArrayDest;
  struct ELEMENT* pElementSrc;
  TCHAR* pKey;

  if (lstrcmp(pArraySrc->pName, pNameDest) == 0)
    return pArraySrc;

  pArrayDest = NewArray(pNameDest);
  pElementSrc = pArraySrc->pFirstElement;

  ALLOC_C_P(TCHAR, pKey, 32);
  while (pElementSrc != NULL)
  {
    GetNextIndex(pArrayDest, pKey);
    NewElement(pArrayDest, pKey, pElementSrc->pKey);
    pElementSrc = pElementSrc->pNext;
  }
  FREE(pKey);

  return pArrayDest;
}
#endif