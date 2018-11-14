using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Outracks.Diagnostics;
using Outracks.Extensions;
using Outracks.IO;
using Uno.Configuration;

namespace Outracks.Fuse
{

	public static class Program
	{
		static Program()
		{
			Thread.CurrentThread.SetInvariantCulture();
		}

		public static string FuseCommandName
		{
			get { return "fuse"; }
		}

		static IReport Log { get; set; }

		public static int Run(List<string> args)
		{
			var fuse = FuseApi.Initialize("Fuse", args);

			Log = fuse.Report;

			Log.Info("Version " + fuse.Version);
			
			if (args.Contains("--version"))
			{
				Console.Out.WriteVersion(fuse.Version);
				return 0;
			}

			// Launch services adds -psn_something to the programs argument list
			// After moving an app from a dmg, and then starting the app
			var psnArgIdx = args.FindIndex(str => str.StartsWith("-psn"));
			if (psnArgIdx >= 0)
			{
				args.RemoveAt(psnArgIdx);
			}

			if(Platform.OperatingSystem == OS.Mac)
				SetMinimumThreads(6); // For faster task creation in Mono. Mono doesn't have a good task system

			var shell = new Shell();

			if (fuse.IsInstalled && shell.IsInsideRepo(AbsoluteDirectoryPath.Parse(Directory.GetCurrentDirectory())))
			{
				Log.Error("Please don't run installed Fuse from inside the Fuse repository. It will get confused by .unoconfigs.", ReportTo.LogAndUser);
				return 1;
			}

			var program =
				new CliProgram(
					Log,
					ColoredTextWriter.Out,
					ColoredTextWriter.Error,
					FuseCommandName,
					() =>
					{
						MoveSdkConfigTmpBackCompatibleThing(shell);
					},
					DashboardCommand.CreateDashboardCommand(),
					OpenCommand.CreateOpenCommand(),
					new LazyCliCommand("daemon", "Start the fuse daemon", false, () => DaemonCommand.CreateDaemonCommand()),
					new LazyCliCommand("daemon-client", "Create a connection to a daemon.", false, () => DaemonClientCommand.Create()),
					new LazyCliCommand("preview", "Preview an app", false, () => PreviewCommand.CreatePreviewCommand()),
					new LazyCliCommand("build", "Build a project for a given target", false, () => BuildCommand.CreateBuildCommand()),
					new LazyCliCommand("create", "Create a project or file from a template", false, () => CreateCommand.CreateCreateCommand()),
					new LazyCliCommand("install", "Install an external component", false, () => InstallCommand.CreateInstallCommand()),
					new LazyCliCommand("event-viewer", "Dump all events", true, () => EventViewerCommand.CreateEventViewerCommand()),
					new LazyCliCommand("tutorial", "Go to tutorials and guides", false, () => TutorialCommand.CreateTutorialCommand()),
					new LazyCliCommand("import", "Import a file to your fuse project", false, () => ImportCommand.CreateImportCommand()),
					new LazyCliCommand("reset-preview", "Causes all active previews to reset to the default state.", true, () => ResetPreviewCliCommand.CreateResetPreviewCommand()),
					new LazyCliCommand("kill-all", "Kill all Fuse processes (even the daemon)", false, () => KillAllCommand.CreateKillAllCommand()),
					new LazyCliCommand("unoconfig", "Print uno config", false, () => UnoConfigCommand.CreateUnoConfigCommand()));

			var ctSource = new CancellationTokenSource();
			Console.CancelKeyPress += (sender, eventArgs) => ctSource.Cancel();

			return program.Main(args.ToArray(), ctSource.Token);
		}

		static void SetMinimumThreads(int numThreads)
		{
			int minWorker, minIOC;
			ThreadPool.GetMinThreads(out minWorker, out minIOC);
			if (!ThreadPool.SetMinThreads(numThreads, minIOC))
				Log.Warn("Failed to set the minimum number of threads to " + numThreads);
		}

		static void MoveSdkConfigTmpBackCompatibleThing(IFileSystem fs)
		{
			var oldPath = Platform.OperatingSystem == OS.Windows
				? AbsoluteFilePath.Parse(Environment.ExpandEnvironmentVariables("%PROGRAMDATA%/Uno/SDKs/.sdkconfig"))
				: AbsoluteFilePath.Parse("/usr/local/share/uno/SDKs/.sdkconfig");

			var newPath = AbsoluteFilePath.Parse(UnoConfig.Current.GetFullPath("SdkConfig"));

			if (fs.Exists(newPath) || !fs.Exists(oldPath))
				return;

			try
			{				
				fs.Create(newPath.ContainingDirectory);
				fs.Copy(oldPath, newPath);
			}
			catch (Exception)
			{
				Log.Debug("Failed to move old .sdkConfig to new location. Not a big deal though.");
			}
		}

		static void WriteVersion(this TextWriter writer, string version)
		{
			writer.WriteLine("Fuse version " + version);
			writer.WriteLine("Copyright (C) 2018-present Fuse Open");
		}


	}
}
