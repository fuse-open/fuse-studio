using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Rectangle
	{
		public static IObservable<Rectangle<T>> Transform<T>(this IObservable<Rectangle<T>> rect, IObservable<Matrix> m)
			where T : INumeric<T>, new()
		{
			return rect.CombineLatest(m, Transform);
		}

		public static Rectangle<T> Transform<T>(this Rectangle<T> rect, Matrix m)
			where T : INumeric<T>, new ()
		{
			var p0 = rect.LeftTop();
			var p1 = rect.RightTop();
			var p2 = rect.LeftBottom();
			var p3 = rect.RightBottom();

			var tp0 = p0.Transform(m);
			var tp1 = p1.Transform(m);
			var tp2 = p2.Transform(m);
			var tp3 = p3.Transform(m);

			var minX = tp0.X.Min(tp1.X.Min(tp2.X.Min(tp3.X)));
			var minY = tp0.Y.Min(tp1.Y.Min(tp2.Y.Min(tp3.Y)));
			var maxX = tp0.X.Max(tp1.X.Max(tp2.X.Max(tp3.X)));
			var maxY = tp0.Y.Max(tp1.Y.Max(tp2.Y.Max(tp3.Y)));

			return FromSides(minX, minY, maxX, maxY);
		}

		// Constructors

		public static Rectangle<T> FromIntervals<T>(Interval<T> horizontalInterval, Interval<T> verticalInterval)
		{
			return new Rectangle<T>(horizontalInterval, verticalInterval);
		}

		public static Rectangle<double> FromSides(double left, double top, double right, double bottom)
		{
			return FromIntervals(
				Interval.FromMinMax(left, right),
				Interval.FromMinMax(top, bottom));
		}

		public static Rectangle<T> FromSides<T>(T left, T top, T right, T bottom)
			where T : INumeric<T>
		{
			return FromIntervals(
				Interval.FromMinMax(left, right),
				Interval.FromMinMax(top, bottom));
		}

		public static Rectangle<IObservable<T>> FromSides<T>(IObservable<T> left, IObservable<T> top, IObservable<T> right, IObservable<T> bottom)
			where T : INumeric<T>
		{
			return FromIntervals(
				Interval.FromMinMax(left, right),
				Interval.FromMinMax(top, bottom));
		}

		public static Rectangle<T> ToRectangle<T>(this Size<T> size) where T : INumeric<T>, new()
		{
			return FromIntervals(
				Interval.FromOffsetLength(new T(), size.Width),
				Interval.FromOffsetLength(new T(), size.Height));
		}

		public static Rectangle<T> FromPositionSize<T>(Point<T> position, Size<T> size)
		{
			return FromPositionSize(position.X, position.Y, size.Width, size.Height);
		}

		public static Rectangle<T> FromPositionSize<T>(T left, T top, T width, T height)
		{
			return FromIntervals(
				Interval.FromOffsetLength(left, width),
				Interval.FromOffsetLength(top, height));
		}

		public static Rectangle<T> Zero<T>() where T : INumeric<T>, new()
		{
			return FromPositionSize(Point.Zero<T>(), Outracks.Size.Zero<T>());
		}

		public static bool Contains<T>(this Rectangle<T> rect, Point<T> p) where T : INumeric<T>, new()
		{
			return rect.HorizontalInterval.Contains(p.X) && rect.VerticalInterval.Contains(p.Y);
		}


		// Mutators

		public static Rectangle<IObservable<T>> With<T>(
			this Rectangle<IObservable<T>> self,
			IObservable<T> left = null,
			IObservable<T> top = null,
			IObservable<T> right = null,
			IObservable<T> bottom = null)
			where T : struct, INumeric<T>
		{
			return FromSides(
				left ?? self.Left(),
				top ?? self.Top(),
				right ?? self.Right(),
				bottom ?? self.Bottom());
		}

		public static Rectangle<T> With<T>(
			this Rectangle<T> self,
			T? left = null,
			T? top = null,
			T? right = null,
			T? bottom = null)
			where T : struct, INumeric<T>
		{
			return FromSides(
				left ?? self.Left(),
				top ?? self.Top(),
				right ?? self.Right(),
				bottom ?? self.Bottom());
		}
		/*
		public static Rectangle<T> With<T>(
			this Rectangle<T> self,
			T left = default(T),
			T top = default(T),
			T right = default(T),
			T bottom = default(T))
			where T : class
		{
			return FromSides(
				left ?? self.Left,
				top ?? self.Top,
				right ?? self.Right,
				bottom ?? self.Bottom);
		}

		public static Rectangle<T> With<T>(
			this Rectangle<T> self,
			Axis2D axis,
			T min = null,
			T max = null)
			where T : class
		{
			return self.WithAxis(axis, a => new Interval<T>(min ?? a.Min, max ?? a.Max));
		}

		public static Rectangle<T> With<T>(
			this Rectangle<T> self,
			Axis2D axis,
			Optional<T> min = default(Optional<T>),
			Optional<T> max = default(Optional<T>))
		{
			return self.WithAxis(axis, a => new Interval<T>(min.Or(a.Min), max.Or(a.Max)));
		}
		*/
		public static IObservable<Rectangle<T>> MoveTo<T>(this IObservable<Rectangle<T>> rect, IObservable<Point<T>> position) where T : INumeric<T>
		{
			return rect.CombineLatest(position, MoveTo);
		}

		public static Rectangle<IObservable<T>> MoveTo<T>(this Rectangle<IObservable<T>> rect, Point<IObservable<T>> position) where T : INumeric<T>
		{
			return FromIntervals(
				Interval.FromOffsetLength(rect.HorizontalInterval.Offset.Add(position.X), rect.Width),
				Interval.FromOffsetLength(rect.VerticalInterval.Offset.Add(position.Y), rect.Height));
		}

		public static Rectangle<T> MoveTo<T>(this Rectangle<T> rect, Point<T> position) where T : INumeric<T>
		{
			return FromPositionSize(
				Point.Create(
					rect.Position.X.Add(position.X),
					rect.Position.Y.Add(position.Y)),
				rect.Size);
		}

		public static Rectangle<T> WithEdge<T>(this Rectangle<T> rect, RectangleEdge edge, T location)
			where T : struct, INumeric<T>
		{
			switch (edge)
			{
				case RectangleEdge.Left:
					return rect.With(location, null, null, null);
				case RectangleEdge.Right:
					return rect.With(null, null, location, null);
				case RectangleEdge.Top:
					return rect.With(null, location, null, null);
				case RectangleEdge.Bottom:
					return rect.With(null, null, null, location);
			}
			throw new ArgumentException();
		}

		public static Rectangle<IObservable<T>> WithEdge<T>(this Rectangle<IObservable<T>> rect, RectangleEdge edge, IObservable<T> location)
			where T : struct, INumeric<T>
		{
			switch (edge)
			{
				case RectangleEdge.Left:
					return rect.With(location, null, null, null);
				case RectangleEdge.Right:
					return rect.With(null, null, location, null);
				case RectangleEdge.Top:
					return rect.With(null, location, null, null);
				case RectangleEdge.Bottom:
					return rect.With(null, null, null, location);
			}
			throw new ArgumentException();
		}

		public static Rectangle<T> WithAxis<T>(this Rectangle<T> rect, Axis2D axis, Func<Interval<T>, Interval<T>> interval)
		{
			return rect.WithAxis(axis, interval(rect[axis]));
		}

		public static Rectangle<T> WithAxis<T>(this Rectangle<T> rect, Axis2D axis, Interval<T> interval)
		{
			return axis == Axis2D.Horizontal
				? new Rectangle<T>(interval, rect.VerticalInterval)
				: new Rectangle<T>(rect.HorizontalInterval, interval);
		}

		public static IObservable<Point<T>> Position<T>(this IObservable<Rectangle<T>> rectangle)
		{
			return rectangle.Select(r => r.Position);
		}
		public static IObservable<Size<T>> Size<T>(this IObservable<Rectangle<T>> rectangle)
		{
			return rectangle.Select(r => r.Size);
		}

		public static Point<IObservable<Points>> Center(this Rectangle<IObservable<Points>> self)
		{
			return Point.Create(
				self.HorizontalInterval.Center(),
				self.VerticalInterval.Center());
		}

		public static Point<T> LeftTop<T>(this Rectangle<T> self)
		{
			return Point.Create(self.Left(), self.Top());
		}

		public static Point<IObservable<T>> LeftTop<T>(this Rectangle<IObservable<T>> self)
		{
			return Point.Create(self.Left(), self.Top());
		}

		public static Point<T> LeftBottom<T>(this Rectangle<T> self)
			where T : INumeric<T>
		{
			return Point.Create(self.Left(), self.Bottom());
		}

		public static Point<IObservable<T>> LeftBottom<T>(this Rectangle<IObservable<T>> self)
			where T : INumeric<T>
		{
			return Point.Create(self.Left(), self.Bottom());
		}

		public static Point<T> RightTop<T>(this Rectangle<T> self)
			where T : INumeric<T>
		{
			return Point.Create(self.Right(), self.Top());
		}

		public static Point<IObservable<T>> RightTop<T>(this Rectangle<IObservable<T>> self)
			where T : INumeric<T>
		{
			return Point.Create(self.Right(), self.Top());
		}

		public static Point<T> RightBottom<T>(this Rectangle<T> self)
			where T : INumeric<T>
		{
			return Point.Create(self.Right(), self.Bottom());
		}

		public static Point<IObservable<T>> RightBottom<T>(this Rectangle<IObservable<T>> self)
			where T : INumeric<T>
		{
			return Point.Create(self.Right(), self.Bottom());
		}

		public static T Left<T>(this Rectangle<T> self)
		{
			return self.HorizontalInterval.Offset;
		}

		public static T Top<T>(this Rectangle<T> self)
		{
			return self.VerticalInterval.Offset;
		}

		public static T Right<T>(this Rectangle<T> self)
			where T : INumeric<T>
		{
			return self.HorizontalInterval.Offset.Add(self.HorizontalInterval.Length);
		}

		public static IObservable<T> Right<T>(this Rectangle<IObservable<T>> self)
			where T : INumeric<T>
		{
			return self.HorizontalInterval.Offset.Add(self.HorizontalInterval.Length);
		}

		public static T Bottom<T>(this Rectangle<T> self)
			where T : INumeric<T>
		{
			return self.VerticalInterval.Offset.Add(self.VerticalInterval.Length);
		}

		public static IObservable<T> Bottom<T>(this Rectangle<IObservable<T>> self)
			where T : INumeric<T>
		{
			return self.VerticalInterval.Offset.Add(self.VerticalInterval.Length);
		}


		public static T GetEdge<T>(this Rectangle<T> self, RectangleEdge edge)
			where T : INumeric<T>
		{
			switch (edge)
			{
				case RectangleEdge.Left:
					return self.Left();
				case RectangleEdge.Top:
					return self.Top();
				case RectangleEdge.Right:
					return self.Right();
				case RectangleEdge.Bottom:
					return self.Bottom();
			}
			throw new ArgumentOutOfRangeException();
		}
		public static IObservable<T> GetEdge<T>(this Rectangle<IObservable<T>> self, RectangleEdge edge)
			where T : INumeric<T>
		{
			switch (edge)
			{
				case RectangleEdge.Left:
					return self.Left();
				case RectangleEdge.Top:
					return self.Top();
				case RectangleEdge.Right:
					return self.Right();
				case RectangleEdge.Bottom:
					return self.Bottom();
			}
			throw new ArgumentOutOfRangeException();
		}
	}

	public struct Rectangle<T> : IEquatable<Rectangle<T>>
	{
		public readonly Interval<T> HorizontalInterval;
		public readonly Interval<T> VerticalInterval;

		public Rectangle(Interval<T> horizontalInterval, Interval<T> verticalInterval)
		{
			HorizontalInterval = horizontalInterval;
			VerticalInterval = verticalInterval;
		}

		public Point<T> Position { get { return new Point<T>(HorizontalInterval.Offset, VerticalInterval.Offset); } }
		public Size<T> Size { get { return new Size<T>(Width, Height); } }

		public T Width { get { return HorizontalInterval.Length; } }
		public T Height { get { return VerticalInterval.Length; } }

		public override string ToString()
		{
			return "{Position = " + Position + ", Size = " + Size + "}";
		}

		public Interval<T> this[Axis2D axis]
		{
			get { return axis == Axis2D.Horizontal ? HorizontalInterval : VerticalInterval; }
		}


		public static bool operator ==(Rectangle<T> left, Rectangle<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Rectangle<T> left, Rectangle<T> right)
		{
			return !(left == right);
		}

		public bool Equals(Rectangle<T> other)
		{
			return HorizontalInterval.Equals(other.HorizontalInterval) && VerticalInterval.Equals(other.VerticalInterval);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Rectangle<T> && Equals((Rectangle<T>)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (HorizontalInterval.GetHashCode() * 397) ^ VerticalInterval.GetHashCode();
			}
		}
	}


}