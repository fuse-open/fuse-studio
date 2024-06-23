namespace SketchConverter.SketchModel
{
	public class SketchColor
	{
		public readonly double Red;
		public readonly double Green;
		public readonly double Blue;
		public readonly double Alpha;

		public SketchColor(double red, double green, double blue, double alpha)
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = alpha;
		}

		public SketchColor()
			: this(1,1,1,1)
		{
		}

		public static SketchColor Black = new SketchColor(0, 0, 0, 1);
	}
}
