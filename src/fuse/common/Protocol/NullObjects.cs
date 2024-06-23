using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Outracks.Fuse.Protocol
{
	public class NullMessagingService : IMessagingService
	{
		public void Dispose() {}

		public IObservable<DateTime> ConnectionLost
		{
			get { return null; }
		}

		public IObservable<T> BroadcastedEvents<T>(bool wantReplay) where T : IEventData
		{
			return Observable.Never<T>();
		}

		public void Broadcast<T>(T theEvent) where T : IEventData
		{
		}

		public IDisposable ProvideOptionally<TRequest, TResponse>(Func<TRequest, Optional<TResponse>> func) where TRequest : IRequestData<TResponse> where TResponse : IResponseData
		{
			return Disposable.Create(() => {});
		}

		public IDisposable Provide<TRequest, TResponse>(Func<TRequest, TResponse> func)
			where TRequest : IRequestData<TResponse>
			where TResponse : IResponseData
		{
			return Disposable.Create(() => { });
		}

		public Task<TResponse> Request<TResponse>(IRequestData<TResponse> request) where TResponse : IResponseData
		{
			//throw new RequestFailed(new Error[0]);
			return new Task<TResponse>(() =>
			{
				throw new RequestFailed( new Error[0]);
			});
		}
	}
}
