using System;
using System.Collections.Generic;

namespace Outracks
{
	public static partial class Vector
	{
		// Constructors 

		public static Vector<T> Create<T>(T x, T y)
		{
			return new Vector<T>(x, y);
		}

		public static Vector<T> Scale<T>(this Vector<T> vec, T scalar) where T : INumeric<T>
		{
			return Create(vec.X.Mul(scalar), vec.Y.Mul(scalar));
		}

		public static Vector<double> RotateCCV(this Vector<double> vector)
		{
			return Create(vector.Y, -vector.X);
		}

		public static Vector<T> RotateCCV<T>(this Vector<T> vector) 
			where T: INumeric<T>
		{
			return Create(vector.Y, vector.X.Inverse());
		}

		public static Vector<IObservable<T>> RotateCCV<T>(this Vector<IObservable<T>> vector)
			where T : INumeric<T>
		{
			return Create(x: vector.Y, y: vector.X.Inverse());
		}

		public static Vector<double> RotateCV(this Vector<double> vector)
		{
			return Create(-vector.Y, vector.X);
		}

		public static Vector<T> RotateCV<T>(this Vector<T> vector) 
			where T: INumeric<T>
		{
			return Create(vector.Y.Inverse(), vector.X);
		}

		public static Vector<IObservable<T>> RotateCV<T>(this Vector<IObservable<T>> vector)
			where T : INumeric<T>
		{
			return Create(x: vector.Y.Inverse(), y: vector.X);
		}

		public static T Length<T>(this Vector<T> vector) where T : INumeric<T>, new()
		{
			return new T().FromDouble(Math.Sqrt(vector.LengthSquared().ToDouble()));
		}

		public static T LengthSquared<T>(this Vector<T> vector) where T : INumeric<T>
		{
			return vector.X.RaisedToTwo().Add(vector.Y.RaisedToTwo());
		}

		public static T RaisedToTwo<T>(this T value) where T : IRing<T>
		{
			return value.Mul(value);
		}
	}

	public partial struct Vector<T> : IEquatable<Vector<T>>
	{
		public readonly T X;
		public readonly T Y;

		public Vector(T x, T y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return "{"+X+", "+Y+"}";
		}

		public static bool operator ==(Vector<T> left, Vector<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Vector<T> left, Vector<T> right)
		{
			return !(left == right);
		}

		public bool Equals(Vector<T> other)
		{
			return EqualityComparer<T>.Default.Equals(X, other.X) && EqualityComparer<T>.Default.Equals(Y, other.Y);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Vector<T> && Equals((Vector<T>) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (EqualityComparer<T>.Default.GetHashCode(X) * 397) ^ EqualityComparer<T>.Default.GetHashCode(Y);
			}
		}
	}
}