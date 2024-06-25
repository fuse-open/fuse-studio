namespace Outracks
{
	public struct Pixels : INumeric<Pixels>
	{
		public readonly double Value;

		public Pixels(double value)
		{
			Value = value;
		}

		public static implicit operator Pixels(double value)
		{
			return new Pixels(value);
		}

		public static explicit operator int(Pixels pixels)
		{
			return (int)pixels.Value;
		}

		public static bool operator ==(Pixels left, Pixels right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Pixels left, Pixels right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Pixels && Equals((Pixels)obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public bool Equals(Pixels other)
		{
			return Value.Equals(other.Value);
		}
		public int CompareTo(Pixels other)
		{
			return Value.CompareTo(other.Value);
		}

		public override string ToString()
		{
			return Value + "px";
		}

		public Pixels Add(Pixels other)
		{
			return new Pixels(Value + other.Value);
		}

		public Pixels Inverse()
		{
			return new Pixels(-Value);
		}
		public Pixels Zero
		{
			get { return new Pixels(0); }
		}

		public Pixels Mul(Pixels other)
		{
			return new Pixels(Value * other.Value);
		}

		public Pixels One
		{
			get { return new Pixels(1); }
		}

		public Pixels Reciprocal()
		{
			return new Pixels(1.0 / Value);
		}

		public Pixels FromDouble(double value)
		{
			return new Pixels(value);
		}

		public double ToDouble()
		{
			return Value;
		}
	}
}