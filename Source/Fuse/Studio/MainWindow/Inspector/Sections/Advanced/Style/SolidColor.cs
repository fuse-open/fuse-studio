namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;

	static class SolidColorSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			return editors.ElementList(
				"Solid Color", element, 
				SourceFragment.FromString("<SolidColor/>"),
				solidColor => CreateSolidColorRow(solidColor, editors)
					.WithInspectorPadding());
		}

		static IControl CreateSolidColorRow(IElement solidColor, IEditorFactory editors)
		{
			var name = solidColor.UxName();
			var color = solidColor.GetColor("Color", Color.White);
			var opacity = solidColor.GetDouble("Opacity", 1.0);
			//var mode = solidColor.GetEnum("BlendMode", BlendMode.Normal);
			
			return Layout.StackFromTop(
				editors.NameRow("Brush Name", name),

				Spacer.Medium,

				Layout.Dock()
					.Left(editors.Color(color).WithLabelAbove("Color"))
					//.Right(editors.Dropdown(mode).WithLabelAbove("Blend Mode"))
					.Fill(Spacer.Medium),

				Spacer.Medium,

				Layout.Dock()
					.Left(editors.Label("Opacity", opacity))
					.Left(Spacer.Medium)
					.Right(editors.Field(opacity).WithWidth(CellLayout.HalfCellWidth))
					.Right(Spacer.Medium)
					.Fill(editors.Slider(opacity, min: 0.0, max: 1.0)));
		}
	}

	enum BlendMode
	{
		Normal,
		Add,
		Screen,
		Multiply
	}
}