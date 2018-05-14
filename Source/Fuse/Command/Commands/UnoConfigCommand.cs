using System;
using System.Threading;
using Uno.Configuration;

namespace Outracks.Fuse
{
	public class UnoConfigCommand : CliCommand
	{
		public static UnoConfigCommand CreateUnoConfigCommand()
		{
			return new UnoConfigCommand(ColoredTextWriter.Out);
		}

		readonly ColoredTextWriter _textWriter;
		readonly HelpArguments _helpArguments;

		public UnoConfigCommand(ColoredTextWriter textWriter) : base("unoconfig", "Print uno config")
		{
			_textWriter = textWriter;
			_helpArguments = new HelpArguments(
				new HelpHeader("fuse " + Name, Description),
				new HelpSynopsis("fuse unoconfig"),
				new HelpDetailedDescription(@"Prints the current uno configuration"),
				Optional.None());
		}

		public override void Help()
		{
			_textWriter.WriteHelp(_helpArguments);
		}

		public override void Run(string[] args, CancellationToken ct)
		{
			PrintUnoConfig(UnoConfig.Current, _textWriter);
		}

		public static void PrintUnoConfig(UnoConfig unoConfig, ColoredTextWriter coloredTextWriter)
		{
			using (coloredTextWriter.PushColor(ConsoleColor.Green))
			{
				coloredTextWriter.WriteLine("Uno settings");
			}
			coloredTextWriter.WriteLine(unoConfig.ToString());
			coloredTextWriter.WriteLine();
			using (coloredTextWriter.PushColor(ConsoleColor.Green))
			{
				coloredTextWriter.WriteLine("Config files");
			}
			foreach (var file in unoConfig.Files)
			{
				coloredTextWriter.WriteLine(file);
			}
		}
	}
}
