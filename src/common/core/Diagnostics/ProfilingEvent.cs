using System;

namespace Outracks
{
	abstract class ProfilingEvent
	{
		public readonly long Ticks;
		protected ProfilingEvent(long ticks)
		{
			Ticks = ticks;
		}

		public static ProfilingEventPush Push(int threadId, CallerInfo callerInfo, long startTicks)
		{
			return new ProfilingEventPush(threadId, callerInfo, startTicks);
		}

		public static ProfilingEventPop Pop(ProfilingEventPush push, long endTicks)
		{
			return new ProfilingEventPop(push, endTicks);
		}
	}

	class ProfilingEventPush : ProfilingEvent
	{
		public readonly int ThreadId;
		public readonly CallerInfo CallerInfo;

		public ProfilingEventPush(int threadId, CallerInfo callerInfo, long ticks)
			: base(ticks)
		{
			ThreadId = threadId;
			CallerInfo = callerInfo;
		}
	}

	class ProfilingEventPop : ProfilingEvent
	{
		readonly ProfilingEventPush _push;

		public TimeSpan TimeSpan
		{
			get { return TimeSpan.FromTicks(Ticks - _push.Ticks); }
		}

		public CallerInfo CallerInfo
		{
			get { return _push.CallerInfo; }
		}

		public DateTime StartTime
		{
			get { return new DateTime(_push.Ticks); }
		}

		public int ThreadId
		{
			get { return _push.ThreadId; }
		}

		public ProfilingEventPop(ProfilingEventPush push, long ticks)
			: base(ticks)
		{
			_push = push;
		}
	}
}