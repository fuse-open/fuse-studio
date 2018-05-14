namespace SketchConverter.SketchModel
{
	public struct SketchPoint
	{
		public readonly double X;
		public readonly double Y;

		public SketchPoint(double x, double y)
		{
			X = x;
			Y = y;
		}

		public static SketchPoint operator /(SketchPoint a, SketchPoint b)
			=> new SketchPoint(a.X / b.X, a.Y / b.Y);

		public static SketchPoint operator *(SketchPoint a, SketchPoint b)
			=> new SketchPoint(a.X * b.X, a.Y * b.Y);

		public static SketchPoint operator +(SketchPoint a, SketchPoint b)
			=> new SketchPoint(a.X + b.X, a.Y + b.Y);

		public static SketchPoint operator -(SketchPoint a, SketchPoint b)
			=> new SketchPoint(a.X - b.X, a.Y - b.Y);

		public static SketchPoint operator *(SketchPoint point, double scale)
			=> new SketchPoint(point.X * scale, point.Y * scale);

		public static SketchPoint operator /(SketchPoint point, double scale)
			=> new SketchPoint(point.X / scale, point.Y / scale);
	}
}
