using System;

namespace Outracks
{
	/// <summary>
	/// 1-indexed line number
	/// </summary>
	public struct LineNumber : IComparable<LineNumber>, IEquatable<LineNumber>
	{
		readonly int _lineMinusOne;

		public LineNumber(int line)
		{
			if (line <= 0) throw new ArgumentOutOfRangeException("line", "LineNumber must be >= 1");
			_lineMinusOne = line - 1;
		}

		public static implicit operator int(LineNumber line)
		{
			return line._lineMinusOne + 1;
		}

		public override string ToString()
		{
			return ((int)this).ToString();
		}

		public bool Equals(LineNumber other)
		{
			return _lineMinusOne == other._lineMinusOne;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LineNumber && Equals((LineNumber)obj);
		}

		public override int GetHashCode()
		{
			return _lineMinusOne;
		}

		public static bool operator ==(LineNumber left, LineNumber right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LineNumber left, LineNumber right)
		{
			return !left.Equals(right);
		}

		public static bool operator <(LineNumber left, LineNumber right)
		{
			return left._lineMinusOne < right._lineMinusOne;
		}

		public static bool operator >(LineNumber left, LineNumber right)
		{
			return left._lineMinusOne > right._lineMinusOne;
		}

		public static bool operator <=(LineNumber left, LineNumber right)
		{
			return left._lineMinusOne <= right._lineMinusOne;
		}

		public static bool operator >=(LineNumber left, LineNumber right)
		{
			return left._lineMinusOne >= right._lineMinusOne;
		}

		public int CompareTo(LineNumber other)
		{
			return _lineMinusOne.CompareTo(other._lineMinusOne);
		}
	}
}