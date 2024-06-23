using SketchConverter.SketchParser;

namespace SketchConverter.SketchModel
{
	public class SketchText : SketchLayer
	{
		public readonly SketchAttributedString AttributedString;
		public readonly SketchTextBoxAlignment TextBoxAlignment;
		public new SketchStyle Style { get { return ((SketchLayer)this).Style.Value; } }

		public SketchText(SketchLayer layer,
			SketchAttributedString attributedString,
			SketchTextBoxAlignment textBoxAlignment) : base(layer)
		{
			if (!layer.Style.HasValue)
			{
				throw new SketchParserException($"Expected '{layer.Name}' to have a style, but didn't find one.");
			}
			AttributedString = attributedString;
			TextBoxAlignment = textBoxAlignment;
		}
	}

	public enum SketchTextBoxAlignment
	{
		Auto,
		Fixed
	}
}
