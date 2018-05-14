using System;
using System.Reactive.Linq;

namespace Outracks
{
    public static partial class Comparable
    {
		public static T Min<T>(this T self, T other) where T : IComparable<T>
		{
			return self.CompareTo(other) < 0
				? self
				: other;
		}

		public static IObservable<T> Min<T>(this IObservable<T> left, T right) where T : IComparable<T>
		{
			return left.Select(l => l.Min(right));
		}

		public static IObservable<T> Min<T>(this IObservable<T> left, IObservable<T> right) where T : IComparable<T>
		{
			return left.CombineLatest(right, Min);
		}
	}

	public static partial class Size
	{
		public static Size<T> Min<T>(this Size<T> size, T minWidth, T minHeight) where T : IComparable<T>
		{
			return Create(
				size.Width.Min(minWidth),
				size.Height.Min(minHeight));
		}

		public static Size<IObservable<T>> Min<T>(this Size<IObservable<T>> left, Size<IObservable<T>> right) where T : IComparable<T>
		{
			return Create(
				left.Width.Min(right.Width),
				left.Height.Min(right.Height));
		}
	}
}