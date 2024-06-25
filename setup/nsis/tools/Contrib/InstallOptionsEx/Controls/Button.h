int CALLBACK BrowseCallbackProc(HWND hwnd, UINT uMsg, LPARAM lp, LPARAM lpData) {
  if (uMsg == BFFM_INITIALIZED && lpData)
    mySendMessage(hwnd, BFFM_SETSELECTION, TRUE, (LPARAM)lpData);

  return 0;
}
