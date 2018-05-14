using SketchConverter.SketchParser;

namespace SketchConverter.SketchModel
{
	public class SketchBitmap : SketchLayer
	{
		public readonly SketchImage Image;
		public new SketchStyle Style { get { return ((SketchLayer)this).Style.Value; } }

		public SketchBitmap(SketchLayer layer, SketchImage image) : base(layer)
		{
			if (!layer.Style.HasValue)
			{
				throw new SketchParserException($"Expected '{layer.Name}' to have a style, but didn't find one.");
			}
			Image = image;
		}
	}
}
