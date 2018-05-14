using System;

namespace Outracks.Fuse
{

	public class Parsed<T>
	{
		public Optional<T> Value { get; set; }
		public string String { get; set; }

		public static Parsed<T> operator |(Parsed<T> a, Parsed<T> b)
		{
			return a.Or(b);
		}

		public static bool operator !(Parsed<T> result)
		{
			return !result.Value.HasValue;
		}

		public static bool operator true(Parsed<T> result)
		{
			return result.Value.HasValue;
		}

		public static bool operator false(Parsed<T> result)
		{
			return !result.Value.HasValue;
		}
	}

	public static class Parsed
	{
		public static Parsed<T> Or<T>(this Parsed<T> left, Parsed<T> right)
		{
			return left.Value.HasValue ? left : right;
		}

		public static Parsed<TOut> Select<TIn, TOut>(this Parsed<TIn> value, Func<TIn, TOut> convert)
		{
			if (!value.Value.HasValue)
				return Failure<TOut>(value.String);

			return Success(convert(value.Value.Value), value.String);
		}

		public static Parsed<T> Success<T>(T value, string stringValue)
		{
			return new Parsed<T>
			{
				Value = value,
				String = stringValue,
			};
		}

		public static Parsed<T> Failure<T>(string strinvValue)
		{
			return new Parsed<T>
			{
				Value = Optional.None(),
				String = strinvValue,
			};
		}

		public static Parsed<T> Create<T>(string stringValue, Optional<T> value)
		{
			return new Parsed<T>
			{
				Value = value,
				String = stringValue,
			};
		}
	}
}