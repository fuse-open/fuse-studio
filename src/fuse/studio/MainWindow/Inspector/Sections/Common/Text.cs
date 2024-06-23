using System.Linq;
using System.Reactive.Linq;
using Outracks.Fuse.Studio.Inspector;
using Outracks.Fuse.Inspector.Editors;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	class TextSection
	{
		public static IControl Create(IProject project, IElement element, IEditorFactory editors)
		{
			return Layout.StackFromTop(
					Spacer.Medium,
					CreateEditors(project, element, editors).WithInspectorPadding(),
					Spacer.Medium)
				.MakeCollapsable(RectangleEdge.Bottom, element.Is("Fuse.Controls.TextControl"));
		}

		public static IControl CreateEditors(IProject project, IElement element, IEditorFactory editors)
		{
			var value = Value(element);
			var font = element.GetString("Font", "");
			var fontSize = element.GetPoints("FontSize", 16);
			var textAlignment = element.GetEnum("TextAlignment", TextAlignment.Left);
			var lineSpacing = element.GetPoints("LineSpacing", 16);
			var textWrapping = element.GetEnum("TextWrapping", TextWrapping.NoWrap);
			var textColor = element.GetColor("TextColor", new Color(0, 0, 0, 1));

			var fonts = project.GlobalElements
				.Where(e => e.Is("Fuse.Font"))
				.SelectPerElement(e => e.UxGlobal().NotNone())
				.ToObservableEnumerable();

			return Layout.StackFromTop(
				editors.Field(value, placeholderText: "Add Text Value here").SetToolTip("Text value"),
				Spacer.Medium,
				DropdownEditor.Create(font, fonts.Select(f => f.ToArray()), editors, placeholderText: "Add Font here").SetToolTip("Font"),
				Spacer.Medium,
				Layout.Dock()
					.Left(editors.Color(textColor).WithWidth(new Points(100)).SetToolTip("Text color"))
					.Right(editors.Field(lineSpacing).WithIcon("Line spacing", Icons.LineSpacing()).WithWidth(new Points(70)))
					.Fill(editors.Field(fontSize).WithIcon("Font size", Icons.FontSize()).WithWidth(new Points(70)).Center()),
				Spacer.Medium,
				Layout.Dock()
					.Left(editors.RadioButton(textWrapping)
						.Option(TextWrapping.NoWrap, (fg, bg) => Icons.NoWrap(fg), "Do not wrap text at end of line")
						.Option(TextWrapping.Wrap, (fg, bg) => Icons.Wrap(fg), "Wrap text at end of line")
						.Control.CenterHorizontally())
					.Right(editors.RadioButton(textAlignment)
						.Option(TextAlignment.Left, (fg, bg) => TextAlignmentIcon.Create(TextAlignment.Left, fg), "Text alignment: Left")
						.Option(TextAlignment.Center, (fg, bg) => TextAlignmentIcon.Create(TextAlignment.Center, fg), "Text alignment: Center")
						.Option(TextAlignment.Right, (fg, bg) => TextAlignmentIcon.Create(TextAlignment.Right, fg), "Text alignment: Right")
						.Control.CenterHorizontally())
					.Fill());
		}

		public static IAttribute<string> Value(IElement element)
		{
			var valueFromAttribute = element["Value"];
			var valueFromContent = element.Content;

			return valueFromContent
				.Select(contentValue =>
					contentValue.HasValue
						? valueFromContent
						: valueFromAttribute)
				.Switch()
				.Convert(s => Parsed.Success(s, s), s => s, "");

		}
	}

	public enum TextTruncation
	{
		Standard,
		None,
	}

	public enum TextWrapping
	{
		NoWrap,
		Wrap,
	}
}
