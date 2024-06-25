using System.Linq;
using System.Threading;

namespace Outracks.Fuse
{
	public class BuildCommand : CliCommand
	{
		public BuildCommand() : base("build", "Build a project")
		{
		}

		public override void Help()
		{
			Uno.CLI.Program.Main("build", "--help");
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			Uno.CLI.Program.Main(new []{"build"}.Concat(args).ToArray());
		}
	}
}
