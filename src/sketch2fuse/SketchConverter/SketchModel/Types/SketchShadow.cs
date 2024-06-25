namespace SketchConverter.SketchModel
{
	public class SketchShadow
	{
		public readonly bool IsEnabled;
		public readonly SketchColor Color;
		public readonly SketchPoint Offset;
		public readonly double BlurRadius;
		public readonly double Spread;

		public SketchShadow(bool isEnabled, SketchColor color, SketchPoint offset, double blurRadius, double spread)
		{
			IsEnabled = isEnabled;
			Color = color;
			Offset = offset;
			BlurRadius = blurRadius;
			Spread = spread;
		}
	}
}
