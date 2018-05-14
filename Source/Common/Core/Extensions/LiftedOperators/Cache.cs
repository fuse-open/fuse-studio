using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks
{

	public static class CacheExtensions
	{
	
		public static IObservable<IEnumerable<TValue>> CachePerElement<TDescriptor, TValue>(
			this IObservable<IEnumerable<TDescriptor>> items,
			Func<TDescriptor, TValue> getValue,
			Action<TValue> valueRemoved = null)
		{
			return items.CachePerElement(d => d, getValue, valueRemoved);
		}

		public static IObservable<IEnumerable<TValue>> CachePerElement<TDescriptor, TKey, TValue>(
			this IObservable<IEnumerable<TDescriptor>> items,
			Func<TDescriptor, TKey> getKey,
			Func<TDescriptor, TValue> getValue,
			Action<TValue> valueRemoved = null)
		{
			return items
				.Scan(
					new { cache = ImmutableDictionary<TKey, TValue>.Empty, list = Enumerable.Empty<TValue>() },
					(state, newDescriptors) =>
					{
						var oldCache = state.cache;
						var newCache = ImmutableDictionary.CreateBuilder<TKey, TValue>();
						var newItems = new List<TValue>();
						foreach (var descriptor in newDescriptors)
						{
							var key = getKey(descriptor);
							var oldValue = oldCache.TryGetValue(key);
							var value = oldValue.HasValue
								? oldValue.Value
								: getValue(descriptor);
							newCache[key] = value;
							newItems.Add(value);
						}

						if (valueRemoved != null)
						{
							foreach (var cache in oldCache)
							{
								if (newCache.ContainsKey(cache.Key) == false)
									valueRemoved(cache.Value);
							}
						}

						return new { cache = newCache.ToImmutable(), list = (IEnumerable<TValue>)newItems };
					})
				.Select(state => state.list)
				.Replay(1)
				.RefCount();
		}
	}
}
