using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks
{
	public static class EnumerableDiffing
	{
		public static IObservable<DiffSequenceResult<T>> DiffSequence<T>(this IObservable<IEnumerable<T>> self, IEqualityComparer<T> comparer = null)
		{
			return self
				.Select(ImmutableList.CreateRange)
				.DiffSequence(comparer);
		}

		public static IObservable<DiffSequenceResult<T>> DiffSequence<T>(this IObservable<IImmutableList<T>> self, IEqualityComparer<T> comparer = null)
		{
			return self
				.BufferPrevious(ImmutableList<T>.Empty)
				.Select(lists => lists.Current.DiffSequence(lists.Previous, comparer));
		}

		public static IObservable<DiffSetResult<T>> DiffSet<T>(this IObservable<IEnumerable<T>> self, IEqualityComparer<T> comparer = null)
		{
			return DiffSet(self.Select(e => e.ToImmutableHashSet()), comparer);
		}

		public static IObservable<DiffSetResult<T>> DiffSet<T>(this IObservable<IImmutableSet<T>> self, IEqualityComparer<T> comparer = null)
		{
			return self
				.BufferPrevious(ImmutableHashSet<T>.Empty)
				.Select(diff => diff.Current.DiffWith(diff.Previous, comparer));
		}


		public static DiffSetResult<T> DiffWith<T>(this IImmutableSet<T> current, IImmutableSet<T> previous, IEqualityComparer<T> equalityComparer = null)
		{
			var comparer = equalityComparer ?? EqualityComparer<T>.Default;
			return new DiffSetResult<T>(
				current.Except(previous, comparer).ToImmutableHashSet(),
				previous.Except(current, comparer).ToImmutableHashSet(),
				current);
		}

		public static DiffSequenceResult<T> DiffSequence<T>(this IImmutableList<T> current, IImmutableList<T> previous, IEqualityComparer<T> equalityComparer = null)
		{
			var comparer = equalityComparer ?? EqualityComparer<T>.Default;
			var previousSet = previous.ToImmutableHashSet();
			var currentSet = current.ToImmutableHashSet();
			return new DiffSequenceResult<T>(
				current.RemoveAll(e => previousSet.Contains(e, comparer)).ToImmutableList(),
				previous.RemoveAll(e => currentSet.Contains(e, comparer)).ToImmutableList(),
				current,
				previous);
		}

	}

	public sealed class DiffSequenceResult<T>
	{
		public readonly IImmutableList<T> Added;

		public readonly IImmutableList<T> Removed;

		public readonly IImmutableList<T> Current;

		public readonly IImmutableList<T> Previous;

		public DiffSequenceResult(IImmutableList<T> added, IImmutableList<T> removed, IImmutableList<T> current, IImmutableList<T> previous)
		{
			Removed = removed;
			Current = current;
			Previous = previous;
			Added = added;
		}
	}
	public sealed class DiffSetResult<T>
	{
		public readonly IImmutableSet<T> Added;

		public readonly IImmutableSet<T> Removed;

		public readonly IImmutableSet<T> Current;

		public DiffSetResult(ImmutableHashSet<T> added, ImmutableHashSet<T> removed, IImmutableSet<T> current)
		{
			Removed = removed;
			Current = current;
			Added = added;
		}
	}

}