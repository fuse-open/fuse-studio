using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Outracks.Fuse.Protocol.Tests
{
	public class FailureClient : IMessagingService
	{
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {}
		public IObservable<IEventData> BroadcastedEvents { get; private set; }
		public IObserver<IEventData> Events { get; private set; }
		public IRequestSender Requests { get; private set; }
		public IRequestReceiver Receiver { get; private set; }

		public FailureClient(IRequestSender requestSender)
		{
			Requests = requestSender;
			BroadcastedEvents = new Subject<IEventData>();
			Events = new Subject<IEventData>();
		}

		public IObservable<DateTime> ConnectionLost { get; private set; }
		IObservable<T> IMessagingService.BroadcastedEvents<T>(bool wantReplay)
		{
			return null;
		}

		public void Broadcast<T>(T theEvent) where T : IEventData {}
		public IDisposable Provide<TRequest, TResponse>(Func<TRequest, TResponse> func) where TRequest : IRequestData<TResponse> where TResponse : IResponseData
		{
			return null;
		}

		public IDisposable ProvideOptionally<TRequest, TResponse>(Func<TRequest, Optional<TResponse>> func)
			where TRequest : IRequestData<TResponse>
			where TResponse : IResponseData
		{
			return null;
		}

		public Task<TResponse> Request<TResponse>(IRequestData<TResponse> request) where TResponse : IResponseData
		{
			return null;
		}
	}

	public class InfinityRequestSender : IRequestSender
	{
		readonly bool _supportHello;

		bool _run = true;

		public InfinityRequestSender(bool supportHello)
		{
			_supportHello = supportHello;
		}

		public Task<TResponse> Send<TResponse>(IRequestData<TResponse> request) where TResponse : IResponseData
		{
			if (_supportHello && request is HelloRequest)
			{
				IResponseData helloResponse = new HelloResponse();
				return Task.FromResult((TResponse)helloResponse);
			}

			return Task.Run(
				() =>
				{
					while (_run)
					{
						Thread.Sleep(1000);
					}

					return (TResponse) ((IResponseData) null);
				});
		}
	}

	public class ExceptionRequestSender : IRequestSender
	{
		readonly bool _supportHello;
		public ExceptionRequestSender(bool supportHello)
		{
			_supportHello = supportHello;
		}

		public async Task<TResponse> Send<TResponse>(IRequestData<TResponse> request) where TResponse : IResponseData
		{
			if (_supportHello && request is HelloRequest)
			{
				IResponseData helloResponse = new HelloResponse();
				return await Task.FromResult((TResponse)helloResponse);
			}

			var tcp = new TaskCompletionSource<TResponse>();
			tcp.SetException(new RequestFailed(new [] { new Error(ErrorCode.Unknown, "Haha lol")}));
			return await tcp.Task;
		}
	}
}