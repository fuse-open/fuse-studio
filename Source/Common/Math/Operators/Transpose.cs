using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Point
	{
		public static Point<IObservable<T>> Transpose<T>(this IObservable<Point<T>> self) where T : IComparable<T>
		{
			self = self.Replay(1).RefCount();
			return Create(
				self.Select(i => i.X).DistinctUntilChanged(),
				self.Select(i => i.Y).DistinctUntilChanged());
		}

		public static IObservable<Point<T>> Transpose<T>(this Point<IObservable<T>> self) where T : IComparable<T>
		{
			return self.X.CombineLatest(self.Y, Create)
				//.Sample(TimeSpan.FromMilliseconds(1))
				;
		}
	}

	public static partial class Vector
	{
		public static Vector<IObservable<T>> Transpose<T>(this IObservable<Vector<T>> self) where T : IComparable<T>
		{
			self = self.Replay(1).RefCount();
			return Create(
				self.Select(i => i.X).DistinctUntilChanged(),
				self.Select(i => i.Y).DistinctUntilChanged());
		}

		public static IObservable<Vector<T>> Transpose<T>(this Vector<IObservable<T>> self) where T : IComparable<T>
		{
			return self.X.CombineLatest(self.Y, Create)
				//.Sample(TimeSpan.FromMilliseconds(1))
				;
		}
	}

	public static partial class Size
	{
		public static Size<IObservable<T>> Transpose<T>(this IObservable<Size<T>> self) where T : IComparable<T>
		{
			self = self.Replay(1).RefCount();
			return Create(
				self.Select(i => i.Width).DistinctUntilChanged(),
				self.Select(i => i.Height).DistinctUntilChanged());
		}

		public static IObservable<Size<T>> Transpose<T>(this Size<IObservable<T>> self) where T : IComparable<T>
		{
			return self.Width.CombineLatest(self.Height, Create)
				//.Sample(TimeSpan.FromMilliseconds(1))
				;
		}
	}

	public static partial class Rectangle
	{
		public static Rectangle<IObservable<T>> Transpose<T>(this IObservable<Rectangle<T>> self) where T : IComparable<T>
		{
			self = self.Replay(1).RefCount();
			return FromIntervals(
				Interval.FromOffsetLength(
					self.Select(r => r.HorizontalInterval.Offset).DistinctUntilChanged(),
					self.Select(r => r.HorizontalInterval.Length).DistinctUntilChanged()),
				Interval.FromOffsetLength(
					self.Select(r => r.VerticalInterval.Offset).DistinctUntilChanged(),
					self.Select(r => r.VerticalInterval.Length).DistinctUntilChanged()));
		}

		public static IObservable<Rectangle<T>> Transpose<T>(this Rectangle<IObservable<T>> self) where T : IComparable<T>
		{
			return self.HorizontalInterval.Transpose()
				.CombineLatest(self.VerticalInterval.Transpose(), FromIntervals)
				//.Sample(TimeSpan.FromMilliseconds(1))
				;
		}
	}

	public static partial class Interval
	{
		public static IObservable<TResult> Combine<T, TResult>(this Interval<IObservable<T>> interval, Func<Interval<T>, TResult> combinator)
		{
			return interval.Transpose().Select(combinator);
		}

		public static IObservable<Interval<T>> Transpose<T>(this Interval<IObservable<T>> self)
		{
			return self.Offset.CombineLatest(self.Length, FromOffsetLength);
		}

		public static Interval<IObservable<T>> Transpose<T>(this IObservable<Interval<T>> self)
		{
			return new Interval<IObservable<T>>(
				self.Select(s => s.Offset),
				self.Select(s => s.Length));
		}
	}
}
