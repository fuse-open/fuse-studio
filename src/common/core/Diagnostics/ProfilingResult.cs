using System;

namespace Outracks
{
	public class ProfilingResult
	{
		public readonly DateTime Started;
		public readonly TimeSpan TimeUsed;
		public readonly int ThreadId;

		public ProfilingResult(DateTime started, TimeSpan timeUsed, int threadId)
		{
			Started = started;
			TimeUsed = timeUsed;
			ThreadId = threadId;
		}
	}
}