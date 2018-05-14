using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Outracks
{
	public struct Interval<T> : IEquatable<Interval<T>>
	{
		public readonly T Offset;
		public readonly T Length;

		public Interval(T offset, T length) 
		{
			Offset = offset;
			Length = length;
		}

		public bool Equals(Interval<T> other)
		{
			return
				EqualityComparer<T>.Default.Equals(Offset, other.Offset) &&
				EqualityComparer<T>.Default.Equals(Length, other.Length);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Interval<T> && Equals((Interval<T>)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (EqualityComparer<T>.Default.GetHashCode(Offset) * 397) ^ EqualityComparer<T>.Default.GetHashCode(Length);
			}
		}

		public static bool operator ==(Interval<T> left, Interval<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Interval<T> left, Interval<T> right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return Offset + " (+ " + Length + ")";
		}
	}

	public static partial class Interval
	{
		public static IObservable<Points> Center(this Interval<IObservable<Points>> self)
		{
			return self.Offset.Add(self.Length.Div(2));
		}

		public static Interval<T> FromOffsetLength<T>(T offset, T length)
		{
			return new Interval<T>(offset, length);
		}

		public static Interval<double> FromMinMax(double min, double max)
		{
			return new Interval<double>(min, max - min);
		}

		public static Interval<IObservable<T>> FromMinMax<T>(IObservable<T> min, IObservable<T> max)
			where T : INumeric<T>
		{
			return new Interval<IObservable<T>>(min, max.Sub(min));
		}

		public static Interval<T> FromMinMax<T>(T min, T max)
			where T : INumeric<T>
		{
			return new Interval<T>(min, max.Sub(min));
		}

		public static bool Contains<T>(this Interval<T> interval, T n) where T : IComparable<T>, INumeric<T>
		{
			// Exploiting the fact that this 'offset >= n - length' is the same as 'offset + length <= n'
			// So that interval.Length can be PositiveInfinity and offset NegativeInfinity. 
			return interval.Offset.LessThanOrEquals(n) && interval.Offset.GreaterThanOrEquals(n.Sub(interval.Length));
		}
	}
}