using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Outracks.Fuse.Protocol
{
	public enum Status
	{
		Success,
		Error,
		Unhandled
	}

	public enum ErrorCode
	{
		Unknown = 0,
		InvalidJson = 1,
		InvalidData = 2,
		InvalidOperation = 3,
		InvalidRequest = 4,
		LostConnection = 5,
		WrongDaemonKey = 6,
	}

	public sealed class Error
	{
		public readonly int Code;
		public readonly string Message;

		public Error(ErrorCode errorCode, string message)
		{
			Code = (int) errorCode;
			Message = message;
		}

		[JsonConstructor]
		Error(int code, string message)
		{
			Code = code;
			Message = message;
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return "(" + Code + ")" + " - " + Message;
		}
	}

	public sealed class Response<T> : IResponseMessage<T> where T : IResponseData
	{
		[JsonIgnore]
		public string MessageType { get { return "Response"; } }

		public int Id { get; private set; }

		public Status Status { get; private set; }

		public List<Error> Errors{ get; private set; }

		public T Result { get; private set; }

		[JsonConstructor]
		public Response(int id, Status status, List<Error> errors, T result)
		{
			Id = id;
			Result = result;
			Status = status;
			Errors = errors;
		}		
	}

	public static class Response
	{
		public static Response<T> Create<T>(IRequestMessage<IRequestData> request, IEnumerable<Error> errors, T data) where T : IResponseData
		{
			var errorsCopy = errors.ToList();

			var status = Status.Success;
			if (!errorsCopy.IsEmpty())
				status = Status.Error;

			return new Response<T>(request.Id, status, errorsCopy, data);
		}

		public static Response<T> CreateSuccess<T>(IRequestMessage<IRequestData> request, T data) where T : IResponseData
		{
			return new Response<T>(request.Id, Status.Success, null, data);
		}

		public static Response<T> CreateUnhandled<T>(IRequestMessage<IRequestData> request, Error reason, T data) where T : IResponseData
		{
			return new Response<T>(request.Id, Status.Unhandled, new List<Error>{ reason }, data);
		}
	}
}