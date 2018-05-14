using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class ObservableMath
	{
		public static Rectangle<IObservable<Points>> RectangleWithSize(Size<IObservable<Points>> size)
		{
			return Rectangle.FromPositionSize(Zero,Zero, size.Width, size.Height);
		}

		//public static IObservable<Points> Cycle(double from, double to)
		//{
		//	var len = (to - from);
		//	return Application.PerFrame.Select(t => new Points(Math.Sin(t * 100.0f) * len + len / 2.0));
		//}

		public static readonly IObservable<Points> Zero = Observable.Return(new Points(0));
		public static readonly Point<IObservable<Points>> ZeroPoint = Point.Create(Zero, Zero);
		public static readonly Size<IObservable<Points>> ZeroSize = Size.Create(Zero, Zero);
		public static readonly Rectangle<IObservable<Points>> ZeroRectangle = Rectangle.FromPositionSize(Zero, Zero, Zero, Zero);

		public static readonly IObservable<Points> Never = Observable.Never<Points>();
		public static readonly Point<IObservable<Points>> NeverPoint = Point.Create(Never, Never);
		public static readonly Size<IObservable<Points>> NeverSize = Size.Create(Never, Never);
		public static readonly Rectangle<IObservable<Points>> NeverRectangle = Rectangle.FromPositionSize(Never, Never, Never, Never);


	}
}