using Outracks.Fusion;

namespace Outracks.Fuse.Theming.Themes
{
	class ColorMap : IColorMap
	{
		public static readonly ColorMap Singleton = new ColorMap();

		static readonly Color OldActive = Color.FromRgb(0x6dc0d2);
		static readonly Color LinkColor = Color.FromRgb(0x707aff);

		public Color Map(Color color)
		{
			if (color == OldActive)
				return LinkColor;

			return color;
		}
	}
}
