using System;

namespace Outracks.Fusion
{

	public static partial class Layout
	{
		public static IControl Center(this IControl control)
		{
			return control.CenterHorizontally().CenterVertically();
		}

		public static IControl CenterHorizontally(this IControl control)
		{
			return Center(control, Axis2D.Horizontal);
		}

		public static IControl CenterVertically(this IControl control)
		{
			return Center(control, Axis2D.Vertical);
		}

		public static IControl Center(this IControl node, Axis2D axis)
		{
			return node.WithFrame(frame => 
				frame.WithAxis(axis, a => a.ShrinkTo(node.DesiredSize[axis])));
		}

		static Interval<IObservable<T>> ShrinkTo<T>(this Interval<IObservable<T>> interval, IObservable<T> length) where T : INumeric<T>, new()
		{
			return Interval.FromOffsetLength(
				offset: interval.Offset.Add(interval.Length.Sub(length).Div(2)).Round(),
				length: length);
		}
	}


}