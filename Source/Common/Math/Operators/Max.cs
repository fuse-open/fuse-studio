using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Comparable
	{
		public static T Max<T>(this T self, T other) where T : IComparable<T>
		{
			return self.CompareTo(other) > 0
				? self
				: other;
		}

		public static Thickness<IObservable<T>> Max<T>(this Thickness<IObservable<T>> a, Thickness<T> b) 
			where T : IComparable<T>
		{
			return a.Select((v, e) => v.Max(b[e]));
		}

		public static IObservable<Thickness<T>> Max<T>(this IObservable<Thickness<T>> a, Thickness<T> b) 
			where T : IComparable<T>
		{
			return a.Select(t => t.Max(b));
		}

		public static Thickness<T> Max<T>(this Thickness<T> a, Thickness<T> b) 
			where T : IComparable<T>
		{
			return Thickness.Create(a.Left.Max(b.Left), a.Top.Max(b.Top), a.Right.Max(b.Right), a.Bottom.Max(b.Bottom));
		}

		public static IObservable<T> Max<T>(this IObservable<T> left, T right) 
			where T : IComparable<T>
		{
			return left.Select(l => l.Max(right));
		}

		public static IObservable<T> Max<T>(this IObservable<T> left, IObservable<T> right) 
			where T : IComparable<T>
		{
			return left.CombineLatest(right, Max);
		}
	}

	public static partial class Size
	{
		public static Size<T> Max<T>(this Size<T> size, T minWidth, T minHeight) 
			where T : IComparable<T>
		{
			return Create(
				size.Width.Max(minWidth),
				size.Height.Max(minHeight));
		}

		public static Size<IObservable<T>> Max<T>(this Size<IObservable<T>> left, Size<IObservable<T>> right) 
			where T : IComparable<T>
		{
			return Create(
				left.Width.Max(right.Width),
				left.Height.Max(right.Height));
		}
	}
}
