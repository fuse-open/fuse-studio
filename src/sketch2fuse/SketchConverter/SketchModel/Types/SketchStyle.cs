using System.Collections.Generic;

namespace SketchConverter.SketchModel
{
	public class SketchStyle
	{
		public readonly IReadOnlyList<SketchFill> Fills;
		public readonly IReadOnlyList<SketchBorder> Borders;
		public readonly IReadOnlyList<SketchShadow> Shadows;
		public readonly IReadOnlyList<SketchShadow> InnerShadows;
		public readonly Optional<SketchBlur> Blur;
		public readonly Optional<double> Opacity;

		public SketchStyle(
			IReadOnlyList<SketchFill> fills,
			IReadOnlyList<SketchBorder> borders,
			IReadOnlyList<SketchShadow> shadows,
			IReadOnlyList<SketchShadow> innerShadows,
			Optional<SketchBlur> blur,
			Optional<double> opacity)
		{
			Fills = fills;
			Borders = borders;
			Shadows = shadows;
			InnerShadows = innerShadows;
			Blur = blur;
			Opacity = opacity;
		}
	}
}
