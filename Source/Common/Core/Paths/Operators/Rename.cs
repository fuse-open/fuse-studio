namespace Outracks.IO
{
	public static class RenamePath
	{
		public static IAbsolutePath Rename(this IAbsolutePath path, string newName)
		{
			return path.MatchWith(
				(AbsoluteFilePath file) => (IAbsolutePath)file.Rename(new FileName(newName)),
				(AbsoluteDirectoryPath dir) => (IAbsolutePath)dir.Rename(new DirectoryName(newName)));
		}

		public static IRelativePath Rename(this IRelativePath path, string newName)
		{
			return path.MatchWith(
				(RelativeFilePath file) => (IRelativePath)file.Rename(new FileName(newName)),
				(RelativeDirectoryPath dir) => (IRelativePath)dir.Rename(new DirectoryName(newName)));
		}

		public static RelativeFilePath Rename(this RelativeFilePath path, FileName newName)
		{
			return new RelativeFilePath(newName, path.BasePath);
		}

		public static AbsoluteFilePath Rename(this AbsoluteFilePath path, FileName newName)
		{
			return new AbsoluteFilePath(newName, path.ContainingDirectory);
		}

		public static RelativeDirectoryPath Rename(this RelativeDirectoryPath path, DirectoryName newName)
		{
			return new RelativeDirectoryPath(newName, path.BasePath);
		}

		public static AbsoluteDirectoryPath Rename(this AbsoluteDirectoryPath path, DirectoryName newName)
		{
			return new AbsoluteDirectoryPath(newName, path.ContainingDirectory);
		}
	}
}
