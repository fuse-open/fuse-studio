namespace SketchConverter.SketchModel
{
	public class SketchFill
	{
		public readonly bool IsEnabled;
		public readonly ISketchBrush Brush;

		public SketchFill(bool isEnabled, ISketchBrush brush)
		{
			IsEnabled = isEnabled;
			Brush = brush;
		}
	}
}
