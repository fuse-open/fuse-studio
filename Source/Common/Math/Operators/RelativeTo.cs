using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Rectangle
	{
		public static IObservable<Rectangle<T>> RelativeTo<T>(this IObservable<Rectangle<T>> self, IObservable<Point<T>> position)
			where T : INumeric<T>
		{
			return self.CombineLatest(position, RelativeTo);
		}

		public static Rectangle<T> RelativeTo<T>(this Rectangle<T> self, Point<T> position)
			where T : INumeric<T>
		{
			return FromIntervals(
				Interval.FromOffsetLength(self.HorizontalInterval.Offset.Sub(position.X), self.HorizontalInterval.Length),
				Interval.FromOffsetLength(self.VerticalInterval.Offset.Sub(position.Y), self.VerticalInterval.Length));
		}

		public static Rectangle<IObservable<T>> RelativeTo<T>(this Rectangle<IObservable<T>> self, Point<T> position)
			where T : INumeric<T>
		{
			return FromIntervals(
				Interval.FromOffsetLength(self.HorizontalInterval.Offset.Sub(position.X), self.HorizontalInterval.Length),
				Interval.FromOffsetLength(self.VerticalInterval.Offset.Sub(position.Y), self.VerticalInterval.Length));
		}

		public static Rectangle<IObservable<T>> RelativeTo<T>(this Rectangle<IObservable<T>> self, Point<IObservable<T>> position)
			where T : INumeric<T>
		{
			return FromIntervals(
				Interval.FromOffsetLength(self.HorizontalInterval.Offset.Sub(position.X), self.HorizontalInterval.Length),
				Interval.FromOffsetLength(self.VerticalInterval.Offset.Sub(position.Y), self.VerticalInterval.Length));
		}
	}
}