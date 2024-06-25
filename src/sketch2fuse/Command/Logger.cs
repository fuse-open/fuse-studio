using System;
using SketchConverter;
using SketchConverter.API;

namespace Command
{
	internal class Logger : ILogger
	{
		public void Info(string info)
		{
			Console.WriteLine("INFO: " + info);
		}

		public void Warning(string warning)
		{
			Console.WriteLine("WARNING: " + warning);
		}

		public void Error(string error)
		{
			Console.WriteLine("ERROR: " + error);
		}
	}
}
