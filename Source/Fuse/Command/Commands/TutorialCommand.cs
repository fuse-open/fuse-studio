using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Outracks.Fuse.Analytics;

namespace Outracks.Fuse
{
	class TutorialCommand : CliCommand
	{
		public static CliCommand CreateTutorialCommand()
		{
			var fuse = FuseApi.Initialize("Fuse", new List<string>());
			return new TutorialCommand(ColoredTextWriter.Out);
		}

		readonly ColoredTextWriter _textWriter;
		readonly HelpArguments _helpArguments;
		const string TutorialsAndGuidesUrl = "https://go.fusetools.com/guides";

		public TutorialCommand(ColoredTextWriter textWriter) : base("tutorial", "Go to tutorials and guides")
		{
			_textWriter = textWriter;
			_helpArguments = new HelpArguments(
				new HelpHeader("fuse " + Name, Description),
				new HelpSynopsis("fuse tutorial"),
				new HelpDetailedDescription(
@"Takes you to our landing page, using the default program set for opening HTTP URL's.
NOTE: It require network connection."),
				Optional.None());
		}

		public override void Help()
		{
			_textWriter.WriteHelp(_helpArguments);
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			Process.Start(TutorialsAndGuidesUrl);
		}
	}
}