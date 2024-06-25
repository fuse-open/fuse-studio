using Outracks.Fuse.Studio.Inspector;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	class StackPanelSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			var orientation = element.GetEnum("Orientation", Orientation.Vertical);
			var itemSpacing = element.GetDouble("ItemSpacing", 0.0);

			return Layout.StackFromTop(
					Spacer.Medium,

					Layout.StackFromLeft(
							editors.RadioButton(orientation)
								.Option(Orientation.Horizontal, (fg, bg) => StackIcon.Create(Axis2D.Horizontal, fg), "Orientation: Horizontal")
								.Option(Orientation.Vertical, (fg, bg) => StackIcon.Create(Axis2D.Vertical, fg), "Orientation: Vertical")
								.Control.WithLabel("Orientation"),
							Spacer.Medium,
							editors.Label("Item Spacing", itemSpacing),
							editors.Field(itemSpacing)
								.WithWidth(CellLayout.HalfCellWidth))
						.WithInspectorPadding(),

					Spacer.Medium)
				.MakeCollapsable(RectangleEdge.Bottom, element.Is("Fuse.Controls.StackPanel"));
		}
	}
	public enum Orientation
	{
		Horizontal,
		Vertical
	}

}
