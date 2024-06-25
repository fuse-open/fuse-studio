using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Outracks.IO
{
	public class PermissionWin : IFilePermission
	{
		public void SetPermission(AbsoluteFilePath file, FileSystemPermission permission, FileSystemGroup group)
		{
			var identity = GetIdentity(group);

			var fileSecurity = new FileSecurity();
			fileSecurity.SetAccessRule(new FileSystemAccessRule(
						identity,
						CreateSystemRightsFile(permission),
						AccessControlType.Allow));

			File.SetAccessControl(file.NativePath, fileSecurity);
		}

		static IdentityReference GetIdentity(FileSystemGroup group)
		{
			IdentityReference identity;
			if (group.HasFlag(FileSystemGroup.Everyone))
				identity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			else if (group.HasFlag(FileSystemGroup.User))
				identity = WindowsIdentity.GetCurrent().User;
			else
				throw new NotImplementedException("Be free to implement it.");
			return identity;
		}

		FileSystemRights CreateSystemRightsFile(FileSystemPermission permission)
		{
			var fileSystemRights = FileSystemRights.Synchronize;

			if (permission.HasFlag(FileSystemPermission.Read))
				fileSystemRights |= FileSystemRights.Read;

			if (permission.HasFlag(FileSystemPermission.Write))
			{
				fileSystemRights |= FileSystemRights.Write;
			}

			if (permission.HasFlag(FileSystemPermission.Execute))
			{
				fileSystemRights |= FileSystemRights.ExecuteFile;
			}

			return fileSystemRights;
		}

		public void SetPermission(
			AbsoluteDirectoryPath dir,
			FileSystemPermission permission,
			FileSystemGroup group,
			bool recursive)
		{
			var dInfo = new DirectoryInfo(dir.NativePath);
			var dSec = new DirectorySecurity();
			dSec.AddAccessRule(new FileSystemAccessRule(
						GetIdentity(group),
						CreateSystemRightsFile(permission),
						AccessControlType.Allow));
			dInfo.SetAccessControl(dSec);

			if(recursive)
				ReplaceAllDescendantPermissionsFromObject(dInfo, dSec, permission, group);
		}

		void ReplaceAllDescendantPermissionsFromObject(
			DirectoryInfo dInfo,
			DirectorySecurity dSecurity,
			FileSystemPermission fileSystemPermission,
			FileSystemGroup group)
		{
			dInfo.SetAccessControl(dSecurity);

			foreach (FileInfo fi in dInfo.GetFiles())
			{
				SetPermission(AbsoluteFilePath.Parse(fi.FullName), fileSystemPermission, group);
			}

			dInfo.GetDirectories().ToList()
				.ForEach(d => ReplaceAllDescendantPermissionsFromObject(d, dSecurity, fileSystemPermission, group));
		}
	}
}