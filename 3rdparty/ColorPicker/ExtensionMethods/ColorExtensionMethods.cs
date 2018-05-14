using System;
using System.Windows.Media;

namespace ColorPicker.ExtensionMethods
{
    public static class ColorExtensionMethods
    {
        public static double Intensity(this Color color)
        {
            return (double)(color.R  + color.G  + color.B  ) /( 3 * 255);
        }

        public static double Brightness(this Color color)
        {
            return (double)Math.Max(Math.Max(color.R, color.G), color.B) / 255;
        }

        public static double SaturationHSB(this Color color)
        {
            var max = (double)Math.Max(Math.Max(color.R, color.G), color.B) / 255;
            if (max == 0) return 0;
            var min = (double)Math.Min(Math.Min(color.R, color.G), color.B) / 255;
            return (max - min)/max;
        }

        public static double Lightness(this Color color)
        {
             var max = (double)Math.Max(Math.Max(color.R, color.G), color.B) / 255;
             var min = (double)Math.Min(Math.Min(color.R, color.G), color.B) / 255;
             return (max + min)/2;
        }

        public static double Chroma(this Color color)
        {
            var max = (double)Math.Max(Math.Max(color.R, color.G), color.B) / 255;
            var min = (double)Math.Min(Math.Min(color.R, color.G), color.B) / 255;
            return max - min;
        }

        public static double SaturationHSL(this Color color)
        {
            var max = (double)Math.Max(Math.Max(color.R, color.G), color.B) / 255;
            var min = (double)Math.Min(Math.Min(color.R, color.G), color.B) / 255;
            var chroma = max - min;

            var lightness = (max + min)/2;
            if (lightness <= .5)
            {
                return chroma/(2*lightness);
            }
            return chroma / (2 - 2 * lightness);
        }


        public static Color WithAlpha(this Color color, byte alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        public static Color WithR(this Color color, byte r)
        {
            return Color.FromArgb(color.A , r, color.G, color.B);
        }

        public static Color WithG(this Color color, byte g)
        {
            return Color.FromArgb(color.A, color.R,g, color.B);
        }
        public static Color WithB(this Color color, byte b)
        {
            return Color.FromArgb(color.A, color.R, color.G, b);
        }
       
    }
}