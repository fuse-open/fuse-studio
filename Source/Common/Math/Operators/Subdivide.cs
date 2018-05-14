using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks
{
	public static class SubdivideExtension
	{
		// Rectangle 

		public static Rectangle<T>[] Subdivide<T>(this Rectangle<T> rect, Axis2D axis, int divisions) where T : INumeric<T>
		{
			return rect.Subdivide(axis, Enumerable.Repeat(1.0, divisions));
		}

		public static Rectangle<T>[] Subdivide<T>(this Rectangle<T> rect, Axis2D axis, IEnumerable<double> divisions) where T : INumeric<T>
		{
			return rect[axis]
				.Subdivide(divisions)
				.Select(interval =>
					axis == Axis2D.Horizontal
						? Rectangle.FromIntervals(interval, rect.VerticalInterval)
						: Rectangle.FromIntervals(rect.HorizontalInterval, interval))
				.ToArray();
		}

		// Interval

		public static Interval<T>[] Subdivide<T>(this Interval<T> interval, IEnumerable<double> divisions) where T : INumeric<T>
		{
			var div = divisions.ToArray();
			var intervals = new Interval<T>[div.Length];

			if (interval.Length.ToDouble() < double.Epsilon)
			{
				for (int i = 0; i < div.Length; i++)
					intervals[i] = interval;
				return intervals;
			}

			var divSum = div.Sum();
			var intervalLength = interval.Length;

			var left = interval.Offset;
			for (int i = 0; i < div.Length; i++)
			{
				var division = div[i];

				var elementLength = left.FromDouble(division / divSum * intervalLength.ToDouble());
				var right = left.Add(elementLength);

				var leftRounded = left.Round();
				intervals[i] = Interval.FromOffsetLength(leftRounded, right.Round().Sub(leftRounded));

				left = right;
			}

			return intervals;
		}

		public static Interval<T>[] Subdivide<T>(this Interval<T> interval, int divisions) where T : INumeric<T>
		{
			var intervals = new Interval<T>[divisions];

			var t = interval.Offset;
			var lengthPerElement = t.FromDouble(interval.Length.ToDouble() / divisions);

			for (int i = 0; i < divisions; i++)
			{
				intervals[i] = Interval.FromOffsetLength(
					interval.Offset.Add(lengthPerElement.Mul(t.FromDouble(i))),
					lengthPerElement);
			}

			return intervals;
		}

		// Size

		public static Size<T> Subdivide<T>(this Size<T> size, Axis2D axis, int divisions) where T : INumeric<T>, new()
		{
			return axis == Axis2D.Horizontal
				? new Size<T>(new T().FromDouble(size.Width.ToDouble() / divisions), size.Height)
				: new Size<T>(size.Width, new T().FromDouble(size.Height.ToDouble() / divisions));
		}

		public static Interval<IObservable<T>>[] Subdivide<T>(this Interval<IObservable<T>> interval, IEnumerable<double> divisions) where T : INumeric<T>, new()
		{
			var div = divisions.ToArray();

			var intervals = new Interval<IObservable<T>>[div.Length];

			var transInterval = interval.Transpose().Select(t => t.Subdivide(div));

			for (int i = 0; i < div.Length; i++)
			{
				var ii = i;
				intervals[i] = transInterval.Select(t => t[ii]).Transpose();
			}

			return intervals;
		}

	}
}