int iItemPlaceholder = -1;

int WINAPI FIELD_LISTVIEW_IOLI_CountItems(LPTSTR str, int nFlags)
{
  //nFlags:
  //0 = Items.
  //1 = SubItems.

  int nItems = 0;
  int nItemsTemp = nItems;
  int bSubItem = FALSE;

  int nResult = 0;

  TCHAR *pszEnd;
  pszEnd = str;

  while (*pszEnd)
  {
    if (*pszEnd == '\x01')
	{
	  if(!bSubItem && nFlags == 0)
        ++nItems;
	  if(bSubItem && nFlags == 1)
        ++nItemsTemp;
	}
    else if (*pszEnd == '\x02')
	  bSubItem = TRUE;
	else if (*pszEnd == '\x03')
	{
	  bSubItem = FALSE;

	  if(nFlags == 1)
	  {
        ++nItemsTemp;

	    if(nItemsTemp > nItems)
		  nItems = nItemsTemp;
	    nItemsTemp = 0;
	  }
	}

	pszEnd = CharNext(pszEnd);
  }

  return nItems;
}

int WINAPI FIELD_LISTVIEW_IOLI_CountSubItems(LPTSTR str, LPTSTR pszHeaderItems)
{
  int nPart = 0;

  int nItems = 0;
  int nItemsTmp = 0;
  int nItemsTemp = nItems;
  int bSubItem = FALSE;

  TCHAR *pszEnd;
  pszEnd = str;

  while(nPart < 2)
  {
    if(nPart == 0 && str)
	  pszEnd = str;
    if(nPart == 1 && pszHeaderItems)
	  pszEnd = pszHeaderItems;

    while (*pszEnd)
	{
      if (*pszEnd == '\x01')
	  {
	    if(!bSubItem && nPart != 0)
          ++nItemsTmp;
	    if(bSubItem && nPart == 0)
          ++nItemsTemp;
	  }
      else if (*pszEnd == '\x02')
	    bSubItem = TRUE;
	  else if (*pszEnd == '\x03')
	  {
	    bSubItem = FALSE;

	    if(nPart == 0)
		{
          ++nItemsTemp;

	      if(nItemsTemp > nItemsTmp)
		    nItemsTmp = nItemsTemp;
	      nItemsTemp = 0;
		}
	  }

	  pszEnd = CharNext(pszEnd);
	}

    if(nItemsTmp > nItems)
	  nItems = nItemsTmp;

	nItemsTmp = 0;

	nPart++;
  }

  return nItems;
}
