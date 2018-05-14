using System;

namespace Outracks
{
	public interface IValidationResult<out T>
	{
		bool HasValue { get; }
		T Value { get; }
		string Error { get; }
	}

	public static class ValidationResult
	{
		public static IValidationResult<T> Valid<T>(T value)
		{
			return new ValidationResult<T>
			{
				Error = "",
				Value = value
			};
		}
		public static IValidationResult<T> Invalid<T>(string error)
		{
			return new ValidationResult<T>
			{
				Error = error,
				Value = Optional.None()
			};
		}

		public static IValidationResult<T> AsValidationResult<T>(this Optional<T> value, string error)
		{
			return new ValidationResult<T>
			{
				Error = error,
				Value = value
			};
		}

		public static TResult MatchWith<T, TResult>(
			this IValidationResult<T> self,
			Func<T, TResult> succsess,
			Func<string, TResult> error)
		{
			return self.HasValue
				? succsess(self.Value)
				: error(self.Error);
		}
	}

	class ValidationResult<T> : IValidationResult<T>
	{
		public string Error { get; set; }
		public Optional<T> Value { get; set; }
		bool IValidationResult<T>.HasValue { get { return Value.HasValue; } }
		T IValidationResult<T>.Value { get { return Value.Value; } }
	}

}
