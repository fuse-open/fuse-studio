using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Rectangle
	{
		public static IObservable<Rectangle<T>> Deflate<T>(this IObservable<Rectangle<T>> self, IObservable<Thickness<T>> thickness)
			where T : INumeric<T>
		{
			return self.Inflate(thickness.Inverse());
		}

		public static Rectangle<T> Deflate<T>(this Rectangle<T> self, Thickness<T> thickness)
			where T : INumeric<T>
		{
			return self.Inflate(thickness.Inverse());
		}

		public static Rectangle<IObservable<T>> Deflate<T>(this Rectangle<IObservable<T>> self, Thickness<T> thickness)
			where T : INumeric<T>
		{
			return self.Inflate(thickness.Inverse());
		}

		public static Rectangle<IObservable<T>> Deflate<T>(this Rectangle<IObservable<T>> self, Thickness<IObservable<T>> thickness)
			where T : INumeric<T>
		{
			return self.Inflate(thickness.Inverse());
		}

		public static IObservable<Rectangle<T>> Inflate<T>(this IObservable<Rectangle<T>> self, IObservable<Thickness<T>> thickness)
			where T : INumeric<T>
		{
			return self.CombineLatest(thickness, Inflate);
		}

		public static Rectangle<T> Inflate<T>(this Rectangle<T> self, Thickness<T> thickness)
			where T : INumeric<T>
		{
			return FromIntervals(
				self.HorizontalInterval.Inflate(thickness.Left, thickness.Right),
				self.VerticalInterval.Inflate(thickness.Top, thickness.Bottom));
		}

		public static Rectangle<IObservable<T>> Inflate<T>(this Rectangle<IObservable<T>> self, Thickness<T> thickness)
			where T : INumeric<T>
		{
			return FromIntervals(
				self.HorizontalInterval.Inflate(thickness.Left, thickness.Right),
				self.VerticalInterval.Inflate(thickness.Top, thickness.Bottom));
		}

		public static Rectangle<IObservable<T>> Inflate<T>(this Rectangle<IObservable<T>> self, Thickness<IObservable<T>> thickness)
			where T : INumeric<T>
		{
			return FromIntervals(
				self.HorizontalInterval.Inflate(thickness.Left, thickness.Right),
				self.VerticalInterval.Inflate(thickness.Top, thickness.Bottom));
		}
	}

	public static partial class Interval
	{
		public static Interval<T> Inflate<T>(this Interval<T> self, T negative, T positive)
			where T : INumeric<T>
		{
			return FromOffsetLength(
				self.Offset.Sub(negative),
				self.Length.Add(negative).Add(positive));
		}

		public static Interval<IObservable<T>> Inflate<T>(this Interval<IObservable<T>> self, T negative, T positive)
			where T : INumeric<T>
		{
			return FromOffsetLength(
				self.Offset.Sub(negative),
				self.Length.Add(negative).Add(positive));
		}

		public static Interval<IObservable<T>> Inflate<T>(this Interval<IObservable<T>> self, IObservable<T> negative, IObservable<T> positive)
			where T : INumeric<T>
		{
			return FromOffsetLength(
				self.Offset.Sub(negative),
				self.Length.Add(negative).Add(positive));
		}
	}
}