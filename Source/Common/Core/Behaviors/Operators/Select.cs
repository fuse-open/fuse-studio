using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Behavior
	{
		public static IBehavior<TResult> Select<TSource, TResult>(this IBehavior<TSource> self, Func<TSource, TResult> f)
		{
			return new SelectBehavior<TSource, TResult>(self, f);
		}
	}

	class SelectBehavior<TSource, TResult> : IBehavior<TResult>
	{
		readonly IBehavior<TSource> _behavior;
		readonly Func<TSource, TResult> _selector;
		readonly IObservable<TResult> _selectedObservable;

		public SelectBehavior(IBehavior<TSource> behavior, Func<TSource, TResult> selector)
		{
			_behavior = behavior;
			_selector = selector;
			IObservable<TSource> obs = _behavior;
			_selectedObservable = obs.Select(selector);
		}

		public IDisposable Subscribe(IObserver<TResult> observer)
		{
			return _selectedObservable.Subscribe(observer);
		}

		public TResult Value
		{
			get { return _selector(_behavior.Value); }
		}
	}
}