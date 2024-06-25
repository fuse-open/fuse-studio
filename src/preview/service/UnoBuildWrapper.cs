using System;
using System.ComponentModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Outracks;
using Outracks.Building;
using Outracks.IO;
using Outracks.Simulator.Parser;
using Outracks.Simulator.Protocol;
using Uno.Build;
using Uno.Build.Packages;
using Uno.Build.Targets;
using Uno.Compiler.Backends.CIL;
using Uno.IO;
using Uno.Logging;
using Uno.UX.Markup.Reflection;
using LogLevel = Uno.Logging.LogLevel;

namespace Fuse.Preview
{
	public class UnoBuildWrapper
	{
		#pragma warning disable 0414
		static LockFile lockFile = null;
		#pragma warning restore 0414
		public static UnoBuildWrapper Create(IFileSystem shell, string version, IObserver<IBinaryMessage> buildEvents, bool isHost, params IErrorHelper[] errorHelpers)
		{
			var cacheCleaner = new CacheCleaner(shell, version);
			var simulatorBuilder = new SimulatorBuilder(shell, cacheCleaner, isHost: isHost, registerLock: (e) => lockFile = e);

			return new UnoBuildWrapper(
				simulatorBuilder,
				buildEvents,
				shell,
				errorHelpers);
		}

		readonly SimulatorBuilder _simulatorBuilder;
		readonly IProgress<IBinaryMessage> _buildEvents;
		readonly IErrorHelper _errorHelpers;

		public UnoBuildWrapper(SimulatorBuilder simulatorBuilder, IObserver<IBinaryMessage> buildEvents, IFileSystem fileSystem, params IErrorHelper[] errorHelpers)
		{
			_simulatorBuilder = simulatorBuilder;
			_buildEvents = buildEvents.ToProgress();
			_errorHelpers = new CombinedErrorHelper
			{
				Helpers = errorHelpers
			};
		}

		/// <exception cref="BuildFailed"></exception>
		/// <exception cref="FailedToCreateOutputDir"></exception>
		public Task<BuildResult> BuildUno(Guid id, BuildProject args, BuildTarget previewTarget, bool directToDevice, CancellationToken cancellationToken, bool quitAfterApkLaunch)
		{
			args.Id = id;

			var loggedEvents = new AccumulatingProgress<IBinaryMessage>(_buildEvents);
			var errorList = new ErrorListAdapter(id, loggedEvents);
			var textWriter = new TextWriterAdapter(id, loggedEvents);

			var tcs = new TaskCompletionSource<BuildResult>();

			var project = _simulatorBuilder.CreateSimulatorProject(args, previewTarget, directToDevice, quitAfterApkLaunch);

			var thread = new Thread(
				() =>
				{
					var logger = new Log(errorList, textWriter);
					if (project.IsVerboseBuild)
						logger.Level = LogLevel.Verbose;

					if (project.BuildLibraries)
						new LibraryBuilder(logger).Build();

					_buildEvents.Report(
						new Started
						{
							Command = args
						});

					var faulty = false;
					try
					{
						var buildResult =
							new ProjectBuilder(
									logger,
									previewTarget,
									project.Options)
								.Build(project.Project);

						if (buildResult.ErrorCount != 0)
						{
							_errorHelpers.OnBuildFailed(buildResult);

							tcs.SetException(new UserCodeContainsErrors());
						}
						else
							tcs.SetResult(buildResult);
					}
					catch (Exception e)
					{
						if (e is ThreadAbortException)
							return;

						tcs.TrySetException(new InternalBuildError(e));
						faulty = true;
					}
					finally
					{
						_buildEvents.Report(
							new Ended
							{
								Command = args,
								Success = !faulty,
								BuildDirectory = AbsoluteDirectoryPath.Parse(project.Project.OutputDirectory)
							});
					}
				})
			{
				Name = "Build " + project.Project.Name + " in " + project.Options.OutputDirectory,
				Priority = ThreadPriority.Normal,
				IsBackground = true,
			};

			thread.Start();
			cancellationToken.Register(
				() =>
				{
					thread.Abort();
					thread.Join();
					tcs.TrySetException(new BuildCanceledException());
				});

			return tcs.Task;
		}
	}

	class CilBackendEarlyOut : Exception
	{
	}

	public class LocalBuild
	{
		readonly BuildResult _buildResult;
		public readonly AbsoluteFilePath Path;

		public LocalBuild(BuildResult buildResult, AbsoluteFilePath path)
		{
			_buildResult = buildResult;
			Path = path;
		}

		public AbsoluteFilePath SaveSimulatorMetadata(IFileSystem fileSystem)
		{
			var cilBackend = (CilResult)_buildResult.BackendResult;
			if (cilBackend == null)
				throw new CilBackendEarlyOut();

			return AbsoluteFilePath.Parse(Outracks.Simulator.Client.DotNetBuild.SaveMetadata(_buildResult));
		}

		public IDataTypeProvider GetTypeInformation()
		{
			return new CompilerDataTypeProvider(_buildResult.Compiler);
		}
	}
}
