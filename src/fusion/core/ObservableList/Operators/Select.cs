using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<TResult> Select<TSource, TResult>(
			this IObservableList<TSource> self,
			Func<TSource, TResult> selector)
		{
			return self.Select(change => change.Select(selector)).UnsafeAsObservableList();
		}

		public static IObservableList<TResult> SelectMany<TSource, TResult>(
			this IObservableList<TSource> self,
			Func<TSource, IObservableList<TResult>> selector)
		{
			return self.Select(selector).Join();
		}
	}
}