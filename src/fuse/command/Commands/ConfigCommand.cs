using System.Linq;
using System.Threading;

namespace Outracks.Fuse
{
	public class ConfigCommand : CliCommand
	{
		public ConfigCommand() : base("config", "Print configuration info")
		{
		}

		public override void Help()
		{
			Uno.CLI.Program.Main("config", "--help");
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			Uno.CLI.Program.Main(new []{"config"}.Concat(args).ToArray());
		}
	}
}
