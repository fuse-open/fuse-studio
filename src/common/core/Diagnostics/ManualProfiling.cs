using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Outracks
{
	/// <summary>
	/// Contains thread safe manual profiling functions.
	//	We can't make a calltree because of the fact that there are no way to get the parent thread.
	/// </summary>
	public static class ManualProfiling
	{
		static readonly ConcurrentBag<ProfilingEvent> _events = new ConcurrentBag<ProfilingEvent>();

		public static Func<T, T> Count<T>(string name)
		{
			return t =>
			{
				Count(name);
				return t;
			};
		}

		public static Func<T, T> Count<T>(string name, Func<T, bool> count)
		{
			return t =>
			{
				Count(name, count(t));
				return t;
			};
		}

		static readonly ConcurrentQueue<string> Counts = new ConcurrentQueue<string>();

		public static void Count(string name, bool count = true)
		{
			if (!count) return;
			Counts.Enqueue(name);
		}

		public static int GetCount(string name)
		{
			return Counts.Count(c => c == name);
		}


		public static IDisposable Push(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var startEvent = ProfilingEvent.Push(
				Thread.CurrentThread.ManagedThreadId,
				new CallerInfo(memberName, sourceFilePath, sourceLineNumber),
				DateTime.Now.Ticks);
			_events.Add(startEvent);

			return Disposable.Create(() => _events.Add(ProfilingEvent.Pop(startEvent, DateTime.Now.Ticks)));
		}

		public static T Profile<T>(Func<T> function,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			using (Push(memberName, sourceFilePath, sourceLineNumber))
				return function();
		}

		public static Dictionary<CallerInfo, List<ProfilingResult>> GetProfilingResults()
		{
			var profilingResult = new Dictionary<CallerInfo, List<ProfilingResult>>();
			var eventsAtCurrentTime = _events.ToArray();
			foreach (var popEvent in eventsAtCurrentTime.OfType<ProfilingEventPop>())
			{
				if(!profilingResult.ContainsKey(popEvent.CallerInfo))
					profilingResult.Add(popEvent.CallerInfo, new List<ProfilingResult>());

				profilingResult[popEvent.CallerInfo].Add(new ProfilingResult(popEvent.StartTime, popEvent.TimeSpan, popEvent.ThreadId));
			}

			return profilingResult;
		}

		public static void DumpAverageAsCSV(StreamWriter writer)
		{
			var results = GetProfilingResults();
			writer.WriteLine("MethodName,Source Path,Source Line,Average Time Used(seconds)");
			foreach (var result in results)
			{
				var callInfo = result.Key;
				var avg = 0.0;
				foreach (var call in result.Value)
				{
					avg += call.TimeUsed.TotalSeconds;
				}

				avg /= result.Value.Count;
				writer.WriteLine("{0},{1},{2},{3}",
					callInfo.Name,
					callInfo.SourceFilePath,
					callInfo.SourceLineNumber,
					avg.ToString(new NumberFormatInfo()));
			}
		}

		public static void DumpCallCountAsCSV(StreamWriter writer)
		{
			var results = GetProfilingResults();
			writer.WriteLine("MethodName,Source Path,Source Line,Call Count");
			foreach (var result in results)
			{
				var callInfo = result.Key;
				writer.WriteLine("{0},{1},{2},{3}",
					callInfo.Name,
					callInfo.SourceFilePath,
					callInfo.SourceLineNumber,
					result.Value.Count);
			}
		}

		public static void DumpTimeUsedIndependentAsCSV(StreamWriter writer)
		{
			var results = GetProfilingResults();
			writer.WriteLine("MethodName,Source Path,Source Line,ThreadId,Started,Time Used");
			foreach (var result in results)
			{
				var callInfo = result.Key;
				foreach (var call in result.Value)
				{
					writer.WriteLine(
						"{0},{1},{2},{3},{4},{5}",
						callInfo.Name,
						callInfo.SourceFilePath,
						callInfo.SourceLineNumber,
						call.ThreadId,
						call.Started.ToString(new DateTimeFormatInfo()),
						call.TimeUsed.TotalSeconds.ToString(new NumberFormatInfo()));
				}
			}
		}
	}

}