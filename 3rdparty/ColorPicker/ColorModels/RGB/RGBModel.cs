using System;
using System.Windows.Media;

namespace ColorPicker.ColorModels.RGB
{
    class RGBModel:ColorModel
    {
        public enum EComponents
        {
            Red,
            Green, 
            Blue
        }

        public double Distance (Color  color1, Color color2)
        {
            return Math.Sqrt(
                Math.Pow(color1.R - color2.R,2) + 
                Math.Pow(color1.G - color2.G,2) + 
                Math.Pow(color1.B - color2.B,2)  
                );
        }
    }
}
