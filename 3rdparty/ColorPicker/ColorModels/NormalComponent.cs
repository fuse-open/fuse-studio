using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPicker.ColorModels
{
    public abstract class NormalComponent : ColorComponent
    {
        //Is the Normal bitmap independent of the specific color (false for all but Hue of HSB)
        public abstract bool IsNormalIndependantOfColor { get; } 

        //Updates the normal Bitmap (The bitmap with the slider)
        public abstract void UpdateNormalBitmap(WriteableBitmap bitmap, Color color);

        //Updates the color plane bitmap (the bitmap where one selects the colors)
        public abstract void UpdateColorPlaneBitmap(WriteableBitmap bitmap, int normalComponentValue);

        //Gets the color corresponding to a selected point (with 255 alpha)
        public abstract Color  ColorAtPoint(Point selectionPoint,int colorComponentValue);

        //Gets the point on the color plane that corresponds to the color (alpha ignored)
        public abstract Point PointFromColor(Color color);
    }
}
