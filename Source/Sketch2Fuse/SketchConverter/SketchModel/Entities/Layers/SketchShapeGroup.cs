using SketchConverter.SketchParser;

namespace SketchConverter.SketchModel
{
	public class SketchShapeGroup : SketchLayer
	{
		public readonly bool HasClippingMask;
		public new SketchStyle Style { get { return ((SketchLayer)this).Style.Value; } }

		public SketchShapeGroup(
			SketchLayer parentLayer,
			bool hasClippingMask
			): base(parentLayer)
		{
			if (!parentLayer.Style.HasValue)
			{
				throw new SketchParserException($"Expected '{parentLayer.Name}' to have a style, but didn't find one.");
			}
			HasClippingMask = hasClippingMask;
		}
	}
}
