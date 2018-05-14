using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static class WithLatestFromExtension
	{
		/// <summary>
		///	Combines the latest from 'source' and 'that' using the combinator only when 'source' produces an element, 
		/// as long as 'that' has a value.
		/// 
		/// The difference between this and the standard Observable.WithLatestFrom is that <see cref="WithLatestFromBuffered{TSource,TSampler,TResult}"/>
		/// will hold on to the last element from `source` when no elements have yet been received from `that`, and will push a combined value
		/// when `that` receives its first element.
		/// </summary>
		public static IObservable<TResult> WithLatestFromBuffered<TSource, TSampler, TResult>(
			this IObservable<TSource> source,
			IObservable<TSampler> that,
			Func<TSource, TSampler, TResult> combinator)
		{
			return Observable.Create(
				(IObserver<TResult> observer) =>
				{
					var mutex = new object();
					var pendingSource = default(Optional<TSource>);
					var anySampler = default(Optional<TSampler>);

					return Disposable.Combine(						
						that.Subscribe(
							o =>
							{
								lock (mutex)
								{
									try
									{
										if (pendingSource.HasValue)
										{
											observer.OnNext(combinator(pendingSource.Value, o));
											pendingSource = Optional.None();
										}
										anySampler = o;
									}
									catch (Exception e)
									{
										observer.OnError(e);
									}
								}
							},
							observer.OnError),
						source.Subscribe(
							v =>
							{
								lock (mutex)
								{
									try
									{
										if (anySampler.HasValue)
											observer.OnNext(combinator(v, anySampler.Value));
										else
											pendingSource = v;	
									}
									catch (Exception e)
									{
										observer.OnError(e);
									}
									
								}
							},
							observer.OnError,
							observer.OnCompleted));
				});
		}
	}
}