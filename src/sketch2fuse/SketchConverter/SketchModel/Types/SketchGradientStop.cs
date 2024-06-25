namespace SketchConverter.SketchModel
{
	public class SketchGradientStop
	{
		public readonly SketchColor Color;
		public readonly double Position;

		public SketchGradientStop(SketchColor color, double position)
		{
			Color = color;
			Position = position;
		}
	}
}
