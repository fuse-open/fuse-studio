using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Outracks.IO;

namespace Outracks.Fuse
{
	public sealed class ProjectDetector
	{
		readonly IFileSystem _fileSystem;

		public ProjectDetector(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		/// <exception cref="ProjectNotFound" />
		public AbsoluteFilePath GetCurrentProject(Optional<IAbsolutePath> maybeProject)
		{
			return GetProject(maybeProject.Or((IAbsolutePath)DirectoryPath.GetCurrentDirectory()));
		}

		/// <exception cref="ProjectNotFound" />
		public AbsoluteFilePath GetProject(IAbsolutePath directoryOrFileInProject)
		{
			try
			{
				return GetProjects(directoryOrFileInProject).FirstOrNone().OrThrow(new ProjectNotFound());
			}
			catch (IOException e)
			{
				throw new ProjectNotFound(e);
			}
			catch (UnauthorizedAccessException e)
			{
				throw new ProjectNotFound(e);
			}
		}

		/// <exception cref="IOException" />
		/// <exception cref="UnauthorizedAccessException" />
		IEnumerable<AbsoluteFilePath> GetProjects(IAbsolutePath directoryOrFileInProject)
		{
			if (!_fileSystem.Exists(directoryOrFileInProject))
				throw new FileNotFoundException(
					"Could not find file or directory `" + directoryOrFileInProject.NativePath + "`",
					directoryOrFileInProject.NativePath);

			return directoryOrFileInProject.MatchWith(
				(AbsoluteFilePath file) => GetProjectsContaining(file),
				(AbsoluteDirectoryPath dir) => GetProjectsInOrContaining(dir));
		}

		/// <exception cref="IOException" />
		/// <exception cref="UnauthorizedAccessException" />
		IEnumerable<AbsoluteFilePath> GetProjectsContaining(AbsoluteFilePath fileInProject)
		{
			if (fileInProject.IsProjectFile())
				return new[] { fileInProject };

			return GetProjectsInOrContaining(fileInProject.ContainingDirectory); // TODO: maybe actually open the project to see if it contains the file
		}

		/// <exception cref="IOException" />
		/// <exception cref="UnauthorizedAccessException" />
		IEnumerable<AbsoluteFilePath> GetProjectsInOrContaining(AbsoluteDirectoryPath projectDirectory)
		{
			var currentDirectory = projectDirectory;
			do
			{
				foreach (var file in GetProjectsIn(currentDirectory))
					yield return file;

			} while ((currentDirectory = currentDirectory.ContainingDirectory) != null);
		}

		/// <exception cref="IOException" />
		/// <exception cref="UnauthorizedAccessException" />
		IEnumerable<AbsoluteFilePath> GetProjectsIn(AbsoluteDirectoryPath dir)
		{
			return _fileSystem.GetFiles(dir).Where(file => file.IsProjectFile());
		}
	}
}