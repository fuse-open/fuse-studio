using Outracks.Diagnostics;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse
{
	public class ApplicationPaths
	{
		public static Optional<AbsoluteDirectoryPath> SublimeTextPath()
		{
			return Platform.OperatingSystem == OS.Mac
				? MacEnvironment.GetPathToApplicationsThatContains("com.sublimetext.3").FirstOrNone()
				: WindowsEnvironment.LookForPathInUninstall("Sublime Text Build").FirstOrNone();
		}

		public static Optional<AbsoluteDirectoryPath> VsCodePath()
		{
			return Platform.OperatingSystem == OS.Mac
				? MacEnvironment.GetPathToApplicationsThatContains("com.microsoft.VSCode").FirstOrNone()
				: WindowsEnvironment.LookForPathInUninstall("Microsoft Visual Studio Code").FirstOrNone();
		}

		public static Optional<AbsoluteDirectoryPath> AtomPath()
		{
			return Platform.OperatingSystem == OS.Mac
				? MacEnvironment.GetPathToApplicationsThatContains("com.github.atom").FirstOrNone()
				: WindowsEnvironment.LookForPathInApplications("atom.exe").FirstOrNone().Select(path => path.ContainingDirectory);
		}
	}
}
