using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;

namespace Outracks.Fuse.Reporting.Tests
{
	public class FormatterTests
	{
		[Test]
		public void SetsCorrectLevel()
		{
			Assert.AreEqual("DEBUG", new ParsedLogMessage(Formatter.Format(LogLevel.Debug, "foo", null)).Level);
			Assert.AreEqual("INFO", new ParsedLogMessage(Formatter.Format(LogLevel.Info, "foo", null)).Level);
			Assert.AreEqual("WARNING", new ParsedLogMessage(Formatter.Format(LogLevel.Warning, "foo", null)).Level);
			Assert.AreEqual("ERROR", new ParsedLogMessage(Formatter.Format(LogLevel.Error, "foo", null)).Level);
			Assert.AreEqual("FATAL", new ParsedLogMessage(Formatter.Format(LogLevel.Fatal, "foo", null)).Level);
		}

		[Test]
		public void SetsCorrectPidAndThread()
		{
			var expected = "[" + Process.GetCurrentProcess().Id + ":" + Thread.CurrentThread.ManagedThreadId + "]";
			Assert.AreEqual(expected, new ParsedLogMessage(Formatter.Format(LogLevel.Info, "foo", null)).Pid);
		}

		[Test]
		public void SetsCorrectMessage()
		{
			Assert.AreEqual("foo", new ParsedLogMessage(Formatter.Format(LogLevel.Debug, "foo", null)).Message);
			Assert.AreEqual("", new ParsedLogMessage(Formatter.Format(LogLevel.Debug, "", null)).Message);
			Assert.AreEqual("42", new ParsedLogMessage(Formatter.Format(LogLevel.Debug, 42, null)).Message);
			Assert.AreEqual("foo bar", new ParsedLogMessage(Formatter.Format(LogLevel.Debug, "foo bar", null)).Message);
			Assert.AreEqual("foo    bar", new ParsedLogMessage(Formatter.Format(LogLevel.Debug, "foo\nbar", null)).Message);
			Assert.AreEqual("foo    bar", new ParsedLogMessage(Formatter.Format(LogLevel.Debug, "foo\r\nbar", null)).Message);
			Assert.AreEqual("", new ParsedLogMessage(Formatter.Format(LogLevel.Debug, null, null)).Message);
		}

		[Test]
		public void IncludesExceptions()
		{
			try
			{
				ThrowyFunction();
				throw new Exception("Expected an exception to be thrown");
			}
			catch (Exception e)
			{
				var msg = new ParsedLogMessage(Formatter.Format(LogLevel.Debug, "foo", e)).Message;
				StringAssert.StartsWith("foo Exception:", msg);
				StringAssert.Contains("Something went wrong here", msg);
				StringAssert.Contains("ThrowyFunction", msg);
			}
		}

		[Test]
		public void HandlesObjectsWithThrowingToString()
		{
			var msg = new ParsedLogMessage(Formatter.Format(LogLevel.Debug, new Throwy(), null)).Message;
			StringAssert.Contains("The log message was non-null, but 'Throwy.ToString()' threw an exception", msg);
			StringAssert.Contains("I'm sorry, Dave, I'm afraid I can't do that", msg);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void ThrowyFunction()
		{
			throw new Exception("Something went wrong here");
		}
	}

	class Throwy
	{
		public override string ToString()
		{
			throw new Exception("I'm sorry, Dave, I'm afraid I can't do that");
		}
	}

	class ParsedLogMessage
	{
		public readonly DateTime DateAndTime;
		public readonly string Level;
		public readonly string Pid;
		public readonly string Message;

		public ParsedLogMessage(string mesasage)
		{
			var regex = new Regex(@"(?<datetime>\S+ \S+) (?<pid>\S+) (?<level>\S+) (?<mesage>.*)");
			var m = regex.Matches(mesasage)[0];
			DateAndTime = DateTime.ParseExact(m.Groups["datetime"].Value, "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture);
			Pid = m.Groups["pid"].Value;
			Level = m.Groups["level"].Value;
			Message = m.Groups["mesage"].Value;
		}
	}
}