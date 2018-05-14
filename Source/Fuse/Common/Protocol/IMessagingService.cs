using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Outracks.Fuse.Protocol
{
	public interface IMessagingService : IDisposable
	{
		IObservable<DateTime> ConnectionLost { get; }

		IObservable<T> BroadcastedEvents<T>(bool wantReplay)
			where T : IEventData;

		void Broadcast<T>(T theEvent)
			where T : IEventData;

		IDisposable ProvideOptionally<TRequest, TResponse>(Func<TRequest, Optional<TResponse>> func) 
			where TRequest : IRequestData<TResponse>
			where TResponse : IResponseData;


		IDisposable Provide<TRequest, TResponse>(Func<TRequest, TResponse> func)
			where TRequest : IRequestData<TResponse>
			where TResponse : IResponseData;

		Task<TResponse> Request<TResponse>(IRequestData<TResponse> request)
			where TResponse : IResponseData;
	}

	public interface IRequestSender
	{
		/// <exception cref="RequestFailed" />
		Task<TResponse> Send<TResponse>(IRequestData<TResponse> request)
			where TResponse : IResponseData;
	}

	public interface IRequestReceiver
	{
		IDisposable SubscribeToRequest<T, TResult>(Func<T, Optional<TResult>> transform)
			where T : IRequestData<TResult>
			where TResult : IResponseData;

		/*IDisposable SubscribeToRequest<T, TResult>(Func<T, IObserver<Error>, TResult> transform)
			where T : IRequestData<TResult>
			where TResult : IResponseData;*/
	}

	public class RequestReceiver : IRequestReceiver
	{
		readonly IReport _report;
		readonly IObservable<IRequestMessage<UnresolvedMessagePayload>> _requests;
		readonly IObserver<IResponseMessage<IResponseData>> _responses;

		public RequestReceiver(
			IReport report,
			IObservable<IRequestMessage<UnresolvedMessagePayload>> requests, 
			IObserver<IResponseMessage<IResponseData>> responses)
		{
			_report = report;
			_requests = requests;
			_responses = responses;
		}

		public IDisposable SubscribeToRequest<T, TResult>(Func<T, Optional<TResult>> transform)
			where T : IRequestData<TResult>
			where TResult : IResponseData
		{
			return _requests
				.Deserialize<T>(_report)
				// TODO: This is deadlock-prone!
				// I would have done something like .ObserveOn(TaskPool) to make sure that requests are -not- all handled 
				// on the same thread, as this is deadlock-prone when a request handler sends a request that is handled 
				// by the same client. However, we then need to go over all handlers to make sure that they are thread-safe.
				.Subscribe(
					r =>
					{
						var request = (IRequestMessage<IRequestData>)r;
						try
						{
							var res = transform(r.Arguments);
							res.MatchWith(
								some: d => _responses.OnNext(Response.CreateSuccess(request, (IResponseData)d)), 
								none: () => _responses.OnNext(Response.CreateUnhandled<IResponseData>(request, null, null)));
						}
						catch (Exception e)
						{
							_responses.OnNext(Response.Create(
								request,
								UnboxException(e).Select(exp => exp.ToError()),
								(IResponseData)null));
						}			
					});
		}

		static IEnumerable<Exception> UnboxException(Exception e)
		{
			var exceptions = new List<Exception>();

			if (e is AggregateException)
			{
				var aggregateException = (AggregateException) e;
				foreach (var exeption in aggregateException.InnerExceptions)
					exceptions.AddRange(UnboxException(exeption));
			}
			else
			{
				exceptions.Add(e);
			}

			return exceptions;
		}
	}

	static class ExceptionToErrorExtensions
	{
		public static Error ToError(this Exception e, ErrorCode errorCode = ErrorCode.Unknown)
		{
			if(e is FuseRequestErrorException)
				return ((FuseRequestErrorException)e).Error;
			else
				return new Error(errorCode, e.Message);
		}
	}

	public class FuseRequestErrorException : Exception
	{
		public readonly Error Error;
		public FuseRequestErrorException(ErrorCode errorCode, string message) : base(message)
		{
			Error = new Error(errorCode, message);
		}

		public override string ToString()
		{
			return Error.ToString();
		}
	}

	public class RequestFailed : Exception
	{
		public readonly ImmutableList<Error> Errors;

		public RequestFailed(IEnumerable<Error> errors) 
			: base("Request failed: " + errors.Select(e => e.ToString()).Join("\n"))
		{
			Errors = errors.ToImmutableList();
		}
	}
}