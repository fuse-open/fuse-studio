using System;
using System.Runtime.InteropServices;

namespace Outracks.Fusion.Windows
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public RECT(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;

		public POINT(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WINDOWPLACEMENT
	{
		public int Length;
		public int Flags;
		public int ShowCmd;
		public POINT MinPosition;
		public POINT MaxPosition;
		public RECT NormalPosition;
	}

	public static class WindowPlacement
	{
		[DllImport("user32.dll")]
		private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;

		public static void SetPlacement(IntPtr windowHandle, WINDOWPLACEMENT winPlacement)
		{
			winPlacement.Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
			winPlacement.Flags = 0;
			winPlacement.ShowCmd = (winPlacement.ShowCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : winPlacement.ShowCmd);
			SetWindowPlacement(windowHandle, ref winPlacement);
		}

		public static WINDOWPLACEMENT GetPlacement(IntPtr windowHandle)
		{
			WINDOWPLACEMENT placement;
			GetWindowPlacement(windowHandle, out placement);
			return placement;
		}
	}
}