using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Numeric
	{
		public static IObservable<T> Add<T>(this IObservable<T> left, IObservable<T> right) where T : INumeric<T>
		{
			return Observable.CombineLatest(left, right, (l, r) => l.Add(r));
		}

		public static IObservable<T> Add<T>(this IObservable<T> left, T right) where T : INumeric<T>
		{
			return left.Select(l => l.Add(right));
		}
	}

	public partial struct Point<T>
	{
		public static Point<T> operator +(Point<T> self, Vector<T> vec)
		{
			return new Point<T>(
				Algebra<T>.Add(self.X, vec.X),
				Algebra<T>.Add(self.Y, vec.Y));
		}
	}

	public partial struct Size<T>
	{
		public static Size<T> operator +(Size<T> left, Size<T> right)
		{
			return new Size<T>(
				Algebra<T>.Add(left.Width, right.Width),
				Algebra<T>.Add(left.Height, right.Height));
		}
	}

	public static class SizeAddExtension
	{
		public static IObservable<Size<T>> Add<T>(this IObservable<Size<T>> left, Size<T> right) where T : INumeric<T>
		{
			return left.Select(l => l + right);
		}
	}
}
