using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Outracks.Fuse.Analytics;

namespace Outracks.Fuse.Protocol
{
	public class Client : IMessagingService
	{
		readonly IMessageConnection _client;
		readonly IObserver<IEventData> _eventBroadcaster;
		readonly RequestSender _requestSender;
		readonly ConcurrentBag<Type> _implements = new ConcurrentBag<Type>();
		readonly IRequestReceiver _receiver;
		readonly IObservable<IEventMessage<UnresolvedMessagePayload>> _broadcastedEvents;
		int _subscribtionId;

		readonly Subject<DateTime> _connectionLost = new Subject<DateTime>();
		readonly IReport _report;

		public IObservable<DateTime> ConnectionLost
		{
			get { return _connectionLost; }
		}

		/// <exception cref="System.TimeoutException">Thrown when no response to publish service request.</exception>
		/// <exception cref="System.AggregateException">Thrown if publish service request failed.</exception>
		public IObservable<T> BroadcastedEvents<T>(bool wantReplay) where T : IEventData
		{
			return Observable.Create<T>(
				listener =>
				{
					var id = Interlocked.Increment(ref _subscribtionId);

					var disposeMe = _broadcastedEvents
						.Where(m => m.SubscriptionId == id)
						.Deserialize<T>(_report)
						.Select(d => d.Data)
						.Subscribe(listener);

					var payloadName = typeof(T).GetPayloadTypeName();
					if (!_requestSender.Send(new SubscribeRequest(payloadName, wantReplay, id))
						.Wait(TimeSpan.FromSeconds(10)))
					{
						throw new TimeoutException("Subscribe to event timed out.");
					}

					return disposeMe;
				});
		}

		public void Broadcast<T>(T theEvent) where T : IEventData
		{
			_eventBroadcaster.OnNext(theEvent);
		}

		/// <exception cref="System.TimeoutException">Thrown when no response to publish service request.</exception>
		/// <exception cref="System.AggregateException">Thrown if publish service request failed.</exception>
		public IDisposable ProvideOptionally<TRequest, TResponse>(Func<TRequest, Optional<TResponse>> func)
			where TRequest : IRequestData<TResponse>
			where TResponse : IResponseData
		{
			var requestType = typeof (TRequest);
			if (!_implements.Contains(requestType))
			{
				var publishServiceRequest = new PublishServiceRequest(new[] { requestType.GetPayloadTypeName() });
				if (!_requestSender.Send(publishServiceRequest)
					.Wait(TimeSpan.FromSeconds(30)))
				{
					throw new TimeoutException("Publish service request timed out.");
				}

				// This is a bit race condition prone, however does not matter.
				// There are no problems with sending publish service request more than one time.
				_implements.Add(requestType);
			}

			return _receiver.SubscribeToRequest(func);
		}

		/// <exception cref="System.TimeoutException">Thrown when no response to publish service request.</exception>
		/// <exception cref="System.AggregateException">Thrown if publish service request failed.</exception>
		public IDisposable Provide<TRequest, TResponse>(Func<TRequest, TResponse> func)
			where TRequest : IRequestData<TResponse>
			where TResponse : IResponseData
		{
			return ProvideOptionally<TRequest, TResponse>((req) => Optional.Some(func(req)));
		}

		public Task<TResponse> Request<TResponse>(IRequestData<TResponse> request) where TResponse : IResponseData
		{
			return _requestSender.Send(request);
		}

		public Client(IMessageConnection client, ISerializer serializer)
		{
			_report = ReportFactory.GetReporter(SystemGuidLoader.LoadOrCreateOrEmpty(), Guid.NewGuid(), "FuseProtocol");
			_client = client;
			var messagesOut = client.OutgoingMessages.Serialized(serializer);

			// TODO: Find a way to either log or do something about parse errors.
			var messagesIn = client.IncomingMessages.TryDeserialize(serializer,
				_report);

			messagesIn.Subscribe(a => { }, () => _connectionLost.OnNext(DateTime.Now));

			_requestSender = new RequestSender(_report, messagesIn, messagesOut);
			_receiver = new RequestReceiver(_report, messagesIn.OfType<IRequestMessage<UnresolvedMessagePayload>>(), messagesOut);
			_eventBroadcaster = Observer.Create<IEventData>(e => messagesOut.OnNext(Event.Create(e)));
			_broadcastedEvents = messagesIn.OfType<IEventMessage<UnresolvedMessagePayload>>();

		}

		public void Dispose()
		{
			_requestSender.Dispose();
			_client.Dispose();
		}
	}
}