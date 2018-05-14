using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using Fuse.Preview;
using Outracks.Building;
using Outracks.Fuse.Live;
using Outracks.Fuse.Protocol;
using Outracks.Fuse.Protocol.Preview;
using Outracks.IO;
using Outracks.IPC;
using Outracks.Simulator;
using Outracks.Simulator.Parser;
using Outracks.Simulator.Protocol;
using Uno.Build;
using Uno.Logging;
using BuildTarget = Outracks.Fuse.Protocol.BuildTarget;

namespace Outracks.Fuse
{
	public class PreviewExported
	{
		readonly ColoredTextWriter _output;
		public PreviewExported(ColoredTextWriter output)
		{
			_output = output;
		}

		/// <exception cref="SocketException" />
		/// <exception cref="UnableToResolveHostNameException" />
		/// <exception cref="BuildFailed" />
		/// <exception cref="RunFailed" />
		public void BuildAndRunExported(PreviewArguments args, IMessagingService client, IFileSystem shell, IFuse fuse)
		{
			var buildEvents = new Subject<IBinaryMessage>();

			var builder = UnoBuildWrapper.Create(
				shell, fuse.Version, buildEvents, false, 
				new MissingRequiredPackageReferences(),
				new InstallAndroidRequired());

			var projectId = ProjectIdComputer.IdFor(args.Project);

			using (PushEventsToDaemon.Start(buildEvents, client, args.Project, projectId, GetBuildTarget(args.Target)))
			using (buildEvents.Subscribe(_output.WriteBuildEvent))
			{

				if (args.Endpoints.Count == 0)
					args = args.AddEndpoints(GetProxyEndPoints());

				var build = builder
					.LoadOrBuildRunnable(args, CancellationToken.None)
					.GetResultAndUnpackExceptions();

				if (args.CompileOnly == false)
					build.Run(buildEvents.ToProgress());
			}
		}

		static BuildTarget GetBuildTarget(PreviewTarget target)
		{
			switch (target)
			{
			case PreviewTarget.Android: return BuildTarget.Android;
			case PreviewTarget.iOS:	return BuildTarget.iOS;
			}

			throw new ArgumentException();
		}


		/// <exception cref="UnableToResolveHostNameException" />
		static IEnumerable<IPEndPoint> GetProxyEndPoints()
		{
			return NetworkHelper
				.GetInterNetworkIps()
				.Select(ip => new IPEndPoint(ip, 12124)); // TODO: Remove localhost address from this list
		}
	}

	static class LegacyExtension
	{
		public static async System.Threading.Tasks.Task<ExportBuild> LoadOrBuildRunnable(this UnoBuildWrapper self, PreviewArguments args, CancellationToken cancellationToken)
		{
			var id = Guid.NewGuid();
			var buildResult = await self.BuildUno(id,
				new BuildProject(
					args.Project.NativePath,
					args.Defines.ToImmutableList(),
					args.BuildLibraries,
					args.IsVerboseBuild),
				args.Target,
				args.DirectToDevice,
				cancellationToken,
				args.QuitAfterApkLaunch);
			return new ExportBuild(buildResult, id);
		}
	}

	public class ExportBuild
	{
		readonly BuildResult _buildResult;
		readonly Guid _buildId;
		public ExportBuild(BuildResult buildResult, Guid buildId)
		{
			_buildResult = buildResult;
			_buildId = buildId;
		}


		public void Run(IProgress<IBinaryMessage> progress)
		{
			// TODO: Do something with our device simulator build...
			var loggedEvents = new AccumulatingProgress<IBinaryMessage>(progress);
			var textWriter = new TextWriterAdapter(_buildId, loggedEvents);

			try
			{
				_buildResult.Run(new Log(textWriter));
			}
			catch (Exception e) 
			{
				throw new RunFailed(e);
			}
		}

	}

}
