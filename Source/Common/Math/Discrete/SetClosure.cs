using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Outracks
{
	public static class SetClosure
	{
		public static IImmutableSet<T> ClosureSetOf<T>(this T seed, Func<T, IEnumerable<T>> operation)
		{
			var set = ImmutableHashSet.Create<T>();

			var workList = new ConcurrentQueue<T>();
			workList.Enqueue(seed);

			T current;
			while (workList.TryDequeue(out current))
			{
				if (set.Contains(current)) continue;

				set = set.Add(current);
				foreach (var other in operation(current))
					workList.Enqueue(other);
			}

			return set;
		}
	}
}