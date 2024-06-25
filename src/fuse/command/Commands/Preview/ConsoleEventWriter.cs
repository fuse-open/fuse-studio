using System;
using Fuse.Preview;
using Outracks.Simulator.Parser;

namespace Outracks.Fuse
{
	public static class ConsoleEventWriter
	{
		public static void WriteRefreshFailed(this ColoredTextWriter output, UserCodeContainsErrors e)
		{
			using (output.PushColor(ConsoleColor.Red))
			{
				output.WriteLine("Refresh failed, see build log");
			}
		}

		public static void WriteRefreshFailed(this ColoredTextWriter output, Exception e)
		{
			using (output.PushColor(ConsoleColor.DarkRed))
			{
				output.WriteLine("Refresh failed: " + e + ": " + e.Message);
				output.WriteLine(e.StackTrace);
			}
		}

		public static void WriteBuildEvent(this ColoredTextWriter output, IBinaryMessage buildEvent)
		{
			buildEvent.DoSome(
				(BuildLogged logEvent) =>
				{
					using(output.PushColor(logEvent.Color == ConsoleColor.Red ? ConsoleColor.Red : (ConsoleColor?)null))
						output.Write(logEvent.Text);
				},
				(BuildIssueDetected arg) =>
				{
					using (output.PushColor(ToColor(arg.Severity)))
					{
						output.WriteLine(arg.ToString().StripSuffix("\n"));
					}
				});
		}

		static ConsoleColor? ToColor(BuildIssueType type)
		{
			if (type == BuildIssueType.Error || type == BuildIssueType.FatalError)
				return ConsoleColor.Red;
			if (type == BuildIssueType.Warning)
				return ConsoleColor.Yellow;
			return null;
		}
	}
}