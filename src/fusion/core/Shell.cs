using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Security;
using Outracks.Fusion;

namespace Outracks.IO
{
	public interface IOldShell : IFileSystem
	{
		string Name { get; }

		void OpenWithDefaultApplication(AbsoluteFilePath path);
		void OpenFolder(AbsoluteDirectoryPath path);
		void ShowInFolder(AbsoluteFilePath path);
		void OpenTerminal(AbsoluteDirectoryPath containingDirectory);
	}

	public interface IFilePermission
	{
		void SetPermission(AbsoluteFilePath file, FileSystemPermission permission, FileSystemGroup group);

		void SetPermission(
			AbsoluteDirectoryPath dir,
			FileSystemPermission permission,
			FileSystemGroup group,
			bool recursive = false);
	}

	public class Shell : IShell
	{
		readonly IOldShell _shellImpl;
		readonly IFilePermission _filePermissionImpl;

		public Shell()
		{
			_shellImpl = ImplementationLocator.CreateInstance<IOldShell>();
			_filePermissionImpl = ImplementationLocator.CreateInstance<IFilePermission>();
		}

		/// <exception cref="InvalidPath" />
		/// <exception cref="SecurityException" />
		public IAbsolutePath ResolveAbsolutePath(string nativePath)
		{
			return _shellImpl.ResolveAbsolutePath(nativePath);
		}

		/// <exception cref="IOException"><paramref name="path"/> is a file name.-or-A network error has occurred. </exception>
		/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission. </exception>
		/// <exception cref="PathTooLongException"/>
		/// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
		public IEnumerable<AbsoluteFilePath> GetFiles(AbsoluteDirectoryPath path)
		{
			return _shellImpl.GetFiles(path);
		}

		public IEnumerable<AbsoluteDirectoryPath> GetDirectories(AbsoluteDirectoryPath path)
		{
			return _shellImpl.GetDirectories(path);
		}

		public IEnumerable<AbsoluteFilePath> GetFiles(AbsoluteDirectoryPath path, string searchPattern)
		{
			return _shellImpl.GetFiles(path, searchPattern);
		}

		public IEnumerable<AbsoluteDirectoryPath> GetDirectories(AbsoluteDirectoryPath path, string searchPattern)
		{
			return _shellImpl.GetDirectories(path, searchPattern);
		}

		public FileStream Open(AbsoluteFilePath path, FileMode mode, FileAccess access, FileShare share)
		{
			return File.Open(path.NativePath, mode, access, share);
		}

		public Stream OpenRead(AbsoluteFilePath path)
		{
			return _shellImpl.OpenRead(path);
		}

		public Stream OpenWrite(AbsoluteFilePath path)
		{
			return _shellImpl.OpenWrite(path);
		}

		public void Delete(IAbsolutePath path)
		{
			_shellImpl.Delete(path);
		}

		public bool Exists(IAbsolutePath path)
		{
			return _shellImpl.Exists(path);
		}

		public void Copy(AbsoluteFilePath source, AbsoluteFilePath destination)
		{
			_shellImpl.Copy(source, destination);
		}

		public void Copy(AbsoluteDirectoryPath source, AbsoluteDirectoryPath destination)
		{
			Directory.CreateDirectory(destination.NativePath);
			foreach (var fileInfo in new DirectoryInfo(source.NativePath).GetFiles())
			{
				fileInfo.CopyTo((destination / fileInfo.Name).NativePath, true);
			}
			foreach (var directoryInfo in new DirectoryInfo(source.NativePath).GetDirectories())
			{
				var sourceDir = (source / directoryInfo.Name);
				var destDir = (destination / directoryInfo.Name);
				Copy(sourceDir, destDir);
			}
		}

		public void Move(AbsoluteFilePath source, AbsoluteFilePath destination)
		{
			_shellImpl.Move(source, destination);
		}

		public void Move(AbsoluteDirectoryPath source, AbsoluteDirectoryPath destination)
		{
			_shellImpl.Move(source, destination);
		}

		public void Create(AbsoluteDirectoryPath directory)
		{
			_shellImpl.Create(directory);
		}

		public Stream Create(AbsoluteFilePath file)
		{
			return _shellImpl.Create(file);
		}

		public Stream CreateNew(AbsoluteFilePath file)
		{
			return _shellImpl.CreateNew(file);
		}

		public IObservable<Unit> Watch(AbsoluteFilePath file)
		{
			return _shellImpl.Watch(file);
		}

		public string Name { get { return _shellImpl.Name; } }

		public void OpenWithDefaultApplication(AbsoluteFilePath path)
		{
			_shellImpl.OpenWithDefaultApplication(path);
		}

		public void OpenFolder(AbsoluteDirectoryPath path)
		{
			//TODO: is this needed ? ps.UpdatePathEnvironment();
			_shellImpl.OpenFolder(path);
		}

		public void ShowInFolder(AbsoluteFilePath path)
		{
			_shellImpl.ShowInFolder(path);
		}

		public void OpenTerminal(AbsoluteDirectoryPath containingDirectory)
		{
			_shellImpl.OpenTerminal(containingDirectory);
		}

		public void SetPermission(AbsoluteFilePath file, FileSystemPermission permission, FileSystemGroup group)
		{
			_filePermissionImpl.SetPermission(file, permission, group);
		}

		public void SetPermission(
			AbsoluteDirectoryPath dir,
			FileSystemPermission permission,
			FileSystemGroup group,
			bool recursive = false)
		{
			_filePermissionImpl.SetPermission(dir, permission, group, recursive);
		}

		public IObservable<FileSystemEventData> Watch(AbsoluteDirectoryPath path, Optional<string> filter = default(Optional<string>))
		{
			return _shellImpl.Watch(path, filter);
		}
	}
}
