using System;
using System.Globalization;
using System.IO;

namespace Outracks
{
	public static class Ratio
	{
		public static Ratio<TNum, TDenom> Read<TNum, TDenom>(BinaryReader reader)
			where TNum: INumeric<TNum>, new()
			where TDenom: INumeric<TDenom>, new()
		{
			return new Ratio<TNum, TDenom>(reader.ReadDouble());
		}

		public static void Write<TNum, TDenom>(BinaryWriter writer, Ratio<TNum, TDenom> size)
			where TNum: INumeric<TNum>, new()
			where TDenom: INumeric<TDenom>, new()
		{
			writer.Write(size.Value);
		}
	}

	public partial struct Ratio<TNum, TDenom> : IEquatable<Ratio<TNum, TDenom>>, IComparable<Ratio<TNum, TDenom>>
		where TNum: INumeric<TNum>, new()
		where TDenom: INumeric<TDenom>, new()
	{
		public readonly double Value;
		public Ratio(double value) 
		{
			Value = value;
		}
		
		public static implicit operator Ratio<TNum, TDenom>(double value)
		{
			return new Ratio<TNum, TDenom>(value);
		}

		public static implicit operator double(Ratio<TNum, TDenom> ratio)
		{
			return ratio.Value;
		}

		public bool Equals(Ratio<TNum, TDenom> other)
		{
			return Value.Equals(other.Value);
		}

		public int CompareTo(Ratio<TNum, TDenom> other)
		{
			return Value.CompareTo(other.Value);
		}

		public Ratio<TDenom, TNum> Reciprocal()
		{
			return new Ratio<TDenom, TNum>(1.0/Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Ratio<TNum, TDenom> && Equals((Ratio<TNum, TDenom>)obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}
	}
	
}