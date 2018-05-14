using System;
using System.Windows.Media;

namespace ColorPicker.ColorModels.Lab
{
	class LabModel
	{
		private const double D65X = 0.9505;
		 private const double D65Y =1.0;
		 private const double D65Z = 1.0890;

		public Color Color (double l, double a , double b)
		{
			double theta = 6.0/29.0;

			double fy = (l+16)/116.0;
			double fx = fy + (a/500.0);
			double fz = fy - (b/200.0);

			var x = (fx > theta) ? D65X*(fx*fx*fx) : (fx - 16.0/116.0)*3*(theta*theta)*D65X;
			var y = (fy > theta) ? D65Y*(fy*fy*fy) : (fy - 16.0/116.0)*3*(theta*theta)*D65Y;
			var z =	(fz > theta)?D65Z * (fz*fz*fz) : (fz - 16.0/116.0)*3*(theta*theta)*D65Z;

            x = (x > 0.9505) ? 0.9505 : ((x < 0) ? 0 : x);
            y = (y > 1.0) ? 1.0 : ((y < 0) ? 0 : y);
            z = (z > 1.089) ? 1.089 : ((z < 0) ? 0 : z);

			double[] Clinear = new double[3];
			Clinear[0] = x*3.2410 - y*1.5374 - z*0.4986; // red
			Clinear[1] = -x*0.9692 + y*1.8760 - z*0.0416; // green
			Clinear[2] = x*0.0556 - y*0.2040 + z*1.0570; // blue

			for(int i=0; i<3; i++)
			{
				Clinear[i] = (Clinear[i]<=0.0031308)? 12.92*Clinear[i] : (1+0.055)* Math.Pow(Clinear[i], (1.0/2.4)) - 0.055;
				Clinear[i] = Math.Min(Clinear[i], 1);
				Clinear[i] = Math.Max(Clinear[i], 0);
			}

			return System.Windows.Media.Color.FromRgb(
				Convert.ToByte(Clinear[0]*255.0),
				Convert.ToByte(Clinear[1]*255.0),
				Convert.ToByte(Clinear[2]*255.0));
				

			 
		}
		public double LComponent(Color color)
		{
			// normalize red, green, blue values
			double rLinear = color.R / 255.0;
			double gLinear = color.G / 255.0;
			double bLinear = color.B / 255.0;

			double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (1 + 0.055), 2.2) : (rLinear / 12.92);
			double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (1 + 0.055), 2.2) : (gLinear / 12.92);
			double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (1 + 0.055), 2.2) : (bLinear / 12.92);

		 
			double CIEY = r * 0.2126 + g * 0.7152 + b * 0.0722;
			 

			double l = 116.0 * Fxyz(CIEY / D65Y) - 16;
			return l;
		}

		public double AComponent(Color color)
		{
			double rLinear = color.R / 255.0;
			double gLinear = color.G / 255.0;
			double bLinear = color.B / 255.0;

			double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (1 + 0.055), 2.2) : (rLinear / 12.92);
			double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (1 + 0.055), 2.2) : (gLinear / 12.92);
			double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (1 + 0.055), 2.2) : (bLinear / 12.92);

			double CIEX = r * 0.4124 + g * 0.3576 + b * 0.1805;
			double CIEY = r * 0.2126 + g * 0.7152 + b * 0.0722;
		 


			double a = 500.0 * (Fxyz(CIEX / D65X) - Fxyz(CIEY / D65Y));
			return a;
		}
			
		
		public double BComponent(Color color)
		{
			double rLinear = color.R / 255.0;
			double gLinear = color.G / 255.0;
			double bLinear = color.B / 255.0;

			double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (1 + 0.055), 2.2) : (rLinear / 12.92);
			double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (1 + 0.055), 2.2) : (gLinear / 12.92);
			double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (1 + 0.055), 2.2) : (bLinear / 12.92);

		
			double CIEY = r * 0.2126 + g * 0.7152 + b * 0.0722;
			double CIEZ = r * 0.0193 + g * 0.1192 + b * 0.9505;


			return  200.0 * (Fxyz(CIEY / D65Y) - Fxyz(CIEZ / D65Z));
			 
		}
		 
		private static double Fxyz(double t)
		{

			return ((t > 0.008856)? Math.Pow(t, (1.0/3.0)) : (7.787*t + 16.0/116.0));
		}
		 
	}
}
