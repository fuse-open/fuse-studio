using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse.Daemon
{
	class PendingRequest
	{
		public readonly int Id;
		public readonly IRequestMessage<IRequestData> Request;
		public readonly PluginClient WhoRequested;

		readonly TaskCompletionSource<IResponseMessage<IResponseData>> _responseCompletion = new TaskCompletionSource<IResponseMessage<IResponseData>>();
		public Task<IResponseMessage<IResponseData>> Task { get { return _responseCompletion.Task; } }

		public PendingRequest(int id, IRequestMessage<IRequestData> request, PluginClient whoRequested)
		{
			Id = id;
			Request = request;
			WhoRequested = whoRequested;
		}

		public void SetResult(IResponseMessage<IResponseData> response)
		{
			_responseCompletion.SetResult(response);
		}
	}

	class PluginClient : IDisposable
	{
		readonly IObserver<IMessage> _outStream;
		readonly Identity<ImmutableList<PendingRequest>> _pendingRequests = Identity.Create(ImmutableList<PendingRequest>.Empty);
		readonly Identity<ImmutableList<string>> _implements = new Identity<ImmutableList<string>>(ImmutableList<string>.Empty);
		readonly string _eventFilter;
		bool _isUsingOldEventSubscription = true;

		public readonly string Identifier;

		public PluginClient(
			IObserver<IMessage> outStream,
			string identifier,
			string []implements,
			string eventFilter)
		{
			_outStream = outStream;
			Identifier = identifier;
			_implements.Update(i => ImmutableList.Create(implements ?? new string[0]));
			_eventFilter = eventFilter;
		}

		public Task<IResponseMessage<IResponseData>> SendRequest(PluginClient whoRequested, int id, IRequestMessage<IRequestData> request)
		{
			var pendingRequest = new PendingRequest(id, request, whoRequested);
			_pendingRequests.Update(l => l.Add(pendingRequest));
			_outStream.OnNext(request.ChangeId(id));
			return pendingRequest.Task;
		}

		public void HandleResponse(IResponseMessage<IResponseData> response)
		{
			foreach (var pendingRequest in _pendingRequests.Value)
			{
				if (pendingRequest.Id == response.Id)
				{
					var request = pendingRequest;
					_pendingRequests.Update(l => l.Remove(request));
					request.SetResult(response);
				}
			}
		}

		public void SendResponse(IResponseMessage<IResponseData> response)
		{
			_outStream.OnNext(response);
		}

		public IDisposable SubscribeToEvent(
			IRequestMessage<SubscribeRequest> subscribeRequest,
			IObservable<IEventMessage<IEventData>> hotMessages,
			IObservable<IEventMessage<IEventData>> replayMessages)
		{
			_isUsingOldEventSubscription = false;

			var subscribeResponse = new SubscribeResponse();
			_outStream.OnNext(Response.CreateSuccess(subscribeRequest, subscribeResponse));

			var subscribeArgs = subscribeRequest.Arguments;
			if (subscribeArgs.Replay)
			{
				return replayMessages
					.Where(d => EventTypeMatchesFilter(d.Name, subscribeRequest.Arguments.Filter))
					.Subscribe(
						data => _outStream.OnNext(new Event<IEventData>(data.Name, data.Data, subscribeArgs.SubscriptionId)));
			}
			else
			{
				return hotMessages
					.Where(d => EventTypeMatchesFilter(d.Name, subscribeRequest.Arguments.Filter))
					.Subscribe(
						data => _outStream.OnNext(new Event<IEventData>(data.Name, data.Data, subscribeArgs.SubscriptionId)));
			}
		}

		public void HandleEvent(IEventMessage<IEventData> evt)
		{
#pragma warning disable 0618

			if (_isUsingOldEventSubscription && EventTypeMatchesFilterOld(evt.Name))
				_outStream.OnNext(new Event<IEventData>(evt.Name, evt.Data));
#pragma warning restore 0618
		}

		[Obsolete("Remove when we are going 100 % in for the new API")]
		bool EventTypeMatchesFilterOld(string eventName)
		{
			return string.IsNullOrEmpty(_eventFilter) || Regex.IsMatch(eventName, _eventFilter);
		}

		bool EventTypeMatchesFilter(string eventName, string filter)
		{
			return Regex.IsMatch(eventName, filter);
		}

		public bool IsImlementorOf(string request)
		{
			return _implements.Value.Contains(request);
		}

		public void Dispose()
		{
			foreach (var pendingRequest in _pendingRequests.Value)
			{
				pendingRequest
					.WhoRequested
					.SendResponse(
						Response.CreateUnhandled(
							pendingRequest.Request,
							new Error(ErrorCode.LostConnection, "Lost connection to implementor of " + pendingRequest.Request.Name),
							(UnresolvedMessagePayload)null));
			}
		}

		public Optional<PublishServiceResponse> AppendSupportedRequests(PublishServiceRequest responseMessage)
		{
			_implements.Update(l => ImmutableList.Create(
				l.Union(responseMessage.RequestNames)
				.ToArray()));
			return new PublishServiceResponse();
		}
	}

	static class RequestAndResponseExtensions
	{
		public static IRequestMessage<IRequestData> ChangeId(this IRequestMessage<IRequestData> request, int id)
		{
			return new Request<IRequestData>(id, request.Name, request.Arguments);
		}

		public static IResponseMessage<IResponseData> ChangeId(this IResponseMessage<IResponseData> response, int id)
		{
			return new Response<IResponseData>(id, response.Status, response.Errors, response.Result);
		}
	}
}