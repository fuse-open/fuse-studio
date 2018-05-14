using System;
using System.Reactive.Linq;

namespace Outracks
{
	public class PreviousCurrentTuple<T>
	{
		public readonly T Previous;
		public readonly T Current;

		public PreviousCurrentTuple(T previous, T current)
		{
			Previous = previous;
			Current = current;
		}
	}

	public class OptionalPreviousCurrentTuple<T>
	{
		public readonly Optional<T> Previous;
		public readonly T Current;

		public OptionalPreviousCurrentTuple(Optional<T> previous, T current)
		{
			Previous = previous;
			Current = current;
		}
	}

	public static class BufferPreviousExtensions
	{

		public static IObservable<PreviousCurrentTuple<T>> BufferPrevious<T>(this IObservable<T> self, T seed)
		{
			return self
				.StartWith(seed)
				.Buffer(2, 1)
				.Where(l => l.Count == 2)
				.Select(list => new PreviousCurrentTuple<T>(list[0], list[1]));
		}

		public static IObservable<OptionalPreviousCurrentTuple<T>> BufferPrevious<T>(this IObservable<T> self)
		{
			return Observable.Create<OptionalPreviousCurrentTuple<T>>(
				observer =>
				{
					var previous = Optional.None<T>();
					return self
						.Select(current =>
						{
							var ret = new OptionalPreviousCurrentTuple<T>(previous, current);
							previous = current;
							return ret;
						})
						.Subscribe(observer);
				}); 
		}

		public static Optional<TResult> ChangesTo<TArgs, TResult>(
			this OptionalPreviousCurrentTuple<TArgs> self,
			Func<TArgs, TResult> select)
		{
			var prev = self.Previous.Select(select);
			var cur = select(self.Current);
			if (prev.Equals(cur))
				return Optional.None();
			return cur;
		}
	}
}