using System.IO;

namespace Outracks.IO
{
	public static class DirectoryPath
	{
		/// <exception cref="System.UnauthorizedAccessException" />
		public static AbsoluteDirectoryPath GetCurrentDirectory()
		{
			return AbsoluteDirectoryPath.Parse(Directory.GetCurrentDirectory());
		}

		/// <exception cref="System.Security.SecurityException" />
		public static AbsoluteDirectoryPath GetTempPath()
		{
			return AbsoluteDirectoryPath.Parse(Path.GetTempPath());
		}

		public static IDirectoryPath Parse(string relativeOrAbsolutePath)
		{
			return Path.IsPathRooted(relativeOrAbsolutePath)
				? (IDirectoryPath)AbsoluteDirectoryPath.Parse(relativeOrAbsolutePath)
				: (IDirectoryPath)RelativeDirectoryPath.Parse(relativeOrAbsolutePath);

		}
	}
}