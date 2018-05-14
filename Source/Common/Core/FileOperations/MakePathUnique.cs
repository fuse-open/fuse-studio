using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Outracks.IO
{
	public static class MakePathUnique
	{

		public static AbsoluteFilePath MakeUnique(this IFileSystem fileSystem, AbsoluteFilePath path)
		{
			return fileSystem.MakeUnique(path, i => path.Rename(CreateNumberName(path.Name, i)));
		}

		public static AbsoluteDirectoryPath MakeUnique(this IFileSystem fileSystem, AbsoluteDirectoryPath path)
		{
			return fileSystem.MakeUnique(path, i => path.Rename(CreateNumberName(path.Name, i)));
		}


		/// <exception cref="FailedToCreateUniqueDirectory"></exception>
		public static AbsoluteDirectoryPath CreateUniqueDirectory(
			this IFileSystem fileSystem, 
			AbsoluteDirectoryPath path, 
			Func<int, AbsoluteDirectoryPath> createName = null,
			Func<int, TimeSpan> sleepTime = null,
			int maxTries = 10)
		{
			createName = createName ?? (no => path.Rename(CreateNumberName(path.Name, no)));
			sleepTime = sleepTime ?? (no => TimeSpan.FromMilliseconds(10));

			var exception = Optional.None<Exception>();
			for (int i = 0; i < maxTries; i++)
			{
				try
				{
					var uniquePath = fileSystem.MakeUnique(path, createName);
					fileSystem.CreateNew(uniquePath);
					Thread.Sleep(sleepTime(i));
					return uniquePath;
				}
				catch (Exception e)
				{
					exception = e;
				}
			}
			throw new FailedToCreateUniqueDirectory(path.ContainingDirectory.ToOptional(), exception);
		}

		static FileName CreateNumberName(FileName original, int number)
		{
			return original
				.WithoutExtension
				.Add(number.ToString())
				.AddExtension(original.Extension);
		}

		public static DirectoryName CreateNumberName(DirectoryName original, int number)
		{
			return original.Add(number.ToString(CultureInfo.InvariantCulture));
		}

		static void CreateNew(this IFileSystem fileSystem, AbsoluteDirectoryPath path)
		{
			var tempDirectory = path.ContainingDirectory / DirectoryName.GetRandomDirectoryName();
			Directory.CreateDirectory(tempDirectory.NativePath);
			fileSystem.Create(path.ContainingDirectory);
			fileSystem.Move(tempDirectory, path);
		}

		public static T MakeUnique<T>(this IFileSystem fileSystem, T path, Func<int, T> createName, Func<T, bool> condition = null) where T : IAbsolutePath
		{
			condition = condition ?? (p => File.Exists(p.NativePath) || Directory.Exists(p.NativePath));
			int i = 1;
			var uniqueFilePath = path;
			while (condition(uniqueFilePath))
			{
				uniqueFilePath = createName(i);
				i++;
			}
			return uniqueFilePath;
		}

	}

	public class FailedToCreateUniqueDirectory : IOException
	{

		public FailedToCreateUniqueDirectory(Optional<AbsoluteDirectoryPath> containingDir, Optional<Exception> innerException)
			: base("Failed to create a unique directory in " + containingDir.Select(d => d.NativePath).Or("<root>") + innerException.Select(e => ": " + e.Message).Or(""), innerException.OrDefault())
		{
			
		}
	}
}