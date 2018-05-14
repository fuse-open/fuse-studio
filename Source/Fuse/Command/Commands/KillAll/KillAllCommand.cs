using System.Collections.Generic;
using System.Threading;
using Outracks.Fuse.Analytics;

namespace Outracks.Fuse
{
	public class KillAllCommand : CliCommand
    {
		public static KillAllCommand CreateKillAllCommand()
		{
			var fuse = FuseApi.Initialize("Fuse", new List<string>());
			return new KillAllCommand(
				fuse,
				ColoredTextWriter.Out,
				new FuseKiller(fuse.Report, fuse.FuseRoot));
		}

		readonly IFuseKiller _killer;
		readonly IFuse _fuse;
		readonly ColoredTextWriter _coloredConsole;

		public KillAllCommand(IFuse fuse, ColoredTextWriter coloredConsole, IFuseKiller killer)
			: base("kill-all", "Kill all Fuse processes (even the daemon)")
		{
			_fuse = fuse;
			_coloredConsole = coloredConsole;
			_killer = killer;
		}

		public override void Help()
		{
			_coloredConsole.WriteHelp(
				new HelpArguments(
					new HelpHeader("Fuse", Description),
					new HelpSynopsis("fuse " + Name),
					new HelpDetailedDescription("Kills all Fuse processes.\nNOTE: It will also close all preview instances."),
					Optional.None()));
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			VersionWriter.Write(_coloredConsole, _fuse.Version);
			_killer.Execute(_coloredConsole);
		}
    }
}
