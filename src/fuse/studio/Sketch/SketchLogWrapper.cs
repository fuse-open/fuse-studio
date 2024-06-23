using System;
using SketchConverter.API;

namespace Outracks.Fuse.Studio {
	class SketchLogWrapper : ILogger
	{
		readonly IObserver<string> _logMessages;
		readonly IReport _log;

		public SketchLogWrapper(IObserver<string> logMessages, IReport log)
		{
			_logMessages = logMessages;
			_log = log;
		}

		public void Info(string info)
		{
			_logMessages.OnNext(info + "\n");
		}

		public void Warning(string warning)
		{
			_logMessages.OnNext(warning + "\n");
		}

		public void Error(string error)
		{
			_log.Error(error);
			_logMessages.OnNext(error + "\n");
		}
	}
}