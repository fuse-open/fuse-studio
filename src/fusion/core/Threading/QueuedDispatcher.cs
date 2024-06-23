using System;
using System.Threading;
using Outracks.Fusion.Threading;

namespace Outracks
{
	public class QueuedDispatcher : DispatcherBase
	{
		readonly DispatcherQueue<Action> _queue;
		readonly Thread _dispatchThread;

		public QueuedDispatcher(CancellationToken token = default(CancellationToken))
		{
			_queue = new DispatcherQueue<Action>();
			var thread = new Thread(() => _queue.Dispatch(a => a(), token))
			{
				IsBackground = true,
			};
			thread.Start();
		}

		public QueuedDispatcher(DispatcherQueue<Action> queue, Thread dispatchThread)
		{
			_queue = queue;
			_dispatchThread = dispatchThread;
		}

		public override void Enqueue(Action action)
		{
			_queue.Enqueue(action);
			if (Thread.CurrentThread == _dispatchThread)
				_queue.DispatchCurrent(a => a());
		}
	}
}