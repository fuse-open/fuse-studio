using System;
using System.Windows;
using System.Windows.Media;

namespace ColorPicker.ColorModels.Lab
{
sealed	class Lightness:NormalComponent 
	{
		private static LabModel sModel = new LabModel();
		public override bool IsNormalIndependantOfColor
		{
			get { return false; }
		}

		public override void UpdateNormalBitmap(System.Windows.Media.Imaging.WriteableBitmap bitmap, System.Windows.Media.Color color)
		{
			unsafe
			{
				bitmap.Lock();
				int currentPixel = -1;
				byte* pStart = (byte*)(void*)bitmap.BackBuffer;
				double iRowUnit = (double)100 / 256;
				double iRowCurrent = 100;
				double a = sModel.AComponent(color);
				double b = sModel.BComponent(color);
				for (int iRow = 0; iRow < bitmap.PixelHeight; iRow++)
				{

					Color lightness = sModel.Color(iRowCurrent, a, b);
					for (int iCol = 0; iCol < bitmap.PixelWidth; iCol++)
					{
						currentPixel++;
						*(pStart + currentPixel * 3 + 0) = lightness.B; //Blue
						*(pStart + currentPixel * 3 + 1) = lightness.G; //Green 
						*(pStart + currentPixel * 3 + 2) = lightness.R; //red
					}

					iRowCurrent -= iRowUnit;

				}

				bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
				bitmap.Unlock();
			}
		}

		public override void UpdateColorPlaneBitmap(System.Windows.Media.Imaging.WriteableBitmap bitmap, int normalComponentValue)
		{
			unsafe
			{
				bitmap.Lock();
				byte* pStart = (byte*)(void*)bitmap.BackBuffer;
				int currentPixel = -1;
				double iRowUnit = (double)1  ;
				double iColUnit = (double)1 ;
				double iRowCurrent = 127;

			  
				double l = (double)normalComponentValue ;
				for (int iRow = 0; iRow < bitmap.PixelHeight; iRow++)
				{
					double b= iRowCurrent;
					double iColCurrent = -128;
					for (int iCol = 0; iCol < bitmap.PixelWidth; iCol++)
					{
						double theta = 6.0 / 29.0;
						double a = iColCurrent;
						double fy = (l + 16) / 116.0;
						double fx = fy + (a / 500.0);
						double fz = fy - (b / 200.0);

						var x = (fx > theta) ? D65X * (fx * fx * fx) : (fx - 16.0 / 116.0) * 3 * (theta * theta) * D65X;
						var y = (fy > theta) ? D65Y * (fy * fy * fy) : (fy - 16.0 / 116.0) * 3 * (theta * theta) * D65Y;
						var z = (fz > theta) ? D65Z * (fz * fz * fz) : (fz - 16.0 / 116.0) * 3 * (theta * theta) * D65Z;

                        x = (x > 0.9505) ? 0.9505 : ((x < 0) ? 0 : x);
                        y = (y > 1.0) ? 1.0 : ((y < 0) ? 0 : y);
                        z = (z > 1.089) ? 1.089 : ((z < 0) ? 0 : z);

						double[] Clinear = new double[3];
						Clinear[0] = x * 3.2410 - y * 1.5374 - z * 0.4986; // red
						Clinear[1] = -x * 0.9692 + y * 1.8760 - z * 0.0416; // green
						Clinear[2] = x * 0.0556 - y * 0.2040 + z * 1.0570; // blue

						for (int i = 0; i < 3; i++)
						{
							Clinear[i] = (Clinear[i] <= 0.0031308) ? 12.92 * Clinear[i] : (1 + 0.055) * Math.Pow(Clinear[i], (1.0 / 2.4)) - 0.055;
                            Clinear[i] = Math.Min(Clinear[i], 1);
                            Clinear[i] = Math.Max(Clinear[i], 0);
			 
						}


						currentPixel++;
						*(pStart + currentPixel * 3 + 0) = Convert.ToByte(Clinear[2] * 255); //Blue
						*(pStart + currentPixel * 3 + 1) = Convert.ToByte(Clinear[1] * 255); //Green 
						*(pStart + currentPixel * 3 + 2) = Convert.ToByte(Clinear[0] * 255); //red
						iColCurrent += iColUnit;
					}
					iRowCurrent -= iRowUnit;
				}
				bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
				bitmap.Unlock();
			}
		}

		public override Color ColorAtPoint(Point selectionPoint, int colorComponentValue)
		{
			var l = colorComponentValue;
			var b = ( 127 - selectionPoint.Y  );
			var a =  selectionPoint.X -128;
			return sModel.Color(l, a, b);
		}

		public override Point PointFromColor(Color color)
		{
			int x =128 + Convert.ToInt32(sModel.AComponent(color));
            int y = 128 - Convert.ToInt32(sModel.BComponent(color));
			return new Point(x, y);
		}

		public override int MinValue
		{
			get { return 0; }
		}

		public override int MaxValue
		{
			get { return 100; }
		}

		public override int Value(System.Windows.Media.Color color)
		{
		   return Convert.ToInt32( sModel.LComponent(color));
		}

		public override string Name
		{
			get {return "LAB_Lightness"; }
		}
		private const double D65X = 0.9505;
		private const double D65Y = 1.0;
		private const double D65Z = 1.0890;
		private static double Fxyz(double t)
		{

			return ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));
		}

	}
}
