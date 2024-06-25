using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Outracks
{
	public static class ObservableBinaryMessage
	{
		public static IObservable<T> TryParse<T>(this IObservable<IBinaryMessage> messages, string type, Func<BinaryReader, T> reader)
		{
			return messages
				.SelectSome(m => m.TryParse(type, reader))
				.Publish().RefCount();
		}
	}

	public static class ObservableExtensions
	{
		public static IObservable<IEnumerable<T>> OrEmpty<T>(this IObservable<Optional<IEnumerable<T>>> self)
		{
			return self.Select(Optional.OrEmpty);
		}

		public static IImmutableSet<T> OrEmpty<T>(this Optional<IImmutableSet<T>> self)
		{
			return self.HasValue
				? self.Value
				: ImmutableHashSet<T>.Empty;
		}


		public static IImmutableList<T> OrEmpty<T>(this Optional<IImmutableList<T>> self)
		{
			return self.HasValue
				? self.Value
				: ImmutableList<T>.Empty;
		}

		public static IImmutableDictionary<TKey, TValue> OrEmpty<TKey, TValue>(
			this Optional<IImmutableDictionary<TKey, TValue>> self)
		{
			return self.HasValue
				? self.Value
				: ImmutableDictionary<TKey, TValue>.Empty;
		}

		public static IObservable<T> Or<T>(this IObservable<Optional<T>> self, IObservable<T> other)
		{
			return self.CombineLatest(other, Optional.Or);
		}

		public static IObservable<T> ReplaceNone<T>(this IObservable<Optional<T>> self, Func<T> fallback)
		{
			return self.Select(o => o.Or(fallback));
		}

		public static IObservable<Optional<T>> Or<T>(this IObservable<Optional<T>> self, IObservable<Optional<T>> other)
		{
			return self.CombineLatest(other, Optional.Or);
		}

		public static IObservable<T> Or<T>(this IObservable<Optional<T>> self, T other)
		{
			return self.Select(s => s.Or(other));
		}

		public static IObservable<T> Or<T>(this IObservable<Optional<T>> self, Func<IObservable<T>> fallback)
		{
			return self.Select(s => s.HasValue
					? Observable.Return(s.Value)
					: fallback())
				.Switch();
		}

		public static IObservable<T> ConnectWhile<T>(this IObservable<T> self, IObservable<bool> condition)
		{
			return condition.DistinctUntilChanged().Switch(c => c ? self : Observable.Never<T>());
		}

		public static IObservable<T> While<T>(this IObservable<T> self, IObservable<bool> condition)
		{
			return self.CombineLatest(condition, Tuple.Create).Where(t => t.Item2).Select(t => t.Item1);
		}

		public static IObservable<T> SampleWhile<T>(this IObservable<T> self, IObservable<bool> condition)
		{
			return self.WithLatestFromBuffered(condition, Tuple.Create).Where(t => t.Item2).Select(t => t.Item1);
		}

		public static IDisposable SubscribeUsing<T>(this IObservable<T> self, Func<T, IDisposable> onNext)
		{
			var previous = Disposable.Empty;

			return Disposable.Combine(
				Disposable.Create(() => previous.Dispose()),
				self.Subscribe(
					onNext: t =>
					{
						previous.Dispose();
						previous = onNext(t);
					},
					onError: e => { },
					onCompleted: () => { }));
		}

		public static IObservable<TOut> Switch<TIn, TOut>(this IObservable<TIn> self, Func<TIn, IObservable<TOut>> selector)
		{
			return self.Select(selector).Switch();
		}

		public static IObservable<T> NotNone<T>(this IObservable<Optional<T>> self)
		{
			return self.SelectMany(o => o);
		}

		public static IEnumerable<T> NotNone<T>(this IEnumerable<Optional<T>> self)
		{
			return self.SelectMany(o => o);
		}

		public static IObservable<T> CatchAndRetry<T>(this IObservable<T> source, TimeSpan delay, Action<Exception> onError = null, IScheduler scheduler = null)
		{
			return source.Catch((Exception e) =>
			{
				if (onError != null) onError(e);
				return Observable.Throw<T>(e).Delay(delay, scheduler ?? Scheduler.Default);
			}).Retry();
		}

		public static IObservable<Optional<T>> StartWithNone<T>(this IObservable<T> value)
		{
			return value.Select(Optional.Some).StartWith(Optional.None());
		}

		public static IObservable<Optional<T>> StartWithNone<T>(this IObservable<Optional<T>> value)
		{
			return value.StartWith(Optional.None());
		}

		/// <summary>
		///     Gets all values from observable that arrives synchronously when subscribing to it,
		///     then returns the last of those values.
		/// </summary>
		public static T LastNonBlocking<T>(this IObservable<T> source)
		{
			Optional<T> result = Optional.None();
			Exception exception = null;
			using (source.Subscribe(v => result = v, ex => exception = ex)) { }
			if (exception != null)
				throw new InvalidOperationException("Got error while trying to pull out last value", exception);
			return result.OrThrow(new InvalidOperationException("No pullable value"));
		}
	}

	public static class BeyondTheObservableUniverse
	{
		public static IObservable<IEnumerable<T>> ToObservableEnumerable<T>(this IObservable<IEnumerable<IObservable<T>>> source)
		{
			return source.Select(s => s.ToObservableEnumerable()).Switch();
		}

		public static IObservable<IEnumerable<T>> ToObservableEnumerable<T>(this IEnumerable<IObservable<T>> enumerable)
		{
			var array = enumerable.ToArray();
			return array.Any()
				? array.CombineLatest()
				: Observable.Return(new T[0]);
		}


	}
}