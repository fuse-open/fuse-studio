namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;
	
	class StyleSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			element = element.As("Fuse.Elements.Element");

			var color = element.GetColor("Color", Color.White);
			var background = element.GetColor("Background", new Color(1, 1, 1, 1));
			var opacity = element.GetDouble("Opacity", 1.0);
	
			return Layout.StackFromTop(
				Separator.Weak,
				Spacer.Medium,

				Layout.Dock()
					.Left(editors.Color(color).WithLabelAbove("Color"))
					.Right(editors.Color(background).WithLabelAbove("Background"))
					.Fill()
					.WithInspectorPadding(),

				Spacer.Medium, Separator.Weak, Spacer.Medium,

				Layout.Dock()
					.Left(editors.Label("Opacity", opacity))
					.Left(Spacer.Medium)
					.Right(editors.Field(opacity).WithWidth(CellLayout.HalfCellWidth))
					.Right(Spacer.Small)
					.Fill(editors.Slider(opacity, min: 0.0, max: 1.0))
					.WithInspectorPadding(),
				
				Spacer.Medium,
				Separator.Weak,
				Spacer.Medium,
				Label.Create(
					"Effects are added as \n" +
					"children of the current element",
					font: Theme.DefaultFont,
					color: Theme.DescriptorText,
					textAlignment: TextAlignment.Center),
				Spacer.Medium,
				StrokeSection.Create(element, editors),
				LinearGradientSection.Create(element, editors),
				SolidColorSection.Create(element, editors),
				DropShadowSection.Create(element, editors),
				Separator.Weak);
		}
	}
}
