using Outracks.IO;

namespace Outracks.Fuse
{
	public static class FileExtensions
	{
		public static bool IsProjectFile(this IAbsolutePath path)
		{
			return path.MatchWith(
				(AbsoluteFilePath file) => file.IsProjectFile(),
				(AbsoluteDirectoryPath dir) => false);
		}

		public static bool IsProjectFile(this AbsoluteFilePath f)
		{
			return f.Name.HasExtension(".unoproj");
		}

		public static bool IsUnoFile(this AbsoluteFilePath f)
		{
			return f.Name.HasExtension(".uno");
		}

		public static bool IsUxFile(this AbsoluteFilePath f)
		{
			return f.Name.HasExtension(".ux");
		}

		public static bool IsExtensionFile(this AbsoluteFilePath f)
		{
			return f.Name.HasExtension(".uxl");
		}

		public static bool IsOtherFile(this AbsoluteFilePath f)
		{
			return !f.IsUnoFile() && !f.IsUxFile() && !f.IsExtensionFile();
		}
	}
}
