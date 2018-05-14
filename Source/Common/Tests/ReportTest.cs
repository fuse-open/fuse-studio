using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;
using NUnit.Framework;
using NSubstitute;
using Outracks.Common.Tests;

namespace Outracks.Fuse.Reporting.Tests
{
	public class ReportTest
	{
		ILog _log;
		TextWriter _outWriter;
		TextWriter _errorWriter;
		TextWriter _consoleOut;
		TextWriter _consoleError;
		Report _report;

		[Test]
		public void ByDefaultReportsToLogOnly()
		{
			_report.Info(42);
			_log.Received(1).Info(42, Arg.Any<Exception>());
			_outWriter.DidNotReceive().WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsToBothLogAndUserSpecifiedOred()
		{
			_report.Info(42, ReportTo.Log | ReportTo.User);
			_log.Received(1).Info(42, Arg.Any<Exception>());
			_outWriter.Received(1).WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsToBothLogAndUserSpecifiedWithLogAndUser()
		{
			_report.Info(42, ReportTo.LogAndUser);
			_log.Received(1).Info(42, Arg.Any<Exception>());
			_outWriter.Received(1).WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsToJustLog()
		{
			// ReSharper disable once RedundantArgumentDefaultValue
			_report.Info(42, ReportTo.Log);
			_log.Received(1).Info(42, Arg.Any<Exception>());
			_outWriter.DidNotReceive().WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsToJustUser()
		{
			_report.Info(42, ReportTo.User);
			_log.DidNotReceive().Info(42, Arg.Any<Exception>());
			_outWriter.Received(1).WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsToJustCloud()
		{
			_report.Info(42, ReportTo.Headquarters);
			_log.DidNotReceive().Info(Arg.Any<object>(), Arg.Any<Exception>());
			_outWriter.DidNotReceive().WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsFatal()
		{
			_report.Fatal(42, null, ReportTo.LogAndUser);
			ReportedTo("Fatal");
			_outWriter.DidNotReceive().WriteLine(Arg.Any<object>());
			_errorWriter.Received(1).WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsError()
		{
			_report.Error(42, ReportTo.LogAndUser);
			ReportedTo("Error");
			_outWriter.DidNotReceive().WriteLine(Arg.Any<object>());
			_errorWriter.Received(1).WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsWarn()
		{
			_report.Warn(42, ReportTo.LogAndUser);
			ReportedTo("Warn");
			_outWriter.Received(1).WriteLine(Arg.Any<object>());
			_errorWriter.DidNotReceive().WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsInfo()
		{
			_report.Info(42, ReportTo.LogAndUser);
			ReportedTo("Info");
			_outWriter.Received(1).WriteLine(Arg.Any<object>());
			_errorWriter.DidNotReceive().WriteLine(Arg.Any<object>());
		}

		[Test]
		public void ReportsDebug()
		{
			_report.Debug(42, ReportTo.LogAndUser);
			ReportedTo("Debug");
			_outWriter.Received(1).WriteLine(Arg.Any<object>());
			_errorWriter.DidNotReceive().WriteLine(Arg.Any<object>());
		}

		[Test]
		[Ignore("Just for manual testing")]
		public void ReportsToCloudWhenProcessIsExiting()
		{
			RunTestProcess("ReportsToCloudWhenProcessIsExiting");
		}

		[Test]
		[Ignore("Just for manual testing")]
		public void ReportsToCloudInUnhandledExceptionCallBack()
		{
			RunTestProcess("ReportsToCloudWhenProcessIsExiting");
		}

		void ReportedTo(string m)
		{
			_log.Received(m.Equals("Fatal") ? 1 : 0).Fatal(Arg.Any<object>(), Arg.Any<Exception>());
			_log.Received(m.Equals("Error") ? 1 : 0).Error(Arg.Any<object>(), Arg.Any<Exception>());
			_log.Received(m.Equals("Warn") ? 1 : 0).Warn(Arg.Any<object>(), Arg.Any<Exception>());
			_log.Received(m.Equals("Info") ? 1 : 0).Info(Arg.Any<object>(), Arg.Any<Exception>());
			_log.Received(m.Equals("Debug") ? 1 : 0).Debug(Arg.Any<object>(), Arg.Any<Exception>());
		}

		static Process RunTestProcess(string argument)
		{
			return Helpers.RunTestProcess(argument, Assembly.GetExecutingAssembly());

		}

		[SetUp]
		public void SetUp()
		{
			_log = Substitute.For<ILog>();
			_outWriter = Substitute.For<TextWriter>();
			_errorWriter = Substitute.For<TextWriter>();
			_consoleOut = Console.Out;
			_consoleError = Console.Error;
			Console.SetOut(_outWriter);
			Console.SetError(_errorWriter);
			_report = new Report(_log, null);
		}

		[TearDown]
		public void TearDown()
		{
			Console.SetOut(_consoleOut);
			Console.SetError(_consoleError);
		}
	}

	//Kept separate from the old tests until we get rid of log4net, will merge then
	public class SelectsCorrectBackend
	{
		[Test]
		public void WhenInitialisedWithoutLogClientLogsToILog()
		{
			var log = Substitute.For<ILog>();
			var report = new Report(log, null);
			report.Info(42);
			log.Received(1).Info(42, Arg.Any<Exception>());
		}

		[Test]
		public void WhenInitialisedWithLogCLientLogsToLogClientAndNotILog()
		{
			var client = Substitute.For<ILogClient>();
			var log = Substitute.For<ILog>();
			var report = new Report(log, client);
			report.Info(42);
			log.DidNotReceive().Info(Arg.Any<object>(), Arg.Any<Exception>());
			client.Received(1).Send(Arg.Any<string>());
		}
	}
}
