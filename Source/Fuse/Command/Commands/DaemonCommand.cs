using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Mono.Options;

namespace Outracks.Fuse
{
	using IO;
	using Diagnostics;
	using Analytics;
	using Daemon;
	using Protocol;
	
	public class DaemonArgs
	{
		public readonly bool Debug;
		public readonly bool IsMinimal;

		public DaemonArgs(bool debug, bool isMinimal)
		{
			Debug = debug;
			IsMinimal = isMinimal;
		}
	}

	public class DaemonCommand : CliCommand
	{
		public static DaemonCommand CreateDaemonCommand()
		{
			var shell = new Shell();
			var coloredConsole = ColoredTextWriter.Out;
			var coloredErrorConsole = ColoredTextWriter.Error;
			var fuse = FuseApi.Initialize("daemon", new List<string>());
			var daemonReport = fuse.Report;
			var localSocketServer = new LocalSocketServer(daemonReport);
			var serviceRunnerFactory = new ServiceRunnerFactory(
				  new Service("CodeAssistance", fuse.CodeAssistance)
				, new Service("Tray", fuse.Tray)
				);

			var daemonSingleInstance = new EnsureSingleUser(
				coloredErrorConsole,
				shell,
				userFile: DaemonPossessionFile);

			return new DaemonCommand(
				coloredConsole,
				daemonReport,
				daemonSingleInstance,
				fuse,
				(a) => new DaemonRunner(
					daemonSingleInstance,
					localSocketServer,
					a.Debug,
					!a.IsMinimal,
					fuse,
					serviceRunnerFactory,
					daemonReport));
		}

		static AbsoluteFilePath DaemonPossessionFile
		{
			get
			{
				if (Platform.OperatingSystem == OS.Mac)
				{
					return AbsoluteFilePath.Parse("/tmp/.daemonPossesion");
				}

				if (Platform.OperatingSystem == OS.Windows)
				{
					return AbsoluteDirectoryPath.Parse(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))
						/ new DirectoryName("Uno")
						/ new FileName(".daemonPossesion");
				}

				throw new PlatformNotSupportedException("Not implemented on platform: " + Outracks.Diagnostics.Platform.OperatingSystem);
			}
		}

		readonly ColoredTextWriter _textWriter; //TODO delete
		readonly IReport _report;
		readonly EnsureSingleUser _ensureSingleUser;
		readonly IFuseLauncher _fuse;
		readonly OptionSet _optionSet;
		readonly HelpArguments _helpArguments;
		readonly Func<DaemonArgs, DaemonRunner> _daemonRunner;
		bool _debug;
		bool _isMinimal;
		bool _isBackground;
		bool _getKey;

		public DaemonCommand(
			ColoredTextWriter textWriter,
			IReport report,
			EnsureSingleUser ensureSingleUser,
			IFuseLauncher fuseLauncher,
			Func<DaemonArgs, DaemonRunner> daemonRunner)
			: base("daemon", "Start the fuse daemon")
		{
			_textWriter = textWriter;
			_report = report;
			_ensureSingleUser = ensureSingleUser;
			_fuse = fuseLauncher;			
			_optionSet = CreateOptions();
			_daemonRunner = daemonRunner;
			_helpArguments = new HelpArguments(
				new HelpHeader("fuse " + Name, Description), 
				new HelpSynopsis("fuse daemon [<Options>]"),
				new HelpDetailedDescription(
@"Fuse daemon starts a local server on port(12122). 
It communicates over our own protocol, with a Plugin API built on top.
Documentation for the protocol and the API will be released later."),
				new HelpOptions(_optionSet.ToTable()));
		}

		public override void Help()
		{
			_textWriter.WriteHelp(_helpArguments);
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			_report.Info("Starting daemon with arguments '" + string.Join(" ", args) + "'");
			_optionSet.Parse(args);

			if (_getKey)
			{
				_report.Info(_ensureSingleUser.GetDaemonKey(), ReportTo.LogAndUser);
			}
			else if (_isBackground)
			{
				StartChildDaemonAndWait(args);
			}
			else
			{
				try
				{
					_daemonRunner(new DaemonArgs(_debug, _isMinimal)).Run();
				}
				catch (SocketException e)
				{
					_report.Exception("Looks like another instance of Fuse is running. Try to kill the old process.", e);
					throw new ExitWithError("Looks like another instance of Fuse is running. Try to kill the old process.");
				}
			}
		}

		void StartChildDaemonAndWait(IEnumerable<string> args)
		{
			_report.Info("Trying to start the daemon as a background process.", ReportTo.LogAndUser);

			var arguments = args.ToImmutableList();
			var backgroundArgIdx = arguments.FindIndex(a => a.Contains("-b") || a.Contains("--background"));
			if(backgroundArgIdx >= 0)
				arguments = arguments.RemoveAt(backgroundArgIdx);

			var process = _fuse.StartFuse("daemon", arguments.ToArray());

			if (process == null)
				throw new ExitWithError("Couldn't start a background process of the daemon.");

			if (!process.StandardOutput.ReadLinesUntil(
				line =>
				{
					if (line.StartsWith("Running at "))
					{
						_report.Info("A daemon background process was successfully started.", ReportTo.LogAndUser);
						return true;
					}
					else if (line.StartsWith("Already running at "))
					{
						_report.Info("A daemon is already running.", ReportTo.LogAndUser);
						return true;
					}

					return false;
				}))
			{
				throw new ExitWithError("Couldn't start a background process of the daemon.");
			}
		}

		OptionSet CreateOptions()
		{
			return new OptionSet()
			{
				{ "m|minimal", "In this mode, only the daemon is started without child processes.", a => _isMinimal = true },
				{ "d|debug", "In this mode all errors are written to console.", a => _debug = true },
				{ "b|background", "Start the daemon as a background process. This process will return when the background process has successfully started.", a => _isBackground = true },
				{ "get-key", "Returns a key that all clients should send when connecting.", a => _getKey = true }
			};
		}
	}
}