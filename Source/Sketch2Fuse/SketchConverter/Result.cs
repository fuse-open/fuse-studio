using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchConverter
{
	public static class Result
	{
		public static Ok<T> Ok<T>(T value)
		{
			return new Ok<T>(value);
		}

		public static Err<T> Err<T>(string error)
		{
			return new Err<T>(error);
		}

		public static Err Err(string error)
		{
			return new Err(error);
		}
	}

	public class Result<T>
	{
		public T Expect(string message = null)
		{
			var ok = this as Ok<T>;
			if (ok != null)
				return ok.Value;

			var err = this as Err<T>;
			if (err != null)
				throw new Exception(((message == null) ? "" : (message + ": ")) + err.Message);

			throw new Exception("Internal error");
		}

		public static implicit operator Result<T>(T value)
		{
			return new Ok<T>(value);
		}

		public static implicit operator Result<T>(Err error)
		{
			return new Err<T>(error.Message);
		}
	}

	public class Ok<T> : Result<T>
	{
		public readonly T Value;

		public Ok(T value)
		{
			Value = value;
		}

	}

	public class Err<T> : Result<T>
	{
		public readonly string Message;

		public Err(string message)
		{
			Message = message;
		}
	}

	public class Err
	{
		public readonly string Message;

		public Err(string message)
		{
			Message = message;
		}
	}

	public static class ResultExtensions
	{
		public static IEnumerable<T> SelectOk<T>(this IEnumerable<Result<T>> self)
		{
			return self
				.Where(v => v is Ok<T>)
				.Select(v => ((Ok<T>) v).Value);
		}
	}
}
