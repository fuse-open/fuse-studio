using System;
using System.Collections.Immutable;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservable<ImmutableList<T>> ToObservableImmutableList<T>(this IObservableList<T> self)
		{
			return self.Scan(
				ImmutableList<T>.Empty,
				(list, changes) => changes.Apply(list));
		}

		public static IObservableList<T> ToObservableList<T>(this IObservable<IImmutableList<T>> self)
		{
			return self
				.Scan(
					new
					{
						PrevList = (IImmutableList<T>)ImmutableList<T>.Empty,
						Changes = Optional.None<ListChange<T>>()
					},
					(acc, newList) => new
					{
						PrevList = newList,
						Changes = ListChange.IncrementalChanges(acc.PrevList, newList),
					})
				.SelectMany(acc => acc.Changes)
				.UnsafeAsObservableList();
		}

		public static IObservable<Optional<T>> FirstOrNone<T>(this IObservableList<T> self)
		{
			return self.ToObservableImmutableList().Select(list => list.FirstOrNone());
		}

		public static IObservable<Optional<T>> LastOrNone<T>(this IObservableList<T> self)
		{
			return self.ToObservableImmutableList().Select(list => list.LastOrNone());
		}

		public static IObservable<T> FirstOr<T>(this IObservableList<T> self, T element)
		{
			return self.ToObservableImmutableList().Select(list => list.FirstOr(element));
		}

		public static IObservable<T> LastOr<T>(this IObservableList<T> self, T element)
		{
			return self.ToObservableImmutableList().Select(list => list.LastOr(element));
		}
	}
}
