using Outracks.Diagnostics;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse
{
	public class ApplicationPaths
	{
		public static Optional<AbsoluteDirectoryPath> SublimeTextPath()
		{
			return Platform.IsMac
				? MacEnvironment.GetPathToApplicationsThatContains("com.sublimetext.3").FirstOrNone()
				: WindowsEnvironment.LookForPathInUninstall("Sublime Text 3").FirstOrNone();
		}

		public static Optional<AbsoluteDirectoryPath> VsCodePath()
		{
			return Platform.IsMac
				? MacEnvironment.GetPathToApplicationsThatContains("com.microsoft.VSCode").FirstOrNone()
				: WindowsEnvironment.LookForPathInPATH("code.cmd").FirstOrNone().Select(path => path.ContainingDirectory)
					.Or(() => WindowsEnvironment.LookForPathInUninstall("Microsoft Visual Studio Code").FirstOrNone());
		}

		public static Optional<AbsoluteDirectoryPath> AtomPath()
		{
			return Platform.IsMac
				? MacEnvironment.GetPathToApplicationsThatContains("com.github.atom").FirstOrNone()
				: WindowsEnvironment.LookForPathInApplications("atom.exe").FirstOrNone().Select(path => path.ContainingDirectory);
		}
	}
}
