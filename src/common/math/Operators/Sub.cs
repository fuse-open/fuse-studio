using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Numeric
	{
		// Numeric = Numeric - Numeric

		public static T Sub<T>(this T left, T right) where T : IGroup<T>
		{
			return left.Add(right.Inverse());
		}

		public static IObservable<T> Sub<T>(this IObservable<T> left, IObservable<T> right) where T : INumeric<T>
		{
			return left.CombineLatest(right, Sub);
		}

		public static IObservable<T> Sub<T>(this IObservable<T> left, T right) where T: INumeric<T>
		{
			return left.Select(l => l.Sub(right));
		}

		public static IObservable<T> Sub<T>(this T left, IObservable<T> right) where T : INumeric<T>
		{
			return right.Select(r => left.Sub(r));
		}
	}

	public static partial class Point
	{
		// Point = Point - Vector

		public static Point<T> Sub<T>(Point<T> self, Vector<T> vec)
		{
			return new Point<T>(
				Algebra<T>.Sub(self.X, vec.X),
				Algebra<T>.Sub(self.Y, vec.Y));
		}

		public static Point<IObservable<T>> Sub<T>(Point<IObservable<T>> left, Vector<T> right)
			where T : INumeric<T>
		{
			return Create(
				left.X.Sub(right.X),
				left.Y.Sub(right.Y));
		}

		public static Point<IObservable<T>> Sub<T>(Point<T> left, Vector<IObservable<T>> right)
			where T : INumeric<T>
		{
			return Create(
				left.X.Sub(right.X),
				left.Y.Sub(right.Y));
		}

	}

	public static partial class Vector
	{
		// Vector = Vector - Vector

		public static Vector<T> Sub<T>(Vector<T> to, Vector<T> from)
		{
			return new Vector<T>(
				Algebra<T>.Sub(to.X, from.X),
				Algebra<T>.Sub(to.Y, from.Y));
		}

		public static Vector<IObservable<T>> Sub<T>(this Vector<IObservable<T>> left, Vector<T> right) where T : INumeric<T>
		{
			return Create(
				left.X.Sub(right.X),
				left.Y.Sub(right.Y));
		}

		public static Vector<IObservable<T>> Sub<T>(this Vector<T> left, Vector<IObservable<T>> right) where T : INumeric<T>
		{
			return Create(
				left.X.Sub(right.X),
				left.Y.Sub(right.Y));
		}

		// Vector = Point - Point

		public static Vector<T> Sub<T>(Point<T> self, Point<T> vec)
		{
			return new Vector<T>(
				Algebra<T>.Sub(self.X, vec.X),
				Algebra<T>.Sub(self.Y, vec.Y));
		}

		public static Vector<IObservable<T>> Sub<T>(Point<IObservable<T>> left, Point<T> right)
			where T : INumeric<T>
		{
			return Create(
				left.X.Sub(right.X),
				left.Y.Sub(right.Y));
		}

		public static Vector<IObservable<T>> Sub<T>(Point<T> left, Point<IObservable<T>> right)
			where T : INumeric<T>
		{
			return Create(
				left.X.Sub(right.X),
				left.Y.Sub(right.Y));
		}
	}

	public static partial class Size
	{
		// Size = Size - Size

		public static Size<T> Sub<T>(Size<T> left, Size<T> right)
		{
			return new Size<T>(
				Algebra<T>.Sub(left.Width, right.Width),
				Algebra<T>.Sub(left.Height, right.Height));
		}

		public static Size<IObservable<T>> Sub<T>(this Size<IObservable<T>> left, Size<T> right) where T : INumeric<T>
		{
			return Create(
				left.Width.Sub(right.Width),
				left.Height.Sub(right.Height));
		}

		public static Size<IObservable<T>> Sub<T>(this Size<T> left, Size<IObservable<T>> right) where T : INumeric<T>
		{
			return Create(
				left.Width.Sub(right.Width),
				left.Height.Sub(right.Height));
		}
	}

}
