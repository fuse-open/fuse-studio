using System.Collections.Generic;
using System.Linq;
using SketchConverter;
using SketchConverter.API;

namespace SketchConverterTests
{
	class MessageListLogger : ILogger
	{
		public void Info(string info)
		{
			Messages.Add("INFO:\t" + info);
		}

		public void Warning(string warning)
		{
			Messages.Add("WARNING:\t" + warning);
		}

		public void Error(string error)
		{
			Messages.Add("ERROR:\t" + error);
		}

		public IList<string> Messages = new List<string>();

		public IList<string> ErrorsAndWarnings()
		{
			return Messages.Where(m => !m.StartsWith("INFO:")).ToList();
		}

		public IEnumerable<string> Warnings()
		{
			return Messages
				.Where(m => m.StartsWith("WARNING:"))
				.Select(e => e.Replace("WARNING:\t",""));
		}
	}
}
