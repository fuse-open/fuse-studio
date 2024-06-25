namespace Outracks.IO
{
	public static class RelativeToExtension
	{
		public static IAbsolutePath Rebase(this IAbsolutePath path, AbsoluteDirectoryPath from, AbsoluteDirectoryPath onto)
		{
			return onto.Combine(path.RelativeTo(from));
		}

		public static AbsoluteFilePath Rebase(this AbsoluteFilePath path, AbsoluteDirectoryPath from, AbsoluteDirectoryPath onto)
		{
			return onto / path.RelativeTo(from);
		}

		public static AbsoluteDirectoryPath Rebase(this AbsoluteDirectoryPath path, AbsoluteDirectoryPath from, AbsoluteDirectoryPath onto)
		{
			return onto / path.RelativeTo(from);
		}

		public static IRelativePath RelativeTo(this IAbsolutePath destination, AbsoluteDirectoryPath source)
		{
			return destination.MatchWith(
				(AbsoluteFilePath file) => (IRelativePath)file.RelativeTo(source),
				(AbsoluteDirectoryPath dir) => (IRelativePath)dir.RelativeTo(source));
		}

		public static RelativeFilePath RelativeTo(this AbsoluteFilePath destination, AbsoluteDirectoryPath source)
		{
			return destination.ContainingDirectory.RelativeTo(source) / destination.Name;
		}

		public static RelativeDirectoryPath RelativeTo(this AbsoluteDirectoryPath destination, AbsoluteDirectoryPath source)
		{
			var commonAncestor = AbsoluteDirectoryPath.FindCommonAncestor(source, destination);

			var sourceToAncestor = source.ToAncestor(commonAncestor);

			var sourceToDestination = sourceToAncestor.ToLeaf(commonAncestor, destination);

			return sourceToDestination;
		}

		static RelativeDirectoryPath ToAncestor(this AbsoluteDirectoryPath leaf, AbsoluteDirectoryPath ancestor)
		{
			return leaf == ancestor ? null : new RelativeDirectoryPath(DirectoryName.ParentDirectory, leaf.ContainingDirectory.ToAncestor(ancestor));
		}

		static RelativeDirectoryPath ToLeaf(this RelativeDirectoryPath self, AbsoluteDirectoryPath ancestor, AbsoluteDirectoryPath leaf)
		{
			return leaf == ancestor ? self : new RelativeDirectoryPath(leaf.Name, self.ToLeaf(ancestor, leaf.ContainingDirectory));
		}
	}

}
