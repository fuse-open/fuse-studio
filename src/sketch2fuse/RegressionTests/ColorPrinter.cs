using System;

namespace RegressionTests
{
	internal class ColorPrinter : IDisposable
	{
		private readonly ConsoleColor _previous;

		public ColorPrinter(ConsoleColor color)
		{
			_previous = Console.ForegroundColor;
			Console.ForegroundColor = color;
		}

		public void Dispose()
		{
			Console.ForegroundColor = _previous;
		}
	}
}
