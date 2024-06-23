using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	static class MainWindowIcons
	{
		public static IControl DockIcon(RectangleEdge edge, Brush brush)
		{
			return Shapes.Rectangle(stroke: Stroke.Create(1, brush))
				.WithOverlay(Shapes.Rectangle(fill: brush)
					.WithHeight(2)
					.WithWidth(2)
					.WithPadding(new Thickness<Points>(2))
					.Dock(edge))
				.WithWidth(20)
				.WithHeight(16)
				.Center();
		}

		public static IControl WindowIcon(Brush brush, Brush bg)
		{
			var front =
				Shapes.Rectangle(fill: bg, stroke: Stroke.Create(1, brush))
					.WithSize(new Size<Points>(16, 11))
					.DockRight()
					.DockTop();

			var back =
				Shapes.Rectangle(stroke: Stroke.Create(1, brush))
					.WithSize(new Size<Points>(16, 11))
					.DockLeft()
					.DockBottom();

			return Control.Empty.WithSize(new Size<Points>(20, 16))
				.WithOverlay(back)
				.WithOverlay(front)
				.Center();
		}

		public static IControl CloseIcon(Points height, Stroke stroke)
		{
			return Layout.Layer(
				Shapes.Line(
					new Point<Points>(0, 0),
					new Point<Points>(height, height),
					stroke: stroke),
				Shapes.Line(
					new Point<Points>(0, height),
					new Point<Points>(height, 0),
					stroke: stroke))
				.WithSize(new Size<Points>(height, height));
		}


		public static IControl AddIcon(Stroke stroke)
		{
			var width = 17.0;
			return Layout.Layer(
				Shapes.Line(
					new Point<Points>(width/2, 0),
					new Point<Points>(width/2, width),
					stroke: stroke),
				Shapes.Line(
					new Point<Points>(0, width/2),
					new Point<Points>(width, width/2),
					stroke: stroke))
				.WithSize(new Size<Points>(width, width));
		}
	}
}