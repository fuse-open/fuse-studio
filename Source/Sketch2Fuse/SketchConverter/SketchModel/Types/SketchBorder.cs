namespace SketchConverter.SketchModel
{
	public class SketchBorder
	{
		public readonly bool IsEnabled;
		public readonly ISketchBrush Brush;
		public readonly double Thickness;
		public readonly SketchBorderPosition Position;

		public SketchBorder(bool isEnabled, ISketchBrush brush, double thickness, SketchBorderPosition position)
		{
			IsEnabled = isEnabled;
			Brush = brush;
			Thickness = thickness;
			Position = position;
		}
	}

	public enum SketchBorderPosition
	{
		Center,
		Inside,
		Outside
	}
}
