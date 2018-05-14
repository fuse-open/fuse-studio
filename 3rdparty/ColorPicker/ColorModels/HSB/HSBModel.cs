using System;
using System.Windows.Media;
using ColorPicker.ExtensionMethods;

namespace ColorPicker.ColorModels.HSB
{
 public  class HSBModel
    {
    public enum EHSBComponent
     {
         Hue=0,
         Saturation= 1,
         Brightness =2
     }

        Color Color(double[] components)
        {
            return Color(components[0], components[1], components[2]);
        }

        public Color Color(double hue, double saturation, double brightness)
        {
            double r = 0;
            double g = 0;
            double b = 0;

            if (saturation == 0)
            {
                r = g = b =brightness;
            }
            else
            {
                // the color wheel consists of 6 sectors. Figure out which sector you're in.
                double sectorPos = hue / 60.0;
                int sectorNumber = (int)(Math.Floor(sectorPos));
                // get the fractional part of the sector
                double fractionalSector = sectorPos - sectorNumber;

                // calculate values for the three axes of the color. 
                double p = brightness * (1.0 - saturation);
                double q = brightness * (1.0 - (saturation * fractionalSector));
                double t = brightness * (1.0 - (saturation * (1 - fractionalSector)));

                // assign the fractional colors to r, g, and b based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        r = brightness;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = brightness;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = brightness;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = brightness;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = brightness;
                        break;
                    case 5:
                        r = brightness;
                        g = p;
                        b = q;
                        break;
                }
            }
       
         
            return System.Windows.Media.Color.FromRgb(
                Convert.ToByte(( r * 255.0)),
                Convert.ToByte(( g * 255.0)),
                Convert.ToByte(( b * 255.0))
                );


        }

        public double HComponent(Color color)
        {
            System.Drawing.Color c = System.Drawing.Color.FromArgb(255,color.R,color.G,color.B );
            return  c.GetHue();
        }

        public Double SComponent(Color color)
        {
            return color.SaturationHSB();
        }

        public Double BComponent(Color color)
        {
            return color.Brightness();
        }

     public Double Component(Color color ,int pos)
     {
         if (pos == 0)
         {
             return HComponent(color);
         }
         else if (pos == 1)
         {
             return SComponent(color);
         }
         else if (pos == 2)
         {
             return BComponent(color);
         }
         else
         {
             throw new Exception("The HSB model has only 3 components");
         }
     }


     public Double Component(Color color, EHSBComponent component)
     {
         return Component(color,(int) component );
     }


    }
}
