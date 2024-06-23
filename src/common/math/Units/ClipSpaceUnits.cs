namespace Outracks
{
	public struct ClipSpaceUnits : INumeric<ClipSpaceUnits>
	{
		public readonly double Value;

		public ClipSpaceUnits(double value)
		{
			Value = value;
		}

		public static implicit operator ClipSpaceUnits(double value)
		{
			return new ClipSpaceUnits(value);
		}

		public static explicit operator int(ClipSpaceUnits pixels)
		{
			return (int)pixels.Value;
		}
		public static implicit operator float(ClipSpaceUnits pixels)
		{
			return (float)pixels.Value;
		}
		public static bool operator ==(ClipSpaceUnits left, ClipSpaceUnits right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ClipSpaceUnits left, ClipSpaceUnits right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is ClipSpaceUnits && Equals((ClipSpaceUnits)obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public bool Equals(ClipSpaceUnits other)
		{
			return Value.Equals(other.Value);
		}
		public int CompareTo(ClipSpaceUnits other)
		{
			return Value.CompareTo(other);
		}

		public override string ToString()
		{
			return Value + "c";
		}

		public ClipSpaceUnits Add(ClipSpaceUnits other)
		{
			return new ClipSpaceUnits(Value + other.Value);
		}

		public ClipSpaceUnits Inverse()
		{
			return new ClipSpaceUnits(-Value);
		}

		public ClipSpaceUnits Zero
		{
			get { return new ClipSpaceUnits(0); }
		}

		public ClipSpaceUnits Mul(ClipSpaceUnits other)
		{
			return new ClipSpaceUnits(Value * other.Value);
		}

		public ClipSpaceUnits One
		{
			get { return new ClipSpaceUnits(1);}
		}

		public ClipSpaceUnits Reciprocal()
		{
			return new ClipSpaceUnits(1.0/Value);
		}

		public ClipSpaceUnits FromDouble(double value)
		{
			return new ClipSpaceUnits(value);
		}

		public double ToDouble()
		{
			return Value;
		}
	}
}