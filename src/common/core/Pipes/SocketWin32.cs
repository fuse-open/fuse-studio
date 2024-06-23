using System;
using System.Runtime.InteropServices;

namespace Outracks
{
	public static class SocketWin32
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags);

		[Flags]
		public enum HANDLE_FLAGS : uint
		{
			None = 0,
			INHERIT = 1,
			PROTECT_FROM_CLOSE = 2
		}
	}
}