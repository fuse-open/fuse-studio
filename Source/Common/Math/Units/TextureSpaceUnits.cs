namespace Outracks
{
	public struct TextureSpaceUnits : INumeric<TextureSpaceUnits>
	{
		public readonly double Value;

		public TextureSpaceUnits(double value)
		{
			Value = value;
		}

		public static implicit operator TextureSpaceUnits(double value)
		{
			return new TextureSpaceUnits(value);
		}

		public static implicit operator float(TextureSpaceUnits pixels)
		{
			return (float)pixels.Value;
		}
		public static bool operator ==(TextureSpaceUnits left, TextureSpaceUnits right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TextureSpaceUnits left, TextureSpaceUnits right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is TextureSpaceUnits && Equals((TextureSpaceUnits)obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public bool Equals(TextureSpaceUnits other)
		{
			return Value.Equals(other.Value);
		}
		public int CompareTo(TextureSpaceUnits other)
		{
			return Value.CompareTo(other);
		}

		public override string ToString()
		{
			return Value + "t";
		}

		public TextureSpaceUnits Add(TextureSpaceUnits other)
		{
			return new TextureSpaceUnits(Value + other.Value);
		}

		public TextureSpaceUnits Inverse()
		{
			return new TextureSpaceUnits(-Value);
		}

		public TextureSpaceUnits Zero
		{
			get { return new TextureSpaceUnits(0); }
		}

		public TextureSpaceUnits Mul(TextureSpaceUnits other)
		{
			return new TextureSpaceUnits(Value * other.Value);
		}

		public TextureSpaceUnits One
		{
			get { return new TextureSpaceUnits(1); }
		}

		public TextureSpaceUnits Reciprocal()
		{
			return new TextureSpaceUnits(1.0 / Value);
		}

		public TextureSpaceUnits FromDouble(double value)
		{
			return new TextureSpaceUnits(value);
		}

		public double ToDouble()
		{
			return Value;
		}
	}
}