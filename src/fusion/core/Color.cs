using System;
using System.Globalization;

namespace Outracks.Fusion
{
	public sealed class Color : IEquatable<Color>
	{
		public static readonly Color AlmostTransparent = new Color(0, 0, 0, 1/255.0f);
		public static readonly Color Transparent = new Color(0, 0, 0, 0);
		public static readonly Color Black = new Color(0, 0, 0, 1);
		public static readonly Color White = new Color(1, 1, 1, 1);

		public bool IsTransparent
		{
			get { return ReferenceEquals(this, Transparent) || Math.Abs(A) < float.Epsilon; }
		}

		public readonly float R;
		public readonly float G;
		public readonly float B;
		public readonly float A;

		public static Color FromBytes(int r, int g, int b, int a = 255)
		{
			return new Color(r / 255.0f, g / 255.0f, b /255.0f, a / 255.0f);
		}

		public static Color FromRgb(int rgb)
		{
			var rgba = (rgb << 8) | 0xff;
			return FromRgba((uint)rgba);
		}

		public static Color FromRgba(uint rgba)
		{
			var r = (rgba >> 24) & 0xff;
			var g = (rgba >> 16) & 0xff;
			var b = (rgba >> 8) & 0xff;
			var a = rgba & 0xff;
			return FromBytes((int)r, (int)g, (int)b, (int)a);
		}

		public Color(float r, float g, float b, float a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public bool Equals(Color other)
		{
			return R.Equals(other.R) && G.Equals(other.G) && A.Equals(other.A) && B.Equals(other.B);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Color && Equals((Color)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = R.GetHashCode();
				hashCode = (hashCode * 397) ^ G.GetHashCode();
				hashCode = (hashCode * 397) ^ A.GetHashCode();
				hashCode = (hashCode * 397) ^ B.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(Color left, Color right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Color left, Color right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return "{" + R.ToString(CultureInfo.InvariantCulture) + ", " + G.ToString(CultureInfo.InvariantCulture) + ", " + B.ToString(CultureInfo.InvariantCulture) + ", " + A.ToString(CultureInfo.InvariantCulture) + "}";
		}

		public Color WithAlpha(float a)
		{
			return new Color(R, G, B, a);
		}

		public Color Mix(Color other, float amount)
		{
			return new Color(
				R + (other.R - R) * amount,
				G + (other.G - G) * amount,
				B + (other.B - B) * amount,
				A + (other.A - A) * amount);
		}
	}
}