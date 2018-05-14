using System;

namespace Outracks
{
	public class TextOffset : IComparable<TextOffset>
	{
		readonly int _offset;
		
		public TextOffset(int offset)
		{
			_offset = offset;
		}

		public static implicit operator int(TextOffset d)
		{
			return d._offset;
		}

		public int CompareTo(TextOffset other)
		{
			return _offset.CompareTo(other._offset);
		}

		public override string ToString()
		{
			return "@"+_offset;
		}

		public override int GetHashCode()
		{
			return _offset.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is TextOffset && Equals((TextOffset)obj);
		}

		public bool Equals(TextOffset other)
		{
			return _offset == other._offset;
		}

		public static bool operator ==(TextOffset a, TextOffset b)
		{
			if ((object)a == null)
				return (object)b == null;

			return a.Equals(b);
		}

		public static bool operator !=(TextOffset a, TextOffset b)
		{
			return !(a == b);
		}

		public static bool operator <(TextOffset a, TextOffset b)
		{
			return a._offset < b._offset;
		}

		public static bool operator >(TextOffset a, TextOffset b)
		{
			return a._offset > b._offset;
		}

		public static bool operator <=(TextOffset a, TextOffset b)
		{
			return a._offset <= b._offset;
		}

		public static bool operator >=(TextOffset a, TextOffset b)
		{
			return a._offset >= b._offset;
		}
	}

}