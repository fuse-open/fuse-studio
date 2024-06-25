using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using Fuse.Preview;
using Outracks.IO;
using Uno.Build;

namespace Outracks.Fuse
{
	public sealed class PreviewArguments
	{
		public readonly AbsoluteFilePath Project;
		public readonly BuildTarget Target;
		public readonly IImmutableSet<IPEndPoint> Endpoints;
		public readonly Optional<string> BuildTag;
		public readonly bool CompileOnly;
		public readonly bool DirectToDevice;
		public readonly IImmutableList<string> Defines;
		public readonly bool IsVerboseBuild;
		public readonly bool BuildLibraries;
		public readonly bool QuitAfterApkLaunch;
		public readonly bool PrintUnoConfig;

		public PreviewArguments(AbsoluteFilePath project, Optional<BuildTarget> target = default(Optional<BuildTarget>),
			IImmutableSet<IPEndPoint> endpoints = null,
			Optional<string> buildTag = default(Optional<string>),
			bool compileOnly = false,
			bool directToDevice = false,
			IImmutableList<string> defines = null,
			bool isVerboseBuild = false,
			bool buildLibraries = false,
			bool quitAfterApkLaunch = false,
			bool printUnoConfig = false)
		{
			Project = project;
			Target = target.Or(PreviewTarget.DotNet);
			BuildTag = buildTag;
			CompileOnly = compileOnly;
			DirectToDevice = directToDevice;
			Endpoints = endpoints ?? ImmutableHashSet<IPEndPoint>.Empty;
			Defines = defines ?? ImmutableList<string>.Empty;
			IsVerboseBuild = isVerboseBuild;
			BuildLibraries = buildLibraries;
			QuitAfterApkLaunch = quitAfterApkLaunch;
			PrintUnoConfig = printUnoConfig;
		}

		public PreviewArguments AddEndpoints(IEnumerable<IPEndPoint> endpoints)
		{
			return With(endpoints: Endpoints.Union(endpoints));
		}

		public PreviewArguments With(
			Optional<AbsoluteFilePath> project = default (Optional<AbsoluteFilePath>),
			Optional<BuildTarget> target = default (Optional<BuildTarget>),
			IImmutableSet<IPEndPoint> endpoints = null,
			Optional<string> buildTag = default (Optional<string>),
			Optional<bool> compileOnly = default(Optional<bool>),
			Optional<bool> directToDevice = default(Optional<bool>),
			Optional<bool> isVerboseBuild = default(Optional<bool>),
			Optional<bool> buildLibraries = default(Optional<bool>),
			Optional<bool> quitAfterApkLaunch = default(Optional<bool>),
			Optional<bool> printUnoConfig = default(Optional<bool>))
		{
			return new PreviewArguments(
				project.Or(Project),
				target.Or(Target),
				endpoints ?? Endpoints,
				buildTag.Or(BuildTag),
				compileOnly.Or(CompileOnly),
				directToDevice.Or(DirectToDevice),
				Defines,
				isVerboseBuild.Or(IsVerboseBuild),
				buildLibraries.Or(BuildLibraries),
				quitAfterApkLaunch.Or(QuitAfterApkLaunch),
				printUnoConfig.Or(PrintUnoConfig));
		}
	}
}
