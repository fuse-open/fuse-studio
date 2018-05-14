using System;
using System.IO;
using System.Threading;
using Outracks.IO;

namespace Fuse.Preview
{
	public class LockFile : IDisposable
	{
		readonly AbsoluteFilePath _path;
		readonly FileStream _handle;

		public LockFile(AbsoluteFilePath path)
		{
			_path = path;
			_handle = new FileStream(path.NativePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
			if (_handle.Length < 1)
			{
				_handle.WriteByte((byte)'f');
				_handle.Flush();
			}
			_handle.Lock(0, 1); // Actually lock a portion of the file
		}

		public void Dispose()
		{
			_handle.Dispose();
		}
	}

	public class OutputDirWithLock
	{
		public readonly AbsoluteDirectoryPath OutputDir;
		public readonly LockFile LockFile;

		public OutputDirWithLock(AbsoluteDirectoryPath outputDir, LockFile lockFile)
		{
			OutputDir = outputDir;
			LockFile = lockFile;
		}
	}

	public class FailedToCreateOutputDir : Exception
	{
		public FailedToCreateOutputDir(AbsoluteDirectoryPath basePath, Exception innerException) 
			: base("Failed to create or reuse output dir in '" + basePath.NativePath + "'" + ": " + innerException.Message, innerException)
		{			
		}
	}

	public class OutputDirGenerator 
	{
		readonly IFileSystem _fileSystem;

		public OutputDirGenerator(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;			
		}

		/// <exception cref="FailedToCreateOutputDir"></exception>
		public OutputDirWithLock CreateOrReuseOutputDir(AbsoluteDirectoryPath baseDir)
		{
			try
			{
				const int fileInUse = 0x20;
				const int fileLocked = 0x21;

				LockFile lockFile = null;
				var path = _fileSystem.MakeUnique(baseDir, 
					createName: no => baseDir.Rename(MakePathUnique.CreateNumberName(baseDir.Name, no)),
					condition: p =>
					{
						try
						{							
							if (!_fileSystem.Exists(p))
								_fileSystem.Create(p);

							var lockFilePath = p / new FileName(".lock");
							lockFile = new LockFile(lockFilePath);
							return false; // If the file wasn't locked, break creating of unique directories.
						}
						catch (IOException e)
						{
							var win32ErrorCode = e.HResult & 0xFFFF;
							if (win32ErrorCode == fileLocked || win32ErrorCode == fileInUse)
								return true; // If the file was locked, continue creating unique directories.
							throw;
						}
					});

				return new OutputDirWithLock(path, lockFile);
			}
			catch (Exception e)
			{
				throw new FailedToCreateOutputDir(baseDir, e);
			}
		}
	}
}