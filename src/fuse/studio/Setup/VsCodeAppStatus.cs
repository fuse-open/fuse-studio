using System.Diagnostics;

namespace Outracks.Fuse.Setup
{
	class VsCodeAppStatus : SoftwareStatus
	{
		public VsCodeAppStatus(IReport report)
			: base("Visual Studio Code", report)
		{
		}

		protected override InstallStatus CheckStatus()
		{
			return ApplicationPaths.VsCodePath().HasValue ? InstallStatus.Installed : InstallStatus.NotInstalled;
		}

		protected override void TryInstall()
		{
			Process.Start("https://code.visualstudio.com/");
		}
	}
}
