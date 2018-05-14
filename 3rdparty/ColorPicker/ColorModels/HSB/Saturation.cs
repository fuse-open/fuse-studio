using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPicker.ColorModels.HSB
{
    class Saturation:NormalComponent 
    {

        private static readonly HSBModel sModel = new HSBModel();

        public override int MinValue
        {
            get { return 0; }
        }

        public override int MaxValue
        {
            get { return 100; }
        }

        public override void UpdateNormalBitmap(WriteableBitmap bitmap, Color color)
        {
            unsafe
            {
                bitmap.Lock();
                int currentPixel = -1;
                byte* pStart = (byte*)(void*)bitmap.BackBuffer;
                double iRowUnit = (double)1 / 256;
                double iRowCurrent = 1;
                double hue = sModel.HComponent(color);
                double brightness = sModel.BComponent(color);
                for (int iRow = 0; iRow < bitmap.PixelHeight; iRow++)
                {

                    Color hueColor = sModel.Color(hue,iRowCurrent,  brightness);
                    for (int iCol = 0; iCol < bitmap.PixelWidth; iCol++)
                    {
                        currentPixel++;
                        *(pStart + currentPixel * 3 + 0) = hueColor.B; //Blue
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
                double iColUnit = (double)360 / 256;
                double iRowCurrent = 1;

                double r = 0;
                double g = 0;
                double b = 0;
                double saturation = (double)normalComponentValue/ 100;
                for (int iRow = 0; iRow < bitmap.PixelHeight; iRow++)
                {
                    double iColCurrent = 359;
                    for (int iCol = 0; iCol < bitmap.PixelWidth; iCol++)
                    {
                        double hue = iColCurrent;
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
                        *(pStart + currentPixel * 3 + 0) = Convert.ToByte(g * 255); //Blue
                        *(pStart + currentPixel * 3 + 1) = Convert.ToByte(b * 255); //Green 
                        *(pStart + currentPixel * 3 + 2) = Convert.ToByte(r * 255); //red
                        iColCurrent -= iColUnit;
                    }
                    iRowCurrent -= iRowUnit;
                }
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }
        }

        public override Color ColorAtPoint(Point selectionPoint, int colorComponentValue)
        {
            var hue = (359 * selectionPoint.X / 255);
            var brightness =(1 - selectionPoint.Y / 255);
            var saturation = (double) colorComponentValue / 100;
            var color = sModel.Color(hue, saturation, brightness);
            
            return color;
        }

        public override Point PointFromColor(Color color)
        {
            int x = Convert.ToInt32(sModel.HComponent(color)/359 * 255);
            int y = 255 - Convert.ToInt32(sModel.BComponent(color) * 255);
            return new Point(x, y);
        }

        public override int Value(System.Windows.Media.Color color)
        {

         
            return Convert.ToInt32((sModel.SComponent(color)) * 100);
        }

        public override string Name
        {
            get { return "HSB_Saturation"; }
        }

        public override bool IsNormalIndependantOfColor
        {
            get {return false; }
        }
    }
}
