using System;
using System.Windows.Media;
using ColorPicker.ExtensionMethods;

namespace ColorPicker.ColorModels.CMYK
{
  public  class CMYKModel
  {


      #region Color

      public enum ECMYKComponent
      {
          Cyan = 0,
          Magenta = 1,
          Yellow = 2,
          Black =3
      }


      Color Color(double[] components)
      {
          return Color(components[0], components[1], components[2], components[3]);
      }

      public Color Color(double cyan, double magenta, double yellow, double black)
      {

          var red =   (255 - cyan - black  ).RestrictToByte()  ;
          var green = (255 - magenta - black).RestrictToByte();
          var blue =  (255 - yellow - black).RestrictToByte();
          return System.Windows.Media.Color.FromRgb( red, green, blue);

      }

       


      #endregion

      #region components

      private double MinComponent(Color color)
      {
          double c = 255 - color.R;
          double m = 255 - color.G ;
          double y = 255 - color.B;

         return   Math.Min(c, Math.Min(m, y));
      }

      public double CComponent(Color color , Double greedieness)
      {
          if (greedieness > 1 || greedieness < 0)
          {
              throw new Exception("Greedieness must be between 0 and 1");
          }
          var min = MinComponent(color);
          return 255 - color.R - min * greedieness ;
      }

      public double CComponent(Color color)
      {
          var min = MinComponent(color);
          return 255 - color.R - min;
      }

      public Double MComponent(Color color, Double greedieness)
      {
          if (greedieness > 1 || greedieness < 0)
          {
              throw new Exception("Greedieness must be between 0 and 1");
          }
          var min = MinComponent(color);
          return 255 - color.G - min*greedieness;
      }

      public Double MComponent(Color color)
      {
          var min = MinComponent(color);
          return 255 - color.G - min;
      }

      public Double YComponent(Color color, Double greedieness)
      {
          var min = MinComponent(color);
          return 255 - color.B - min * greedieness;
      }


      public Double YComponent(Color color)
      {
          var min = MinComponent(color);
          return 255 - color.B - min;
      }

      public Double KComponent(Color color, Double greedieness)
      {
          var min = MinComponent(color);
          return min*greedieness;  
      }


       public Double KComponent(Color color)
       {
           var min = MinComponent(color);
           return min;
       }

      #endregion
  }
}
