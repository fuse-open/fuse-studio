using System.Collections.Generic;

namespace SketchConverter.SketchModel
{
	public class SketchLinearGradientBrush : ISketchBrush
	{
		public readonly SketchPoint From;
		public readonly SketchPoint To;

		public readonly IReadOnlyList<SketchGradientStop> Stops;

		public SketchLinearGradientBrush(SketchPoint fromPoint, SketchPoint toPoint, IReadOnlyList<SketchGradientStop> stops)
		{
			From = fromPoint;
			To = toPoint;
			Stops = stops;
		}
	}
}
