using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static class SelectWithStateExtension
	{
		public static IObservable<TResult> SelectWithState<T, TState, TResult>(
			this IObservable<T> self,
			TState initialState,
			Func<T, TState, SelectResult<TResult, TState>> select)
		{
			return self
				.Scan(
					new SelectResult<TResult, TState>(value: default(TResult), nextState: initialState),
					(acc, t) => select(t, acc.NextState))
				.Select(tp => tp.Value);
		}

		public static IObservable<TResult> SelectWithMutableState<TSource, TState, TResult>(
			this IObservable<TSource> self,
			Func<TState> initialState,
			Func<TSource, TState, TResult> select)
		{
			return Observable.Create<TResult>(
				observer =>
				{
					var state = initialState();
					return self.Select(x => select(x, state)).Subscribe(observer);
				});
		}
	}

	public static class SelectResult
	{
		public static SelectResult<TValue, TState> Create<TValue, TState>(TValue value, TState nextState)
		{
			return new SelectResult<TValue, TState>(value, nextState);
		}
	}

	public class SelectResult<TValue, TState>
	{
		public readonly TValue Value;
		public readonly TState NextState;
		public SelectResult(TValue value, TState nextState)
		{
			Value = value;
			NextState = nextState;
		}
	}
}