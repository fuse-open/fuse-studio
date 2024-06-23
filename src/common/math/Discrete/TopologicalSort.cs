using System;
using System.Collections.Generic;

namespace Outracks
{
	public static class TopologicalSort
	{
		/// <exception cref="CycleDetected"></exception>
		public static IEnumerable<T> OrderByTopology<T>(
			this IEnumerable<T> nodes,
			Func<T, IEnumerable<T>> getDependencies)
		{
			var visited = new Dictionary<T, bool>();
			var sorted = new List<T>();

			Action<T> visit = null;
			visit = (item) =>
			{
				bool inProcess;
				var alreadyVisited = visited.TryGetValue(item, out inProcess);

				if (alreadyVisited && inProcess)
					throw new CycleDetected();

				if (!alreadyVisited)
				{
					visited[item] = true;

					foreach (var dependency in getDependencies(item))
						visit(dependency);

					visited[item] = false;
					sorted.Add(item);
				}
			};

			foreach (var item in nodes)
				visit(item);

			return sorted;
		}
	}
}