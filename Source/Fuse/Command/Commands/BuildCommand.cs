using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security;
using System.Threading;
using Outracks.Simulator.Parser;

namespace Outracks.Fuse
{
	using Analytics;
	using Protocol;
	using Protocol.Preview;
	using IO;
	using BuildTypeData = Protocol.BuildTypeData;

	class FailedToStartBuild : Exception
	{
		public FailedToStartBuild(Exception innerException)
			: base("Failed to start build: " + innerException.Message, innerException)
		{ }
	}

	public class BuildCommand : CliCommand
	{
		public static BuildCommand CreateBuildCommand()
		{
			var shell = new Shell();
			var projectDetector = new ProjectDetector(shell);
			var fuse = FuseApi.Initialize("Fuse", new List<string>());
			return new BuildCommand(
				fuse,
				ColoredTextWriter.Out,
				ColoredTextWriter.Error,
				fuse.Report,
				projectDetector,
				fileSystem: shell,
				uno: fuse.Uno);
		}

		readonly ColoredTextWriter _outWriter;
		readonly ColoredTextWriter _errorWriter;
		readonly IReport _report;
		readonly IFuseLauncher _daemonSpawner;
		readonly IExternalApplication _uno;
		readonly ProjectDetector _projectDetector;
		readonly IFileSystem _fileSystem;

		public BuildCommand(
			IFuseLauncher daemonSpawner,
			ColoredTextWriter outWriter,
			ColoredTextWriter errorWriter,
			IReport report,
			ProjectDetector projectDetector,
			IFileSystem fileSystem,
			IExternalApplication uno)
			: base("build", "Build a project for a given target")
		{
			_daemonSpawner = daemonSpawner;
			_outWriter = outWriter;
			_errorWriter = errorWriter;
			_report = report;
			_projectDetector = projectDetector;
			_fileSystem = fileSystem;
			_uno = uno;
		}

		public override void Help()
		{
			new BuildHelpWriter(
				Description, 
				FetchHelp()
			).Write(_outWriter);
		}

		string FetchHelp()
		{
			var startInfo = new ProcessStartInfo
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "uno",
				Arguments = "build --help",
				RedirectStandardOutput = true,
				UseShellExecute = false
			};

			var proc = new Process
			{
				StartInfo = startInfo
			};

			proc.Start();
			proc.WaitForExit();

			var o = proc.StandardOutput;
			var lines = new List<string>();

			while (!o.EndOfStream)
				lines.Add(o.ReadLine());

			proc.Close();

			return lines.SkipWhile((s, i) => i < 3).ToArray().Join("\n");
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			_report.Info("Building with arguments '" + string.Join(" ", args) + "'");
			try
			{
				using (var client = _daemonSpawner.ConnectOrSpawn(
					identifier: "Builder",
					timeout: TimeSpan.FromMinutes(1)))
				{

					Func<string, Optional<string>> parseNameArg = s => TryParseArg("n", "name", s);
					Func<string, Optional<string>> parseTargetArg = s => TryParseArg("t", "target", s);
					Func<Optional<string>, BuildTarget> targetToTargetEnum = s =>
					{
						if (!s.HasValue)
							return BuildTarget.Unknown;
						try
						{
							return (BuildTarget) Enum.Parse(typeof (BuildTarget), s.Value);
						}
						catch (ArgumentException)
						{
							return BuildTarget.Unknown;
						}
					};

					var nameArgs = args.Select(parseNameArg).NotNone();
					var nameArg = nameArgs.FirstOrNone();

					var targetArgs = args.Select(parseTargetArg).NotNone();
					var targetArg = targetArgs.FirstOrNone();

					args = args.Where(s => !parseNameArg(s).HasValue).ToArray(); //uno does not accept name arg so strip it


					var buildId = Guid.NewGuid();

					//_report.Info(string.Format("Calling '{0} {1}', with buildId '{2}'", startInfo.FileName, startInfo.Arguments, buildId));

					var maybeFileOrProject = args
						.FirstOrNone(a => !a.StartsWith("-"))
						.Select(_fileSystem.ResolveAbsolutePath);

					var projPath = _projectDetector.GetCurrentProject(maybeFileOrProject);

					var target = targetToTargetEnum(targetArg);
					client.Broadcast(
						new BuildStartedData
						{
							BuildId = buildId,
							Target = target,
							BuildType = BuildTypeData.FullCompile,
							ProjectId = ProjectIdComputer.IdFor(projPath),
							ProjectPath = projPath.NativePath,
							BuildTag = nameArg.Or("")
						});

					var outputData = new Subject<string>();
					var errorData = new Subject<string>();

					// ReSharper disable once AccessToDisposedClosure
					using (Observable.Merge(outputData, errorData).Subscribe(line => BroadCastBuildLog(line, client, buildId)))
					using (outputData.Select( line => line + "\n").Subscribe(_outWriter.Write))
					using (errorData.Select( line => line + "\n").Subscribe(_errorWriter.Write))
					{
						Process proc;
						try
						{
							proc = _uno.Start(new ProcessStartInfo
							{
								WindowStyle = ProcessWindowStyle.Hidden,
								Arguments = "build " + args.Select(
								s =>
								{
									if (s.EndsWith("\\"))
										return "\"" + s + " \"";
									else
										return "\"" + s + "\"";
								}).Join(" "),
								RedirectStandardOutput = true,
								RedirectStandardError = true,
								UseShellExecute = false
							});						}
						catch (Exception e)
						{
							throw new FailedToStartBuild(e);
						}

						proc.ConsumeOutAndErr(outputData, errorData).Wait();
						proc.WaitForExit();
						var status = proc.ExitCode == 0 ? BuildStatus.Success : BuildStatus.Error;

						client.Broadcast(
							new BuildEndedData
							{
								BuildId = buildId,
								Status = status
							});

						if (status == BuildStatus.Success)
						{
							_report.Info("Build " + buildId + " succeeded");
						}
						else
						{
							_report.Error("Build " + buildId + " failed");
							throw new UserCodeContainsErrors();
						}
					}
				}
			}
			catch (DaemonException e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (InvalidPath e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (SecurityException e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (ProjectNotFound e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (FailedToStartBuild e)
			{
				throw new ExitWithError(e.Message);
			}
			catch (UserCodeContainsErrors e)
			{
				throw new ExitWithError(e.Message);
			}
		
		}

		void BroadCastBuildLog(string line, IMessagingService client, Guid buildId)
		{
			client.Broadcast(
				new BuildLoggedData
				{
					BuildId = buildId,
					Message = line + "\n"
				});
		}

		static Optional<string> TryParseArg(string shortName, string longName, string arg)
        {
            var prefixes = new[] { "-"+shortName+"=", "--"+longName+"=" };

            foreach (var prefix in prefixes)
                if (arg.StartsWith(prefix))
                    return arg.Substring(prefix.Length);

            return Optional.None();
        }
	}

	class BuildHelpWriter
	{
		readonly string _description;
		readonly string _detailedDescription;

		public BuildHelpWriter(string description, string detailedDesc)
		{
			_description = description;
			_detailedDescription = detailedDesc;
		}

		public void Write(ColoredTextWriter textWriter)
		{
			textWriter.WriteHelp(new HelpArguments(
				new HelpHeader("fuse build", _description),
				new HelpSynopsis(Usage),
				new HelpDetailedDescription(_detailedDescription),
				Optional.None()
				));
		}

		static string Usage
		{
			get { return "fuse build <options> [<project-file-or-dir>]"; }
		}
	}
}
