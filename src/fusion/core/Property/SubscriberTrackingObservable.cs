using System;

namespace Outracks
{
	public class SubscriberTrackingObservable<T> : IObservable<T>
	{
		readonly Action<int> _report;
		readonly IObservable<T> _observable;

		int _count;

		public SubscriberTrackingObservable(IObservable<T> observable, Action<int> report)
		{
			_observable = observable;
			_report = report;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			_count++;
			_report(_count);
			return Disposable.Combine(
				_observable.Subscribe(observer),
				Disposable.Create(
					() =>
					{
						_count--;
						_report(_count);
					}));
		}
	}
}