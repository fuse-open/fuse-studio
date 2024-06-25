using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	class EachSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			var items = element.GetString("Items", "");
			var count = element.GetDouble("Count", 0);

			return Layout.StackFromTop(
					Spacer.Medium,

					Layout.StackFromLeft(
							editors.Field(items).WithLabelAbove("Items"),
							Spacer.Medium,
							editors.Field(count).WithLabelAbove("Count"))
						.WithInspectorPadding(),

					Spacer.Medium)
				.MakeCollapsable(RectangleEdge.Bottom, element.Is("Fuse.Reactive.Each"));
		}
	}
}
