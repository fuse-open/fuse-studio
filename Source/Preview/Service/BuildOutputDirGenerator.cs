using System;
using System.Collections.Concurrent;
using System.Threading;
using Outracks;
using Outracks.IO;

namespace Fuse.Preview
{
	public class BuildOutputDirLock
	{
		public readonly AbsoluteDirectoryPath BuildDir;
		readonly Action _disposed;
		int _refCount;
		volatile bool _isDisposed;
		readonly object _mutex = new object();

		public BuildOutputDirLock(AbsoluteDirectoryPath buildDir, Action disposed)
		{
			BuildDir = buildDir;
			_disposed = disposed;
		}

		public Optional<IDisposable> Lock()
		{
			lock (_mutex)
			{
				if (_isDisposed)
					return Optional.None();

				Interlocked.Increment(ref _refCount);
			}
			return Optional.Some(DisposeFactory());
		}

		public IDisposable DisposeFactory()
		{
			return Disposable.Create(
				() =>
				{
					Interlocked.Decrement(ref _refCount);

					lock (_mutex)
					{
						if (_refCount > 0)
							return;

						_disposed();
						_isDisposed = true;
					}
				});
		}
	}

	public class BuildOutputDirGenerator 
	{
		readonly IFileSystem _fileSystem;
		readonly ConcurrentDictionary<AbsoluteDirectoryPath, BuildOutputDirLock> _lockedBuildDirectories = new ConcurrentDictionary<AbsoluteDirectoryPath, BuildOutputDirLock>();

		public BuildOutputDirGenerator(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public AbsoluteDirectoryPath Acquire(AbsoluteDirectoryPath baseDirectory)
		{
			var buildLock = new Optional<BuildOutputDirLock>();
			_fileSystem.MakeUnique(baseDirectory,
				createName: no => baseDirectory.Rename(MakePathUnique.CreateNumberName(baseDirectory.Name, no)),
				condition: p =>
				{
					buildLock = new BuildOutputDirLock(
						p,
						() =>
						{
							BuildOutputDirLock tmp;
							var tries = 5;
							while (!_lockedBuildDirectories.TryRemove(p, out tmp) && tries >= 0)
							{
								--tries;
							}
						});
					var result = _lockedBuildDirectories.AddOrUpdate(
						p,
						buildLock.Value,
						(buildOut, oldLock) => oldLock);

					// If we did update an already lock - try to iterate over other possibilities.
					return result != buildLock;
				});

			buildLock.Do(bl => bl.Lock());
			return buildLock
				.OrThrow(new Exception("Failed to generate a build output directory."))
				.BuildDir;
		}

		public IDisposable Lock(AbsoluteDirectoryPath outputDir)
		{
			return GetLockedDirectory(outputDir) // First check if we already have a lock for the directory
				.Or(() => GetLockedDirectory(Acquire(outputDir))) // Else acquire a new lock
				.SelectMany(d => d.Lock()) // Do the locking
				.Or(Disposable.Empty); // We don't care if we fail to lock
		}

		public void Release(AbsoluteDirectoryPath outputDir)
		{
			GetLockedDirectory(outputDir).Select(d => d.DisposeFactory()).Do(d => d.Dispose());
		}

		Optional<BuildOutputDirLock> GetLockedDirectory(AbsoluteDirectoryPath outputDir)
		{
			BuildOutputDirLock tmp;
			return _lockedBuildDirectories.TryGetValue(outputDir, out tmp) ? Optional.Some(tmp) : Optional.None();
		}
	}
}