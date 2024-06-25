using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	class TextInputSection
	{
		public static IControl Create(IProject project, IElement element, IEditorFactory editors)
		{
			var placeholder = element.GetString("PlaceholderText", "");
			var placeholdercolor = element.GetColor("PlaceholderColor", Color.Black);

			return Layout.StackFromTop(
					Spacer.Medium,

					TextSection.CreateEditors(project, element, editors)
						.WithInspectorPadding(),

					Spacer.Medium,

					editors.Field(placeholder)
						.WithLabel("Placeholder")
						.WithInspectorPadding(),

					Spacer.Medium,

					editors.Color(placeholdercolor)
						.WithLabel("Placeholder Color")
						.WithInspectorPadding(),

					Spacer.Medium)
				.MakeCollapsable(RectangleEdge.Bottom, element.Is("Fuse.Controls.TextInput"));
		}
	}
}
