using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Outracks.IO;

namespace Outracks.Fuse.Protocol
{
	interface IResponseTask
	{
		void SetResult(IResponseData response);

		void SetException(Exception e);
	}

	class FailedToDeserializeException : Exception
	{
		public FailedToDeserializeException() : base("Failed to deserialize the response.")
		{			
		}
	}

	class ResponseTask<TResponseData> : IResponseTask 
		where TResponseData : IResponseData
	{
		readonly IReport _report;
		readonly TaskCompletionSource<TResponseData> _tcs;
		public ResponseTask(IReport report, TaskCompletionSource<TResponseData> tcs)
		{
			_report = report;
			_tcs = tcs;
		}

		public void SetResult(IResponseData response)
		{
			var unresolvedMessage = (UnresolvedMessagePayload)response;
			var deserializedMsg = unresolvedMessage.Deserialize<TResponseData>(_report);
			if(deserializedMsg.HasValue)
				_tcs.SetResult(deserializedMsg.Value);
			else
				SetException(new FailedToDeserializeException());
		}

		public void SetException(Exception e)
		{
			_tcs.TrySetException(e);
		}
	}

	public class RequestSender : IDisposable, IRequestSender
	{
		readonly IReport _report;
		readonly IObserver<IMessage> _output;
		readonly IDisposable _inputSubscription;

		readonly ConcurrentDictionary<int, IResponseTask> _awaitingTasks = new ConcurrentDictionary<int, IResponseTask>();

		public RequestSender(IReport report, IObservable<IMessage> input, IObserver<IMessage> output)
		{
			_inputSubscription = input.OfType<IResponseMessage<IResponseData>>().Subscribe(OnResponse);
			_report = report;
			_output = output;
		}

		void OnResponse(IResponseMessage<IResponseData> response)
		{
			IResponseTask tcs;
			if (!_awaitingTasks.TryRemove(response.Id, out tcs))
				return;

			if (response.Status == Status.Success)
				tcs.SetResult(response.Result);
			else
				tcs.SetException(new RequestFailed(response.Errors));
		}
		
		public Task<TResponse> Send<TResponse>(IRequestData<TResponse> request) where TResponse : IResponseData
		{
			var id = NewRequestId();
			var tcs = new TaskCompletionSource<TResponse>();
			_awaitingTasks[id] = new ResponseTask<TResponse>(_report, tcs);
			_output.OnNext(Request.Create(id, request));
			return tcs.Task;
		}

		static int _id;
		int NewRequestId()
		{
			return Interlocked.Increment(ref _id);
		}

		public void Dispose()
		{
			_inputSubscription.Dispose();
		}
	}

	public static class RequestSenderExtensions
	{
		public static Optional<T> TryRequestWithRetry<T>(
			this IMessagingService sender,
			IRequestData<T> request,
			Func<int, TimeSpan> tryTimeout = null,
			int? maxTries = null)
			where T : IResponseData
		{
			try
			{
				return sender.RequestWithRetry(request, tryTimeout, maxTries).GetResultAndUnpackExceptions();
			}
			catch (Exception)
			{
				return Optional.None();
			}
		}

		/// <exception cref="AggregateException"></exception>
		public static async Task<T> RequestWithRetry<T>(
			this IMessagingService sender,
			IRequestData<T> request,
			Func<int, TimeSpan> tryTimeout,
			int? maxTries = null) 
			where T : IResponseData
		{
			var exceptions = new List<Exception>();
			for (var i = 0; maxTries.HasValue == false || i < maxTries; i++)
			{
				try
				{
					return await sender.Request(request)
						.TimeoutAfter(tryTimeout != null ? tryTimeout(i) : Timeout.InfiniteTimeSpan);
				}
				catch (RequestFailed e)
				{
					exceptions.Add(e);
					Thread.Sleep(TimeSpan.FromMilliseconds(200));
				}
				catch (Exception e)
				{
					exceptions.Add(e);
					break; // Something not expected happened lets forward this.
				}
			}

			throw new AggregateException("Retry count of " + (maxTries.HasValue ? maxTries.Value.ToString(CultureInfo.InvariantCulture.NumberFormat) : "infinity") + " exceeded", exceptions);
		}


	}
}