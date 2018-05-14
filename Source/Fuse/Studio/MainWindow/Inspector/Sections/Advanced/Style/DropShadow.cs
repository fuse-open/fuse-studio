namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;

	static class DropShadowSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			return editors.ElementList(
				"Shadow", element,
				SourceFragment.FromString("<Shadow/>"),
				shadow => CreateShadowRow(shadow, editors)
					.WithInspectorPadding());
		}

		static IControl CreateShadowRow(IElement shadow, IEditorFactory editors)
		{
			var name = shadow.UxName();
			var angle = shadow.GetAngle("Angle", 90);
			var color = shadow.GetColor("Color", Color.Black);
			var distance = shadow.GetPoints("Distance", 0);
			var size = shadow.GetPoints("Size", 5);

			return Layout.StackFromTop(
				editors.NameRow("Shadow Name", name),

				Spacer.Medium,

				Layout.Dock()
					.Left(editors.Color(color).WithLabelAbove("Color"))
					// TODO: Changing from Background to PerPixel crashes fuselibs at runtime (09-05-2017)
					//.Right(editors.Dropdown(mode).WithLabelAbove("Mode"))
					.Right(editors.Field(distance).WithLabelAbove("Distance").WithWidth(CellLayout.HalfCellWidth))
					.Right(Spacer.Medium)
					.Right(editors.Field(size).WithLabelAbove("Size").WithWidth(CellLayout.HalfCellWidth))
					.Fill(Spacer.Medium),

				Spacer.Medium,
				Layout.Dock()
					.Left(editors.Label("Angle", angle))
					.Left(Spacer.Medium)
					.Right(editors.Field(angle).WithWidth(CellLayout.HalfCellWidth))
					.Right(Spacer.Medium)
					.Fill(editors.Slider(angle, min: 0.0, max: 360.0)));
		}

		enum ShadowMode
		{
			PerPixel,
			Background
		}
	}
}