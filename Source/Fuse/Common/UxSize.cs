namespace Outracks.Fuse
{
	public struct UxSize
	{
		public static UxSize Points(Points value)
		{
			return new UxSize
			{
				PointsValue = value,
			};
		}
		public static UxSize Pixels(Pixels value)
		{
			return new UxSize
			{
				PixelsValue = value,
			};
		}

		public static UxSize Percentages(Percentages value)
		{
			return new UxSize
			{
				PercentagesValue = value,
			};
		}

		public Optional<Points> PointsValue { get; private set; }
		public Optional<Pixels> PixelsValue { get; private set; }
		public Optional<Percentages> PercentagesValue { get; private set; }
	}
}