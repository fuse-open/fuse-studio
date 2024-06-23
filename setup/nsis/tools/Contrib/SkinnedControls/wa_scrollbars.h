/* wa_scrollbars.h */
#ifndef WA_SCROLLBARS_INCLUDED
#define WA_SCROLLBARS_INCLUDED

#ifdef __cplusplus
extern "C" {
#endif

int wascrollbars_initapp(void);
void wascrollbars_uninitapp(void);

int wascrollbars_init(HWND hwnd);
void wascrollbars_uninit(HWND hwnd);

BOOL wascrollbars_handleMessages(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam, LRESULT* pResult);

#ifdef __cplusplus
}
#endif

#endif