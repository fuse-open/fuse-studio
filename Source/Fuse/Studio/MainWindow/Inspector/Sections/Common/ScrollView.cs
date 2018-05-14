namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;

	class ScrollViewSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			var scrollDirection = element.GetEnum("AllowedScrollDirections", ScrollDirections.Vertical);

			return Layout.StackFromTop(
					Spacer.Medium,

					editors.Dropdown(scrollDirection)
						.WithLabel("Scroll Direction")
						.WithInspectorPadding(),
					
					Spacer.Medium)
				.MakeCollapsable(RectangleEdge.Bottom, element.Is("Fuse.Controls.ScrollView"));
		}
	}

	public enum ScrollDirections
	{
		Left,
		Right,
		Up,
		Down,
		Horizontal,
		Vertical,
		Both,
		All
	}
}
