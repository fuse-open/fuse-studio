using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservable<T> AggregateAssoc<T>(
			this IObservableList<T> self,
			T seed,
			Func<T, T, T> associativeAccumulator)
		{
			return self
				.Scan(
					SumTree<T>.Empty(seed, associativeAccumulator),
					(tree, changes) => changes.Apply(tree))
				.Select(tree => tree.Sum());
		}
	}
}