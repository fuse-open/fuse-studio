using System;
using System.Text.RegularExpressions;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Fuse.Components
{
	public class AtomPlugin : ComponentInstaller
	{
		public AtomPlugin() : base("atom-plugin", "Install Fuse plugin for Atom.") {}

		public override void Install()
		{
				try
				{
					var apmPath = GetApmPath();
					var result = RunProcess(apmPath, "install fuse", TimeSpan.FromSeconds(60));
					if (result.ExitCode != 0)
					{
						throw new Exception("'" + apmPath + "' exited with code='" + result.ExitCode + "', stdout='" + result.StdOut + "', stderr='" + result.StdErr + "'");
					}
				}
				catch (Exception e)
				{
					throw new PluginInstallerFailed(e.Message);
				}
		}

		public override ComponentStatus Status
		{
			get
			{
				try
				{
					var apmPath = GetApmPath();
					var result = RunProcess(apmPath, "list -i --bare", TimeSpan.FromSeconds(10));
					if (result.ExitCode != 0)
					{
						throw new Exception("'" + apmPath + "' exited with code='" + result.ExitCode + "', stdout='" + result.StdOut + "', stderr='" + result.StdErr + "'");
					}
					return new Regex("(^|\\n)fuse@").IsMatch(result.StdOut) ? ComponentStatus.Installed : ComponentStatus.NotInstalled;
				}
				catch (Exception e)
				{
					throw new PluginInstallerFailed(e.Message);
				}
			}
		}

		static AbsoluteDirectoryPath GetApmPath()
		{
			if (Platform.IsWindows)
			{
				var atomPath = ApplicationPaths.AtomPath();
				if (!atomPath.HasValue)
				{
					//It's not possible to detect the Atom path on Windows 7, so show a message in that case
					var os = Environment.OSVersion;
					var win7Info = (os.Platform == PlatformID.Win32NT && os.Version.Major == 6 && os.Version.Minor == 0)
						? " Note that we are unable to detect the Atom installation path on Windows 7."
						: "";
					throw new Exception("Failed to find Atom path. Is Atom installed?" + win7Info);
				}
				return atomPath.Value / ".." / "bin" / "apm.cmd";
			}
			else
			{
				return AbsoluteDirectoryPath.Parse("apm");
			}
		}
	}
}
