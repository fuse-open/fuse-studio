namespace Outracks
{
	public struct GlWindowPixels : INumeric<GlWindowPixels>
	{
		public readonly int Value;

		public GlWindowPixels(int value)
		{
			Value = value;
		}

		public static implicit operator GlWindowPixels(int value)
		{
			return new GlWindowPixels(value);
		}

		public static implicit operator int(GlWindowPixels pixels)
		{
			return (int)pixels.Value;
		}
		public static bool operator ==(GlWindowPixels left, GlWindowPixels right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GlWindowPixels left, GlWindowPixels right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is GlWindowPixels && Equals((GlWindowPixels)obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public bool Equals(GlWindowPixels other)
		{
			return Value.Equals(other.Value);
		}
		public int CompareTo(GlWindowPixels other)
		{
			return Value.CompareTo(other);
		}

		public override string ToString()
		{
			return Value + "c";
		}

		public GlWindowPixels Add(GlWindowPixels other)
		{
			return new GlWindowPixels(Value + other.Value);
		}

		public GlWindowPixels Inverse()
		{
			return new GlWindowPixels(-Value);
		}

		public GlWindowPixels Zero
		{
			get { return new GlWindowPixels(0); }
		}

		public GlWindowPixels Mul(GlWindowPixels other)
		{
			return new GlWindowPixels(Value * other.Value);
		}

		public GlWindowPixels One
		{
			get { return new GlWindowPixels(1); }
		}

		public GlWindowPixels Reciprocal()
		{
			return new GlWindowPixels(1 / Value);
		}

		public GlWindowPixels FromDouble(double value)
		{
			return new GlWindowPixels((int)value);
		}

		public double ToDouble()
		{
			return Value;
		}
	}
}