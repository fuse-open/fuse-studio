using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Outracks
{
	public static class GraphInversion
	{
		public static ILookup<T, T> ToChildLookup<T>(this IEnumerable<T> paths, Func<T, Optional<T>> selectParent)
		{
			var dict = new ConcurrentDictionary<T, ImmutableHashSet<T>>();

			var worklist = new ConcurrentQueue<T>(paths);
			T child;
			while (worklist.TryDequeue(out child))
			{
				var parent = selectParent(child);
				if (parent.HasValue)
				{
					dict.AddOrUpdate(
						key: parent.Value,
						addValueFactory: (_) => ImmutableHashSet.Create(child),
						updateValueFactory: (_, parentSet) => parentSet.Add(child));

					worklist.Enqueue(parent.Value);
				}
			}
			return dict.ToLookup();
		}
	}
}