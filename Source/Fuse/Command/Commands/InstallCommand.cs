using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mono.Options;
using Outracks.Fuse.Components;
using Outracks.Fuse.ModuleInstaller;
using Outracks.Fuse.Setup;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	enum InstallMode
	{
		Install,
		CheckStatus,
	}

	public class InstallCommand : CliCommand
	{
		public static CliCommand CreateInstallCommand()
		{
			var fuse = FuseApi.Initialize("Fuse", new List<string>());
			return new InstallCommand(
				ColoredTextWriter.Out,
				fuse);
		}

		readonly ComponentInstallers _componentInstallers;
		readonly ColoredTextWriter _output;
		readonly OptionSet _options;
		readonly IFuse _fuse;
		InstallMode _mode = InstallMode.Install;

		public InstallCommand(
			ColoredTextWriter output,
			IFuse fuse)
			: base("install", "Install an external component")
		{
			_componentInstallers = new ComponentInstallers(fuse);
			_output = output;
			_fuse = fuse;
			_options = new OptionSet()
			{
				{ "s|status", "Check install status of a package.", a => _mode = InstallMode.CheckStatus }
			};
		}

		public override void Help()
		{
			var helpArguments = new HelpArguments(
				new HelpHeader("fuse " + Name, Description),
				new HelpSynopsis("fuse install [<Options>] <Component>"),
				new HelpDetailedDescription(
@"This command downloads and installs a component. 
NOTE: Network connection is required."),
				new HelpOptions(
					new[] {
						_options.ToTable(),
						new Table("Components", 
							GetComponentInstallers())
					}));
			_output.WriteHelp(helpArguments);
		}

		IEnumerable<Row> GetComponentInstallers()
		{
			return _componentInstallers
				.Components
				.Select(c => new Row(c.Name, c.Description));
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			VersionWriter.Write(_output, _fuse.Version);

			Application.Initialize(args);

			var remainingArgs = _options.Parse(args).ToArray();
			var packageName = remainingArgs.TryGetAt(0).OrThrow(new ExitWithError("Fuse install requires a valid component name. See 'fuse help install' for details."));

			var component = _componentInstallers.Components.First(c => c.Name == packageName);

			try
			{
				switch (_mode)
				{
					case InstallMode.Install:
						{
							using (_output.PushColor(ConsoleColor.Yellow))
								_output.WriteLine("Starting " + component.Name + " installer");
							component.Install();
							using (_output.PushColor(ConsoleColor.Yellow))
								_output.WriteLine("Done installing " + component.Name);
						}
						break;
					case InstallMode.CheckStatus:
						if (component.Status == ComponentStatus.Installed)
						{
							using (_output.PushColor(ConsoleColor.Green))
								_output.WriteLine(packageName + " is installed.");
						}
						else if (component.Status == ComponentStatus.UpdateAvailable)
						{
							throw new ExitWithError("An update is available to " + packageName, (byte)ComponentStatus.UpdateAvailable);
						}
						else
						{
							throw new ExitWithError(packageName + " is not installed.", (byte)ComponentStatus.NotInstalled);
						}
						break;
				}
			}
			catch (PluginInstallerFailed e)
			{
				throw new ExitWithError(e.Message);
			}
		}
	}
}