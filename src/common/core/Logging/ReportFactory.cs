using System;
using Outracks.Diagnostics;
using Outracks.Fuse.Analytics;

namespace Outracks
{
	public class ReportFactory
	{
		public static IReport GetReporter(Guid systemId, Guid sessionId, string name)
		{
			return new Report(
				LogFactory.GetLogger(name),
				CreateClient());
		}

		public static readonly IReport FallbackReport;

		static ReportFactory()
		{
			FallbackReport = new NullReport(); //Protect against someone indirectly using `FallbackReport` during execution of `GetReporter()`
			FallbackReport = GetReporter(SystemGuidLoader.LoadOrCreateOrEmpty(), Guid.NewGuid(), "fuse X");
		}

		static ILogClient CreateClient()
		{
			bool useLogServer = false;
			if (!useLogServer)
			{
				return null;
			}
			switch (Platform.OperatingSystem)
			{
			case OS.Windows:
				return null;
			case OS.Mac:
				return new UnixSocketLogClient();
			default:
				throw new Exception("Unsupported platform");
			}
		}

		private class NullReport : IReport
		{
			public void Fatal(object o, Exception exception = null, ReportTo destination = ReportTo.Log) { }
			public void Error(object o, ReportTo destination = ReportTo.Log) { }
			public void Exception(object o, Exception e, ReportTo destination = ReportTo.Log) { }
			public void Warn(object o, ReportTo destination = ReportTo.Log) { }
			public void Info(object o, ReportTo destination = ReportTo.Log) { }
			public void Debug(object o, ReportTo destination = ReportTo.Log) { }
			public IReport ConfigureSync(bool reportSynchronously) { return this; }
		}
	}
}
