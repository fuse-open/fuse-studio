namespace SketchConverter.SketchModel
{
	public class SketchRect
	{
		public readonly double X;
		public readonly double Y;

		public readonly double Width;
		public readonly double Height;

		public readonly bool ConstrainProportions;

		public SketchRect(double x, double y, double width, double height, bool constrainProportions)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			ConstrainProportions = constrainProportions;
		}
	}
}
