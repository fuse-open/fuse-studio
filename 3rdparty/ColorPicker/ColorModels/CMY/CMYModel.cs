using System;
using System.Windows.Media;

namespace ColorPicker.ColorModels.CMY
{
  public class CMYModel
    {
        public enum ECMYComponent
        {
            Cyan = 0,
            Magenta = 1,
            Yellow = 2,
        }


        #region components

        public double CComponent(Color color)
        {
            return 255 - color.R ;
        }


        public Double MComponent(Color color)
        {
            return 255 - color.G ;
        }

        public Double YComponent(Color color)
        {
            return 255 - color.B;
        }


        #endregion
    }
}
