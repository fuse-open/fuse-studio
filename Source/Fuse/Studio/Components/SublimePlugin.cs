using System;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Fuse.Components
{
	class SublimePlugin : ComponentInstaller
	{
		readonly AbsoluteDirectoryPath _modulesPath;
		readonly AbsoluteFilePath _installScript;

		public SublimePlugin(AbsoluteDirectoryPath fuseModulesDir)
			: base("sublime-plugin", "Install Sublime Text plugin for Fuse")
		{
			_modulesPath = fuseModulesDir / "SublimePlugin";
			var scriptName = (Platform.OperatingSystem == OS.Windows ? "install.bat" : "install.sh");
			_installScript = _modulesPath / new FileName(scriptName);
		}

		public override void Install()
		{
			{
				try
				{
					var result = RunProcess(_installScript, "", TimeSpan.FromSeconds(60), _modulesPath);
					if (result.ExitCode != 0)
					{
						throw new Exception("Installation script exited with code='" + result.ExitCode + "', stdout='" + result.StdOut + "', stderr='" + result.StdErr + "'");
					}
				}
				catch (Exception e)
				{
					throw new PluginInstallerFailed(e.Message);
				}
			}
		}

		public override ComponentStatus Status
		{
			get
			{
				try
				{
					var p = RunProcess(_installScript, "-s", TimeSpan.FromSeconds(10), _modulesPath);
					if (!Enum.IsDefined(typeof(ComponentStatus), p.ExitCode))
						throw new PluginInstallerFailed("Unknown install status from " + Name + ": exit code='" + p.ExitCode + "', stdout='" + p.StdOut + "', stderr='" + p.StdErr + "'");

					return (ComponentStatus)p.ExitCode;
				}
				catch (Exception e)
				{
					throw new PluginInstallerFailed(e.Message);
				}
			}
		}
	}
}
