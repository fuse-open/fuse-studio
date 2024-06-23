using System;
using System.Collections.Generic;
using System.Linq;

namespace Outracks
{
	public static class EachExtension
    {
		// TODO: i'm considering inlining this, as it hides the loop-with-side-effects construct, which should be visible -L
        public static void Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
                action(item);
        }
    }

	public static class EnumerableExtensions
	{
		public static T FirstOr<T>(this IEnumerable<T> self, T element)
		{
			return self.FirstOrNone().Or(element);
		}

		public static T FirstOr<T>(this IEnumerable<T> self, Func<T> element)
		{
			return self.FirstOrNone().Or(element);
		}

		public static Optional<T> FirstOrNone<T>(this IEnumerable<T> self, Func<T, bool> where)
		{
			var it = self.Where(where).GetEnumerator();
			return !it.MoveNext()
				? Optional.None<T>()
				: Optional.Some(it.Current);
		}

		public static Optional<T> FirstOrNone<T>(this IEnumerable<T> self)
		{
			var it = self.GetEnumerator();
			return !it.MoveNext()
				? Optional.None<T>()
				: Optional.Some(it.Current);
		}

		public static T LastOr<T>(this IEnumerable<T> self, T element)
		{
			return self.LastOrNone().Or(element);
		}

		public static Optional<T> LastOrNone<T>(this IEnumerable<T> self)
		{
			var it = self.GetEnumerator();
			var current = Optional.None<T>();
			while (it.MoveNext())
				current = it.Current;
			return current;
		}

		public static IEnumerable<T> Yield<T>(this T item)
		{
			yield return item;
		}
	}

	public static class EnumerableNullHandling
	{
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> self)
	    {
		    return self ?? new T[0];
	    }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> self) where T : class
        {
            return self.Where(s => s != null);
        }

	    public static T[] NullSafeToArray<T>(this IEnumerable<T> self)
	    {
		    return self == null ? new T[0] : self.ToArray();
	    }
    }
}
