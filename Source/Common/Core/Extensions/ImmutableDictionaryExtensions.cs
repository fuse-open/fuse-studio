using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Outracks
{

	public static class ImmutableListExtensions
	{
		public static IImmutableList<T> Take<T>(this IImmutableList<T> self, int count)
		{
			return Enumerable.Take(self, count).ToImmutableList();
		}
	}

	public static class ImmutableDictionaryExtensions
	{
		public static Optional<TValue> TryGetTarget<TValue>(this WeakReference<TValue> weakReference) where TValue : class
		{
			TValue value;
			if (weakReference.TryGetTarget(out value))
				return value;
			return Optional.None();
		}

		public static IImmutableSet<T> RemoveAll<T>(this IImmutableSet<T> set, Func<T, bool> predicate)
		{
			return set.Except(set.Where(predicate));
		}

		public static IImmutableList<T> Where<T>(this IImmutableList<T> self, Func<T, bool> predicate)
		{
			return self.RemoveAll(t => !predicate(t));
		}

		public static IImmutableDictionary<TKey, TValue> Replace<TKey, TValue>(
			this IImmutableDictionary<TKey, TValue> self,
			TKey key,
			Func<TValue, TValue> transform)
		{
			var oldValue = self[key];
			var newValue = transform(oldValue);
			return self.SetItem(key, newValue);
		}
		public static IImmutableDictionary<TKey, TValue> ReplaceAll<TKey, TValue>(
			this IImmutableDictionary<TKey, TValue> self,
			Func<TValue, TValue> transform)
		{
			var res = self;
			foreach (var key in res.Keys)
				res = res.Replace(key, transform);
			return res;
		}
		public static IImmutableSet<TValue> Replace<TValue>(
			this IImmutableSet<TValue> self,
			TValue value,
			TValue with)
		{
			return self.Remove(value).Add(with);
		}
	}
}