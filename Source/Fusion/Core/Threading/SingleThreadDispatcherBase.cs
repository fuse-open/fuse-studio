using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Outracks.Fusion;
using Outracks.Fusion.Threading;

namespace Outracks
{
	public abstract class SingleThreadDispatcherBase : DispatcherBase
	{
		static readonly TimeSpan BusyLogThreshold = TimeSpan.FromSeconds(0.5);
		readonly Thread _thread;
		int _drainDepth;

		readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

		protected SingleThreadDispatcherBase(Thread thread)
		{
			_thread = thread;
		}

		protected bool RunningOnDispatcherThread { get { return _thread == Thread.CurrentThread; } }

		public override void Enqueue(Action action)
		{
			_queue.Enqueue(action);
			Flush();
		}

		protected abstract void Flush();

		protected void Drain()
		{
			Stopwatch stopwatch = _drainDepth == 0 ? Stopwatch.StartNew() : null;
			_drainDepth++;
			try
			{
				Action action;
				while (_queue.TryDequeue(out action))
				{
					try
					{
						action();
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception from CatchAll: " + e);
					}
				}
			}
			finally
			{
				_drainDepth--;
				if (stopwatch != null)
				{
					stopwatch.Stop();
					if (stopwatch.Elapsed > BusyLogThreshold)
					{
						Console.WriteLine("Dispatcher busy for {0} seconds (UI unresponsive)", stopwatch.Elapsed.TotalSeconds);
					}
				}
			}
		}
	}
}