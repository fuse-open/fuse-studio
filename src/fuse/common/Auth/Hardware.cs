using Outracks.Fuse.Auth.Utilities;
using System;
using Uno.CLI;
using Uno.Diagnostics;

namespace Outracks.Fuse.Auth
{
	public static class Hardware
	{
		public static readonly string UID = GenerateUID();

		static string GenerateUID()
		{
			if (PlatformDetection.IsMac)
			{
				return Shell.Default
					.GetOutput("system_profiler", "SPHardwareDataType")
					.Grep("Hardware UUID:")[0].Split(':')[1].Trim();
			}
			else if (PlatformDetection.IsWindows)
			{
				return Win32HardwareInfo.GenerateUID("fuse X");
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
