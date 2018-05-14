using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPicker.ColorModels.HSB
{
    class Hue : NormalComponent 
    {
        private static readonly HSBModel  sModel = new HSBModel();
        
        
        public override int MinValue
        {
            get { return 0; }
        }

        public override int MaxValue
        {
            get {return 359; }
        }

        public override void UpdateNormalBitmap(WriteableBitmap bitmap, Color color)
        {

            unsafe
            {
                bitmap.Lock();
                int currentPixel = -1;
                byte* pStart = (byte*)(void*)bitmap.BackBuffer;
                double iRowUnit = (double)360/256;
               double iRowCurrent = 359;
                for (int iRow = 0; iRow < bitmap.PixelHeight; iRow++)
                {

                    Color hueColor = sModel.Color(iRowCurrent, 1, 1);
                    for (int iCol = 0; iCol < bitmap.PixelWidth; iCol++)
                    {
                        currentPixel++;
                        *(pStart + currentPixel * 3 + 0) = hueColor.B ; //Blue
                        *(pStart + currentPixel * 3 + 1) = hueColor.G; //Green 
                        *(pStart + currentPixel * 3 + 2) = hueColor.R; //red
                    }
                         
                    iRowCurrent -= iRowUnit;
                  
                }

                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }


        }

        public override void UpdateColorPlaneBitmap(WriteableBitmap bitmap, int normalComponentValue)
        {
            unsafe
            {
                bitmap.Lock();
                byte* pStart = (byte*)(void*)bitmap.BackBuffer;
                int currentPixel = -1;
                double iRowUnit = (double)1 / 256;
                double iColUnit = (double)1 / 256;
                double iRowCurrent = 1;
                
                  double r = 0;
                  double g = 0;
                  double b = 0;
                double hue = 359 - normalComponentValue;
                for (int iRow = 0; iRow < bitmap.PixelHeight; iRow++)
                {
                   double iColCurrent = 0;
                    for (int iCol = 0; iCol < bitmap.PixelWidth; iCol++)
                    {
                        double saturation = iColCurrent;
                        double brightness = iRowCurrent;
                        //Taken from HSBModel for speed purposes

                     

                        if (saturation == 0)
                        {
                            r = g = b = brightness;
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

                       
                        currentPixel++;
                        *(pStart + currentPixel * 3 + 0) = Convert.ToByte( g*255); //Blue
                        *(pStart + currentPixel * 3 + 1) = Convert.ToByte(b * 255); //Green 
                        *(pStart + currentPixel * 3 + 2) = Convert.ToByte(r * 255); //red
                        iColCurrent += iColUnit;
                    }
                    iRowCurrent -= iRowUnit;
                }
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }
        }


        //Original version , however too slow so the hsb calculation was inlined into the function so as to avoid conversion to 
        //color and back.
        //public override void UpdateColorPlaneBitmap(WriteableBitmap bitmap, int normalComponentValue)
        //{
        //    unsafe
        //    {
        //        bitmap.Lock();
        //        byte* pStart = (byte*)(void*)bitmap.BackBuffer;
        //        int currentPixel = -1;
        //        double iRowUnit = (double)1 / 256;
        //        double iColUnit = (double)1 / 256;
        //        double iRowCurrent = 1;


        //        for (int iRow = 0; iRow < bitmap.PixelHeight; iRow++)
        //        {
        //            double iColCurrent = 0;
        //            for (int iCol = 0; iCol < bitmap.PixelWidth; iCol++)
        //            {
        //                Color hueColor = sModel.Color(normalComponentValue, iColCurrent, iRowCurrent);
        //                currentPixel++;
        //                *(pStart + currentPixel * 3 + 0) = hueColor.B; //Blue
        //                *(pStart + currentPixel * 3 + 1) = hueColor.G; //Green 
        //                *(pStart + currentPixel * 3 + 2) = hueColor.R; //red
        //                iColCurrent += iColUnit;
        //            }
        //            iRowCurrent -= iRowUnit;
        //        }
        //        bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
        //        bitmap.Unlock();
        //    }
        //}



        public override Color ColorAtPoint(Point selectionPoint, int colorComponentValue)
        {
            var hue = colorComponentValue;
            var brightness =  (1 - selectionPoint.Y/255);
            var saturation =  (selectionPoint.X/255);
            return sModel.Color(hue, saturation, brightness);
        }

        public override Point PointFromColor(Color color)
        {
            int x = Convert.ToInt32(sModel.SComponent(color) * 255);
            int y = 255 - Convert.ToInt32(sModel.BComponent(color) * 255);
            return new Point(x,y);
        }

        public override int Value(Color color)
        {
            return Convert.ToInt32(sModel.HComponent( color));
        }

        public override string Name
        {
            get { return "HSB_Blue"; }
        }

        public override bool IsNormalIndependantOfColor
        {
            get { return true; }
        }
    }
}
