using System;

namespace Outracks.IO
{
	public static class BackupFile
	{
		public static BackedUpFile BackupAndDeleteFile(this IFileSystem fileSystem, AbsoluteFilePath path)
		{
			var backup = fileSystem.MakeUnique(path.AddExtension("bak"));
			if (fileSystem.Exists(path))
				fileSystem.Move(path, backup);

			return new BackedUpFile(fileSystem, path, backup);
		}
	}

	public class BackedUpFile : IDisposable
	{
		readonly IFileSystem _fileSystem;
		readonly AbsoluteFilePath _original;
		readonly AbsoluteFilePath _backup;

		public BackedUpFile(IFileSystem fileSystem, AbsoluteFilePath original, AbsoluteFilePath backup)
		{
			_fileSystem = fileSystem;
			_original = original;
			_backup = backup;
		}

		public void Restore()
		{
			if (_fileSystem.Exists(_backup))
			{
				_fileSystem.Delete(_original);
				_fileSystem.Move(_backup, _original);
			}
		}

		public void Dispose()
		{
			if (_fileSystem.Exists(_backup))
			{
				_fileSystem.Delete(_backup);
			}
		}
	}
}