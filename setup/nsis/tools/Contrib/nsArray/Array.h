#ifndef __ARRAY_H__
#define __ARRAY_H__

// Comment out as needed to reduce DLL size!
#define ARRAY_CLEAR
#define ARRAY_SORT
//#define ARRAY_COPY
#define ARRAY_JOIN
#define ARRAY_SPLIT
//#define ARRAY_TOSTRING

// An element of an array.
struct ELEMENT
{
  // The array key or index (string).
  TCHAR* pKey;
  // The array element data (string).
  TCHAR* pData;
  // Pointer to the next element.
  struct ELEMENT* pNext;
  // Pointer to the previous element.
  struct ELEMENT* pPrev;
};

// An array of elements.
struct ARRAY
{
  // Array name.
  TCHAR* pName;
  // Pointer to the first element in the array.
  struct ELEMENT* pFirstElement;
  // Pointer to the last element in the array.
  struct ELEMENT* pLastElement;
  // Pointer to the current element in the array for array iteration.
  struct ELEMENT* pCurrentElement;
  // The number of elements.
  int nCount;
  // The next index number for a new element without a specified key.
  int iNextIndex;
  // Pointer to the next array.
  struct ARRAY* pNext;
  // Pointer to the previous array.
  struct ARRAY* pPrev;
};

// A list of arrays.
struct LIST
{
  // Pointer to the first array in the list.
  struct ARRAY* pFirst;
  // Pointer to the last array in the list.
  struct ARRAY* pLast;
  // The number of arrays.
  int nCount;
};

#ifdef ARRAY_SORT
// Sort options.
enum SA_FLAGS
{
  // Sort descending instead of ascending.
  SA_DESCENDING = 1,
  // Numeric comparison rather than string.
  SA_NUMERIC = 2,
  // Ignore character case.
  SA_IGNORE_CASE = 4,
  // Sort by keys rather than by values.
  SA_BY_KEYS = 8,
  // Sort the values only (leave the keys in the original order).
  SA_VALUES_ONLY = 16
};
#endif

// The list of arrays.
struct LIST g_arrays;

// Initializes pointers to NULL and array count to 0.
void InitArrays();
// Deallocates all arrays and their data.
void UnInitArrays();
// Adds a new array with the given name. If the array already exists then it is cleared.
struct ARRAY* NewArray(TCHAR* pName);
// Deletes the given array.
void DeleteArray(struct ARRAY* pArray);
// Removes all elements from the given array.
void ClearArray(struct ARRAY* pArray);
// Gets the array with the given name.
struct ARRAY* GetArray(TCHAR* pName);
// Fills the given buffer with the next element index for the given array.
void GetNextIndex(struct ARRAY* pArray, TCHAR* pKey);
// Adds a new element to the given array. Returns the new element.
struct ELEMENT* NewElement(struct ARRAY* pArray, TCHAR* pKey, TCHAR* pData);
// Deletes the given element from the given array.
void DeleteElement(struct ARRAY* pArray, struct ELEMENT* pElement);
// Gets the element in the given array with the given key.
struct ELEMENT* GetElement(struct ARRAY* pArray, TCHAR* pKey);
// Gets the element in the given array at the given index.
struct ELEMENT* GetElementAt(struct ARRAY* pArray, int iIndex);
// Gets the element in the given array that matches the given value (case insensitive).
struct ELEMENT* GetElementByVal(struct ARRAY* pArray, TCHAR* pData);
// Gets the next element in the current array iteration.
struct ELEMENT* GetNextElement(struct ARRAY* pArray);
// Gets the previous element in the current array iteration.
struct ELEMENT* GetPrevElement(struct ARRAY* pArray);
// Sets the data for the given element.
void SetElementData(struct ELEMENT* pElement, TCHAR* pData);

#ifdef ARRAY_SORT
// Sorts the given array.
void SortArray(struct ARRAY* pArray, enum SA_FLAGS flags);
#endif

#ifdef ARRAY_COPY
// Copies the given array to a new array with the given name.
struct ARRAY* CopyArray(struct ARRAY* pArraySrc, TCHAR* pNameDest);
// Copies the keys from the given array to a new array with the given name.
struct ARRAY* CopyArrayKeys(struct ARRAY* pArraySrc, TCHAR* pNameDest);
#endif

#define ALLOC(T, V) T* V = (T*)GlobalAlloc(GMEM_FIXED, sizeof(T))
#define ALLOC_P(T, V) V = (T*)GlobalAlloc(GMEM_FIXED, sizeof(T))
#define ALLOC_C(T, V, C) T* V = (T*)GlobalAlloc(GMEM_FIXED, C * sizeof(T))
#define ALLOC_C_P(T, V, C) V = (T*)GlobalAlloc(GMEM_FIXED, C * sizeof(T))
#define ALLOC_STRCPY(V, S) V = (TCHAR*)GlobalAlloc(GMEM_FIXED, (lstrlen(S) + 1) * sizeof(TCHAR)); lstrcpy(V, S)
#define FREE(P) GlobalFree(P)

#endif