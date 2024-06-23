using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks
{
	public static class SetEqualityComparer
	{
		public static IObservable<IEnumerable<T>> DistinctUntilSetChanged<T>(this IObservable<IEnumerable<T>> self)
		{
			return self.DistinctUntilSetChangedInternal<IEnumerable<T>, T>();
		}

		public static IObservable<IImmutableList<T>> DistinctUntilSetChanged<T>(this IObservable<IImmutableList<T>> self)
		{
			return self.DistinctUntilSetChangedInternal<IImmutableList<T>, T>();
		}
		public static IObservable<IImmutableSet<T>> DistinctUntilSetChanged<T>(this IObservable<IImmutableSet<T>> self)
		{
			return self.DistinctUntilSetChangedInternal<IImmutableSet<T>, T>();
		}

		static IObservable<T> DistinctUntilSetChangedInternal<T, TElement>(this IObservable<T> self)
			where T : IEnumerable<TElement>
		{
			return self.DistinctUntilChanged(new SetEqualityComparer<T, TElement>());
		}

		public static IEqualityComparer<IEnumerable<T>> Create<T>()
		{
			return new SetEqualityComparer<IEnumerable<T>, T>();
		}
	}

	class SetEqualityComparer<T, TElement> : IEqualityComparer<T> where T : IEnumerable<TElement>
	{
		public bool Equals(T x, T y)
		{
			return x.SetEquals(y);
		}

		public int GetHashCode(T obj)
		{
			return obj.Count();
		}
	}

}