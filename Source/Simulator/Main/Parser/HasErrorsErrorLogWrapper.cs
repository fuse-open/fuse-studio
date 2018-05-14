using Uno.UX.Markup.Common;

namespace Outracks.Simulator.Parser
{
	class HasErrorsErrorLogWrapper : IMarkupErrorLog
	{
		public bool HasErrors { get; private set; }

		readonly IMarkupErrorLog _innerLog;

		public HasErrorsErrorLogWrapper(IMarkupErrorLog innerLog)
		{
			_innerLog = innerLog;
		}

		public void ReportError(string message)
		{
			HasErrors = true;
			_innerLog.ReportError(message);
		}

		public void ReportWarning(string message)
		{
			_innerLog.ReportWarning(message);
		}

		public void ReportError(string path, int line, string message)
		{
			HasErrors = true;
			_innerLog.ReportError(path, line, message);
		}

		public void ReportWarning(string path, int line, string message)
		{
			_innerLog.ReportWarning(path, line, message);
		}
	}
}
