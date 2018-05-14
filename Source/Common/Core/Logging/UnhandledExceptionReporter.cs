using System;
using Outracks.Diagnostics;

namespace Outracks
{
	public static class UnhandledExceptionReporter
	{
		public static void ReportUnhandledExceptions(this AppDomain domain, IReport report)
		{
			domain.UnhandledException += (sender, e) =>
			{
				var exception = e.ExceptionObject as Exception;
				if (exception != null)
				{
					report.ConfigureSync(true).Fatal(
						"Unhandled exception: " + exception.Message,
						exception,
						ReportTo.Log | ReportTo.User | ReportTo.Headquarters);

					if (Platform.OperatingSystem == OS.Mac)
						Environment.ExitCode = 255;
					else
						Environment.Exit(255);
				}
			};

		}
	}
}