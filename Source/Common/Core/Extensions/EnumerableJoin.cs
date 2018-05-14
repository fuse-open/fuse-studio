using System;
using System.Collections.Generic;
using System.Linq;

namespace Outracks
{
	public static class EnumerableJoin
	{
		public static IEnumerable<T> Join<T>(this IEnumerable<IEnumerable<T>> groups)
		{
			return groups.SelectMany(g => g);
		}

		public static IEnumerable<T> Join<T>(this IEnumerable<IEnumerable<T>> groups, params T[] seperatorElements)
		{
			return Join(groups, () => seperatorElements);
		}

		public static IEnumerable<T> Join<T>(this IEnumerable<T> items, Func<T> seperatorElement)
		{
			return Join(items.Select(i => (IEnumerable<T>)new [] { i }), () => new[] { seperatorElement() });
		}

		public static IEnumerable<T> Join<T>(this IEnumerable<IEnumerable<T>> groups, Func<T> seperatorElement)
		{
			return Join(groups, () => new [] { seperatorElement() });
		}

		static IEnumerable<T> Join<T>(this IEnumerable<IEnumerable<T>> groups, Func<IEnumerable<T>> seperatorElements)
		{
			var e = groups.GetEnumerator();
			if (!e.MoveNext())
			{
				yield break;
			}

			var first = e.Current;
			if (!e.MoveNext())
			{
				foreach (var item in first)
					yield return item;
				yield break;
			}

			var second = e.Current; // TODO: figure out a way to reuse these elements without enumerating the enumerable again

			foreach (var item in first)
				yield return item;

			foreach (var group in groups.Skip(1))
			{
				foreach (var sep in seperatorElements())
					yield return sep;

				foreach (var item in group)
					yield return item;
			}
		}
	}
}