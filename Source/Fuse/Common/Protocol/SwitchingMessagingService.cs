using System;
using System.Reactive.Linq;

namespace Outracks.Fuse.Protocol
{
	public static partial class MessagingService
	{
		public static IMessagingService Switch(this IObservable<IMessagingService> source, bool handleDispose = false)
		{
			return new SwitchingMessagingService
			{
				Current = source,
				Disposable = handleDispose
					? source.SubscribeUsing(c => c)
					: Disposable.Empty,
			};
		}

		class SwitchingMessagingService : IMessagingService
		{
			public IObservable<IMessagingService> Current;
			public IDisposable Disposable;

			public void Dispose()
			{
				Disposable.Dispose();
			}

			public IObservable<DateTime> ConnectionLost
			{
				get { return Current.Switch(s => ConnectionLost); }
			}

			public IObservable<T> BroadcastedEvents<T>(bool wantReplay) where T : IEventData
			{
				return Current.Switch(s => s.BroadcastedEvents<T>(wantReplay));
			}

			public void Broadcast<T>(T theEvent) where T : IEventData
			{
				Current.Take(1).Subscribe(s => s.Broadcast(theEvent));
			}

			public IDisposable ProvideOptionally<TRequest, TResponse>(Func<TRequest, Optional<TResponse>> func)
				where TRequest : IRequestData<TResponse>
				where TResponse : IResponseData
			{
				return Current.SubscribeUsing(s => s.ProvideOptionally(func));
			}

			public IDisposable Provide<TRequest, TResponse>(Func<TRequest, TResponse> func)
				where TRequest : IRequestData<TResponse>
				where TResponse : IResponseData
			{
				return Current.SubscribeUsing(s => s.Provide(func));
			}

			public async System.Threading.Tasks.Task<TResponse> Request<TResponse>(IRequestData<TResponse> request)
				where TResponse : IResponseData
			{
				return await (await Current.FirstAsync()).Request(request);
			}
		}
	}

	
}