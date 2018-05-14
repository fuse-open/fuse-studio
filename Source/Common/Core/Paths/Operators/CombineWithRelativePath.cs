namespace Outracks.IO
{
	public static class CombineWithRelativePath
	{
		public static IAbsolutePath Combine(this AbsoluteDirectoryPath basePath, IRelativePath path)
		{
			return path == null 
				? basePath 
				: path.MatchWith(
					(RelativeFilePath file) => (IAbsolutePath)basePath.Combine(file),
					(RelativeDirectoryPath dir) => (IAbsolutePath)basePath.Combine(dir));
		}

		public static IRelativePath Combine(this RelativeDirectoryPath basePath, IRelativePath path)
		{
			return path == null 
				? basePath 
				: path.MatchWith(
					(RelativeFilePath file) => (IRelativePath)basePath.Combine(file),
					(RelativeDirectoryPath dir) => (IRelativePath)basePath.Combine(dir));
		}

		public static AbsoluteFilePath Combine(this AbsoluteDirectoryPath basePath, RelativeFilePath path)
		{
			return basePath.Combine(path.BasePath).Combine(path.Name);
		}

		public static RelativeFilePath Combine(this RelativeDirectoryPath basePath, RelativeFilePath path)
		{
			return basePath.Combine(path.BasePath).Combine(path.Name);
		}

		public static AbsoluteDirectoryPath Combine(this AbsoluteDirectoryPath basePath, RelativeDirectoryPath relativePath)
		{
			return relativePath == null ? basePath : basePath.Combine(relativePath.BasePath).Combine(relativePath.Name);
		}

		public static RelativeDirectoryPath Combine(this RelativeDirectoryPath basePath, RelativeDirectoryPath relativePath)
		{
			return relativePath == null ? basePath : basePath.Combine(relativePath.BasePath).Combine(relativePath.Name);
		}
	}

	public partial class AbsoluteDirectoryPath
	{
		public static IAbsolutePath operator /(AbsoluteDirectoryPath containingDir, IRelativePath relativeFilePath)
		{
			return containingDir.Combine(relativeFilePath);
		}

		public static AbsoluteFilePath operator /(AbsoluteDirectoryPath containingDir, RelativeFilePath relativeFilePath)
		{
			return containingDir.Combine(relativeFilePath);
		}

		public static AbsoluteDirectoryPath operator /(AbsoluteDirectoryPath containingDir, RelativeDirectoryPath relativeDirectoryPath)
		{
			return containingDir.Combine(relativeDirectoryPath);
		}
	}

	public partial class RelativeDirectoryPath
	{
		public static IRelativePath operator /(RelativeDirectoryPath containingDir, IRelativePath relativeFilePath)
		{
			return containingDir.Combine(relativeFilePath);
		}

		public static RelativeFilePath operator /(RelativeDirectoryPath containingDir, RelativeFilePath relativeFilePath)
		{
			return containingDir.Combine(relativeFilePath);
		}

		public static RelativeDirectoryPath operator /(RelativeDirectoryPath containingDir, RelativeDirectoryPath relativeDirectoryPath)
		{
			return containingDir.Combine(relativeDirectoryPath);
		}
	}

}
