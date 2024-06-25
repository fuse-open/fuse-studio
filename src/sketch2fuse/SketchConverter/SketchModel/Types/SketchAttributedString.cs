using System.Collections.Generic;

namespace SketchConverter.SketchModel
{
	public class SketchAttributedString
	{
		public readonly string Contents;
		public readonly IReadOnlyList<SketchStringAttribute> Attributes;

		public SketchAttributedString(string contents, IReadOnlyList<SketchStringAttribute> attributes)
		{
			Contents = contents;
			Attributes = attributes;
		}

	}

	public enum SketchTextAlignment
	{
		Left,
		Right,
		Center,
		Justified,
		Natural
	}
}
