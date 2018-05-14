using System;
using System.Threading;
using Outracks;
using Outracks.IO;
using Outracks.Simulator.Parser;
using Outracks.Simulator.Protocol;
using Uno;
using Uno.Build;
using Uno.Build.Packages;
using Uno.Build.Targets;
using Uno.IO;
using Uno.Logging;

namespace Fuse.Preview
{
	class UnoBuild : ISimulatorBuilder
	{
		#pragma warning disable 0414
		LockFile lockFile = null;
		#pragma warning restore 0414
		public UnoBuild(Version version)
		{
			_fileSystem = new Shell();
			var cacheCleaner = new CacheCleaner(_fileSystem, version);
			_simulatorBuilder = new SimulatorBuilder(_fileSystem, cacheCleaner, isHost: true, registerLock: (e) => lockFile = e);
		}

		readonly SimulatorBuilder _simulatorBuilder;
		readonly IFileSystem _fileSystem;

		public Optional<ProjectBuild> TryBuild(BuildProject args, Log logger)
		{
			try
			{
				var project = _simulatorBuilder.CreateSimulatorProject(args);

				var libraryBuilder = new LibraryBuilder(new Disk(logger), BuildTargets.Package) { Express = true };
				var projectBuilder = new ProjectBuilder(logger, new LocalSimulatorTarget(), project.Options);

				if (project.IsVerboseBuild)
					logger.Level = Uno.Logging.LogLevel.Verbose;

				if (project.BuildLibraries)
					libraryBuilder.Build();

				var buildResult = projectBuilder.Build(project.Project);

				if (buildResult.ErrorCount != 0)
				{
					return Optional.None();
				}

				var b = new LocalBuild(buildResult, AbsoluteFilePath.Parse(args.ProjectPath));
				return new ProjectBuild(b.Path.NativePath, b.SaveSimulatorMetadata(_fileSystem).NativePath, b.GetTypeInformation());
			}
			catch (ThreadAbortException)
			{
				
			}
			catch (SourceException e)
			{
				logger.Error(e.Source, null, e.Message);
			}
			catch (Exception e)
			{
				logger.Error(e.Message);
			}
			return Optional.None();
		}
	}
}