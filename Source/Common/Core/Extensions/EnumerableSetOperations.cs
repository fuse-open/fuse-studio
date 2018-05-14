using System.Collections.Generic;
using System.Linq;

namespace Outracks
{
	public static class EnumerableSetOperations
	{
		public static bool SetEquals<T>(this IEnumerable<T> items, IEnumerable<T> others)
		{
			return new HashSet<T>(items).SetEquals(others);
		}

		public static IEnumerable<T> Intersect<T>(this IEnumerable<T> self, T item)
		{
			return self.Intersect(new[] { item });
		}

		public static IEnumerable<T> UnionOne<T>(this IEnumerable<T> self, T item)
		{
			return self.Union(new[] { item });
		}

		public static IEnumerable<T> ExceptOne<T>(this IEnumerable<T> self, T item)
		{
			return self.Except(new[] { item });
		}

		public static IEnumerable<T> ConcatOne<T>(this IEnumerable<T> self, T item)
		{
			return self.Concat(new[] { item });
		}
	}
}