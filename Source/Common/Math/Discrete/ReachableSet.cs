using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Outracks
{
	public static class ReachableSet
	{
		public static IDictionary<T, IImmutableSet<T>> CreateReachableSets<T>(
			this IEnumerable<T> allElements,
			Func<T, IEnumerable<T>> directlyReachable)
		{
			var sets = new ConcurrentDictionary<T, IImmutableSet<T>>();

			foreach (var element in allElements)
				sets[element] = element.CreateReachableSet(directlyReachable);

			return sets;
		}

		public static IImmutableSet<T> CreateReachableSet<T>(
			this T seed, 
			Func<T, IEnumerable<T>> directlyReachable)
		{
			var set = ImmutableHashSet.Create<T>();

			var workList = new ConcurrentQueue<T>();
			workList.Enqueue(seed);

			T current;
			while (workList.TryDequeue(out current))
			{
				foreach (var other in directlyReachable(current))
				{
					if (set.Contains(other))
						continue;

					set = set.Add(other);
					workList.Enqueue(other);
				}
			}

			return set;
		}
	}
}