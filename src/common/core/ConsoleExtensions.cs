using System;
using System.IO;

namespace Outracks
{
	public static class ConsoleExtensions
	{
		public static void RedirectStreamsToLogFiles(string programName)
		{
			for (int i = 0; i < 64; i++)
			{
				try
				{
					// Redirect output streams to disk (one file per process).
					var dir = Path.GetDirectoryName(typeof(ConsoleExtensions).Assembly.Location);
					Console.SetError(new StreamWriter(new FileStream(Path.Combine(dir, $"{programName}-stderr{i}.txt"), FileMode.OpenOrCreate, FileAccess.Write)) { AutoFlush = true });
					Console.SetOut(new StreamWriter(new FileStream(Path.Combine(dir, $"{programName}-stdout{i}.txt"), FileMode.OpenOrCreate, FileAccess.Write)) { AutoFlush = true });
					break;
				}
				catch
				{
				}
			}
		}
	}
}
