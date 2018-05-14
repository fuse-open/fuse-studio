using System;

namespace ColorPicker.ExtensionMethods
{
 public static   class DoubleExtensionMethods
    {

     public static int AsPercent (this double number)
     {
         return Convert.ToInt32(number*100);
     }

     public static  double PercentageOf(this double number , double value)
     {
         return (number/value)*100;
     }

     public static double RestrictToRange(this double number , double min, double max)
     {
         return Math.Min(Math.Max(number, min), max);
     }

     public static byte RestrictToByte(this double number)
     {
         return Convert.ToByte(number.RestrictToRange(0,255));
     }

    }
}
