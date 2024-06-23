using Outracks.Fuse.Components;

namespace Outracks.Fuse.Setup
{
	class VsCodeExtensionStatus : SoftwareStatus
	{
		readonly object _fuseInstallLock = new object();
		readonly VsCodeExtension _vsCodeExtension;

		public VsCodeExtensionStatus(IFuse fuse)
			: base("Visual Studio Code Extension", fuse.Report)
		{
			_vsCodeExtension = new VsCodeExtension(fuse.ComponentsDir);
		}

		protected override InstallStatus CheckStatus()
		{
			lock (_fuseInstallLock)
			{
				return _vsCodeExtension.Status.AsInstallStatus();
			}
		}

		protected override void TryInstall()
		{
			SetStatus(InstallStatus.Installing);
			lock (_fuseInstallLock)
			{
				SetStatus(InstallStatus.Installing); //Set it again, since a pending update might have changed it
				_vsCodeExtension.Install();
				SetStatus(_vsCodeExtension.Status.AsInstallStatus());
			}
		}
	}
}
