using System.Diagnostics;

namespace SketchImporter.UxGenerator
{
	[DebuggerDisplay("{Value} {Unit}")]
	public class UxSize : IUxSerializeable
	{
		protected bool Equals(UxSize other)
		{
			return Value.Equals(other.Value) && Unit == other.Unit;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((UxSize) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Value.GetHashCode() * 397) ^ (int) Unit;
			}
		}

		public float Value { get; set; }
		public UxUnit Unit { get; set; } = UxUnit.Points;

		public UxSize(float value, UxUnit unit)
		{
			Value = value;
			Unit = unit;
		}

		public static implicit operator UxSize(UxFloat value) => new UxSize(value.Value, UxUnit.Points);

		UxSize WithValue(float value)
		{
			return new UxSize(value, Unit);
		}

		public static UxSize operator *(UxSize size, float factor)
			=> size.WithValue(size.Value * factor);

		public static UxSize operator /(UxSize size, float divisor)
			=> size.WithValue(size.Value / divisor);

		public static bool operator ==(UxSize a, UxSize b)
		{
			return a?.Value == b?.Value
				   && a?.Unit == b?.Unit;
		}

		public static bool operator !=(UxSize a, UxSize b)
		{
			return !(a == b);
		}

		public static UxSize Points(float value)
			=> new UxSize(value, UxUnit.Points);

		public static UxSize Percent(float value)
			=> new UxSize(value, UxUnit.Percent);

		public static UxSize Pixels(float value)
			=> new UxSize(value, UxUnit.Pixels);

		string SerializeUnit()
		{
			switch (Unit)
			{
				case UxUnit.Percent:
					return "%";
				case UxUnit.Pixels:
					return "px";
				case UxUnit.Points:
				default:
					return "";
			}
		}

		public string SerializeUx(UxSerializerContext ctx)
		{
			return new UxFloat(Value).SerializeUx(ctx) + SerializeUnit();
		}
	}
}
