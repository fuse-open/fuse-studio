using System;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	public enum SymbolSize
	{
		Small,
		Medium,
		Large,
		Contain,
	}

	public static class Arrow
	{
		public static IControl WithShaft(RectangleEdge edge, SymbolSize size = SymbolSize.Contain, Brush brush = default(Brush))
		{
			var stroke = Stroke.Create(1.0, brush | Theme.Active);

			return ResizeSymbol(edge, size, Control.BindNativeFrame(frame =>
				WithoutShaft(edge, brush, frame)
					.WithOverlay(
						edge.NormalAxis() == Axis2D.Horizontal
							? Shapes.Line(
								Point.Create(frame.Left(), frame.Center().Y),
								Point.Create(frame.Right(), frame.Center().Y),
								stroke)
							: Shapes.Line(
								Point.Create(frame.Center().X, frame.Top()),
								Point.Create(frame.Center().X, frame.Bottom()),
								stroke))))
			.WithBackground(Color.Transparent); // TODO: not sure why this is needed
		}

		public static IControl WithoutShaft(RectangleEdge edge, SymbolSize size = SymbolSize.Contain, Brush brush = default(Brush))
		{
			return ResizeSymbol(edge, size, Control.BindNativeFrame(frame =>
				WithoutShaft(edge, brush, frame)))
			.WithBackground(Color.Transparent); // TODO: not sure why this is needed
		}

		static IControl ResizeSymbol(RectangleEdge edge, SymbolSize size, IControl control)
		{
			switch (size)
			{
				case SymbolSize.Small:
					return control.WithSize(edge.NormalAxis() == Axis2D.Horizontal
						? Size.Create<Points>(4, 7)
						: Size.Create<Points>(7, 4));
				case SymbolSize.Medium:
					return control.WithSize(edge.NormalAxis() == Axis2D.Horizontal
						? Size.Create<Points>(5, 9)
						: Size.Create<Points>(9, 5));
				case SymbolSize.Large:
					return control.WithSize(edge.NormalAxis() == Axis2D.Horizontal
						? Size.Create<Points>(7, 13)
						: Size.Create<Points>(13, 7));
				default:
					return control;
			}
		}

		static IControl WithoutShaft(RectangleEdge edge, Brush brush, Rectangle<IObservable<Points>> frame)
		{
			var stroke = Stroke.Create(1.0, brush | Theme.Active);

			var to = edge.NormalAxis() == Axis2D.Horizontal
				? Point.Create(frame.GetEdge(edge), frame.Center().Y)
				: Point.Create(frame.Center().X, frame.GetEdge(edge));

			var width = frame[edge.NormalAxis().Opposite()].Length.Mul(0.5);

			var bottom = Vector.Create(width, width);
			switch (edge)
			{
				case RectangleEdge.Bottom:
					bottom = bottom.RotateCCV();
					break;
				case RectangleEdge.Right:
					bottom = bottom.RotateCCV().RotateCCV();
					break;
				case RectangleEdge.Top:
					bottom = bottom.RotateCCV().RotateCCV().RotateCCV();
					break;
			}

			var top = bottom.RotateCCV();

			return Layout.Layer(
				Shapes.Line(to, to + top, stroke),
				Shapes.Line(to, to + bottom, stroke));
		}
	}
}