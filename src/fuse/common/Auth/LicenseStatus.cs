namespace Outracks.Fuse.Auth
{
	public enum LicenseStatus
	{
		OK,
		Unregistered,
		Offline,
		Expired,
	}

	public static class LicenseStatusExtensions
	{
		public static bool IsValid(this LicenseStatus status)
		{
			return status == LicenseStatus.OK || status == LicenseStatus.Offline;
		}
	}
}
