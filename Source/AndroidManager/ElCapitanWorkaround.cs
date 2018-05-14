using System;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	public class ElCapitanWorkaround
	{
		public static bool IsInvalid(IAbsolutePath path)
		{
			return Platform.OperatingSystem == OS.Mac 
				&& Platform.OperatingSystemVersion >= Platform.ElCapitan
				&& path.IsOrIsRootedIn(AbsoluteDirectoryPath.Parse("/usr/share"));
		}

		public static bool IsInvalidWithMessage(IProgress<InstallerEvent> progress, IAbsolutePath path)
		{
			if (IsInvalid(path))
			{
				progress.Report(new InstallerMessage("'" + path.NativePath + "'" + " is rooted in '/usr/share', which is disallowed in El Capitan (10.11) and later versions of OS X."));
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}