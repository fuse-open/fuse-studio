using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Behavior
	{
		public static IBehavior<TResult> CombineLatest<TSource1, TSource2, TResult>(
			this IBehavior<TSource1> self,
			IBehavior<TSource2> other,
			Func<TSource1, TSource2, TResult> selector)
		{
			return new CombineLatestBehavior<TSource1, TSource2, TResult>(self, other, selector);
		}
	}

	class CombineLatestBehavior<TSource1, TSource2, TResult> : IBehavior<TResult>
	{
		readonly IBehavior<TSource1> _behavior1;
		readonly IBehavior<TSource2> _behavior2;
		readonly Func<TSource1, TSource2, TResult> _selector;
		readonly IObservable<TResult> _combinedObservable;

		public CombineLatestBehavior(IBehavior<TSource1> behavior1, IBehavior<TSource2> behavior2, Func<TSource1, TSource2, TResult> selector)
		{
			_behavior1 = behavior1;
			_behavior2 = behavior2;
			_selector = selector;
			IObservable<TSource1> obs1 = _behavior1;
			IObservable<TSource2> obs2 = _behavior2;
			_combinedObservable = obs1.CombineLatest(obs2, selector);
		}

		public IDisposable Subscribe(IObserver<TResult> observer)
		{
			return _combinedObservable.Subscribe(observer);
		}

		public TResult Value
		{
			get { return _selector(_behavior1.Value, _behavior2.Value); }
		}
	}
}