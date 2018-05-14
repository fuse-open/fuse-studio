using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Outracks.Diagnostics;
using Outracks.IPC;

namespace Outracks.Common.TestProcess
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			Log(string.Join(",", args.Select(s => "'" + s + "'")));

			try
			{
				FindAndRunMethod(args);
			}
			catch (Exception e)
			{
				if (e is TargetInvocationException)
				{
					Log(e.InnerException.ToString());
					throw e.InnerException;
				}
				else
				{
					Log(e.ToString());
					throw;
				}
			}
			Log("Exiting\n");
		}

		private static void Log(string s)
		{
			File.AppendAllText(Path.Combine(TmpDir, "testprocess.log"), DateTime.Now + " : " + s + "\n");
		}

		static string TmpDir
		{
			get { return Platform.OperatingSystem == OS.Mac ? "/tmp" : Path.GetTempPath(); } //TODO maybe GetTempPath works cross platform
		}

		private static void FindAndRunMethod(string[] args)
		{
			if (args.Length < 1)
			{
				throw new Exception("args.Length must be at least 1, you gave " + args.Length);
			}

			var methodName = args[0];

			var method = typeof(Program)
				.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
				.FirstOrDefault(m => m.Name == methodName);
			if (method == null)
			{
				throw new Exception("Method '" + methodName + "' not found");
			}

			method.Invoke(null, new object[] {args.Skip(1).ToArray()});
		}

		private static void CrashingClientMakesHostReadZeroBytes(string[] args)
		{
			var pipeName = GetPipeNameOrThrow(args);
			var client = Pipe.Connect(pipeName).Result;
			Log("Connected");
			var message = new byte[] {0x01};
			client.Write(message, 0, 1);
			HangForever();
		}

		private static void CrashingHostMakesClientWritingThrow(string[] args)
		{
			var pipeName = GetPipeNameOrThrow(args);
			var host = Pipe.Host(pipeName).Result;
			Log("Connected");
			var buffer = new byte[1];
			host.Read(buffer, 0, 1);
			HangForever();
		}

		private static void CompletesWhenClientCrashes(string[] args)
		{
			var pipeName = GetPipeNameOrThrow(args);
			var client = Pipe.Connect(pipeName).Result;
			Log("Connected");
			var messageWithASingleInt42 = Convert.FromBase64String("A0ludAQAAAAqAAAA"); //Compute this in the corresponding test method if the format changes
			client.Write(messageWithASingleInt42, 0, messageWithASingleInt42.Length);
			HangForever();
		}

		private static void NotifiesCallbackWhenHostCrashes(string[] args)
		{
			var pipeName = GetPipeNameOrThrow(args);
			var client = Pipe.Host(pipeName).Result;
			Log("Connected");
			var messageWithASingleInt42 = Convert.FromBase64String("A0ludAQAAAAqAAAA"); //Compute this in the corresponding test method if the format changes
			var buffer = new byte[messageWithASingleInt42.Length];
			client.Read(buffer, 0, buffer.Length);
			HangForever();
		}

		private static PipeName GetPipeNameOrThrow(string[] args)
		{
			if (args.Length != 1)
			{
				throw new Exception("Expected 1 argument, got " + args.Length);
			}
			var pipeName = args[0];
			Log("Using pipe '" + pipeName + "'");
			return new PipeName(pipeName);
		}

		private static void HangForever()
		{
			new TaskCompletionSource<bool>().Task.Wait();
		}


		private static void ReportsToCloudWhenProcessIsExiting(string[] args)
		{
			var client = ReportFactory.GetReporter(new Guid(), new Guid(), "name");
			var message = "Test: ReportsToCloudWhenProcessIsExiting at '" + DateTime.Now + "'";
			client.Exception(message, new Exception(message), ReportTo.Headquarters);
			//And then we just exit without waiting
		}

		private static void ReportsToCloudFromUnhandledExceptionHandler(string[] args)
		{
			var client = ReportFactory.GetReporter(new Guid(), new Guid(), "name");
			AppDomain.CurrentDomain.ReportUnhandledExceptions(client);
			var message = "Test: ReportsToCloudFromUnhandledExceptionHandler at '" + DateTime.Now + "'";
			throw new Exception(message);
			//And then we just exit without waiting
		}
	}
}