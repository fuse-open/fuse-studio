using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Outracks
{
	public class DispatcherQueue<T>
	{
		readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		readonly AutoResetEvent _signal = new AutoResetEvent(false);

		public void Dispatch(Action<T> action, CancellationToken ct)
		{
			ct.Register(() => _signal.Set());
			while (!ct.IsCancellationRequested)
			{
				DispatchCurrent(action);
				_signal.WaitOne();
			}
		}

		public void DispatchCurrent(Action<T> action, Optional<TimeSpan> timeout = default(Optional<TimeSpan>))
		{
			var watch = Stopwatch.StartNew();
			
			do
			{
				T message;
				if (!_queue.TryDequeue(out message))
					return;

				action(message);
			} 
			while (timeout.Select(t => watch.Elapsed < t).Or(true));
			Console.WriteLine("Timeout!");
		}

		public void Enqueue(T value)
		{
			_queue.Enqueue(value);
			_signal.Set();
		}
	}
}