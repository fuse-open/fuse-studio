using System;
using System.Reactive.Linq;
using Outracks;

public static partial class Behavior
{
	public static IBehavior<T> DistinctUntilChanged<T>(this IBehavior<T> self)
	{
		return new DistinctUntilChangedBehavior<T>(self);
	}

	class DistinctUntilChangedBehavior<T> : IBehavior<T>
	{
		readonly IBehavior<T> _behavior;
		readonly IObservable<T> _distinctObservable;

		public DistinctUntilChangedBehavior(IBehavior<T> behavior)
		{
			_behavior = behavior;
			IObservable<T> obs = behavior;
			_distinctObservable = obs.DistinctUntilChanged();
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _distinctObservable.Subscribe(observer);
		}

		public T Value
		{
			get { return _behavior.Value; }
		}
	}
}