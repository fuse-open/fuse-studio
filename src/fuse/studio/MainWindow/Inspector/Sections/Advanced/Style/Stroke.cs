using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	static class StrokeSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			return editors.ElementList(
				"Stroke", element,
				SourceFragment.FromString("<Stroke Width=\"3\" Color=\"#f00\"/>"),
				stroke => CreateStrokeRow(stroke, editors)
					.WithInspectorPadding())
				.MakeCollapsable(RectangleEdge.Top, element.Is("Fuse.Controls.Shape"));
		}

		static IControl CreateStrokeRow(IElement stroke, IEditorFactory editors)
		{
			var name = stroke.UxName();
			var color = stroke.GetColor("Color", Color.Black);
			var alignment = stroke.GetEnum("Alignment", StrokeAlignment.Inside);
			var width = stroke.GetPoints("Width", 1.0);

			return Layout.StackFromTop(
				editors.NameRow("Stroke Name", name),

				Spacer.Medium,

				Layout.Dock()
					.Left(editors.Color(color).WithLabelAbove("Color"))
					.Left(Spacer.Small)
					.Right(
						editors.Field(width).WithLabelAbove("Width")
							.WithWidth(CellLayout.HalfCellWidth))
					.Right(Spacer.Small)
					.Fill(editors.Dropdown(alignment).WithLabelAbove("Alignment")));
		}
	}

	enum StrokeAlignment
	{
		Center,
		Inside,
		Outside,
	}
}