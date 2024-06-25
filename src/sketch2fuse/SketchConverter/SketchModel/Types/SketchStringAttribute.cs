namespace SketchConverter.SketchModel
{
    public class SketchStringAttribute
    {
        public readonly SketchColor Color;
        public readonly double FontSize;
        public readonly SketchTextAlignment Alignment;

        public SketchStringAttribute(SketchColor color, double fontSize, SketchTextAlignment alignment)
        {
            Color = color;
            FontSize = fontSize;
            Alignment = alignment;
        }
    }
}