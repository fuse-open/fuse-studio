using System;
using Outracks.AndroidManager;
using Outracks.Fuse.Setup;

namespace Outracks.Fuse.Components
{
	using IO;

	public class AndroidInstaller : ComponentInstaller
	{
		public AndroidInstaller()
			: base("android", "Install all required components to build for Android.") 
		{}

		public override void Install()
		{
			var shell = new Shell();
			var configLoader = new ConfigLoader(shell);
			var progress = new ConsoleInstallProgress();
			var dialog = new ConsoleDialog(ColoredTextWriter.Out, Console.In);

			try
			{
				new InstallCommand(shell, configLoader, dialog, progress).Run(new InstallOptions
				{
					Noninteractive = false //TODO: not sure about this args.Contains("-i") || args.Contains("--non-interactive")
				});
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
					var shell = new Shell();
					var configLoader = new ConfigLoader(shell);
					return new VerifyCommand(shell, configLoader, new ConsoleInstallProgress()).Run() ? ComponentStatus.Installed : ComponentStatus.NotInstalled;
				}
				catch (Exception e)
				{
					throw new PluginInstallerFailed(e.Message);
				}
			}
		}
	}
}
