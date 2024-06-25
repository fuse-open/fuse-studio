using Outracks.Fusion;

namespace Outracks.Fuse.Studio.Inspector
{
	public static class TextAlignmentIcon
	{
		public static IControl Create(TextAlignment alignment, Brush fill)
		{
			return Layout.StackFromTop(
				Control.Empty.WithHeight(1).WithWidth(15),
				Align(Shapes.Rectangle(fill: fill).WithWidth(13).WithHeight(1), alignment),
				Control.Empty.WithHeight(1),
				Align(Shapes.Rectangle(fill: fill).WithWidth(11).WithHeight(1), alignment),
				Control.Empty.WithHeight(1),
				Align(Shapes.Rectangle(fill: fill).WithWidth(13).WithHeight(1), alignment),
				Control.Empty.WithHeight(1),
				Align(Shapes.Rectangle(fill: fill).WithWidth(7).WithHeight(1), alignment),
				Control.Empty.WithHeight(1),
				Align(Shapes.Rectangle(fill: fill).WithWidth(11).WithHeight(1), alignment))
				.WithPadding(left: new Points(1), right: new Points(1));
		}

		static IControl Align(IControl self, TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Left:
					return self.Dock(RectangleEdge.Left);
				case TextAlignment.Right:
					return self.Dock(RectangleEdge.Right);
				default:
					return self.CenterHorizontally();
			}
		}
	}
}