namespace Outracks
{
	public struct Percentages : INumeric<Percentages>
	{
		public readonly double Value;

		public Percentages(double value)
		{
			Value = value;
		}

		public static implicit operator Percentages(double value)
		{
			return new Percentages(value);
		}

		public static explicit operator int(Percentages pixels)
		{
			return (int)pixels.Value;
		}

		public static bool operator ==(Percentages left, Percentages right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Percentages left, Percentages right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Percentages && Equals((Percentages)obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public bool Equals(Percentages other)
		{
			return Value.Equals(other.Value);
		}
		public int CompareTo(Percentages other)
		{
			return Value.CompareTo(other.Value);
		}

		public override string ToString()
		{
			return Value + "%";
		}

		public Percentages Add(Percentages other)
		{
			return new Percentages(Value + other.Value);
		}

		public Percentages Inverse()
		{
			return new Percentages(-Value);
		}
		public Percentages Zero
		{
			get { return new Percentages(0); }
		}

		public Percentages Mul(Percentages other)
		{
			return new Percentages(Value * other.Value);
		}

		public Percentages One
		{
			get { return new Percentages(1); }
		}

		public Percentages Reciprocal()
		{
			return new Percentages(1.0 / Value);
		}

		public Percentages FromDouble(double value)
		{
			return new Percentages(value);
		}

		public double ToDouble()
		{
			return Value;
		}
	}
}