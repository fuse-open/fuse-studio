using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading;
using Outracks.Fuse.Analytics;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse
{
	public class ResetPreviewCliCommand : CliCommand
	{
		public static CliCommand CreateResetPreviewCommand()
		{
			var fuse = FuseApi.Initialize("Fuse", new List<string>());
			return new ResetPreviewCliCommand(ColoredTextWriter.Out, fuse);
		}

		readonly IFuseLauncher _daemonSpawner;
		readonly ColoredTextWriter _textWriter;
		readonly HelpArguments _helpArguments;

		public ResetPreviewCliCommand(ColoredTextWriter textWriter, IFuseLauncher daemonSpawner)
			: base("reset-preview", "Causes all active previews to reset to the default state.", true)
		{
			_textWriter = textWriter;
			_daemonSpawner = daemonSpawner;
			_helpArguments = new HelpArguments(
				new HelpHeader("fuse " + Name, Description),
				new HelpSynopsis("fuse reset-preview"),
				new HelpDetailedDescription(@"Causes all active previews to reset to their default application state."), 
				Optional.None());
		}
		public override void Run(string[] args, CancellationToken ct)
		{
			using (var client =
				_daemonSpawner.ConnectOrSpawn(
					identifier: "PreviewReset",
					timeout: TimeSpan.FromMinutes(1)))
			{
				client.Broadcast(new ResetPreviewEvent());
			}	
		}
		public override void Help()
		{
			_textWriter.WriteHelp(_helpArguments);
		}

	}
}
