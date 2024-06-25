using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks
{
	public static class SequenceEqualityComparer
	{
		public static IObservable<IEnumerable<T>> DistinctUntilSequenceChanged<T>(this IObservable<IEnumerable<T>> self)
		{
			return self.DistinctUntilSequenceChangedInternal<IEnumerable<T>, T>();
		}

		public static IObservable<IImmutableList<T>> DistinctUntilSequenceChanged<T>(this IObservable<IImmutableList<T>> self)
		{
			return self.DistinctUntilSequenceChangedInternal<IImmutableList<T>, T>();
		}
		public static IObservable<IImmutableSet<T>> DistinctUntilSequenceChanged<T>(this IObservable<IImmutableSet<T>> self)
		{
			return self.DistinctUntilSequenceChangedInternal<IImmutableSet<T>, T>();
		}

		static IObservable<T> DistinctUntilSequenceChangedInternal<T, TElement>(this IObservable<T> self)
			where T : IEnumerable<TElement>
		{
			return self.DistinctUntilChanged(new SequenceEqualityComparer<T, TElement>());
		}

		public static IEqualityComparer<IEnumerable<T>> Create<T>()
		{
			return new SequenceEqualityComparer<IEnumerable<T>, T>();
		}
	}

	class SequenceEqualityComparer<T, TElement> : IEqualityComparer<T> where T : IEnumerable<TElement>
	{
		public bool Equals(T x, T y)
		{
			return x.SequenceEqual(y);
		}

		public int GetHashCode(T obj)
		{
			const int prime = 31;
			var result = 1;
			foreach (var elm in obj)
			{
				result = result * prime + elm.GetHashCode();
			}
			return result;
		}
	}

}