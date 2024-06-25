using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	class LayoutSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			element = element.As("Fuse.Elements.Element");

			var alignment = element.GetEnum("Alignment", Alignment.Default);
			var dock = element.GetEnum("Dock", Dock.Fill);
			var layoutRole = element.GetEnum("LayoutRole", LayoutRole.Standard);
			var layer = element.GetEnum("Layer", Layer.Layout);

			return Layout.StackFromTop(
				Separator.Weak,
				Spacer.Medium,

				AlignmentEditor.Create(alignment, editors).WithInspectorPadding(),

				Spacer.Medium, Separator.Weak,

				Layout.StackFromTop(
						Spacer.Medium,
						DockEditor.Create(dock, editors).WithInspectorPadding(),
						Spacer.Medium, Separator.Weak)
					.MakeCollapsable(RectangleEdge.Bottom, element.IsInDockPanelContext()),

				Spacer.Medium,

				SpacingSection.Create(element, editors),

				Spacer.Medium, Separator.Weak, Spacer.Medium,

				Layout.Dock()
					.Left(editors.Dropdown(layoutRole).WithLabelAbove("Layout Role"))
					.Right(editors.Dropdown(layer).WithLabelAbove("Layer"))
					.Fill()
					.WithInspectorPadding(),

				Spacer.Medium,
				Separator.Weak);
		}
	}

	enum Layer
	{
		Underlay,
		Background,
		Layout,
		Overlay,
	}

	enum LayoutRole
	{
		Standard,
		Placeholder,
		Inert,
		Independent,
	}
}
