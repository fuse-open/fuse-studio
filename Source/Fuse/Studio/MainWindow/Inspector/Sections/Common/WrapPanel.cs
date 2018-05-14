using Outracks.Fuse.Designer.Inspector;

namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;
	
	class WrapPanelSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			var orientation = element.GetEnum("Orientation", Orientation.Vertical);
			var flowDirection = element.GetEnum("FlowDirection", FlowDirection.LeftToRight);

			return Layout.StackFromTop(
					Spacer.Small,
					
					Layout.StackFromLeft(
						editors.RadioButton(orientation)
							.Option(Orientation.Horizontal, (fg, bg) => StackIcon.Create(Axis2D.Horizontal, fg), "Orientation: Horizontal")
							.Option(Orientation.Vertical, (fg, bg) => StackIcon.Create(Axis2D.Vertical, fg), "Orientation: Vertical")
							.Control.WithLabelAbove("Orientation"),
						Spacer.Medium,
						editors.Dropdown(flowDirection)
							.WithLabelAbove("Flow Direction"))
						.WithInspectorPadding(),
					
					Spacer.Medium)
				.MakeCollapsable(RectangleEdge.Bottom, element.Is("Fuse.Controls.WrapPanel"));
		}
	}

	enum FlowDirection
	{
		LeftToRight,
		RightToLeft,
	}
}
