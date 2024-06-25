using System;
using System.Collections;
using System.Collections.Generic;

namespace SketchConverter
{
	public static class NullToNone
	{
		/// <summary>
		/// Convert null to None and other values to Some(T)
		/// </summary>
		public static Optional<T> ToOptional<T>(this T value) where T : class
		{
			return value != null
				? Optional.Some(value)
				: Optional.None();
		}
	}

	public class None
	{
	}

	public static partial class Optional
	{
		public static Optional<T> Some<T>(T value)
		{
			return new Optional<T>(true, value);
		}

		public static Optional<T> None<T>()
		{
			return new Optional<T>(false);
		}

		public static None None()
		{
			return new None();
		}
	}

	public struct Optional<T> : IEquatable<Optional<T>>, IEnumerable<T>
	{
		readonly bool _hasValue;
		readonly T _value;

		public bool HasValue
		{
			get { return _hasValue; }
		}

		public T Value
		{
			get
			{
				if (!_hasValue) throw new InvalidOperationException();
				return _value;
			}
		}

		internal Optional(bool hasValue, T value = default(T))
		{
			_hasValue = hasValue;
			_value = value;
		}

		public static implicit operator Optional<T>(T value)
		{
			return new Optional<T>(true, value);
		}

		public static implicit operator Optional<T>(None value)
		{
			return new Optional<T>(false);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Optional<T> && Equals((Optional<T>)obj);
		}

		public bool Equals(Optional<T> other)
		{
			return _hasValue.Equals(other._hasValue) && EqualityComparer<T>.Default.Equals(_value, other._value);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_hasValue.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(_value);
			}
		}

		public static bool operator ==(Optional<T> left, Optional<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Optional<T> left, Optional<T> right)
		{
			return !left.Equals(right);
		}

		public Optional<TResult> As<TResult>() where TResult : T
		{
			return _hasValue && _value is TResult
				? Optional.Some((TResult)_value)
				: Optional.None();
		}

		public Optional<T> Where(Func<T, bool> predicate)
		{
			return _hasValue && predicate(_value)
				? Optional.Some(_value)
				: Optional.None();
		}

		public void Do(Action<T> some)
		{
			if (_hasValue)
				some(_value);
		}


		public void Do(Action<T> some, Action none)
		{
			if (_hasValue)
				some(_value);
			else
				none();
		}

		public void MatchWith(Action<T> some, Action none)
		{
			if (_hasValue)
				some(_value);
			else
				none();
		}

		public TResult MatchWith<TResult>(
			Func<T, TResult> some,
			Func<TResult> none)
		{
			return _hasValue ? some(_value) : none();
		}


		public override string ToString()
		{
			return HasValue
				? "Some {" + Value + "}"
				: "None";
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (_hasValue)
				yield return _value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

}