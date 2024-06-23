using System;
using Uno;

namespace Outracks
{
	public static partial class Optional
	{
		public static Optional<TResult> Select<T, TResult>(this Optional<T> self, Func<T, TResult> transform)
		{
			return self.HasValue
				? Optional.Some(transform(self.Value))
				: Optional.None<TResult>();
		}

		public static Optional<TResult> SelectMany<T, TResult>(this Optional<T> self, Func<T, Optional<TResult>> transform)
		{
			return self.HasValue
				? transform(self.Value)
				: Optional.None<TResult>();
		}
	}
}