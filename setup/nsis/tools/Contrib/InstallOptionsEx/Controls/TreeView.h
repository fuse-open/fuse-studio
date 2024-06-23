//================================================================
// TreeView Special Flags
//================================================================

// TREEVIEW_UNCHECKED    = Unselected *Items*
// TREEVIEW_CHECKED      = Selected *Items*
// TREEVIEW_READONLY     = Read Only *Items*
// TREEVIEW_NOCHECKBOX   = No CheckBox *All Items*
// TREEVIEW_BOLD         = Bold *All*
// TREEVIEW_EXPANDED     = Expand *Parent Items*
// TREEVIEW_PART_UNCHECK = Swap between 3rd state to 1st *Parent Items* (Reserved)
// TREEVIEW_PART_CHECK   = Swap between 3rd state to 2st *Parent Items* (Reserved)
// TREEVIEW_PART_FIXED   = Stay on 3rd state *Parent Items* (Reserved)
// TREEVIEW_AUTOCHECK    = Automatic Check (only for function FIELD_TREEVIEW_Check)

#define TREEVIEW_UNCHECKED     0x00000000
#define TREEVIEW_CHECKED       0x00000001
#define TREEVIEW_READONLY      0x00000002
#define TREEVIEW_NOCHECKBOX    0x00000004
#define TREEVIEW_BOLD          0x00000008
#define TREEVIEW_EXPANDED      0x00000010
#define TREEVIEW_PART_UNCHECK  0x00000020
#define TREEVIEW_PART_CHECK    0x00000040
#define TREEVIEW_PART_FIXED    0x00000080
#define TREEVIEW_AUTOCHECK     0xFFFFFFFF

