using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Outracks.Diagnostics
{
	public enum OS
	{
		Windows,
		Mac,
		X11,
		Other
	}

	public static class Platform
	{
		public static Version WindowsVista = new Version(6, 0);
		public static Version Windows8_1 = new Version(6, 3);
		public static Version ElCapitan = new Version(10, 11);
		public static readonly OS OperatingSystem;
		public static readonly Version OperatingSystemVersion;

		static Platform ()
		{
			if (Path.DirectorySeparatorChar == '\\')
			{
				OperatingSystem = OS.Windows;
				OperatingSystemVersion = Environment.OSVersion.Version;
			}
			else if (IsRunningOnMac())
			{
				OperatingSystem = OS.Mac;
				OperatingSystemVersion = GetOperatingSystemVersionOSX();
			}
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				OperatingSystem = OS.X11;
			}
			else
				OperatingSystem = OS.Other;
		}

		[DllImport("libc")]
		static extern int uname(IntPtr buf);

		// Copied from https://github.com/jpobst/Pinta/blob/master/Pinta.Core/Managers/SystemManager.cs
		static bool IsRunningOnMac ()
		{			
			IntPtr buf = IntPtr.Zero;
			try 
			{
				buf = Marshal.AllocHGlobal (8192);
				// This is a hacktastic way of getting sysname from uname ()
				if (uname (buf) == 0) 
				{
					string os = Marshal.PtrToStringAnsi (buf);
					if (os == "Darwin")
						return true;
				}
			} 
			catch 
			{
			} 
			finally 
			{
				if (buf != IntPtr.Zero)
					Marshal.FreeHGlobal (buf);
			}
			
			return false;
		}

		public static Version GetOperatingSystemVersionOSX()
		{
			var swProductVersion = Process.Start(new ProcessStartInfo()
			{
				UseShellExecute = false,
				RedirectStandardOutput = true,
				FileName = "sw_vers",
				Arguments = "-productVersion",
			});

			var version = swProductVersion.StandardOutput.ReadToEnd().TrimEnd();

			return new Version(version);
		}
	}
}