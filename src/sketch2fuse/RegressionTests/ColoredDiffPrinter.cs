using System;
using System.Collections.Generic;

namespace RegressionTests
{
	public static class ColoredDiffPrinter
	{
		public static void Print(IEnumerable<FileDiff> diffs)
		{
			foreach (var diff in diffs)
			{
				Console.WriteLine(diff.Summary);
				foreach (var line in diff.Diff.Split('\n'))
				{
					using (new ColorPrinter(ColorForDiffLine(line)))
					{
						Console.WriteLine(line);
					}
				}
			}
		}

		private static ConsoleColor ColorForDiffLine(string line)
		{
			if (line.StartsWith("-"))
			{
				return ConsoleColor.DarkRed;
			}
			if (line.StartsWith("+"))
			{
				return ConsoleColor.DarkGreen;
			}
			return ConsoleColor.White;
		}
	}
}
