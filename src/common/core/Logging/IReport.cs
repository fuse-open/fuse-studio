using System;

namespace Outracks
{
	public interface IReport
	{
		void Fatal(object o, Exception exception = null, ReportTo destination = ReportTo.Log);
		void Error(object o, ReportTo destination = ReportTo.Log);
		void Exception(object o, Exception e, ReportTo destination = ReportTo.Log);
		void Warn(object o, ReportTo destination = ReportTo.Log);
		void Info(object o, ReportTo destination = ReportTo.Log);
		void Debug(object o, ReportTo destination = ReportTo.Log);
		IReport ConfigureSync(bool reportSynchronously);
	}
}