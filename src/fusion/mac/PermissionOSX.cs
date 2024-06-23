using System.IO;
using Mono.Unix.Native;

namespace Outracks.IO
{
	public class PermissionOSX : IFilePermission
	{
		public void SetPermission(
			AbsoluteFilePath file,
			FileSystemPermission permission,
			FileSystemGroup group)
		{
			var permissionWithGroupBits = GetPermissionBits(permission, @group);

			Syscall.chmod(file.NativePath, (FilePermissions)permissionWithGroupBits);
		}

		public void SetPermission(
			AbsoluteDirectoryPath dir,
			FileSystemPermission permission,
			FileSystemGroup group,
			bool recursive)
		{
			var permissionBits = GetPermissionBits(permission, group);
			Syscall.chmod(dir.NativePath, (FilePermissions)permissionBits);

			if (recursive)
			{
				foreach (var file in Directory.GetFiles(dir.NativePath))
				{
					var filePermission = (int)permission &~ (int)FileSystemPermission.Execute;
					SetPermission(AbsoluteFilePath.Parse(file), (FileSystemPermission)filePermission, group);
				}

				foreach (var directory in Directory.GetDirectories(dir.NativePath))
				{
					SetPermission(AbsoluteDirectoryPath.Parse(directory), permission, group, recursive);
				}
			}
		}

		static int GetPermissionBits(FileSystemPermission permission, FileSystemGroup @group)
		{
			var permissionBits = 0;
			if (permission.HasFlag(FileSystemPermission.Execute))
				permissionBits |= (int)FilePermissions.S_IXOTH;
			if (permission.HasFlag(FileSystemPermission.Read))
				permissionBits |= (int)FilePermissions.S_IROTH;
			if (permission.HasFlag(FileSystemPermission.Write))
				permissionBits |= (int)FilePermissions.S_IWOTH;

			var permissionWithGroupBits = 0;
			if (group.HasFlag(FileSystemGroup.Others))
				permissionWithGroupBits |= permissionBits;

			if (group.HasFlag(FileSystemGroup.Group))
				permissionWithGroupBits |= permissionBits << 3;

			if (group.HasFlag(FileSystemGroup.User))
				permissionWithGroupBits |= permissionBits << 6;

			return permissionWithGroupBits;
		}
	}
}