using System;

namespace Outracks.Fusion
{
	public static partial class Layout
	{
		public static IControl DockTopLeft(this IControl dockedControl)
		{
			return dockedControl.Dock(RectangleEdge.Left).Dock(RectangleEdge.Top);
		}

		public static IControl DockTopRight(this IControl dockedControl)
		{
			return dockedControl.Dock(RectangleEdge.Right).Dock(RectangleEdge.Top);
		}

		public static IControl DockBottomLeft(this IControl dockedControl)
		{
			return dockedControl.Dock(RectangleEdge.Left).Dock(RectangleEdge.Bottom);
		}

		public static IControl DockBottomRight(this IControl dockedControl)
		{
			return dockedControl.Dock(RectangleEdge.Right).Dock(RectangleEdge.Bottom);
		}

		public static IControl DockLeft(this IControl dockedControl, IControl fill = null)
		{
			return dockedControl.Dock(RectangleEdge.Left, fill);
		}

		public static IControl DockRight(this IControl dockedControl, IControl fill = null)
		{
			return dockedControl.Dock(RectangleEdge.Right, fill);
		}

		public static IControl DockBottom(this IControl dockedControl, IControl fill = null)
		{
			return dockedControl.Dock(RectangleEdge.Bottom, fill);
		}

		public static IControl DockTop(this IControl dockedControl, IControl fill = null)
		{
			return dockedControl.Dock(RectangleEdge.Top, fill);
		}
		
		public static IControl Dock(this IControl dockedControl, RectangleEdge location, IControl fillControl = null)
		{
			return Dock(location, dockedControl, fillControl);
		}

		public static IControl Dock(RectangleEdge edge, IControl dockedControl, IControl fillControl = null)
		{
			return fillControl == null
				? dockedControl.WithFrame(old => DockArea(old, edge.FlipVerticallyOnMac(), dockedControl.DesiredSize))
				: Dock().Dock(dockedControl, edge).Fill(fillControl);
		}
		
		static Rectangle<IObservable<Points>> DockArea(
			Rectangle<IObservable<Points>> old,
			RectangleEdge edge,
			Size<IObservable<Points>> desiredSize)
		{
			var desiredLength = desiredSize[edge.NormalAxis()];

			return edge.NormalAxis() == Axis2D.Horizontal
				? Rectangle.FromIntervals(
					edge.IsMinimal() 
						? Interval.FromOffsetLength(old.Left(), desiredLength)
						: Interval.FromOffsetLength(old.Right().Sub(desiredLength), desiredLength),
					old.VerticalInterval)
				: Rectangle.FromIntervals(
					old.HorizontalInterval,
					edge.IsMinimal()
						? Interval.FromOffsetLength(old.Top(), desiredLength)
						: Interval.FromOffsetLength(old.Bottom().Sub(desiredLength), desiredLength));
		}
	}
}