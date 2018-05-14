using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Numeric
	{
		public static IObservable<TNum> Mul<TNum, TDenom>(this IObservable<TDenom> left, IObservable<Ratio<TNum, TDenom>> right)
			where TNum : INumeric<TNum>, new()
			where TDenom : INumeric<TDenom>, new()
		{
			return left.CombineLatest(right, (l, r) => l.Mul(r));
		}

		public static IObservable<T> Mul<T>(this IObservable<T> left, IObservable<T> right) where T : INumeric<T>, new()
		{
			return left.CombineLatest(right, (l, r) => l.Mul(r));
		}

		public static IObservable<T> Mul<T>(this IObservable<T> left, double right) where T : INumeric<T>, new()
		{
			return left.Select(l => new T().FromDouble(l.ToDouble() * right));
		}

		public static IObservable<T> Mul<T>(this IObservable<T> left, IObservable<double> right) where T : INumeric<T>, new()
		{
			return left.CombineLatest(right, Mul);
		}

		public static IObservable<T> Mul<T>(this T left, IObservable<double> right) where T : INumeric<T>, new()
		{
			return right.Select(r => left.Mul(r));
		}

		public static T Mul<T>(this T left, double right) where T : INumeric<T>, new()
		{
			return new T().FromDouble(left.ToDouble() * right);
		}

		public static IObservable<T> Mul<T>(this IObservable<T> left, T right) where T : INumeric<T>, new()
		{
			return left.Select(l => l.Mul(right));
		}

		public static TResult Mul<T, TResult>(this T value, Ratio<TResult, T> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return new TResult().FromDouble(value.ToDouble() * ratio.Value);
		}


	}

	public static partial class ContentFrame
	{
		public static ContentFrame<TResult> Mul<T, TResult>(this ContentFrame<T> frame, Ratio<TResult, T> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return new ContentFrame<TResult>(
				frame.FrameBuonds.Mul(ratio),
				frame.ContentBounds.Mul(ratio));
		}
	}

	public static partial class Rectangle
	{

		public static Rectangle<TResult> Mul<T, TResult>(this Rectangle<T> rectangle, Ratio<TResult, T> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return FromPositionSize(
				rectangle.Position.Mul(ratio),
				rectangle.Size.Mul(ratio));
		}
	}

	public static partial class Point
	{
		public static Point<TResult> Mul<T, TResult>(this Point<T> point, Ratio<TResult, T> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return Create(
				point.X.Mul(ratio),
				point.Y.Mul(ratio));
		}
	}

	public static partial class Size
	{
		public static Size<TResult> Mul<T, TResult>(this Size<T> size, Ratio<TResult, T> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return Create(
				size.Width.Mul(ratio),
				size.Height.Mul(ratio));
		}
	}

	public static partial class Vector
	{
		public static Vector<TResult> Mul<T, TResult>(this Vector<T> size, Ratio<TResult, T> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return Create(
				size.X.Mul(ratio),
				size.Y.Mul(ratio));
		}
	}

	public partial struct Ratio<TNum, TDenom>
	{
		public static Size<TNum> operator *(Size<TDenom> size, Ratio<TNum, TDenom> ratio)
		{
			return size.Mul(ratio);
		}

		public static Point<TNum> operator *(Point<TDenom> point, Ratio<TNum, TDenom> ratio)
		{
			return point.Mul(ratio);
		}

		public static Vector<TNum> operator *(Vector<TDenom> vec, Ratio<TNum, TDenom> ratio)
		{
			return vec.Mul(ratio);
		}
	
		public static TNum operator *(TDenom value, Ratio<TNum, TDenom> ratio)
		{
			return value.Mul(ratio);
		}
	}
}
