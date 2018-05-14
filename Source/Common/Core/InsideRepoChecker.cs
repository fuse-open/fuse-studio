namespace Outracks.IO
{
	public static class InsideRepoChecker
	{
		public static bool IsInsideRepo(this IFileSystem shell, AbsoluteDirectoryPath dir)
		{
			while (dir.ContainingDirectory != null)
			{
				if (shell.Exists(dir / new FileName(".fusereporoot")))
				{
					return true;
				}
				dir = dir.ContainingDirectory;
			}
			return false;
		}
	}
}
