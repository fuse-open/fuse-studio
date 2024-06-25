using System;
using Outracks.IO;

namespace Outracks.Fuse.Components
{
	abstract class ScriptInstaller : ComponentInstaller
	{
		readonly AbsoluteDirectoryPath _workingDir;
		readonly AbsoluteFilePath _install;

		protected ScriptInstaller(string name, string description, AbsoluteDirectoryPath componentsDir, string componentName)
			: base(name, description)
		{
			_workingDir = componentsDir / componentName;
			_install = _workingDir / new FileName("install.js");
		}

		protected ScriptInstaller(string name, string description, AbsoluteDirectoryPath componentsDir)
			: this(name, description, componentsDir, name)
		{
		}

		public override void Install()
		{
			try
			{
				var result = RunScript(_install, "", TimeSpan.FromSeconds(60), _workingDir);
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

		public override ComponentStatus Status
		{
			get
			{
				try
				{
					var p = RunScript(_install, "-s", TimeSpan.FromSeconds(10), _workingDir);
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
