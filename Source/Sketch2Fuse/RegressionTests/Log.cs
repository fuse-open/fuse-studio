using System;

namespace RegressionTests
{
	internal static class Log
	{
		public static void Info(string message)
		{
			using (new ColorPrinter(ConsoleColor.Blue))
				Console.WriteLine(message);
		}

		public static void Success(string message)
		{
			using (new ColorPrinter(ConsoleColor.DarkGreen))
				Console.WriteLine(message);
		}

		public static void Warn(string message)
		{
			using (new ColorPrinter(ConsoleColor.DarkYellow))
				Console.WriteLine(message);
		}

		public static void Error(string message)
		{
			using (new ColorPrinter(ConsoleColor.DarkRed))
				Console.WriteLine(message);
		}

		public static string Prompt(string message)
		{
			using (new ColorPrinter(ConsoleColor.Blue))
			{
				Console.Write(message);
				return Console.ReadLine();
			}
		}
	}
}
