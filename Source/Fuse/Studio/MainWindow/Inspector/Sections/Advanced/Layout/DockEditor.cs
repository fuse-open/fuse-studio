using Outracks.Fuse.Inspector.Editors;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	enum Dock
	{
		Left,
		Right,
		Top,
		Bottom,
		Fill,
	}

	class DockEditor
	{
		public static IEditorControl Create(IAttribute<Dock> attribute, IEditorFactory editors)
		{
			var padding = Optional.Some(new Points(18.0));

			var diagonalArrowCenterMargin = 1.0;
			var diagonalArrowEdgeMargin = 4.0;
			var diagonalArrowTickOffset = 3.0;

			var straightArrowLength = 10.0;
			var straightArrowHalfLength = straightArrowLength / 2.0;
			var straightArrowTickOffset = 2.0;

			return new EditorControl<Dock>(
				editors,
				attribute,

				Layout.StackFromLeft(
					CustomRadioButton.Create(
						attribute,
						Dock.Fill,
						"Dock: Fill",
						(backgroundColor, color, stroke) => Layout.Layer(
							CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
							Shapes.Rectangle(fill: color)
								.WithSize(new Size<Points>(CustomRadioButton.ButtonDim - 4.0, CustomRadioButton.ButtonDim - 4.0)).Center(),
							// Upper-left arrow
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - diagonalArrowCenterMargin, CustomRadioButton.ButtonDim / 2.0 - diagonalArrowCenterMargin),
								new Point<Points>(diagonalArrowEdgeMargin, diagonalArrowEdgeMargin),
								stroke),
							Shapes.Line(
								new Point<Points>(diagonalArrowEdgeMargin, diagonalArrowEdgeMargin),
								new Point<Points>(diagonalArrowEdgeMargin + diagonalArrowTickOffset, diagonalArrowEdgeMargin),
								stroke),
							Shapes.Line(
								new Point<Points>(diagonalArrowEdgeMargin, diagonalArrowEdgeMargin),
								new Point<Points>(diagonalArrowEdgeMargin, diagonalArrowEdgeMargin + diagonalArrowTickOffset),
								stroke),
							// Upper-right arrow
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + diagonalArrowCenterMargin, CustomRadioButton.ButtonDim / 2.0 - diagonalArrowCenterMargin),
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin, diagonalArrowEdgeMargin),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin, diagonalArrowEdgeMargin),
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin - diagonalArrowTickOffset, diagonalArrowEdgeMargin),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin, diagonalArrowEdgeMargin),
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin, diagonalArrowEdgeMargin + diagonalArrowTickOffset),
								stroke),
							// Lower-left arrow
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - diagonalArrowCenterMargin, CustomRadioButton.ButtonDim / 2.0 + diagonalArrowCenterMargin),
								new Point<Points>(diagonalArrowEdgeMargin, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin),
								stroke),
							Shapes.Line(
								new Point<Points>(diagonalArrowEdgeMargin, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin),
								new Point<Points>(diagonalArrowEdgeMargin + diagonalArrowTickOffset, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin),
								stroke),
							Shapes.Line(
								new Point<Points>(diagonalArrowEdgeMargin, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin),
								new Point<Points>(diagonalArrowEdgeMargin, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin - diagonalArrowTickOffset),
								stroke),
							// Lower-right arrow
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + diagonalArrowCenterMargin, CustomRadioButton.ButtonDim / 2.0 + diagonalArrowCenterMargin),
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin),
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin - diagonalArrowTickOffset, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin),
								new Point<Points>(CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin, CustomRadioButton.ButtonDim - diagonalArrowEdgeMargin - diagonalArrowTickOffset),
								stroke)))
					.WithPadding(right: padding),

					CustomRadioButton.Create(
						attribute,
						Dock.Left,
						"Dock: Left",
						(backgroundColor, color, stroke) => Layout.Layer(
							CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
							Shapes.Rectangle(fill: color)
								.WithSize(new Size<Points>(CustomRadioButton.SmallRectSize.Height, CustomRadioButton.ButtonDim)).DockLeft(),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength, CustomRadioButton.ButtonDim / 2.0),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength, CustomRadioButton.ButtonDim / 2.0),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength, CustomRadioButton.ButtonDim / 2.0),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength + straightArrowTickOffset, CustomRadioButton.ButtonDim / 2.0 - straightArrowTickOffset),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength, CustomRadioButton.ButtonDim / 2.0),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength + straightArrowTickOffset, CustomRadioButton.ButtonDim / 2.0 + straightArrowTickOffset),
								stroke)))
					.WithPadding(right: padding),

					CustomRadioButton.Create(
						attribute,
						Dock.Bottom,
						"Dock: Bottom",
						(backgroundColor, color, stroke) => Layout.Layer(
							CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
							Shapes.Rectangle(fill: color)
								.WithSize(new Size<Points>(CustomRadioButton.ButtonDim, CustomRadioButton.SmallRectSize.Height)).DockBottom(),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0, CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0, CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0, CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - straightArrowTickOffset, CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength - straightArrowTickOffset),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0, CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + straightArrowTickOffset, CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength - straightArrowTickOffset),
								stroke)))
					.WithPadding(right: padding),

					CustomRadioButton.Create(
						attribute,
						Dock.Top,
						"Dock: Top",
						(backgroundColor, color, stroke) => Layout.Layer(
							CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
							Shapes.Rectangle(fill: color)
								.WithSize(new Size<Points>(CustomRadioButton.ButtonDim, CustomRadioButton.SmallRectSize.Height)).DockTop(),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0, CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0, CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0, CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - straightArrowTickOffset, CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength + straightArrowTickOffset),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0, CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + straightArrowTickOffset, CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength + straightArrowTickOffset),
								stroke)))
					.WithPadding(right: padding),

					CustomRadioButton.Create(
						attribute,
						Dock.Right,
						"Dock: Right",
						(backgroundColor, color, stroke) => Layout.Layer(
							CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
							Shapes.Rectangle(fill: color)
								.WithSize(new Size<Points>(CustomRadioButton.SmallRectSize.Height, CustomRadioButton.ButtonDim)).DockRight(),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 - straightArrowHalfLength, CustomRadioButton.ButtonDim / 2.0),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength, CustomRadioButton.ButtonDim / 2.0),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength, CustomRadioButton.ButtonDim / 2.0),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength - straightArrowTickOffset, CustomRadioButton.ButtonDim / 2.0 - straightArrowTickOffset),
								stroke),
							Shapes.Line(
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength, CustomRadioButton.ButtonDim / 2.0),
								new Point<Points>(CustomRadioButton.ButtonDim / 2.0 + straightArrowHalfLength - straightArrowTickOffset, CustomRadioButton.ButtonDim / 2.0 + straightArrowTickOffset),
								stroke))))
				.CenterHorizontally()
				.WithBackground(editors.ExpressionButton(attribute).DockRight()));
		}
	}
}
