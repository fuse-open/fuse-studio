using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reflection;
using System.Security;
using Outracks.Diagnostics;

namespace Outracks.IO
{
	public class InvalidPath : ArgumentException
	{
		public readonly string Path;
		public InvalidPath(string path, Exception innerException = null) 
			: base ("Invalid path '" + path + "'", innerException)
		{
			Path = path;
		}
	}

	public interface IFileSystem
	{
		/// <exception cref="InvalidPath" />
		/// <exception cref="SecurityException" />
		IAbsolutePath ResolveAbsolutePath(string nativePath);

		/// <exception cref="IOException"><paramref name="path"/> is a file name.-or-A network error has occurred. </exception>
		/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission. </exception>
		/// <exception cref="PathTooLongException"/>
		/// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
		IEnumerable<AbsoluteFilePath> GetFiles(AbsoluteDirectoryPath path);
		
		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		IEnumerable<AbsoluteFilePath> GetFiles(AbsoluteDirectoryPath path, string searchPattern);


		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		IEnumerable<AbsoluteDirectoryPath> GetDirectories(AbsoluteDirectoryPath path);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		IEnumerable<AbsoluteDirectoryPath> GetDirectories(AbsoluteDirectoryPath path, string searchPattern);

		bool Exists(IAbsolutePath path);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		FileStream Open(AbsoluteFilePath path, FileMode mode, FileAccess access, FileShare share);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		Stream OpenRead(AbsoluteFilePath path);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		Stream OpenWrite(AbsoluteFilePath path);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		void Delete(IAbsolutePath path);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		void Copy(AbsoluteFilePath source, AbsoluteFilePath destination);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		void Move(AbsoluteFilePath source, AbsoluteFilePath destination);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// TODO: this method should always throw if destination exists (without race condition)
		void Move(AbsoluteDirectoryPath source, AbsoluteDirectoryPath destination);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		void Create(AbsoluteDirectoryPath directory);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		Stream Create(AbsoluteFilePath file);

		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		Stream CreateNew(AbsoluteFilePath file);

		IObservable<Unit> Watch(AbsoluteFilePath file);

		IObservable<FileSystemEventData> Watch(AbsoluteDirectoryPath path, Optional<string> filter = default(Optional<string>));
	}

	public static class AssemblyExtensions
	{
		public static AbsoluteFilePath GetCodeBaseFilePath(this Assembly assembly)
		{
			if (Platform.OperatingSystem == OS.Windows)
				return AbsoluteFilePath.Parse(assembly.CodeBase.StripPrefix("file:///"));
			else
				return AbsoluteFilePath.Parse(assembly.CodeBase.StripPrefix("file://"));
		}
	}

	public static class FileSystemExtensions
	{
		public static Optional<IAbsolutePath> TryResolveAbsolutePath(this IFileSystem fs, string nativePath)
		{
			try
			{
				return Optional.Some(fs.ResolveAbsolutePath(nativePath));
			}
			catch (InvalidPath)
			{
				return Optional.None();
			}
			catch (SecurityException)
			{
				return Optional.None();
			}
		}

		public static void Move(this IFileSystem fileSystem, IAbsolutePath source, IAbsolutePath destination)
		{
			source.Do(
				(AbsoluteFilePath file) => fileSystem.Move(file, (AbsoluteFilePath) destination),
				(AbsoluteDirectoryPath dir) => fileSystem.Move(dir, (AbsoluteDirectoryPath) destination));
		}

		public static void CreateIfNotExists(this IFileSystem fileSystem, AbsoluteDirectoryPath dir)
		{
			if (!fileSystem.Exists(dir))
				fileSystem.Create(dir);
		}
	}
}