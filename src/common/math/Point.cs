using System;
using System.Collections.Generic;

namespace Outracks
{
	public static partial class Point
	{
		public static Point<T> Create<T>(T u, T v, Axis2D firstAxis)
		{
			return firstAxis == Axis2D.Horizontal ? Create(u, v) : Create(v, u);
		}

		public static Point<T> Create<T>(T x, T y)
		{
			return new Point<T>(x, y);
		}

		public static Point<T> Zero<T>() where T : INumeric<T>, new()
		{
			return new Point<T>(new T().Zero, new T().Zero);
		}

		public static Point<T> WithAxis<T>(this Point<T> point, Axis2D axis, T value)
		{
			return axis == Axis2D.Horizontal
				? new Point<T>(value, point.X)
				: new Point<T>(point.Y, value);
		}

		public static Point<T> WithAxis<T>(this Point<T> point, Axis2D axis, Func<T,T> transform)
		{
			return axis == Axis2D.Horizontal
				? new Point<T>(transform(point.Y), point.X)
				: new Point<T>(point.Y, transform(point.X));
		}
	}

	public partial struct Point<T> : IEquatable<Point<T>> //where T : INumeric<T>
	{
		static Point()
		{
			Algebras.Initialize();
		}

		public readonly T X;
		public readonly T Y;

		public Point(T x, T y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return "{"+X+", "+Y+"}";
		}

		public static bool operator ==(Point<T> left, Point<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Point<T> left, Point<T> right)
		{
			return !(left == right);
		}

		public bool Equals(Point<T> other)
		{
			return EqualityComparer<T>.Default.Equals(X, other.X) && EqualityComparer<T>.Default.Equals(Y, other.Y);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Point<T> && Equals((Point<T>) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (EqualityComparer<T>.Default.GetHashCode(X) * 397) ^ EqualityComparer<T>.Default.GetHashCode(Y);
			}
		}

		public Vector<T> ToVector()
		{
			return new Vector<T>(X, Y);
		}
	}
}