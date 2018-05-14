using System.Diagnostics;

namespace Outracks.Fuse.Setup
{
	class SublimeAppStatus : SoftwareStatus
	{
		public SublimeAppStatus(IReport report)
			: base("Sublime Text 3", report)
		{
		}

		protected override InstallStatus CheckStatus()
		{
			return ApplicationPaths.SublimeTextPath().HasValue ? InstallStatus.Installed : InstallStatus.NotInstalled;
		}

		protected override void TryInstall()
		{
			Process.Start("https://go.fusetools.com/sublime-text-3");
		}
	}
}