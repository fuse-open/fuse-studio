using Outracks.Fusion;

namespace Outracks.Fuse
{
	static class AlignmentIcon
	{
		public static IControl VerticalFill(Brush fill)
		{
			return Layout.StackFromLeft(
				Control.Empty.WithWidth(3),
				Shapes.Rectangle(fill: fill).WithWidth(7).WithHeight(13),
				Control.Empty.WithWidth(1),
				Shapes.Rectangle(fill: fill).WithWidth(3).WithHeight(13),
				Control.Empty.WithWidth(3))
				.WithPadding(top: new Points(2), bottom: new Points(2))
				.WithOverlay(Shapes.Rectangle(fill: fill).WithHeight(1).Dock(RectangleEdge.Top))
				.WithOverlay(Shapes.Rectangle(fill: fill).WithHeight(1).Dock(RectangleEdge.Bottom))
				.WithPadding(new Thickness<Points>(1));
		}

		public static IControl HorizontalFill(Brush fill)
		{
			return Layout.StackFromTop(
				Control.Empty.WithHeight(3),
				Shapes.Rectangle(fill: fill).WithHeight(7).WithWidth(13),
				Control.Empty.WithHeight(1),
				Shapes.Rectangle(fill: fill).WithHeight(3).WithWidth(13),
				Control.Empty.WithHeight(3))
				.WithPadding(left: new Points(2), right: new Points(2))
				.WithOverlay(Shapes.Rectangle(fill: fill).WithWidth(1).Dock(RectangleEdge.Left))
				.WithOverlay(Shapes.Rectangle(fill: fill).WithWidth(1).Dock(RectangleEdge.Right))
				.WithPadding(new Thickness<Points>(1));
		}

		public static IControl Vertical(RectangleEdge edge, Brush fill)
		{
			return Layout.StackFromLeft(
				Control.Empty.WithWidth(3),
				Shapes.Rectangle(fill: fill).WithWidth(7).WithHeight(7).Dock(edge, Control.Empty.WithHeight(13-7)),
				Control.Empty.WithWidth(1),
				Shapes.Rectangle(fill: fill).WithWidth(3).WithHeight(13),
				Control.Empty.WithWidth(3))
				.WithPadding(top: new Points(2), bottom: new Points(2))
				.WithOverlay(Shapes.Rectangle(fill: fill).WithHeight(1).Dock(edge))
				.WithPadding(new Thickness<Points>(1));
		}

		public static IControl Horizontal(RectangleEdge edge, Brush fill)
		{
			return Layout.StackFromTop(
				Control.Empty.WithHeight(3),
				Shapes.Rectangle(fill: fill).WithHeight(7).WithWidth(7).Dock(edge, Control.Empty.WithWidth(13 - 7)),
				Control.Empty.WithHeight(1),
				Shapes.Rectangle(fill: fill).WithHeight(3).WithWidth(13),
				Control.Empty.WithHeight(3))
				.WithPadding(left: new Points(2), right: new Points(2))
				.WithOverlay(Shapes.Rectangle(fill: fill).WithWidth(1).Dock(edge))
				.WithPadding(new Thickness<Points>(1));
		}

		public static IControl VerticalCenter(Brush fill)
		{
			return Layout.StackFromLeft(
				Control.Empty.WithWidth(3),
				Shapes.Rectangle(fill: fill).WithWidth(7).WithHeight(7).CenterVertically(),
				Control.Empty.WithWidth(1),
				Shapes.Rectangle(fill: fill).WithWidth(3).WithHeight(13).CenterVertically(),
				Control.Empty.WithWidth(3))
				.WithPadding(top: new Points(2), bottom: new Points(2))
				.WithOverlay(Shapes.Rectangle(fill: fill).WithHeight(1).CenterVertically())
				.WithPadding(new Thickness<Points>(1));
		}

		public static IControl HorizontalCenter(Brush fill)
		{
			return Layout.StackFromTop(
				Control.Empty.WithHeight(3),
				Shapes.Rectangle(fill: fill).WithHeight(7).WithWidth(7).CenterHorizontally(),
				Control.Empty.WithHeight(1),
				Shapes.Rectangle(fill: fill).WithHeight(3).WithWidth(13).CenterHorizontally(),
				Control.Empty.WithHeight(3))
				.WithPadding(left: new Points(2), right: new Points(2))
				.WithOverlay(Shapes.Rectangle(fill: fill).WithWidth(1).CenterHorizontally())
				.WithPadding(new Thickness<Points>(1));
		}
	}
}