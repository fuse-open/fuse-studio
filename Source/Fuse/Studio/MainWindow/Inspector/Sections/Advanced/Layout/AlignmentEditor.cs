using Outracks.Fuse.Inspector.Editors;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	enum Alignment
	{
		Default,
		Left,
		HorizontalCenter,
		Right,
		Top,
		VerticalCenter,
		Bottom,
		TopLeft,
		TopCenter,
		TopRight,
		CenterLeft,
		Center,
		CenterRight,
		BottomLeft,
		BottomCenter,
		BottomRight,
	}

	class AlignmentEditor
	{
		public static IEditorControl Create(IAttribute<Alignment> attribute, IEditorFactory editors)
		{
			var smallPadding = Optional.Some(new Points(5.0));
			var largePadding = Optional.Some(new Points(16.0));

			return new EditorControl<Alignment>(
				editors,
				attribute,

				Layout.StackFromTop(
					Layout.StackFromLeft(
						CustomRadioButton.Create(
							attribute,
							Alignment.Default,
							"Alignment: Default",
							(backgroundColor, color, stroke) =>
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color))
						.WithPadding(right: smallPadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.Center,
							"Alignment: Center",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								CustomRadioButton.CreateSmallRect(color).Center()))
						.WithPadding(right: largePadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.Bottom,
							"Alignment: Bottom",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								Shapes.Rectangle(fill: color)
								.WithSize(new Size<Points>(CustomRadioButton.ButtonDim, CustomRadioButton.SmallRectSize.Height)).DockBottom()))
						.WithPadding(right: smallPadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.Top,
							"Alignment: Top",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								Shapes.Rectangle(fill: color)
								.WithSize(new Size<Points>(CustomRadioButton.ButtonDim, CustomRadioButton.SmallRectSize.Height)).DockTop()))
						.WithPadding(right: largePadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.Left,
							"Alignment: Left",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								Shapes.Rectangle(fill: color)
								.WithSize(new Size<Points>(CustomRadioButton.SmallRectSize.Height, CustomRadioButton.ButtonDim)).DockLeft()))
						.WithPadding(right: smallPadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.Right,
							"Alignment: Right",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								Shapes.Rectangle(fill: color)
								.WithSize(new Size<Points>(CustomRadioButton.SmallRectSize.Height, CustomRadioButton.ButtonDim)).DockRight()))
						.WithPadding(right: largePadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.HorizontalCenter,
							"Alignment: Horizontal center",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								Shapes.Line(
									new Point<Points>(0.0, 0.0),
									new Point<Points>(0.0, CustomRadioButton.ButtonDim - 4.0),
									stroke)
								.WithSize(new Size<Points>(1.0, CustomRadioButton.ButtonDim - 4.0)).Center(),
								CustomRadioButton.CreateSmallRect(color).Center()))
						.WithPadding(right: smallPadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.VerticalCenter,
							"Alignment: Vertical center",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								Shapes.Line(
									new Point<Points>(0.0, 0.0),
									new Point<Points>(CustomRadioButton.ButtonDim - 4.0, 0.0),
									stroke)
								.WithSize(new Size<Points>(CustomRadioButton.ButtonDim - 4.0, 1.0)).Center(),
								CustomRadioButton.CreateSmallRect(color).Center())))
					.WithPadding(bottom: new Points(9.0)),

					Layout.StackFromLeft(
						CustomRadioButton.Create(
							attribute,
							Alignment.BottomCenter,
							"Alignment: Bottom center",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								CustomRadioButton.CreateSmallRect(color).DockBottom().CenterHorizontally()))
						.WithPadding(right: smallPadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.TopCenter,
							"Alignment: Top center",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								CustomRadioButton.CreateSmallRect(color).DockTop().CenterHorizontally()))
						.WithPadding(right: largePadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.CenterLeft,
							"Alignment: Center left",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								CustomRadioButton.CreateSmallRect(color).DockLeft().CenterVertically()))
						.WithPadding(right: smallPadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.CenterRight,
							"Alignment: Center right",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								CustomRadioButton.CreateSmallRect(color).DockRight().CenterVertically()))
						.WithPadding(right: largePadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.BottomLeft,
							"Alignment: Bottom left",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								CustomRadioButton.CreateSmallRect(color).DockBottomLeft()))
						.WithPadding(right: smallPadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.BottomRight,
							"Alignment: Bottom right",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								CustomRadioButton.CreateSmallRect(color).DockBottomRight()))
						.WithPadding(right: largePadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.TopLeft,
							"Alignment: Top left",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								CustomRadioButton.CreateSmallRect(color).DockTopLeft()))
						.WithPadding(right: smallPadding),

						CustomRadioButton.Create(
							attribute,
							Alignment.TopRight,
							"Alignment: Top right",
							(backgroundColor, color, stroke) => Layout.Layer(
								CustomRadioButton.CreateBackgroundRect(backgroundColor, color),
								CustomRadioButton.CreateSmallRect(color).DockTopRight()))))
				.CenterHorizontally()
				.WithBackground(editors.ExpressionButton(attribute).DockRight()));
		}
	}
}
