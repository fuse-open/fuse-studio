using System;

namespace Outracks
{
	public struct Points : IEquatable<Points>, IComparable<Points>, INumeric<Points>
	{
		public readonly double Value;

		public Points(double value)
		{
			Value = value;
		}

		// TODO: should this really be implicit?
		public static implicit operator Points(double value)
		{
			return new Points(value);
		}

		public static explicit operator float(Points points)
		{
			return (float)points.Value;
		}

		public static implicit operator double(Points points)
		{
			return points.Value;
		}

		public static bool operator ==(Points left, Points right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Points left, Points right)
		{
			return !(left == right);
		}

		public static Points operator +(Points left, Points right)
		{
			return left.Add(right);
		}
		public static Points operator -(Points left, Points right)
		{
			return left.Sub(right);
		}
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Points && Equals((Points)obj);
		}

		public override int GetHashCode()
		{
			return 0; // Don't bother
		}

		public bool Equals(Points other)
		{
			return Math.Abs(Value - other.Value) < 0.1; // that's basically the same point if anyone cares
		}

		public int CompareTo(Points other)
		{
			return Value.CompareTo(other.Value);
		}

		public override string ToString()
		{
			return Value + "pt";
		}

		public Points Add(Points other)
		{
			return new Points(Value + other.Value);
		}

		public Points Inverse()
		{
			return new Points(-Value);
		}

		public Points Zero
		{
			get { return new Points(0); }
		}

		public Points Mul(Points other)
		{
			return new Points(Value * other.Value);
		}

		public Points One
		{
			get { return new Points(1); }
		}

		public Points Reciprocal()
		{
			return new Points(1.0 / Value);
		}

		public Points FromDouble(double value)
		{
			return new Points(value);
		}

		public double ToDouble()
		{
			return Value;
		}
	}
}