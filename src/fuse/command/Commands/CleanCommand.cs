using System;
using System.Linq;
using System.Threading;

namespace Outracks.Fuse
{
	public class CleanCommand : CliCommand
	{
		public CleanCommand() : base("clean", "Clean a project")
		{
		}

		public override void Help()
		{
			Uno.CLI.Program.Main("clean", "--help");
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			Uno.CLI.Program.Main(new []{"clean"}.Concat(args).ToArray());
			Console.WriteLine("Clean completed");
		}
	}
}
