#ifndef __NSARRAY_H__
#define __NSARRAY_H__

#define NSISFUNC(name) void __declspec(dllexport) name(HWND hWndParent, int string_size, TCHAR* variables, stack_t** stacktop, extra_parameters* extra)
#define DLL_INIT() EXDLL_INIT(); extra->RegisterPluginCallback((HMODULE)g_hInstance, PluginCallback)

#endif