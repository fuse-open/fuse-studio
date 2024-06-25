using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace Outracks
{
	public class ReplayQueueSubject<T> : ISubject<T>
	{
		readonly object _gate = new object();
		readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		readonly Subject<T> _subject = new Subject<T>();

		public void OnNext(T value)
		{
			lock (_gate)
			{
				if (!_subject.HasObservers)
					_queue.Enqueue(value);
				else
					_subject.OnNext(value);
			}
		}

		public void OnError(Exception error)
		{
			lock (_gate)
			{
				_subject.OnError(error);
			}
		}

		public void OnCompleted()
		{
			lock (_gate)
			{
				_subject.OnCompleted();
			}
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			lock (_gate)
			{
				T replayedValue;
				while (_queue.TryDequeue(out replayedValue))
					observer.OnNext(replayedValue);
				return _subject.Subscribe(observer);
			}
		}
	}
}