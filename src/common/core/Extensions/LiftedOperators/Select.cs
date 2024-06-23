using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks
{
	public static class SelectExtension
	{
		public static IObservable<Optional<TResult>> SelectPerElement<TDescriptor, TResult>(
			this IObservable<Optional<TDescriptor>> items,
			Func<TDescriptor, TResult> calculateResult)
		{
			return items.Select(i => i.Select(calculateResult));
		}

		public static IObservable<IEnumerable<TResult>> SelectPerElement<TDescriptor, TResult>(
			this IObservable<IEnumerable<TDescriptor>> items,
			Func<TDescriptor, TResult> calculateResult)
		{
			return items.Select(i => i.Select(calculateResult));
		}

		public static IObservable<TOut> SelectSome<TIn, TOut>(this IObservable<TIn> self, Func<TIn, Optional<TOut>> transform)
		{
			return self.Select(transform).NotNone();
		}
	}
}
