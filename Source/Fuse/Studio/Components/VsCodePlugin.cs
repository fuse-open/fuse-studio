using System;
using System.Text.RegularExpressions;
using Outracks.Fuse.Setup;

namespace Outracks.Fuse.Components 
{
	using Diagnostics;
	using IO;

	public class VsCodePlugin : ComponentInstaller
	{
		public VsCodePlugin() : base("vscode-plugin", "Install Visual Studio Code plugin for Fuse.") {}

		public override void Install()
		{
			try
			{
				var codePath = GetVsCodePath();
				var result = RunProcess(codePath, "--install-extension iGN97.fuse-vscode", TimeSpan.FromSeconds(60));
				if (result.ExitCode != 0)
				{
					throw new Exception("'" + codePath + "' exited with code='" + result.ExitCode + "', stdout='" + result.StdOut + "', stderr='" + result.StdErr + "'");
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
					var codePath = GetVsCodePath();
					var result = RunProcess(codePath, "--list-extensions", TimeSpan.FromSeconds(10));
					if (result.ExitCode != 0)
					{
						throw new Exception("'" + codePath + "' exited with code='" + result.ExitCode + "', stdout='" + result.StdOut + "', stderr='" + result.StdErr + "'");
					}
					return new Regex("(^|\\n)iGN97.fuse-vscode\n").IsMatch(result.StdOut) ? ComponentStatus.Installed : ComponentStatus.NotInstalled;
				}
				catch (Exception e)
				{
					throw new PluginInstallerFailed(e.Message);
				}
			}
		}

		static AbsoluteDirectoryPath GetVsCodePath()
		{
			var vsCodePath = ApplicationPaths.VsCodePath();
			if (!vsCodePath.HasValue)
			{
				throw new Exception("Failed to find Visual Studio Code path. Is Visual Studio Code installed?");
			}
			return Platform.OperatingSystem == OS.Windows ?
				vsCodePath.Value / "bin" / "code.cmd" :
				vsCodePath.Value / "Contents" / "Resources" / "app" / "bin" / "code";
		}
	}
}
