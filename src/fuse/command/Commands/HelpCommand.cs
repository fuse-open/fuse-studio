using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Outracks.Fuse
{
	public class HelpCommand : CliCommand
	{
		readonly ColoredTextWriter _textWriter;
		readonly IEnumerable<CliCommand> _commands;
		readonly string _fuseProgramName;

		public HelpCommand(ColoredTextWriter textWriter, IEnumerable<CliCommand> commands, string fuseProgramName)
			: base("help", "Shows help for the specified command")
		{
			_textWriter = textWriter;
			_commands = commands;
			_fuseProgramName = fuseProgramName;
		}

		public override void Help()
		{
			_textWriter.WriteHelp(new HelpArguments(
				new HelpHeader("fuse " + Name, Description),
				new HelpSynopsis("fuse help [command]"),
				new HelpDetailedDescription(
					"You may find more information about fuse at our homepage.\n"  +
					"Run 'fuse tutorial' command to quickly go there."),
				Optional.None()));
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			if (args.Length != 1)
			{
				WriteFuseUsage();
				return;
			}

			var firstArg = args[0];

			var command =
				_commands
					.FirstOrNone(cmd => cmd.Name == firstArg)
					.OrThrow(new ExitWithError("'" + firstArg + "' is not a fuse command. See '" + _fuseProgramName + " help'."));

			command.Help();
		}

		void WriteFuseUsage()
		{
			_textWriter.WriteHelp(new HelpArguments(
				new HelpHeader("fuse " + Name, Optional.None()),
				new HelpSynopsis("fuse [command] [arguments] [--version]"),
				new HelpDetailedDescription(
					"Fuse consists of a set of tools that simplifies app development for a range of platforms and devices."),
				new HelpOptions(
					new Table("Commands",
						_commands
							.Where(c => !c.IsSecret)
							.OrderBy(cmd => cmd.Name)
							.Select(cmd => new Row(cmd.Name, cmd.Description))))));
		}
	}

}
