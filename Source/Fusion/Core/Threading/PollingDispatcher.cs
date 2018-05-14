using System;
using System.Threading;
using Outracks.Fusion.Threading;

namespace Outracks
{
	public class PollingDispatcher : DispatcherBase
	{
		readonly Thread _mainThread;
		readonly DispatcherQueue<Action> _queue = new DispatcherQueue<Action>();

		public PollingDispatcher(Thread mainThread)
		{
			_mainThread = mainThread;
		}

		public void DispatchCurrent()
		{
			_queue.DispatchCurrent(a => a());
		}

		public override void Enqueue(Action action)
		{
			_queue.Enqueue(action);
			if (Thread.CurrentThread == _mainThread)
				DispatchCurrent();
		}
	}
}