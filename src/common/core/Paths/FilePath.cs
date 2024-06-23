using System.IO;

namespace Outracks.IO
{
	public static class FilePath
	{
		/// <exception cref="System.IO.IOException" />
		public static AbsoluteFilePath CreateTempFile()
		{
			return AbsoluteFilePath.Parse(Path.GetTempFileName());
		}

		public static AbsoluteFilePath ParseAndMakeAbsolute(string path)
		{
			return ParseAndMakeAbsolute(path, DirectoryPath.GetCurrentDirectory());
		}

		public static AbsoluteFilePath ParseAndMakeAbsolute(IFilePath path, AbsoluteDirectoryPath root)
		{
			return path as AbsoluteFilePath ?? ParseAndMakeAbsolute(((RelativeFilePath) path).NativeRelativePath, root);
		}

		public static AbsoluteFilePath ParseAndMakeAbsolute(string path, AbsoluteDirectoryPath root)
		{
			var filePath = Parse(path);
			return filePath.MatchWith(
				(AbsoluteFilePath absolutePath) => absolutePath,
				(RelativeFilePath relativePath) => root / relativePath);
		}

		public static IFilePath Parse(string relativeOrAbsolutePath)
		{
			return Path.IsPathRooted(relativeOrAbsolutePath)
				? (IFilePath) AbsoluteFilePath.Parse(relativeOrAbsolutePath)
				: (IFilePath) RelativeFilePath.Parse(relativeOrAbsolutePath);

		}
	}
}