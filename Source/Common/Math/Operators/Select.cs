using System;

namespace Outracks
{
	public static partial class Point
	{
		public static Point<TDst> Select<TSrc, TDst>(this Point<TSrc> r, Func<TSrc, TDst> transform)
		{
			return Create(transform(r.X), transform(r.Y));
		}
	}

	public static partial class Vector
	{
		public static Vector<TDst> Select<TSrc, TDst>(this Vector<TSrc> r, Func<TSrc, TDst> transform)
		{
			return Create(transform(r.X), transform(r.Y));
		}
	}

	public static partial class Size
	{
		public static Size<TDst> Select<TSrc, TDst>(this Size<TSrc> r, Func<TSrc, TDst> transform)
		{
			return Create(transform(r.Width), transform(r.Height));
		}
	}

	public static partial class Rectangle
	{
		public static Rectangle<TDst> Select<TSrc, TDst>(this Rectangle<TSrc> r, Func<TSrc, TDst> transform)
		{
			return FromIntervals(
				r.HorizontalInterval.Select(transform),
				r.VerticalInterval.Select(transform));
		}
	}

	public static partial class Interval
	{
		public static Interval<TDst> Select<TSrc, TDst>(this Interval<TSrc> r, Func<TSrc, TDst> transform)
		{
			return FromOffsetLength(transform(r.Offset), transform(r.Length));
		}
	}
}