void WINAPI FIELD_TREEVIEW_Check(HWND hCtrl, HTREEITEM hItem, int nCheck, int nParam, bool bCalledFromParent)
{

// Step 1: Check/uncheck itself.
//================================================================

  // Get Item Information
  //--------------------------------------------------------------
  TVITEM tvItem;
  tvItem.mask = TVIF_STATE | TVIF_PARAM | TVIF_CHILDREN;
  tvItem.stateMask = TVIS_STATEIMAGEMASK;
  tvItem.hItem = hItem;
  TreeView_GetItem(hCtrl, &tvItem);

  // "3rd state" checkboxes
  //--------------------------------------------------------------
  if(tvItem.lParam & TREEVIEW_PART_FIXED)
  {
    if((tvItem.state >> 12) >= 3 && !(tvItem.lParam & TREEVIEW_CHECKED)) // If item state is leaving the 3rd state
	{
      if(nCheck == TREEVIEW_AUTOCHECK) nCheck = TREEVIEW_CHECKED;
	  tvItem.lParam |= TREEVIEW_CHECKED;
	}
    else
	{
      if(nCheck == TREEVIEW_AUTOCHECK) nCheck = TREEVIEW_UNCHECKED;
	  tvItem.lParam &= ~TREEVIEW_CHECKED;
	}
	tvItem.state = INDEXTOSTATEIMAGEMASK(3 - 1); // SPECIAL: Stays there
  }
  // "3rd state <-> checked" checkboxes
  //--------------------------------------------------------------
  else if(tvItem.lParam & TREEVIEW_PART_CHECK)
  {
    if((tvItem.state >> 12) >= 3) // If item state is leaving the 3rd state
	{
	  tvItem.state = INDEXTOSTATEIMAGEMASK((tvItem.state >> 12) - 2); // SPECIAL: Goes to checked state
	  tvItem.lParam |= TREEVIEW_CHECKED;
      if(nCheck == TREEVIEW_AUTOCHECK) nCheck = TREEVIEW_CHECKED;
	}
    else
	{
	  tvItem.lParam &= ~TREEVIEW_CHECKED;
      if(nCheck == TREEVIEW_AUTOCHECK) nCheck = TREEVIEW_UNCHECKED;
	}
  }
  // "3rd state <-> unchecked" checkboxes
  //--------------------------------------------------------------
  else if (tvItem.lParam & TREEVIEW_PART_UNCHECK)
  {
	if((tvItem.state >> 12) == 1) // If item state is leaving the unchecked state
	{
	  tvItem.state = INDEXTOSTATEIMAGEMASK((tvItem.state >> 12) + 1); // SPECIAL: Goes to 3rd state
	  tvItem.lParam |= TREEVIEW_CHECKED;
      if(nCheck == TREEVIEW_AUTOCHECK) nCheck = TREEVIEW_CHECKED;
	}
	else if ((tvItem.state >> 12) >= 3) // If item state is leaving the 3rd state
	{
	  tvItem.state = INDEXTOSTATEIMAGEMASK((tvItem.state >> 12) - 3); // SPECIAL: Goes to unchecked state
	  tvItem.lParam &= ~TREEVIEW_CHECKED;
      if(nCheck == TREEVIEW_AUTOCHECK) nCheck = TREEVIEW_UNCHECKED;
	}
  }
  // "Read only" checkboxes
  //--------------------------------------------------------------
  else if (tvItem.lParam & TREEVIEW_READONLY)
  {
    if(!bCalledFromParent) tvItem.state = INDEXTOSTATEIMAGEMASK((tvItem.state >> 12) - 1); // SPECIAL: Stays the same
  }
  // "Normal" checkboxes (Leaving from Checked state)
  //--------------------------------------------------------------
  else if (nParam == 0xFFFFFFFF)
  {
	if ((tvItem.state >> 12) >= 3)
	{
      tvItem.state = INDEXTOSTATEIMAGEMASK((tvItem.state >> 12) - 2); // SPECIAL: Goes to checked state
	  tvItem.lParam |= TREEVIEW_CHECKED;
      if(nCheck == TREEVIEW_AUTOCHECK) nCheck = TREEVIEW_CHECKED;
	}
	else
	if ((tvItem.state >> 12) == 2)
	{
      tvItem.state = INDEXTOSTATEIMAGEMASK((tvItem.state >> 12) - 2); // SPECIAL: Goes to unchecked state
	  tvItem.lParam &= ~TREEVIEW_CHECKED;
      if(nCheck == TREEVIEW_AUTOCHECK) nCheck = TREEVIEW_UNCHECKED;
	}
	else
	{
	  tvItem.lParam |= TREEVIEW_CHECKED;
      if(nCheck == TREEVIEW_AUTOCHECK) nCheck = TREEVIEW_CHECKED;
	  goto Step2;
	}
  }
  // Checkbox creation (used when creating the control)
  //--------------------------------------------------------------
  else
  {
    // No checkboxes (TREEVIEW_NOCHECKBOX)
    //------------------------------------
	if(nParam & TREEVIEW_NOCHECKBOX)
	  tvItem.state = INDEXTOSTATEIMAGEMASK(0);

    // Read-only checkboxes (TREEVIEW_READONLY)
    //-----------------------------------------
	else
	if(nParam & TREEVIEW_READONLY)
	{
	  if(nParam & TREEVIEW_CHECKED)
        // Read-only checked checkboxes (TREEVIEW_READONLY | TREEVIEW_CHECKED)
		tvItem.state = INDEXTOSTATEIMAGEMASK(5);
	  else
        // Read-only unchecked checkboxes (TREEVIEW_READONLY)
		tvItem.state = INDEXTOSTATEIMAGEMASK(4);
	}
	else
    // Checked checkboxes (TREEVIEW_CHECKED)
    //-----------------------------------------
	if(nParam & TREEVIEW_CHECKED)
	  tvItem.state = INDEXTOSTATEIMAGEMASK(2);

    // Bold items (TREEVIEW_BOLD)
    //---------------------------
	if(nParam & TREEVIEW_BOLD)
	{
	  tvItem.state |= TVIS_BOLD;
	  tvItem.stateMask |= TVIS_BOLD;
	}
    // Expanded items (TREEVIEW_EXPANDED)
    //-----------------------------------
	if(nParam & TREEVIEW_EXPANDED)
	{
	  tvItem.state |= TVIS_EXPANDED;
	  tvItem.stateMask |= TVIS_EXPANDED;
	}

    tvItem.lParam = nParam;
	nCheck = nParam;
  }

  TreeView_SetItem(hCtrl, &tvItem);

Step2:

// Step 2: Detect and check/uncheck all children items of "hItem".
//================================================================
  if(nParam == 0xFFFFFFFF)
  {
    TVITEM tvChildItem;
    HTREEITEM hChildItem = TreeView_GetChild(hCtrl, hItem);
    while(hChildItem)
	{
	  tvChildItem.mask = TVIF_CHILDREN | TVIF_STATE | TVIF_PARAM;
      tvChildItem.stateMask = TVIS_STATEIMAGEMASK;
      tvChildItem.hItem = hChildItem;
      TreeView_GetItem(hCtrl, &tvChildItem);

 	  tvChildItem.mask = TVIF_STATE;
      tvChildItem.stateMask = TVIS_STATEIMAGEMASK;

	  if(tvChildItem.cChildren == 1)
	  {
	    FIELD_TREEVIEW_Check(hCtrl, hChildItem, nCheck, 0xFFFFFFFF, TRUE);
	  }

      if(!(tvChildItem.lParam & TREEVIEW_NOCHECKBOX))
	  {
        if(tvChildItem.lParam & TREEVIEW_PART_FIXED)
		{
	      tvChildItem.state = INDEXTOSTATEIMAGEMASK(3);
          TreeView_SetItem(hCtrl, &tvChildItem);
		}
        else if(tvChildItem.lParam & TREEVIEW_PART_CHECK)
		{
	      if(nCheck == TREEVIEW_CHECKED)
		  {
		    tvChildItem.state = INDEXTOSTATEIMAGEMASK(2);
	        tvChildItem.lParam |= TREEVIEW_CHECKED;
		  }
	      else
		  {
		    tvChildItem.state = INDEXTOSTATEIMAGEMASK(3);
	        tvChildItem.lParam &= ~TREEVIEW_CHECKED;
		  }
          TreeView_SetItem(hCtrl, &tvChildItem);
		}
	    else if(tvChildItem.lParam & TREEVIEW_PART_UNCHECK)
		{
	      if(nCheck == TREEVIEW_CHECKED)
		  {
	        tvChildItem.state = INDEXTOSTATEIMAGEMASK(3);
	        tvChildItem.lParam |= TREEVIEW_CHECKED;
		  }
	      else
		  {
		    tvChildItem.state = INDEXTOSTATEIMAGEMASK(1);
	        tvChildItem.lParam &= ~TREEVIEW_CHECKED;
		  }
          TreeView_SetItem(hCtrl, &tvChildItem);
		}
	    else if(!(tvChildItem.lParam & TREEVIEW_READONLY))
		{
	      if(nCheck == TREEVIEW_CHECKED)
		  {
		    tvChildItem.state = INDEXTOSTATEIMAGEMASK(2);
	        tvChildItem.lParam |= TREEVIEW_CHECKED;
		  }
	      else
		  {
		    tvChildItem.state = INDEXTOSTATEIMAGEMASK(1);
	        tvChildItem.lParam &= ~TREEVIEW_CHECKED;
		  }
          TreeView_SetItem(hCtrl, &tvChildItem);
		}
	  }

      hChildItem = TreeView_GetNextSibling(hCtrl, hChildItem);
	}
  }

// Step 3: Detect and check/uncheck parent items.
//================================================================
  if(!bCalledFromParent)
  {
    HTREEITEM hParentItem = TreeView_GetParent(hCtrl, hItem);
    HTREEITEM hBackupParentItem = hParentItem;
	while(hParentItem)
    {
      TVITEM tvParentItem;
  	  tvParentItem.mask = TVIF_STATE | TVIF_PARAM;
      tvParentItem.stateMask = TVIS_STATEIMAGEMASK;
      tvParentItem.hItem = hParentItem;
      TreeView_GetItem(hCtrl, &tvParentItem);

      if(tvParentItem.lParam & TREEVIEW_NOCHECKBOX)
	  {
	    hParentItem = TreeView_GetParent(hCtrl, hParentItem);
		continue;
	  }
      else
	  {
  	    HTREEITEM hSiblingItem = TreeView_GetChild(hCtrl, hBackupParentItem);
        bool bFirstSibling = TRUE;

  	    while(hSiblingItem != NULL)
		{
  	      TVITEM tvSiblingItem;
  	      tvSiblingItem.hItem = hSiblingItem;
  	      tvSiblingItem.mask = TVIF_STATE | TVIF_PARAM;
          tvSiblingItem.stateMask = TVIS_STATEIMAGEMASK;
  	      TreeView_GetItem(hCtrl, &tvSiblingItem);

          if(!(tvSiblingItem.lParam & TREEVIEW_NOCHECKBOX))
		  {
            if(nParam == 0xFFFFFFFF && hItem == hSiblingItem)
              tvSiblingItem.state = INDEXTOSTATEIMAGEMASK((tvSiblingItem.state >> 12) + 1);

  	        if(bFirstSibling == TRUE)
			{
  		      tvParentItem.state = tvSiblingItem.state;
  		      tvParentItem.lParam = tvSiblingItem.lParam;
  		      bFirstSibling = FALSE;
			}
  	        else
			{
			  if (((tvParentItem.lParam & TREEVIEW_PART_UNCHECK) && (tvSiblingItem.lParam & TREEVIEW_PART_CHECK)) ||
				  ((tvParentItem.lParam & TREEVIEW_PART_CHECK) && (tvSiblingItem.lParam & TREEVIEW_PART_UNCHECK)))
			  {
			    tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
                tvParentItem.lParam &= ~TREEVIEW_PART_UNCHECK;
                tvParentItem.lParam &= ~TREEVIEW_PART_CHECK;
                tvParentItem.lParam |= TREEVIEW_PART_FIXED;
			  }
		      else
			  if (tvSiblingItem.lParam & TREEVIEW_PART_UNCHECK)
			  {
			    tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
                tvParentItem.lParam |= TREEVIEW_PART_UNCHECK;
			  }
		      else
			  if (tvSiblingItem.lParam & TREEVIEW_PART_CHECK)
			  {
			    tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
                tvParentItem.lParam |= TREEVIEW_PART_CHECK;
			  }
		      else
			  if (tvSiblingItem.lParam & TREEVIEW_PART_FIXED)
			  {
			    tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
                tvParentItem.lParam |= TREEVIEW_PART_FIXED;
			  }
		      else
			  if (((tvParentItem.state >> 12) == 3) && ((tvSiblingItem.state >> 12) == 1))
			  {
			    tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
			  }
		      else
  	          if((((tvParentItem.state >> 12) == 3) && ((tvSiblingItem.state >> 12) == 2)) ||
			     (((tvParentItem.state >> 12) == 2) && ((tvSiblingItem.state >> 12) == 3)))
			  {
			    tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
			  }
		      else
  	          if((((tvParentItem.state >> 12) == 1) && ((tvSiblingItem.state >> 12) == 2)) ||
			     (((tvParentItem.state >> 12) == 2) && ((tvSiblingItem.state >> 12) == 1)))
			  {
			    tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
			  }
  		      else
  	          if((((tvParentItem.state >> 12) == 4) && ((tvSiblingItem.state >> 12) == 1)) ||
			     (((tvParentItem.state >> 12) == 1) && ((tvSiblingItem.state >> 12) == 4)))
			  {
  		        tvParentItem.state = INDEXTOSTATEIMAGEMASK(1);
                tvParentItem.lParam = TREEVIEW_PART_UNCHECK;
			  }
  		      else
  	          if((((tvParentItem.state >> 12) == 4) && ((tvSiblingItem.state >> 12) == 2)) ||
			     (((tvParentItem.state >> 12) == 2) && ((tvSiblingItem.state >> 12) == 4)))
			  {
  		        tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
                tvParentItem.lParam = TREEVIEW_PART_UNCHECK;
			  }
  		      else
  	          if((((tvParentItem.state >> 12) == 5) && ((tvSiblingItem.state >> 12) == 4)) ||
			     (((tvParentItem.state >> 12) == 4) && ((tvSiblingItem.state >> 12) == 5)))
			  {
  		        tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
			  }
  		      else
  	          if((((tvParentItem.state >> 12) == 5) && ((tvSiblingItem.state >> 12) == 1)) ||
			     (((tvParentItem.state >> 12) == 1) && ((tvSiblingItem.state >> 12) == 5)))
			  {
  		        tvParentItem.state = INDEXTOSTATEIMAGEMASK(3);
                tvParentItem.lParam = TREEVIEW_PART_CHECK;
			  }
  		      else
  	          if((((tvParentItem.state >> 12) == 5) && ((tvSiblingItem.state >> 12) == 2)) ||
			     (((tvParentItem.state >> 12) == 2) && ((tvSiblingItem.state >> 12) == 5)))
			  {
  		        tvParentItem.state = INDEXTOSTATEIMAGEMASK(2);
                tvParentItem.lParam = TREEVIEW_PART_CHECK;
			  }
			}
		  }

  	      TreeView_SetItem(hCtrl, &tvParentItem);
  	      hSiblingItem = TreeView_GetNextSibling(hCtrl, hSiblingItem);
		}
	  }
	  hBackupParentItem = hParentItem = TreeView_GetParent(hCtrl, hParentItem);
	}
  }

  return;
}
