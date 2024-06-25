using System;
using System.Threading;

namespace Outracks.Fuse
{
	class LazyCliCommand : CliCommand
	{
		public readonly Lazy<CliCommand> CliCommand;

		public LazyCliCommand(string name, string description, bool isSecret, Func<CliCommand> cliCommand)
			: base(name, description, isSecret)
		{
			CliCommand = new Lazy<CliCommand>(cliCommand);
		}

		public override void Help()
		{
			CliCommand.Value.Help();
		}

		/// <exception cref="ExitWithError" />
		public override void Run(string[] args, CancellationToken ct)
		{
			CliCommand.Value.Run(args, ct);
		}
	}
}