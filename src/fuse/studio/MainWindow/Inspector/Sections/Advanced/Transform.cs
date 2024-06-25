using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	public class TransformSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			element = element.As("Fuse.Elements.Element");

			var snapMin = element.GetBoolean("SnapMinTransform", false);
			var snapMax = element.GetBoolean("SnapMaxTransform", false);
			var origin = element.GetEnum("TransformOrigin", TransformOrigin.Center);

			return Layout.StackFromTop(
				Separator.Weak,
				Spacer.Medium,

				editors.Dropdown(origin).WithLabel("Transform Origin")
					.WithInspectorPadding(),

				Spacer.Medium,

				Layout.StackFromTop(
						editors.Switch(snapMin).WithLabel("Snap to Min Transform"),
						Spacer.Medium,
						editors.Switch(snapMax).WithLabel("Snap to Max Transform"),
						Spacer.Medium)
					.MakeCollapsable(RectangleEdge.Top, element.Is("Fuse.Controls.ScrollViewBase"))
					.WithInspectorPadding(),

				RotationSection.Create(element, editors),
				Separator.Weak);
		}
	}

	// Not really an enum, but from UX it looks like one so this should be fine
	enum TransformOrigin
	{
		Anchor,
		Center,
		HorizontalBoxCenter,
		TopLeft,
		VerticalBoxCenter,
	}
}
