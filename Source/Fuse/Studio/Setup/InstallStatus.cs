using System;
using Outracks.Fuse.Components;

namespace Outracks.Fuse.Setup {

	public enum InstallStatus
	{
		Unknown,
		NotInstalled,
		UpdateAvailable,
		Installed,
		Installing
	}

	static class ComponentStatusExtensions
	{
		public static InstallStatus AsInstallStatus(this ComponentStatus status)
		{
			switch (status)
			{
				case ComponentStatus.NotInstalled:
					return InstallStatus.NotInstalled;
				case ComponentStatus.Installed:
					return InstallStatus.Installed;
				case ComponentStatus.UpdateAvailable:
					return InstallStatus.UpdateAvailable;
				default:
					throw new Exception("Unknown ComponentStatus " + status);
			}
		}
	}
}