namespace Outracks
{
	public struct Inches : INumeric<Inches>
	{
		public readonly double Value;

		public Inches(double inches)
		{
			Value = inches;
		}

		public static implicit operator Inches(double value)
		{
			return new Inches(value);
		}

		public bool Equals(Inches other)
		{
			return Value.Equals(other.Value);
		}

		public int CompareTo(Inches other)
		{
			return Value.CompareTo(other);
		}

		public Inches Add(Inches other)
		{
			return new Inches(Value + other.Value);
		}

		public Inches Inverse()
		{
			return new Inches(-Value);
		}

		public Inches Zero { get { return new Inches(0); }  }
		public Inches Mul(Inches other)
		{
			return new Inches(Value * other.Value);
		}

		public Inches One { get { return new Inches(1); } }
		public Inches Reciprocal()
		{
			return new Inches(1.0/Value);
		}

		public Inches FromDouble(double value)
		{
			return new Inches(value);
		}

		public double ToDouble()
		{
			return Value;
		}
	}
}