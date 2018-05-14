namespace SketchConverter.SketchModel
{
	public class SketchSolidColorBrush : ISketchBrush
	{
		public readonly SketchColor Color;

		public SketchSolidColorBrush(SketchColor color)
		{
			Color = color;
		}
	}
}
