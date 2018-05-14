using System;
using System.Diagnostics;
using System.Globalization;

namespace Outracks
{
	static class Formatter {
		//Does not throw
		public static string Format(LogLevel level, object message, Exception exception)
		{
			return FormattedNow() + " " +
				FormattedProcessAndThreadId() + " " +
				FormattedLevel(level) + " " +
				FormattedMessage(message) + FormattedException(exception);
		}

		//Does not throw
		static string FormattedLevel(LogLevel level)
		{
			return level.ToString().ToUpper(CultureInfo.InvariantCulture);
		}

		//Does not throw
		static string FormattedProcessAndThreadId()
		{
			return "[" + ProcessId() + ":" + System.Threading.Thread.CurrentThread.ManagedThreadId + "]";
		}

		//Does not throw
		static int ProcessId()
		{
			try
			{
				return Process.GetCurrentProcess().Id;
			}
			catch (Exception)
			{
				return 0;
			}
		}

		//Does not throw
		static string FormattedMessage(object message)
		{
			try
			{
				return OnSingleLine(Convert.ToString(message));
			}
			catch (Exception e)
			{
				return "(The log message was non-null, but '" + message.GetType().Name + ".ToString()' threw an exception: " + FormattedException(e) + ")";
			}
		}

		//Does not throw
		static string FormattedNow()
		{
			try
			{
				return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture);
			}
			catch (ArgumentOutOfRangeException) //Could theoretically happen. FormatException can not happen.
			{
				return "0000-00-00 00:00:00,000 ";
			}
		}

		//Does not throw
		static string FormattedException(Exception exception)
		{
			if (exception == null)
			{
				return "";
			}
			return " Exception: " + OnSingleLine(exception.ToString());
		}

		//Does not throw
		static string OnSingleLine(string message)
		{
			return string.IsNullOrEmpty(message) ? "" : message.Replace("\r", "").Replace("\n", "    ");
		}
	}
}