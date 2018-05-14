using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class Shapes
	{
		public static IControl Rectangle(Stroke stroke = null, Brush fill = default(Brush), IObservable<CornerRadius> cornerRadius = null)
		{
			return Implementation.RectangleFactory(stroke ?? Stroke.Empty, fill, cornerRadius.ToOptional());
		}

		public static IControl Circle(Stroke stroke = null, Brush fill = default(Brush))
		{
			return Implementation.CircleFactory(stroke ?? Stroke.Empty, fill);
		}

		public static IControl Line(Point<Points> start, Point<Points> end, Stroke stroke)
		{
			return Line(start.Select(Observable.Return), end.Select(Observable.Return), stroke);
		}

		public static IControl Line(Point<IObservable<Points>> start, Point<IObservable<Points>> end, Stroke stroke)
		{
			return Implementation.LineFactory(start, end, stroke);
		}

		public static class Implementation
		{
			public static Func<Stroke, Brush, IControl> CircleFactory;

			public static Func<Stroke, Brush, Optional<IObservable<CornerRadius>>, IControl> RectangleFactory;

			public static Func<Point<IObservable<Points>>, Point<IObservable<Points>>, Stroke, IControl> LineFactory;
		}
	}
}