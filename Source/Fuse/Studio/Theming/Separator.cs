using System.Reactive.Linq;

namespace Outracks.Fuse
{
	using Fusion;

	public static class Separator
	{
		public static Stroke WeakStroke
		{
			get { return Stroke.Create(1, Theme.WeakLineBrush); }
		}
		
		public static Stroke MediumStroke
		{
			get { return Stroke.Create(1, Theme.LineBrush); }
		}

		public static IControl Weak
		{
			get { return Line(WeakStroke); }
		}
		public static IControl Shadow
		{
			get { return Layout.StackFromTop( 
				Line(MediumStroke),
				Line(WeakStroke)); }
		}


		public static IControl Medium
		{
			get { return Line(MediumStroke); }
		}

		public static IControl Field
		{
			get { return Line(Theme.FieldStroke); }
		}

		public static IControl HorizontalLine(Brush brush, Points thickness)
		{
			return Shapes.Rectangle(fill: brush).WithHeight(thickness).WithWidth(thickness);
		}

		public static IControl Line(Stroke stroke)
		{
			return Shapes.Rectangle(fill: stroke.Brush)
				.WithWidth(stroke.Thickness.Select(p => new Points(p)))
				.WithHeight(stroke.Thickness.Select(p => new Points(p)));
		}
	}
}