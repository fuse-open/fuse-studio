namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;

	class VisibilitySection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			element = element.As("Fuse.Elements.Element");

			var visibility = element.GetEnum("Visibility", Visibility.Visible);
			var hitTestMode = element.GetEnum("HitTestMode", HitTestMode.None);

			return Layout.StackFromTop(
				Separator.Weak,
				Spacer.Medium,
				editors.Dropdown(visibility).WithLabel("Visibility")
					.WithInspectorPadding(),
				Spacer.Medium,
				editors.Dropdown(hitTestMode).WithLabel("Hit Test Mode")
					.WithInspectorPadding(),
				Spacer.Medium,
				Separator.Weak);
		}
	}

	enum HitTestMode
	{
		None,
		LocalVisual,
		LocalBounds,
		Children,
		LocalVisualAndChildren,
		LocalBoundsAndChildren,
	}

	enum Visibility
	{
		Visible, 
		Hidden, 
		Collapsed
	}
}
