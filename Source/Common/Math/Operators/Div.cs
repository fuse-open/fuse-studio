using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Numeric
	{
		public static IObservable<Ratio<TNum, TDenom>> Div<TNum, TDenom>(this IObservable<TNum> left, IObservable<TDenom> right)
			where TNum : INumeric<TNum>, new()
			where TDenom : INumeric<TDenom>, new()
		{
			return left.CombineLatest(right, Div);
		}

		public static IObservable<TDenom> Div<TNum, TDenom>(this IObservable<TNum> left, IObservable<Ratio<TNum, TDenom>> right)
			where TNum : INumeric<TNum>, new()
			where TDenom : INumeric<TDenom>, new()
		{
			return left.CombineLatest(right, Div);
		}


		public static IObservable<T> Div<T>(this IObservable<T> left, int right) where T : INumeric<T>, new()
		{
			return left.Select(l => new T().FromDouble(l.ToDouble() / right));
		}

		public static TResult Div<T, TResult>(this T value, Ratio<T, TResult> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return value.Mul(ratio.Reciprocal());
		}

		public static Ratio<TNum, TDenom> Div<TNum, TDenom>(this TNum self, TDenom other) 
			where TNum : INumeric<TNum>, new() 
			where TDenom : INumeric<TDenom>, new()
		{
			return self.ToDouble() / other.ToDouble();
		}
	}

	public static partial class ContentFrame
	{
		public static ContentFrame<TResult> Div<T, TResult>(this ContentFrame<T> frame, Ratio<T, TResult> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return frame.Mul(ratio.Reciprocal());
		}
	}

	public static partial class Rectangle
	{
		public static Rectangle<TResult> Div<T, TResult>(this Rectangle<T> rectangle, Ratio<T, TResult> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return rectangle.Mul(ratio.Reciprocal());
		}
	}

	public static partial class Vector
	{
		public static Vector<TResult> Div<T, TResult>(this Vector<T> point, Ratio<T, TResult> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return point.Mul(ratio.Reciprocal());
		}
	}

	public static partial class Point
	{
		public static Point<TResult> Div<T, TResult>(this Point<T> point, Ratio<T, TResult> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return point.Mul(ratio.Reciprocal());
		}
	}

	public static partial class Size
	{
		public static Size<TResult> Div<T, TResult>(this Size<T> size, Ratio<T, TResult> ratio)
			where T : INumeric<T>, new()
			where TResult : INumeric<TResult>, new()
		{
			return size.Mul(ratio.Reciprocal());
		}
	}

	public partial struct Ratio<TNum, TDenom>
	{
		public static Vector<TDenom> operator /(Vector<TNum> vec, Ratio<TNum, TDenom> ratio)
		{
			return vec.Div(ratio);
		}

		public static Size<TDenom> operator /(Size<TNum> size, Ratio<TNum, TDenom> ratio)
		{
			return size.Div(ratio);
		}

		public static Point<TDenom> operator /(Point<TNum> size, Ratio<TNum, TDenom> ratio)
		{
			return size.Div(ratio);
		}

		public static TDenom operator /(TNum num, Ratio<TNum, TDenom> ratio)
		{
			return num.Div(ratio);
		}
	}
}
