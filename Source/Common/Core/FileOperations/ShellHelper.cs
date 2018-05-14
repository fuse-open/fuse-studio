using System;
using System.IO;

namespace Outracks.IO
{
	public static class ShellHelper
	{
		public static bool TryPureMove<T>(T source, T destination, Action<string, string> moveOp) where T : IAbsolutePath
		{
			if (!source.SharesRootWith(destination))
				return false; // pure move does not work across volumes

			try
			{
				moveOp(source.NativePath, destination.NativePath);
				return true;
			}
			catch (IOException e)
			{
				if ((UInt32)e.HResult == 0x800700B7) // Cannot create a file when that file already exists.
					throw;

				return false;
			}
			catch (Exception)
			{
				// TODO: log this?			
				return false;
			}
		}

		public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			var dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			var dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			var files = dir.GetFiles();
			foreach (var file in files)
			{
				var temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs)
			{
				foreach (var subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}
	}
}