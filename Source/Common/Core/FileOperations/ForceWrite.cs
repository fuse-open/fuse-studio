using System;

namespace Outracks.IO
{
	public static class ForceWrite
	{
		public static void ForceWriteText(this IFileSystem fileSystem, AbsoluteFilePath dst, string contents)
		{
			// TODO: What does this do, or why?
			using (var backup = fileSystem.BackupAndDeleteFile(dst))
			{
				try
				{
					fileSystem.Create(dst.ContainingDirectory);
					fileSystem.WriteNewText(dst, contents);
				}
				catch (Exception)
				{
					backup.Restore();
					throw;
				}
			}
		}
	}
}