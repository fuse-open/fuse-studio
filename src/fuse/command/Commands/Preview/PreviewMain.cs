using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Fuse.Preview;
using Outracks.Fuse.Protocol;
using Outracks.Fuse.Protocol.Messages;
using Outracks.Fuse.Protocol.Preview;
using Outracks.IO;
using Uno.Configuration;

namespace Outracks.Fuse
{
	public sealed class PreviewMain
	{
		readonly PreviewExported _exportedPreview;

		readonly IFuse _fuse;
		readonly IFileSystem _shell;
		readonly ColoredTextWriter _textWriter;

		public PreviewMain(
			IFileSystem shell,
			IFuse fuse,
			PreviewExported exportedPreview,
			ColoredTextWriter textWriter)
		{
			_fuse = fuse;
			_exportedPreview = exportedPreview;
			_textWriter = textWriter;
			_shell = shell;
		}

		/// <exception cref="FailedToSpawnDaemon" />
		/// <exception cref="FailedToConnectToDaemon" />
		/// <exception cref="FailedToGreetDaemon" />
		/// <exception cref="FailedToCreateOutputDir"></exception>
		public void Preview(PreviewArguments args)
		{
			if (!_fuse.IsLicenseValid() && args.Target.Identifier != PreviewTarget.DotNet.Identifier)
			{
				_fuse.Report.Error("Please activate fuse X before using preview.");
				_fuse.Report.Info("Open fuse X for activation instructions, or visit " + WebLinks.Dashboard);
				return;
			}

			_fuse.Report.Info("Starting preview for " + args.Project + ", target " + args.Target, ReportTo.LogAndUser);

			if (args.PrintUnoConfig)
			{
				PrintUnoConfig(UnoConfig.Current, _textWriter);
			}

			if (args.Target.Identifier == PreviewTarget.DotNet.Identifier)
			{
				LaunchDesignerAndSubscribe(args).Wait();
				return;
			}

			var client = GetMessagingService(args).ToObservable().Switch();

			_exportedPreview.BuildAndRunExported(args, client, _shell, _fuse);
		}

		static void PrintUnoConfig(UnoConfig unoConfig, ColoredTextWriter coloredTextWriter)
		{
			using (coloredTextWriter.PushColor(ConsoleColor.Green))
			{
				coloredTextWriter.WriteLine("Uno settings");
			}
			coloredTextWriter.WriteLine(unoConfig.ToString());
			coloredTextWriter.WriteLine();
			using (coloredTextWriter.PushColor(ConsoleColor.Green))
			{
				coloredTextWriter.WriteLine("Config files");
			}
			foreach (var file in unoConfig.Files)
			{
				coloredTextWriter.WriteLine(file);
			}
		}

		async Task LaunchDesignerAndSubscribe(PreviewArguments args)
		{
			var client = await GetMessagingService(args);
			var projectId = ProjectIdComputer.IdFor(args.Project);
			var closed = SubscribeForProjectClosed(client, projectId);
			SubscribeForBuildStarted(client, projectId);
			SubscribeForLog(client, projectId);

			var openArgs = args.Project.ToString().Yield()
				.Concat(args.Defines.Select(d => "-D" + d))
				.Concat(args.IsVerboseBuild ? Optional.Some("-v") : Optional.None())
				.ToArray();

			_fuse.Report.Info("Opening " + string.Join(",", openArgs.Select(a => "'" + a + "'")), ReportTo.LogAndUser);
			_fuse.StartFuse("open", openArgs);
			await closed;
		}

		static async Task SubscribeForProjectClosed(IMessagingService client, Guid projectId)
		{
			var firstAsync = client.BroadcastedEvents<ProjectClosed>(wantReplay: false).SubscribeOn(TaskPoolScheduler.Default)
				.FirstAsync(e => (e.ProjectId == projectId));
			try
			{
				await firstAsync;
			}
			catch (InvalidOperationException)
			{
				//This can happen when the user presses Ctrl+C and the client shuts down, in which case we just want to exit cleanly
			}
		}

		void SubscribeForBuildStarted(IMessagingService client, Guid projectId)
		{
			client.BroadcastedEvents<BuildStartedData>(wantReplay: false).SubscribeOn(TaskPoolScheduler.Default)
				.Subscribe(e =>
					{
						if (e.ProjectId == projectId && e.Target == BuildTarget.DotNet )
						{
							SubscribeForBuildLogged(client, e.BuildId);
						}
					});
		}

		void SubscribeForBuildLogged(IMessagingService client, Guid buildId)
		{
			client.BroadcastedEvents<BuildLoggedData>(wantReplay: false).SubscribeOn(TaskPoolScheduler.Default)
				.Subscribe(e =>
					{
						if (e.BuildId == buildId)
						{
							_textWriter.WriteLine(e.Message.Trim());
						}
					});
		}

		void SubscribeForLog(IMessagingService client, Guid projectId)
		{
			client.BroadcastedEvents<LogEvent>(wantReplay: false).SubscribeOn(TaskPoolScheduler.Default)
				.Subscribe(e =>
					{
						if (new Guid(e.ProjectId) == projectId && e.DeviceName == "Viewport")
						{
							_textWriter.WriteLine(e.Message.Trim());
						}
					});
		}

		Task<IMessagingService> GetMessagingService(PreviewArguments args)
		{
			if (args.CompileOnly)
				return Task.FromResult((IMessagingService)new NullMessagingService());

			return _fuse.ConnectOrSpawnAsync(
				identifier: "preview " + args.Project.NativePath);
		}
	}
}
