using System.Windows.Media;

namespace ColorPicker.ColorModels
{
   public abstract class ColorComponent
    {
        //The largest possible value for a component (value when slider at top)
        public abstract int MaxValue { get; }

        //The smallest possible value for a component (value when slider at bottom)
        public abstract int MinValue { get; }

       //The value of the component for a given color
        public abstract int Value(Color color);

       //The name of the color component (used to avoid reflection)
        public abstract string Name{ get; }
    }
}
