using System;
using log4net;

namespace Outracks
{
	public class Report : IReport
	{
		readonly ILog _log;
		readonly ILogClient _logClient;
		readonly bool _synchronousCloudReporting;
		bool _hasReportedFailedLogging;

		internal Report(ILog log, ILogClient logClient, bool synchronousCloudReporting = false)
		{
			_log = log;
			_logClient = logClient;
			_synchronousCloudReporting = synchronousCloudReporting;
		}

		public void Fatal(object o, Exception exception = null, ReportTo destination = ReportTo.Log)
		{
			SendReport(destination, LogLevel.Fatal, o, exception);
		}

		public void Error(object o, ReportTo destination = ReportTo.Log)
		{
			SendReport(destination, LogLevel.Error, o, null);
		}

		public void Exception(object o, Exception e, ReportTo destination = ReportTo.Log)
		{
			SendReport(destination, LogLevel.Error, o, e);
		}

		public void Warn(object o, ReportTo destination = ReportTo.Log)
		{
			SendReport(destination, LogLevel.Warning, o, null);
		}

		public void Info(object o, ReportTo destination = ReportTo.Log)
		{
			SendReport(destination, LogLevel.Info, o, null);
		}

		public void Debug(object o, ReportTo destination = ReportTo.Log)
		{
			SendReport(destination, LogLevel.Debug, o, null);
		}

		public IReport ConfigureSync(bool reportSynchronously)
		{
			return new Report(_log, _logClient, reportSynchronously);
		}

		void SendReport(ReportTo destination, LogLevel level, object message, Exception exception)
		{
			if (destination.HasFlag(ReportTo.Log))
			{
				try
				{
					if (_logClient != null)
					{
						_logClient.Send(Formatter.Format(level, message, exception));
					}
					else
					{
						switch (level)
						{
							case LogLevel.Fatal: _log.Fatal(message, exception); break;
							case LogLevel.Error: _log.Error(message, exception); break;
							case LogLevel.Warning: _log.Warn(message, exception); break;
							case LogLevel.Info: _log.Info(message, exception); break;
							case LogLevel.Debug: _log.Debug(message, exception); break;
						}
					}
				}
				catch (Exception e)
				{
					LoggingFailed(e);
				}
			}

			if (destination.HasFlag(ReportTo.User))
			{
				if (level == LogLevel.Error || level == LogLevel.Fatal)
				{
					Console.Error.WriteLine(message);
				}
				else
				{
					Console.Out.WriteLine(message);
				}
			}

		}

		void LoggingFailed(Exception exception)
		{
			if (_hasReportedFailedLogging)
				return;

			_hasReportedFailedLogging = true;
		}
	}

	class ErrorWithoutException : Exception
	{
		public ErrorWithoutException(string message) : base(message) { }
	}

	public interface ILogClient {
		void Send(string message);
	}

	public enum LogLevel
	{
		Fatal,
		Error,
		Warning,
		Info,
		Debug
	}

	[Flags]
	public enum ReportTo
	{
		None = 0,

		Log = 1 << 1,
		User = 1 << 2,
		Headquarters = 1 << 3,

		LogAndUser = Log | User,
	}
}
