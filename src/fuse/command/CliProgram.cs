using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Outracks.Fuse
{
	public class CliProgram
	{
		readonly ColoredTextWriter _error;

		readonly IReport _log;
		readonly string _programName;
		readonly DefaultCliCommand _defaultCommand;
		readonly Action _beforeRun;
		readonly List<CliCommand> _commands = new List<CliCommand>();

		public CliProgram(
			IReport log,
			ColoredTextWriter outputWriter,
			ColoredTextWriter errorWriter,
			string programName,
			Action beforeRun,
			DefaultCliCommand defaultCommand,
			params CliCommand[] otherCommands)
		{
			_log = log;
			_programName = programName;
			_defaultCommand = defaultCommand;
			_beforeRun = beforeRun;
			_error = errorWriter;
			_commands.Add(defaultCommand);
			_commands.AddRange(otherCommands);
			_commands.Add(new HelpCommand(outputWriter, _commands, programName));
		}

		public int Main(string[] args, CancellationToken ct)
		{
			try
			{
				_beforeRun();
				args.TryGetAt(0).Do(
					some: arg => GetCommand(arg).Run(args.Skip(1).ToArray(), ct),
					none: () => _defaultCommand.RunDefault(args, ct));

				return 0;
			}
			catch (UnknownCommand e)
			{
				_log.Error(e.Command + " is not a valid command.");
				WriteErrorLine("'" + e.Command + "' is not a valid command. See '" + _programName + " help'.");
				return 1;
			}
			catch (ExitWithError e)
			{
				_log.Error(e.ErrorOutput);
				WriteErrorLine(e.ErrorOutput);
				return e.ExitCode;
			}
			catch (Exception e)
			{
				WriteErrorLine("unhandled exception: " + e.Message);

				using (_error.PushColor(ConsoleColor.Red))
					_error.WriteStackTraces(e);

				throw;
			}
		}

		void WriteErrorLine(string error)
		{
			using(_error.PushColor(ConsoleColor.Red))
				_error.WriteLine(_programName + ": " + error);
		}

		/// <param name="name"></param>
		/// <exception cref="UnknownCommand"/>
		/// <returns></returns>
		CliCommand GetCommand(string name)
		{
			return _commands.FirstOrNone(cmd => cmd.Name == name).OrThrow(new UnknownCommand(name));
		}
	}
}