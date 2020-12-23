using System;
using System.Runtime.InteropServices;

namespace Outracks.UnoHost.Windows
{
	static class Egl
	{
		[DllImport("libEGL.dll", EntryPoint = "eglGetCurrentSurface")]
		public static extern IntPtr GetCurrentSurface(int readdraw);

		[DllImport("libEGL.dll", EntryPoint = "eglQuerySurfacePointerANGLE")]
		public static extern bool QuerySurfacePointer(IntPtr display, IntPtr surface, int attribute, ref IntPtr outSurface);

		[DllImport("libEGL.dll", EntryPoint = "eglQuerySurface")]
		public static extern bool QuerySurface(IntPtr display, IntPtr surface, int attribute, ref int value);

		[DllImport("libEGL.dll", EntryPoint = "eglGetCurrentDisplay")]
		public static extern IntPtr GetCurrentDisplay();

		[DllImport("libEGL.dll", EntryPoint = "eglGetConfigs")]
		public static extern bool GetConfigs(IntPtr display, IntPtr[] configs, int configSize, ref int numConfigs);

		[DllImport("libEGL.dll", EntryPoint = "eglGetConfigAttrib")]
		public static extern bool GetConfigAttrib(IntPtr display, IntPtr config, int attrib, ref int value);

		[DllImport("libEGL.dll", EntryPoint = "eglChooseConfig")]
		public static extern bool ChooseConfig(IntPtr display, int[] attribList, IntPtr[] configs, int configSize, ref int numConfigs);

		[DllImport("libEGL.dll", EntryPoint = "eglCreatePbufferFromClientBuffer")]
		public static extern IntPtr CreatePbufferFromClientBuffer(IntPtr display, int buftype, IntPtr buffer, IntPtr config, int[] attribList);

		[DllImport("libEGL.dll", EntryPoint = "eglBindTexImage")]
		public static extern void BindTexImage(IntPtr display, IntPtr surface, int buffer);

		[DllImport("libEGL.dll", EntryPoint = "eglCreateContext")]
		public static extern IntPtr CreateContext(IntPtr display, IntPtr[] configs, IntPtr shareContext, int[] attribList);

		[DllImport("libEGL.dll", EntryPoint = "eglGetError")]
		public static extern int GetError();

		public const int EGL_NONE = 0x3038;
		public const int EGL_WIDTH = 0x3057;
		public const int EGL_HEIGHT = 0x3056;
		public const int EGL_TEXTURETARGET = 0x3081;
		public const int EGL_TEXTURE_2D = 0x305F;
		public const int EGL_TEXTURE_FORMAT = 0x3080;
		public const int EGL_TEXTURE_RGBA = 0x305E;
		public const int EGL_COLOR_BUFFER_TYPE = 0x303F;
		public const int EGL_ALPHA_SIZE = 0x3021;
		public const int EGL_RED_SIZE = 0x3024;
		public const int EGL_GREEN_SIZE = 0x3023;
		public const int EGL_BLUE_SIZE = 0x3022;
		public const int EGL_SURFACE_TYPE = 0x3033;
		public const int EGL_PBUFFER_BIT = 0x0001;
		public const int EGL_BACK_BUFFER = 0x3084;
		public const int EGL_D3D_TEXTURE_2D_SHARE_HANDLE_ANGLE = 0x3200;
	}
}