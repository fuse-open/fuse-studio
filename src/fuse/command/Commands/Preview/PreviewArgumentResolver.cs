using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Outracks.IO;

namespace Outracks.Fuse
{
	/// <summary>
	/// Figures out the project and optionally ux document to preview based argument
	/// </summary>
	public sealed class PreviewArgumentResolver
	{
		readonly ProjectDetector _projects;
		readonly IFileSystem _fileSystem;

		public PreviewArgumentResolver(ProjectDetector projects, IFileSystem fileSystem)
		{
			_projects = projects;
			_fileSystem = fileSystem;
		}

		/// <exception cref="InvalidPath" />
		/// <exception cref="System.Security.SecurityException" />
		/// <exception cref="ProjectNotFound" />
		public PreviewArguments Resolve(List<string> mutableArgs)
		{
			var compileOnly = mutableArgs
				.Remove("--compile-only");

			var directToDevice = mutableArgs
				.Remove("--direct-to-device");

			var quitAfterApkLaunch = mutableArgs
				.Remove("--quit-after-apk-launch");

			var project = mutableArgs
				.TryRemoveAt(0)
				.Select(_fileSystem.ResolveAbsolutePath);

			var defines = mutableArgs
				.Where(a => a.StartsWith("-D"))
				.Select(a => a.Substring("-D".Length))
				.ToImmutableList();

			mutableArgs.RemoveAll(a => a.StartsWith("-D"));

			return new PreviewArguments(
				project: _projects.GetCurrentProject(project),
				compileOnly: compileOnly,
				directToDevice: directToDevice,
				defines: defines,
				quitAfterApkLaunch: quitAfterApkLaunch);
		}
	}
}