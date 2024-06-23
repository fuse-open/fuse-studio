using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Behavior
	{
		public static IBehavior<T> Switch<T>(this IBehavior<IBehavior<T>> self)
		{
			return new SwitchBehavior<T>(self);
		}

		public static IBehavior<TResult> Switch<TSource, TResult>(
			this IBehavior<TSource> self,
			Func<TSource, IBehavior<TResult>> f)
		{
			return self.Select(f).Switch();
		}
	}

	class SwitchBehavior<T> : IBehavior<T>
	{
		readonly IBehavior<IBehavior<T>> _behavior;
		readonly IObservable<T> _switchedObservable;

		public SwitchBehavior(IBehavior<IBehavior<T>> behavior)
		{
			_behavior = behavior;
			IObservable<IObservable<T>> obs = _behavior;
			_switchedObservable = obs.Switch();
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _switchedObservable.Subscribe(observer);
		}

		public T Value
		{
			get { return _behavior.Value.Value; }
		}
	}
}