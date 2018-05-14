using System;
using System.Collections.Generic;
using System.Linq;

namespace Outracks
{
	public static class EnumerableIndexing
	{
		public static int IndexOf<T>(this IEnumerable<T> items, T itemToFind)
		{
			return items.IndexOfFirst<T>(item => item.Equals(itemToFind));
		}

		public static int IndexOfFirst<T>(this IEnumerable<T> items, Predicate<T> where)
		{
			var i = 0;
			foreach (var item in items)
			{
				if (where(item))
					return i;
				i++;
			}
			return -1;
		}

		public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
		{
			return !items.Any(predicate);
		}

		public static bool IsEmpty<T>(this IEnumerable<T> items)
		{
			return !items.Any();
		}

		public static bool IsSingle<T>(this IEnumerable<T> items)
		{
			return items.HasCount(1);
		}

		public static bool HasCount<T>(this IEnumerable<T> items, int count)
		{
			int c = 0;
			foreach (var item in items)
			{
				c++;
				if (c > count) return false;
			}
			return c == count;
		}

		public static Optional<T> TryRemoveAt<T>(
			this IList<T> args,
			int index)
		{
			if (args.Count <= index)
				return Optional.None();

			var item = args[index];
			args.RemoveAt(index);
			return item;
		}

		public static Optional<T> TryGetAt<T>(
			this IEnumerable<T> args,
			int index)
		{
			var enumerator = args.GetEnumerator();

			for (int i = 0; i < index + 1; i++)
				if (!enumerator.MoveNext())
					return Optional.None();

			return enumerator.Current;
		}


		public static Optional<TResult> TryGetAt<TArgs, TResult>(
			this IEnumerable<TArgs> args,
			int index,
			Func<TArgs, TResult> transform)
		{
			var enumerator = args.GetEnumerator();

			for (int i = 0; i < index + 1; i++)
				if (!enumerator.MoveNext())
					return Optional.None();

			return transform(enumerator.Current);
		}
	}
}