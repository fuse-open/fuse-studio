using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Outracks
{
	public static class Lookup
	{
		public static ILookup<TKey, TValue> Empty<TKey, TValue>()
		{
			return Enumerable.Empty<KeyValuePair<TKey, IEnumerable<TValue>>>().ToLookup();
		}

		public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, ImmutableList<TValue>>> values)
		{
			return new DictionaryLookup<TKey, TValue, ImmutableList<TValue>>(values.ToDictionary(kv => kv.Key, kv => kv.Value));
		}

		public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> values)
		{
			return new DictionaryLookup<TKey, TValue, IEnumerable<TValue>>(values.ToDictionary(kv => kv.Key, kv => kv.Value));
		}

		public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IDictionary<TKey, ImmutableList<TValue>> dictionary)
		{
			return new DictionaryLookup<TKey, TValue, ImmutableList<TValue>>(dictionary);
		}

		public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IDictionary<TKey, IEnumerable<TValue>> dictionary)
		{
			return new DictionaryLookup<TKey, TValue, IEnumerable<TValue>>(dictionary);
		}

		public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IDictionary<TKey, ImmutableHashSet<TValue>> dictionary)
		{
			return new DictionaryLookup<TKey, TValue, ImmutableHashSet<TValue>>(dictionary);
		}
	}
}
