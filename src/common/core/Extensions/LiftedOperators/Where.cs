using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks
{
	public static class WhereExtensions
	{
		public static IObservable<IEnumerable<T>> WhereEachElement<T>(
			this IObservable<IEnumerable<T>> items,
			Func<T, bool> predicate)
		{
			return items.Where(elements => elements.All(predicate));
		}

		public static IObservable<IEnumerable<TDescriptor>> WherePerElement<TDescriptor>(
			this IObservable<IEnumerable<TDescriptor>> items,
			Func<TDescriptor, IObservable<bool>> calculateResult)
		{
			return items
				.Switch(item =>
					item.Select(e => calculateResult(e).Select(r => r ? Optional.Some(e) : Optional.None()))
						.ToObservableEnumerable())
				.Select(item =>
					item.NotNone());
		}

		public static IObservable<IEnumerable<TDescriptor>> WherePerElement<TDescriptor>(
			this IObservable<IEnumerable<TDescriptor>> items,
			Func<TDescriptor, bool> calculateResult)
		{
			return items.Select(i => i.Where(calculateResult));
		}

		public static IObservable<Optional<TDescriptor>> WherePerElement<TDescriptor>(
			this IObservable<Optional<TDescriptor>> items,
			Func<TDescriptor, bool> calculateResult)
		{
			return items.Select(i => i.Where(calculateResult));
		}
	}
}
