using Outracks.Fuse.Components;

namespace Outracks.Fuse.Setup
{
	class SublimePluginStatus : SoftwareStatus
	{
		readonly object _fuseInstallLock = new object();
		readonly SublimePlugin _sublimePlugin;

		public SublimePluginStatus(IFuse fuse)
			: base("Sublime Fuse Plugin", fuse.Report)
		{
			_sublimePlugin = new SublimePlugin(fuse.ComponentsDir);
		}

		protected override InstallStatus CheckStatus()
		{
			lock (_fuseInstallLock)
			{
				return _sublimePlugin.Status.AsInstallStatus();
			}
		}

		protected override void TryInstall()
		{
			SetStatus(InstallStatus.Installing);
			lock (_fuseInstallLock)
			{
				SetStatus(InstallStatus.Installing); //Set it again, since a pending update might have changed it
				_sublimePlugin.Install();
				SetStatus(_sublimePlugin.Status.AsInstallStatus());
			}
		}
	}
}