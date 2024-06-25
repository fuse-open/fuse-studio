namespace Outracks.IO
{
	public static class CombineWithName
	{
		public static AbsoluteFilePath Combine(this AbsoluteDirectoryPath basePath, FileName fileName)
		{
			return new AbsoluteFilePath(fileName, basePath);
		}

		public static RelativeFilePath Combine(this RelativeDirectoryPath basePath, FileName fileName)
		{
			return new RelativeFilePath(fileName, basePath);
		}

		public static AbsoluteDirectoryPath Combine(this AbsoluteDirectoryPath basePath, DirectoryName directoryName)
		{
			if (directoryName.IsCurrentDirectory)
				return basePath;

			if (directoryName.IsParentDirectory)
				return basePath.ContainingDirectory;

			return new AbsoluteDirectoryPath(directoryName, basePath);
		}

		public static RelativeDirectoryPath Combine(this RelativeDirectoryPath basePath, DirectoryName directoryName)
		{
			if (directoryName.IsCurrentDirectory)
				return basePath;

			if (directoryName.IsParentDirectory && basePath.HasContainingDirectory())
				return basePath.GetContainingDirectory();

			return new RelativeDirectoryPath(directoryName, basePath);
		}

		static bool HasContainingDirectory(this RelativeDirectoryPath path)
		{
			return path != null && !path.Name.IsParentDirectory;
		}

		static RelativeDirectoryPath GetContainingDirectory(this RelativeDirectoryPath path)
		{
			return path.BasePath;
		}
	}

	public partial class AbsoluteDirectoryPath
	{
		public static AbsoluteFilePath operator /(AbsoluteDirectoryPath containingDir, FileName fileName)
		{
			return containingDir.Combine(fileName);
		}

		public static AbsoluteDirectoryPath operator /(AbsoluteDirectoryPath containingDir, DirectoryName fileName)
		{
			return containingDir.Combine(fileName);
		}
	}

	public partial class RelativeDirectoryPath
	{
		public static RelativeFilePath operator /(RelativeDirectoryPath containingDir, FileName fileName)
		{
			return containingDir.Combine(fileName);
		}

		public static RelativeDirectoryPath operator /(RelativeDirectoryPath containingDir, DirectoryName fileName)
		{
			return containingDir.Combine(fileName);
		}
	}

}
